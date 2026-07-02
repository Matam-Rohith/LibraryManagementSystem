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
using Serilog;
using System.Text;
using System.Reflection;

// Serilog bootstrap
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .WriteTo.Console()
    .Enrich.FromLogContext());

// PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
bool isPostgres = connectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase)
    || connectionString.Contains("postgresql", StringComparison.OrdinalIgnoreCase)
    || connectionString.Contains("postgres", StringComparison.OrdinalIgnoreCase);

if (isPostgres)
    builder.Services.AddDbContext<AppDbContext>(opt =>
        opt.UseNpgsql(connectionString, o => o.EnableRetryOnFailure(3)));
else
    builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(connectionString));

// Identity
builder.Services.AddIdentity<User, IdentityRole>(opt =>
{
    opt.Password.RequireDigit = true;
    opt.Password.RequiredLength = 8;
    opt.Password.RequireUppercase = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// JWT
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("MemberOrAdmin", policy => policy.RequireRole("Admin", "Member"));
});

// Repositories
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IBorrowRepository, BorrowRepository>();
builder.Services.AddScoped<IFineRepository, FineRepository>();
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();

// Services
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IBorrowService, BorrowService>();
builder.Services.AddScoped<IFineService, FineService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// MongoDB Activity Logs
builder.Services.AddSingleton<IActivityLogService, ActivityLogService>();

// Open Library external API (HttpClient)
builder.Services.AddHttpClient<IOpenLibraryService, OpenLibraryService>();

// Health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "postgresql", tags: new[] { "db" });

// Swagger with annotations + XML docs
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Library Management System API",
        Version = "v1",
        Description = "ASP.NET Core 8 Web API for managing library books, members, borrowing, returns, reservations, and fines.\n\n" +
                      "**Roles:** Admin (full access), Member (read + borrow/reserve)\n\n" +
                      "**External API:** Open Library integration for book metadata\n\n" +
                      "**Activity Logs:** All actions logged to MongoDB",
        Contact = new OpenApiContact { Name = "Matam Rohith", Url = new Uri("https://github.com/Matam-Rohith") },
        License = new OpenApiLicense { Name = "MIT" }
    });
    c.EnableAnnotations();
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(opt =>
    opt.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        logger.LogInformation("Applying database schema...");
        await db.Database.EnsureCreatedAsync();
        foreach (var role in new[] { "Admin", "Member" })
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        if (await userManager.FindByEmailAsync("admin@library.com") == null)
        {
            var admin = new User
            {
                FullName = "Library Admin",
                Email = "admin@library.com",
                UserName = "admin@library.com",
                MembershipId = "LIB-ADMIN-001"
            };
            await userManager.CreateAsync(admin, "Admin@123456");
            await userManager.AddToRoleAsync(admin, "Admin");
        }
        logger.LogInformation("Database seeded successfully.");
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "Database initialization failed.");
        throw;
    }
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseCors("AllowAll");
app.UseSerilogRequestLogging();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Library Management System API v1");
    c.RoutePrefix = "swagger";
    c.DocumentTitle = "Library API Docs";
    c.DisplayRequestDuration();
});

app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapHealthChecks("/health");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
