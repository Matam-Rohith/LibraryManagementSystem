using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Middleware;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Repositories;
using LibraryManagementSystem.Repositories.Interfaces;
using LibraryManagementSystem.Services;
using LibraryManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;
using Serilog;
using System.Text;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

static string NormalizePostgresConnectionString(string value)
{
    if (!value.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) && !value.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase)) return value;
    var uri = new Uri(value);
    var parts = uri.UserInfo.Split(':', 2);
    return new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host, Port = uri.Port > 0 ? uri.Port : 5432,
        Username = Uri.UnescapeDataString(parts[0]),
        Password = parts.Length > 1 ? Uri.UnescapeDataString(parts[1]) : string.Empty,
        Database = uri.AbsolutePath.Trim('/'), SslMode = SslMode.Require, TrustServerCertificate = true
    }.ConnectionString;
}
var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration).WriteTo.Console().Enrich.FromLogContext());
var rawConnectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DefaultConnection is required.");
var connectionString = NormalizePostgresConnectionString(rawConnectionString);
bool isPostgres = connectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase) || connectionString.Contains("postgres", StringComparison.OrdinalIgnoreCase);
if (isPostgres) builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(connectionString, o => o.EnableRetryOnFailure(3)));
else builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(connectionString));

builder.Services.AddIdentity<User, IdentityRole>(opt => { opt.Password.RequireDigit = true; opt.Password.RequiredLength = 8; opt.Password.RequireUppercase = true; }).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is required.");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(opt => opt.TokenValidationParameters = new TokenValidationParameters { ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true, ValidIssuer = builder.Configuration["Jwt:Issuer"], ValidAudience = builder.Configuration["Jwt:Audience"], IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)) });
builder.Services.AddAuthorization(options => { options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin")); options.AddPolicy("MemberOrAdmin", policy => policy.RequireRole("Admin", "Member")); });

builder.Services.AddRateLimiter(options => { options.RejectionStatusCode = StatusCodes.Status429TooManyRequests; options.AddFixedWindowLimiter("api", limiter => { limiter.PermitLimit = 120; limiter.Window = TimeSpan.FromMinutes(1); limiter.QueueLimit = 0; }); });
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IBorrowRepository, BorrowRepository>();
builder.Services.AddScoped<IFineRepository, FineRepository>();
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IBorrowService, BorrowService>();
builder.Services.AddScoped<IFineService, FineService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<IActivityLogService, ActivityLogService>();
builder.Services.AddHttpClient<IOpenLibraryService, OpenLibraryService>(client => { client.Timeout = TimeSpan.FromSeconds(10); });
builder.Services.AddHealthChecks();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo { Title = "Library Management System API", Version = "v1", Description = "ASP.NET Core 8 REST API for books, members, borrowing, reservations, and fines." }); c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { In = ParameterLocation.Header, Description = "Enter: Bearer {your JWT token}", Name = "Authorization", Type = SecuritySchemeType.ApiKey, Scheme = "Bearer" }); c.AddSecurityRequirement(new OpenApiSecurityRequirement { { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() } }); });
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(opt => opt.AddPolicy("Frontend", p => { if (allowedOrigins.Length == 0) p.AllowAnyOrigin(); else p.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod(); }));
var app = builder.Build();
using (var scope = app.Services.CreateScope()) { var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>(); try { var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>(); var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>(); var db = scope.ServiceProvider.GetRequiredService<AppDbContext>(); await db.Database.EnsureCreatedAsync();
        var universityCatalog = new[]
        {
            ("Operating System Concepts", "Abraham Silberschatz", "Computer Science", "Wiley", 2018), ("Computer Networks", "Andrew S. Tanenbaum", "Computer Science", "Pearson", 2021), ("Database System Concepts", "Abraham Silberschatz", "Computer Science", "McGraw-Hill", 2019), ("Clean Code", "Robert C. Martin", "Software Engineering", "Prentice Hall", 2008), ("Design Patterns", "Erich Gamma", "Software Engineering", "Addison-Wesley", 1994), ("Introduction to Algorithms", "Thomas H. Cormen", "Computer Science", "MIT Press", 2022), ("Artificial Intelligence: A Modern Approach", "Stuart Russell", "Artificial Intelligence", "Pearson", 2021), ("Machine Learning", "Tom M. Mitchell", "Artificial Intelligence", "McGraw-Hill", 2017), ("Deep Learning", "Ian Goodfellow", "Artificial Intelligence", "MIT Press", 2016), ("Computer Architecture", "David A. Patterson", "Computer Engineering", "Morgan Kaufmann", 2020),
            ("Digital Design", "Morris Mano", "Electronics", "Pearson", 2018), ("Microprocessors and Microcontrollers", "Krishna Kant", "Electronics", "PHI Learning", 2019), ("Signals and Systems", "Alan V. Oppenheim", "Electronics", "Pearson", 2016), ("Communication Systems", "Simon Haykin", "Electronics", "Wiley", 2014), ("Power Systems", "C. L. Wadhwa", "Electrical Engineering", "New Age", 2018), ("Electrical Machines", "I. J. Nagrath", "Electrical Engineering", "McGraw-Hill", 2017), ("Engineering Mechanics", "S. Timoshenko", "Mechanical Engineering", "McGraw-Hill", 2015), ("Thermodynamics", "Yunus Cengel", "Mechanical Engineering", "McGraw-Hill", 2019), ("Fluid Mechanics", "R. K. Rajput", "Mechanical Engineering", "S. Chand", 2018), ("Manufacturing Engineering", "Serope Kalpakjian", "Mechanical Engineering", "Pearson", 2016),
            ("Engineering Mathematics I", "B. S. Grewal", "Mathematics", "Khanna Publishers", 2020), ("Higher Engineering Mathematics", "B. V. Ramana", "Mathematics", "McGraw-Hill", 2018), ("Linear Algebra", "Gilbert Strang", "Mathematics", "Wellesley-Cambridge", 2016), ("Calculus", "James Stewart", "Mathematics", "Cengage", 2019), ("Discrete Mathematics", "Kenneth Rosen", "Mathematics", "McGraw-Hill", 2019), ("Engineering Physics", "R. K. Gaur", "Physics", "Dhanpat Rai", 2017), ("Engineering Chemistry", "Jain and Jain", "Chemistry", "Dhanpat Rai", 2018), ("Organic Chemistry", "Morrison and Boyd", "Chemistry", "Pearson", 2016), ("University Physics", "Hugh Young", "Physics", "Pearson", 2020), ("Physical Chemistry", "Puri Sharma Pathania", "Chemistry", "Vishal", 2017),
            ("Principles of Marketing", "Philip Kotler", "Business", "Pearson", 2022), ("Financial Accounting", "T. S. Grewal", "Commerce", "Sultan Chand", 2020), ("Corporate Finance", "Stephen Ross", "Finance", "McGraw-Hill", 2021), ("Human Resource Management", "Gary Dessler", "Management", "Pearson", 2020), ("Operations Management", "Jay Heizer", "Management", "Pearson", 2019), ("Business Statistics", "S. P. Gupta", "Statistics", "Sultan Chand", 2018), ("Managerial Economics", "Dominick Salvatore", "Economics", "Oxford", 2017), ("Entrepreneurship Development", "S. S. Khanka", "Entrepreneurship", "S. Chand", 2019), ("Business Communication", "Meenakshi Raman", "Communication", "Oxford", 2018), ("Organizational Behaviour", "Stephen Robbins", "Management", "Pearson", 2021),
            ("Gray's Anatomy for Students", "Richard Drake", "Medicine", "Elsevier", 2020), ("Medical Physiology", "Guyton and Hall", "Medicine", "Elsevier", 2021), ("Robbins Pathology", "Vinay Kumar", "Medicine", "Elsevier", 2020), ("Pharmacology", "K. D. Tripathi", "Pharmacy", "Jaypee", 2019), ("Community Medicine", "K. Park", "Public Health", "Banarsidas", 2021), ("Biochemistry", "U. Satyanarayana", "Biotechnology", "Elsevier", 2019), ("Molecular Biology", "Robert Weaver", "Biotechnology", "McGraw-Hill", 2018), ("Microbiology", "Prescott", "Biology", "McGraw-Hill", 2020), ("Human Anatomy", "B. D. Chaurasia", "Medicine", "CBS", 2018), ("Dental Materials", "Anusavice", "Dentistry", "Elsevier", 2017),
            ("Constitutional Law", "H. M. Seervai", "Law", "Universal", 2019), ("Jurisprudence", "Salmond", "Law", "Sweet and Maxwell", 2018), ("Criminal Law", "Ratanlal and Dhirajlal", "Law", "LexisNexis", 2020), ("Law of Contracts", "Avtar Singh", "Law", "Eastern Book", 2021), ("Company Law", "A. Ramaiya", "Law", "LexisNexis", 2019), ("Modern Indian History", "Bipan Chandra", "Humanities", "Orient BlackSwan", 2018), ("World History", "William McNeill", "History", "Oxford", 2017), ("Introduction to Psychology", "Atkinson and Hilgard", "Psychology", "Cengage", 2020), ("Sociology", "Anthony Giddens", "Social Sciences", "Polity", 2019), ("Research Methodology", "C. R. Kothari", "Research", "New Age", 2018)
        };
        var existingIsbns = db.Books.Select(b => b.ISBN).ToHashSet();
        var additions = universityCatalog.Select((x, i) => new Book { Title = x.Item1, Author = x.Item2, Category = x.Item3, Publisher = x.Item4, PublishedYear = x.Item5, ISBN = $"97800000{i + 1000:0000}", TotalCopies = 3 + (i % 6), AvailableCopies = 3 + (i % 6) }).Where(b => !existingIsbns.Contains(b.ISBN)).ToList();
        if (additions.Count > 0) { db.Books.AddRange(additions); await db.SaveChangesAsync(); } foreach (var role in new[] { "Admin", "Member", "Assistant" }) if (!await roleManager.RoleExistsAsync(role)) await roleManager.CreateAsync(new IdentityRole(role)); if (await userManager.FindByEmailAsync("admin@library.com") == null) { var admin = new User { FullName = "Library Admin", Email = "admin@library.com", UserName = "admin@library.com", MembershipId = "LIB-ADMIN-001" }; await userManager.CreateAsync(admin, "Admin@123456"); await userManager.AddToRoleAsync(admin, "Admin"); } } catch (Exception ex) { logger.LogError(ex, "Database initialization failed — app will still start."); } }
app.UseMiddleware<ExceptionMiddleware>();
app.UseCors("Frontend");
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseSerilogRequestLogging();
app.UseSwagger();
app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Library Management System API v1"); c.RoutePrefix = "swagger"; });
app.MapGet("/", () => Results.Redirect("/index.html"));
app.MapFallbackToFile("index.html");
app.MapHealthChecks("/health");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers().RequireRateLimiting("api");
app.Run();
