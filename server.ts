import express, { Request, Response, NextFunction } from 'express';
import cors from 'cors';
import path from 'path';
import { fileURLToPath } from 'url';
import jwt from 'jsonwebtoken';
import bcrypt from 'bcryptjs';
import dotenv from 'dotenv';

dotenv.config();

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const app = express();
const PORT = process.env.PORT || 3000;
const JWT_SECRET = process.env.JWT_SECRET || process.env.JWT_SECRET_KEY || 'libra-super-secret-key-at-least-32-chars-long';

app.use(cors());
app.use(express.json());

// In-Memory Data Models
interface User {
  id: string;
  fullName: string;
  email: string;
  passwordHash: string;
  role: 'Admin' | 'Member' | 'Assistant';
  membershipId: string;
  createdAt: string;
}

interface Book {
  id: number;
  title: string;
  author: string;
  category: string;
  publisher: string;
  publishedYear: number;
  isbn: string;
  totalCopies: number;
  availableCopies: number;
}

interface BorrowRecord {
  id: number;
  userId: string;
  bookId: number;
  borrowedAt: string;
  dueDate: string;
  returnedAt: string | null;
  isReturned: boolean;
}

interface Reservation {
  id: number;
  userId: string;
  bookId: number;
  reservedAt: string;
  expiresAt: string;
  isActive: boolean;
}

interface Fine {
  id: number;
  userId: string;
  borrowRecordId: number;
  amount: number;
  isPaid: boolean;
  issuedAt: string;
  paidAt: string | null;
}

interface ActivityLog {
  id: number;
  userId?: string;
  action: string;
  details: string;
  timestamp: string;
}

// State Stores
const users: Map<string, User> = new Map();
const books: Map<number, Book> = new Map();
const borrowRecords: Map<number, BorrowRecord> = new Map();
const reservations: Map<number, Reservation> = new Map();
const fines: Map<number, Fine> = new Map();
const activityLogs: ActivityLog[] = [];

let bookIdSeq = 1;
let borrowIdSeq = 1;
let reservationIdSeq = 1;
let fineIdSeq = 1;
let logIdSeq = 1;

function logActivity(action: string, details: string, userId?: string) {
  activityLogs.unshift({
    id: logIdSeq++,
    userId,
    action,
    details,
    timestamp: new Date().toISOString()
  });
  if (activityLogs.length > 500) activityLogs.pop();
}

// Seed Users
const adminPasswordHash = bcrypt.hashSync('Admin@123456', 10);
const memberPasswordHash = bcrypt.hashSync('Member@123456', 10);

const adminUser: User = {
  id: 'usr-admin-01',
  fullName: 'Library Admin',
  email: 'admin@library.com',
  passwordHash: adminPasswordHash,
  role: 'Admin',
  membershipId: 'LIB-ADMIN-001',
  createdAt: new Date().toISOString()
};
users.set(adminUser.email.toLowerCase(), adminUser);

const memberUser: User = {
  id: 'usr-member-01',
  fullName: 'Student Member',
  email: 'member@library.com',
  passwordHash: memberPasswordHash,
  role: 'Member',
  membershipId: 'LIB-MEMBER-001',
  createdAt: new Date().toISOString()
};
users.set(memberUser.email.toLowerCase(), memberUser);

// Seed Catalog (50 University Titles)
const initialCatalog = [
  ["Operating System Concepts", "Abraham Silberschatz", "Computer Science", "Wiley", 2018],
  ["Computer Networks", "Andrew S. Tanenbaum", "Computer Science", "Pearson", 2021],
  ["Database System Concepts", "Abraham Silberschatz", "Computer Science", "McGraw-Hill", 2019],
  ["Clean Code", "Robert C. Martin", "Software Engineering", "Prentice Hall", 2008],
  ["Design Patterns", "Erich Gamma", "Software Engineering", "Addison-Wesley", 1994],
  ["Introduction to Algorithms", "Thomas H. Cormen", "Computer Science", "MIT Press", 2022],
  ["Artificial Intelligence: A Modern Approach", "Stuart Russell", "Artificial Intelligence", "Pearson", 2021],
  ["Machine Learning", "Tom M. Mitchell", "Artificial Intelligence", "McGraw-Hill", 2017],
  ["Deep Learning", "Ian Goodfellow", "Artificial Intelligence", "MIT Press", 2016],
  ["Computer Architecture", "David A. Patterson", "Computer Engineering", "Morgan Kaufmann", 2020],
  ["Digital Design", "Morris Mano", "Electronics", "Pearson", 2018],
  ["Microprocessors and Microcontrollers", "Krishna Kant", "Electronics", "PHI Learning", 2019],
  ["Signals and Systems", "Alan V. Oppenheim", "Electronics", "Pearson", 2016],
  ["Communication Systems", "Simon Haykin", "Electronics", "Wiley", 2014],
  ["Power Systems", "C. L. Wadhwa", "Electrical Engineering", "New Age", 2018],
  ["Electrical Machines", "I. J. Nagrath", "Electrical Engineering", "McGraw-Hill", 2017],
  ["Engineering Mechanics", "S. Timoshenko", "Mechanical Engineering", "McGraw-Hill", 2015],
  ["Thermodynamics", "Yunus Cengel", "Mechanical Engineering", "McGraw-Hill", 2019],
  ["Fluid Mechanics", "R. K. Rajput", "Mechanical Engineering", "S. Chand", 2018],
  ["Manufacturing Engineering", "Serope Kalpakjian", "Mechanical Engineering", "Pearson", 2016],
  ["Engineering Mathematics I", "B. S. Grewal", "Mathematics", "Khanna Publishers", 2020],
  ["Higher Engineering Mathematics", "B. V. Ramana", "Mathematics", "McGraw-Hill", 2018],
  ["Linear Algebra", "Gilbert Strang", "Mathematics", "Wellesley-Cambridge", 2016],
  ["Calculus", "James Stewart", "Mathematics", "Cengage", 2019],
  ["Discrete Mathematics", "Kenneth Rosen", "Mathematics", "McGraw-Hill", 2019],
  ["Engineering Physics", "R. K. Gaur", "Physics", "Dhanpat Rai", 2017],
  ["Engineering Chemistry", "Jain and Jain", "Chemistry", "Dhanpat Rai", 2018],
  ["Organic Chemistry", "Morrison and Boyd", "Chemistry", "Pearson", 2016],
  ["University Physics", "Hugh Young", "Physics", "Pearson", 2020],
  ["Physical Chemistry", "Puri Sharma Pathania", "Chemistry", "Vishal", 2017],
  ["Principles of Marketing", "Philip Kotler", "Business", "Pearson", 2022],
  ["Financial Accounting", "T. S. Grewal", "Commerce", "Sultan Chand", 2020],
  ["Corporate Finance", "Stephen Ross", "Finance", "McGraw-Hill", 2021],
  ["Human Resource Management", "Gary Dessler", "Management", "Pearson", 2020],
  ["Operations Management", "Jay Heizer", "Management", "Pearson", 2019],
  ["Business Statistics", "S. P. Gupta", "Statistics", "Sultan Chand", 2018],
  ["Managerial Economics", "Dominick Salvatore", "Economics", "Oxford", 2017],
  ["Entrepreneurship Development", "S. S. Khanka", "Entrepreneurship", "S. Chand", 2019],
  ["Business Communication", "Meenakshi Raman", "Communication", "Oxford", 2018],
  ["Organizational Behaviour", "Stephen Robbins", "Management", "Pearson", 2021],
  ["Gray's Anatomy for Students", "Richard Drake", "Medicine", "Elsevier", 2020],
  ["Medical Physiology", "Guyton and Hall", "Medicine", "Elsevier", 2021],
  ["Robbins Pathology", "Vinay Kumar", "Medicine", "Elsevier", 2020],
  ["Pharmacology", "K. D. Tripathi", "Pharmacy", "Jaypee", 2019],
  ["Community Medicine", "K. Park", "Public Health", "Banarsidas", 2021],
  ["Biochemistry", "U. Satyanarayana", "Biotechnology", "Elsevier", 2019],
  ["Molecular Biology", "Robert Weaver", "Biotechnology", "McGraw-Hill", 2018],
  ["Microbiology", "Prescott", "Biology", "McGraw-Hill", 2020],
  ["Human Anatomy", "B. D. Chaurasia", "Medicine", "CBS", 2018],
  ["Dental Materials", "Anusavice", "Dentistry", "Elsevier", 2017]
] as const;

initialCatalog.forEach(([title, author, category, publisher, year], idx) => {
  const id = bookIdSeq++;
  const copies = 3 + (idx % 6);
  books.set(id, {
    id,
    title,
    author,
    category,
    publisher,
    publishedYear: year,
    isbn: `97800000${(idx + 1000).toString().padStart(4, '0')}`,
    totalCopies: copies,
    availableCopies: copies
  });
});

// Seed initial borrow records, fines, and reservations
const sampleRecord1: BorrowRecord = {
  id: borrowIdSeq++,
  userId: memberUser.id,
  bookId: 1, // Operating System Concepts
  borrowedAt: new Date(Date.now() - 5 * 24 * 60 * 60 * 1000).toISOString(),
  dueDate: new Date(Date.now() + 9 * 24 * 60 * 60 * 1000).toISOString(),
  returnedAt: null,
  isReturned: false
};
borrowRecords.set(sampleRecord1.id, sampleRecord1);
const b1 = books.get(1);
if (b1) b1.availableCopies = Math.max(0, b1.availableCopies - 1);

// An overdue active borrow record (due 6 days ago)
const sampleRecord2: BorrowRecord = {
  id: borrowIdSeq++,
  userId: memberUser.id,
  bookId: 4, // Clean Code
  borrowedAt: new Date(Date.now() - 20 * 24 * 60 * 60 * 1000).toISOString(),
  dueDate: new Date(Date.now() - 6 * 24 * 60 * 60 * 1000).toISOString(),
  returnedAt: null,
  isReturned: false
};
borrowRecords.set(sampleRecord2.id, sampleRecord2);
const b4 = books.get(4);
if (b4) b4.availableCopies = Math.max(0, b4.availableCopies - 1);

// A returned late record that generated an unpaid fine
const sampleRecord3: BorrowRecord = {
  id: borrowIdSeq++,
  userId: memberUser.id,
  bookId: 5, // Design Patterns
  borrowedAt: new Date(Date.now() - 24 * 24 * 60 * 60 * 1000).toISOString(),
  dueDate: new Date(Date.now() - 10 * 24 * 60 * 60 * 1000).toISOString(),
  returnedAt: new Date(Date.now() - 3 * 24 * 60 * 60 * 1000).toISOString(),
  isReturned: true
};
borrowRecords.set(sampleRecord3.id, sampleRecord3);

const sampleFine: Fine = {
  id: fineIdSeq++,
  userId: memberUser.id,
  borrowRecordId: sampleRecord3.id,
  amount: 70, // 7 days late * ₹10
  isPaid: false,
  issuedAt: sampleRecord3.returnedAt!,
  paidAt: null
};
fines.set(sampleFine.id, sampleFine);

// A sample active reservation
const sampleRes: Reservation = {
  id: reservationIdSeq++,
  userId: memberUser.id,
  bookId: 6, // Introduction to Algorithms
  reservedAt: new Date(Date.now() - 1 * 24 * 60 * 60 * 1000).toISOString(),
  expiresAt: new Date(Date.now() + 2 * 24 * 60 * 60 * 1000).toISOString(),
  isActive: true
};
reservations.set(sampleRes.id, sampleRes);

logActivity('Catalog Initialized', '50 university textbooks indexed and circulation desk opened');
logActivity('Book Issued', 'Issued "Operating System Concepts" to Student Member', memberUser.id);
logActivity('Fine Generated', 'Fine of ₹70 issued to Student Member for late return of "Design Patterns"', memberUser.id);
logActivity('Book Reserved', 'Student Member placed reservation on "Introduction to Algorithms"', memberUser.id);


// DTO formatting helper
function formatBook(b: Book) {
  return {
    id: b.id,
    title: b.title,
    author: b.author,
    isbn: b.isbn,
    category: b.category,
    genre: b.category,
    publisher: b.publisher,
    publishedYear: b.publishedYear,
    published_year: b.publishedYear,
    totalCopies: b.totalCopies,
    total_copies: b.totalCopies,
    availableCopies: b.availableCopies,
    available_copies: b.availableCopies
  };
}

function findUserByIdOrEmail(identifier: string): User | undefined {
  for (const u of users.values()) {
    if (u.id === identifier || u.email.toLowerCase() === identifier.toLowerCase()) {
      return u;
    }
  }
  return undefined;
}

function formatBorrowRecord(r: BorrowRecord) {
  const user = findUserByIdOrEmail(r.userId);
  const book = books.get(r.bookId);
  const isOverdue = !r.isReturned && new Date(r.dueDate) < new Date();

  return {
    id: r.id,
    userId: r.userId,
    user_id: r.userId,
    userFullName: user?.fullName || r.userId,
    user_name: user?.fullName || r.userId,
    bookId: r.bookId,
    book_id: r.bookId,
    bookTitle: book?.title || `Book #${r.bookId}`,
    book_title: book?.title || `Book #${r.bookId}`,
    bookAuthor: book?.author || '',
    book_author: book?.author || '',
    isbn: book?.isbn || '',
    book_isbn: book?.isbn || '',
    borrowedAt: r.borrowedAt,
    borrowed_at: r.borrowedAt,
    dueDate: r.dueDate,
    due_date: r.dueDate,
    returnedAt: r.returnedAt,
    returned_at: r.returnedAt,
    isReturned: r.isReturned,
    is_returned: r.isReturned,
    isOverdue,
    is_overdue: isOverdue
  };
}

function formatReservation(res: Reservation) {
  const user = findUserByIdOrEmail(res.userId);
  const book = books.get(res.bookId);
  return {
    id: res.id,
    userId: res.userId,
    userFullName: user?.fullName || res.userId,
    bookId: res.bookId,
    bookTitle: book?.title || `Book #${res.bookId}`,
    reservedAt: res.reservedAt,
    expiresAt: res.expiresAt,
    isActive: res.isActive
  };
}

function formatFine(f: Fine) {
  const user = findUserByIdOrEmail(f.userId);
  const record = borrowRecords.get(f.borrowRecordId);
  const book = record ? books.get(record.bookId) : undefined;
  return {
    id: f.id,
    userId: f.userId,
    userFullName: user?.fullName || f.userId,
    borrowRecordId: f.borrowRecordId,
    bookTitle: book?.title || '',
    amount: f.amount,
    isPaid: f.isPaid,
    issuedAt: f.issuedAt,
    paidAt: f.paidAt
  };
}

// Authentication Middleware
interface AuthRequest extends Request {
  user?: {
    id: string;
    email: string;
    fullName: string;
    role: string;
  };
}

function authenticateToken(req: AuthRequest, res: Response, next: NextFunction) {
  const authHeader = req.headers['authorization'];
  const token = authHeader && authHeader.split(' ')[1];
  if (!token) return next();

  jwt.verify(token, JWT_SECRET, (err, decoded: any) => {
    if (!err && decoded) {
      req.user = {
        id: decoded.userId || decoded.id || decoded.nameid,
        email: decoded.email,
        fullName: decoded.fullName || decoded.name,
        role: decoded.role
      };
    }
    next();
  });
}

function requireAuth(req: AuthRequest, res: Response, next: NextFunction) {
  if (!req.user) {
    return res.status(401).json({ error: 'Unauthorized. Authentication token required.' });
  }
  next();
}

app.use(authenticateToken);

// ==================== HEALTH ====================
app.get('/health', (req, res) => {
  res.json({
    status: 'healthy',
    service: 'Library Management System API',
    timestamp: new Date().toISOString()
  });
});

// ==================== AUTH ====================
const handleRegister = async (req: Request, res: Response) => {
  const fullName = req.body.fullName || req.body.full_name || req.body.name;
  const email = req.body.email;
  const password = req.body.password;
  const role = req.body.role === 'Admin' ? 'Admin' : (req.body.role === 'Assistant' ? 'Assistant' : 'Member');

  if (!email || !password || !fullName) {
    return res.status(400).json({ message: 'Full name, email, and password are required.' });
  }

  if (users.has(email.toLowerCase())) {
    return res.status(400).json({ message: 'User with this email already exists.' });
  }

  const user: User = {
    id: `usr-${Date.now()}`,
    fullName,
    email: email.toLowerCase(),
    passwordHash: bcrypt.hashSync(password, 10),
    role,
    membershipId: `LIB${Date.now().toString().slice(-6)}`,
    createdAt: new Date().toISOString()
  };

  users.set(user.email, user);
  logActivity('User Registered', `New ${role} registered: ${email}`, user.id);

  const token = jwt.sign(
    { userId: user.id, email: user.email, fullName: user.fullName, role: user.role },
    JWT_SECRET,
    { expiresIn: '24h' }
  );

  const expiresAt = new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString();
  return res.json({
    token,
    access_token: token,
    email: user.email,
    fullName: user.fullName,
    name: user.fullName,
    role: user.role,
    userId: user.id,
    user_id: user.id,
    expiresAt,
    user: {
      id: user.id,
      email: user.email,
      fullName: user.fullName,
      full_name: user.fullName,
      role: user.role,
      membershipId: user.membershipId,
      membership_id: user.membershipId
    }
  });
};

const handleLogin = async (req: Request, res: Response) => {
  const email = req.body.email?.trim()?.toLowerCase();
  const password = req.body.password;

  if (!email || !password) {
    return res.status(400).json({ error: 'Email and password are required', message: 'Email and password are required.' });
  }

  const user = users.get(email);
  if (!user || !bcrypt.compareSync(password, user.passwordHash)) {
    return res.status(401).json({ error: 'Invalid credentials', message: 'Invalid credentials.' });
  }

  const token = jwt.sign(
    { userId: user.id, email: user.email, fullName: user.fullName, role: user.role },
    JWT_SECRET,
    { expiresIn: '24h' }
  );

  const expiresAt = new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString();
  logActivity('User Login', `${user.fullName} (${user.role}) logged in`, user.id);

  return res.json({
    token,
    access_token: token,
    email: user.email,
    fullName: user.fullName,
    name: user.fullName,
    role: user.role,
    userId: user.id,
    user_id: user.id,
    expiresAt,
    user: {
      id: user.id,
      email: user.email,
      fullName: user.fullName,
      full_name: user.fullName,
      name: user.fullName,
      role: user.role,
      membershipId: user.membershipId,
      membership_id: user.membershipId
    }
  });
};

const handleMe = (req: AuthRequest, res: Response) => {
  if (!req.user) {
    return res.status(401).json({ error: 'Unauthorized' });
  }
  const user = findUserByIdOrEmail(req.user.id) || findUserByIdOrEmail(req.user.email);
  if (!user) {
    return res.status(404).json({ error: 'User not found' });
  }
  return res.json({
    id: user.id,
    email: user.email,
    fullName: user.fullName,
    full_name: user.fullName,
    name: user.fullName,
    role: user.role,
    membershipId: user.membershipId,
    membership_id: user.membershipId
  });
};

app.post(['/api/auth/register', '/api/Auth/register'], handleRegister);
app.post(['/api/auth/login', '/api/Auth/login'], handleLogin);
app.get(['/api/auth/me', '/api/Auth/me'], handleMe);

// ==================== MEMBERS & USERS ====================
app.get(['/api/members', '/api/Members'], (req: AuthRequest, res: Response) => {
  const list = Array.from(users.values()).map(u => ({
    id: u.id,
    fullName: u.fullName,
    full_name: u.fullName,
    email: u.email,
    role: u.role,
    membershipId: u.membershipId,
    membership_id: u.membershipId,
    createdAt: u.createdAt
  }));
  res.json(list);
});

// ==================== LIBRARY STATS ====================
app.get(['/api/stats', '/api/Stats'], (req: Request, res: Response) => {
  const allBooks = Array.from(books.values());
  const allBorrows = Array.from(borrowRecords.values());
  const allReservations = Array.from(reservations.values());
  const allFines = Array.from(fines.values());

  const totalTitles = allBooks.length;
  const totalCopies = allBooks.reduce((sum, b) => sum + b.totalCopies, 0);
  const availableCopies = allBooks.reduce((sum, b) => sum + b.availableCopies, 0);
  const activeLoans = allBorrows.filter(r => !r.isReturned).length;
  const now = new Date();
  const overdueLoans = allBorrows.filter(r => !r.isReturned && new Date(r.dueDate) < now).length;
  const activeReservations = allReservations.filter(r => r.isActive).length;
  const unpaidFines = allFines.filter(f => !f.isPaid);
  const unpaidFinesCount = unpaidFines.length;
  const unpaidFinesTotal = unpaidFines.reduce((sum, f) => sum + f.amount, 0);

  res.json({
    totalTitles,
    totalCopies,
    availableCopies,
    activeLoans,
    overdueLoans,
    activeReservations,
    unpaidFinesCount,
    unpaidFinesTotal
  });
});

// ==================== BOOKS ====================
function getParam(val: unknown): string {
  if (Array.isArray(val)) return String(val[0] || '');
  return String(val ?? '');
}

const handleGetBooks = (req: Request, res: Response) => {
  const search = ((req.query.search || req.query.q || '') as string).toLowerCase().trim();
  const category = ((req.query.category || req.query.genre || '') as string).trim();

  let list = Array.from(books.values());
  if (search) {
    list = list.filter(b =>
      b.title.toLowerCase().includes(search) ||
      b.author.toLowerCase().includes(search) ||
      b.isbn.toLowerCase().includes(search) ||
      b.category.toLowerCase().includes(search)
    );
  }
  if (category) {
    list = list.filter(b => b.category.toLowerCase() === category.toLowerCase());
  }

  res.json(list.map(formatBook));
};

const handleGetBookById = (req: Request, res: Response) => {
  const id = parseInt(getParam(req.params.id), 10);
  const book = books.get(id);
  if (!book) return res.status(404).json({ message: 'Book not found' });
  res.json(formatBook(book));
};

const handleCreateBook = (req: AuthRequest, res: Response) => {
  const { title, author, category, publisher, publishedYear, totalCopies, isbn } = req.body;
  if (!title || !author) {
    return res.status(400).json({ message: 'Title and author are required.' });
  }

  const id = bookIdSeq++;
  const copies = parseInt(totalCopies, 10) || 1;
  const newBook: Book = {
    id,
    title,
    author,
    category: category || 'General',
    publisher: publisher || 'Self-Published',
    publishedYear: parseInt(publishedYear, 10) || new Date().getFullYear(),
    isbn: isbn || `97800000${id.toString().padStart(4, '0')}`,
    totalCopies: copies,
    availableCopies: copies
  };

  books.set(id, newBook);
  logActivity('Book Created', `Added "${title}" (ISBN: ${newBook.isbn})`, req.user?.id);
  res.status(201).json(formatBook(newBook));
};

const handleUpdateBook = (req: AuthRequest, res: Response) => {
  const id = parseInt(getParam(req.params.id), 10);
  const book = books.get(id);
  if (!book) return res.status(404).json({ message: 'Book not found' });

  const { title, author, category, publisher, publishedYear, totalCopies } = req.body;
  if (title !== undefined) book.title = title;
  if (author !== undefined) book.author = author;
  if (category !== undefined) book.category = category;
  if (publisher !== undefined) book.publisher = publisher;
  if (publishedYear !== undefined) book.publishedYear = parseInt(publishedYear, 10);

  if (totalCopies !== undefined) {
    const newTotal = parseInt(totalCopies, 10);
    const diff = newTotal - book.totalCopies;
    book.totalCopies = newTotal;
    book.availableCopies = Math.max(0, book.availableCopies + diff);
  }

  books.set(id, book);
  logActivity('Book Updated', `Updated "${book.title}" (ID: ${id})`, req.user?.id);
  res.json(formatBook(book));
};

const handleDeleteBook = (req: AuthRequest, res: Response) => {
  const id = parseInt(getParam(req.params.id), 10);
  if (!books.has(id)) return res.status(404).json({ message: 'Book not found' });
  const title = books.get(id)?.title;
  books.delete(id);
  logActivity('Book Deleted', `Deleted "${title}" (ID: ${id})`, req.user?.id);
  res.status(204).send();
};

app.get(['/api/books', '/api/Books'], handleGetBooks);
app.get(['/api/books/:id', '/api/Books/:id'], handleGetBookById);
app.post(['/api/books', '/api/Books'], handleCreateBook);
app.put(['/api/books/:id', '/api/Books/:id'], handleUpdateBook);
app.delete(['/api/books/:id', '/api/Books/:id'], handleDeleteBook);

// ==================== BORROW & RETURN ====================
const handleGetAllBorrows = (req: AuthRequest, res: Response) => {
  // If user is regular Member, filter to user's borrows
  let list = Array.from(borrowRecords.values());
  if (req.user && req.user.role === 'Member') {
    list = list.filter(r => r.userId === req.user?.id || r.userId === req.user?.email);
  }
  res.json(list.map(formatBorrowRecord));
};

const handleGetBorrowsByUser = (req: Request, res: Response) => {
  const userId = getParam(req.params.userId);
  const list = Array.from(borrowRecords.values()).filter(
    r => r.userId === userId || r.userId.toLowerCase() === userId.toLowerCase()
  );
  res.json(list.map(formatBorrowRecord));
};

const handleGetOverdue = (req: Request, res: Response) => {
  const now = new Date();
  const overdue = Array.from(borrowRecords.values()).filter(
    r => !r.isReturned && new Date(r.dueDate) < now
  );
  res.json(overdue.map(formatBorrowRecord));
};

const handleIssueBook = (req: AuthRequest, res: Response) => {
  const bookId = parseInt(req.body.bookId || req.body.book_id, 10);
  const userId = req.body.userId || req.body.user_id || req.user?.id || req.user?.email || 'usr-member-01';
  const dueDays = parseInt(req.body.dueDays || req.body.days, 10) || 14;

  if (!bookId) return res.status(400).json({ error: 'Book ID is required', message: 'Book ID is required.' });

  const book = books.get(bookId);
  if (!book) return res.status(404).json({ error: 'Book not found', message: 'Book not found.' });

  if (book.availableCopies <= 0) {
    return res.status(400).json({ error: 'No copies available for this book.', message: 'No copies available for this book.' });
  }

  book.availableCopies--;
  books.set(bookId, book);

  const now = new Date();
  const dueDate = new Date(now.getTime() + dueDays * 24 * 60 * 60 * 1000);

  const record: BorrowRecord = {
    id: borrowIdSeq++,
    userId,
    bookId,
    borrowedAt: now.toISOString(),
    dueDate: dueDate.toISOString(),
    returnedAt: null,
    isReturned: false
  };

  borrowRecords.set(record.id, record);
  logActivity('Book Issued', `Issued "${book.title}" to ${userId}`, req.user?.id);
  res.status(201).json(formatBorrowRecord(record));
};

const handleReturnBook = (req: AuthRequest, res: Response) => {
  const recordId = parseInt(getParam(req.params.id) || req.body.borrowRecordId || req.body.borrow_record_id, 10);
  if (!recordId) return res.status(400).json({ error: 'Borrow record ID is required' });

  const record = borrowRecords.get(recordId);
  if (!record) return res.status(404).json({ error: 'Borrow record not found' });
  if (record.isReturned) return res.status(400).json({ error: 'Book already returned.' });

  const returnedAt = new Date();
  record.isReturned = true;
  record.returnedAt = returnedAt.toISOString();

  let fineAmount = 0;
  const dueDate = new Date(record.dueDate);
  if (returnedAt > dueDate) {
    const diffDays = Math.ceil((returnedAt.getTime() - dueDate.getTime()) / (1000 * 60 * 60 * 24));
    fineAmount = diffDays * 10;
    const fine: Fine = {
      id: fineIdSeq++,
      userId: record.userId,
      borrowRecordId: record.id,
      amount: fineAmount,
      isPaid: false,
      issuedAt: returnedAt.toISOString(),
      paidAt: null
    };
    fines.set(fine.id, fine);
    logActivity('Fine Generated', `Fine of ₹${fineAmount} issued to ${record.userId} for late return`, req.user?.id);
  }

  const book = books.get(record.bookId);
  if (book) {
    book.availableCopies = Math.min(book.totalCopies, book.availableCopies + 1);
    books.set(book.id, book);
  }

  borrowRecords.set(record.id, record);
  logActivity('Book Returned', `Book #${record.bookId} returned by ${record.userId}`, req.user?.id);

  res.json({
    message: 'Book returned successfully',
    fine: fineAmount,
    record: formatBorrowRecord(record)
  });
};

app.get(['/api/borrow', '/api/Borrow'], handleGetAllBorrows);
app.get(['/api/borrow/user/:userId', '/api/Borrow/user/:userId'], handleGetBorrowsByUser);
app.get(['/api/borrow/overdue', '/api/Borrow/overdue'], handleGetOverdue);
app.post(['/api/borrow/issue', '/api/Borrow/issue', '/api/borrow', '/api/Borrow'], handleIssueBook);
app.post(['/api/borrow/return', '/api/Borrow/return'], handleReturnBook);
app.put(['/api/borrow/:id/return', '/api/Borrow/:id/return'], handleReturnBook);

// ==================== RESERVATIONS ====================
app.get(['/api/reservations', '/api/Reservations'], (req, res) => {
  res.json(Array.from(reservations.values()).map(formatReservation));
});

app.get(['/api/reservations/user/:userId', '/api/Reservations/user/:userId'], (req, res) => {
  const userId = getParam(req.params.userId);
  const list = Array.from(reservations.values()).filter(
    r => r.userId === userId || r.userId.toLowerCase() === userId.toLowerCase()
  );
  res.json(list.map(formatReservation));
});

app.post(['/api/reservations', '/api/Reservations'], (req: AuthRequest, res) => {
  const bookId = parseInt(req.body.bookId || req.body.book_id, 10);
  const userId = req.body.userId || req.body.user_id || req.user?.id || req.user?.email || 'usr-member-01';

  if (!bookId) return res.status(400).json({ message: 'Book ID is required.' });

  const book = books.get(bookId);
  if (!book) return res.status(404).json({ message: 'Book not found.' });

  const reservation: Reservation = {
    id: reservationIdSeq++,
    userId,
    bookId,
    reservedAt: new Date().toISOString(),
    expiresAt: new Date(Date.now() + 3 * 24 * 60 * 60 * 1000).toISOString(),
    isActive: true
  };

  reservations.set(reservation.id, reservation);
  logActivity('Book Reserved', `Reserved "${book.title}" for ${userId}`, req.user?.id);
  res.status(201).json(formatReservation(reservation));
});

app.delete(['/api/reservations/:id', '/api/Reservations/:id'], (req: AuthRequest, res) => {
  const id = parseInt(getParam(req.params.id), 10);
  const r = reservations.get(id);
  if (!r) return res.status(404).json({ message: 'Reservation not found' });
  r.isActive = false;
  reservations.set(id, r);
  logActivity('Reservation Cancelled', `Cancelled reservation #${id}`, req.user?.id);
  res.status(204).send();
});

app.post(['/api/reservations/:id/fulfill', '/api/Reservations/:id/fulfill'], (req: AuthRequest, res) => {
  const id = parseInt(getParam(req.params.id), 10);
  const r = reservations.get(id);
  if (!r) return res.status(404).json({ message: 'Reservation not found' });
  if (!r.isActive) return res.status(400).json({ message: 'Reservation is not active' });

  const book = books.get(r.bookId);
  if (!book) return res.status(404).json({ message: 'Book not found' });
  if (book.availableCopies <= 0) {
    return res.status(400).json({ message: 'No copies available to issue' });
  }

  book.availableCopies--;
  books.set(book.id, book);
  r.isActive = false;
  reservations.set(r.id, r);

  const now = new Date();
  const record: BorrowRecord = {
    id: borrowIdSeq++,
    userId: r.userId,
    bookId: r.bookId,
    borrowedAt: now.toISOString(),
    dueDate: new Date(now.getTime() + 14 * 24 * 60 * 60 * 1000).toISOString(),
    returnedAt: null,
    isReturned: false
  };

  borrowRecords.set(record.id, record);
  logActivity('Reservation Fulfilled', `Issued reserved book "${book.title}" to ${r.userId}`, req.user?.id);
  res.json({
    message: 'Reservation fulfilled into active loan',
    record: formatBorrowRecord(record)
  });
});

// ==================== FINES ====================
app.get(['/api/fines', '/api/Fines'], (req, res) => {
  res.json(Array.from(fines.values()).map(formatFine));
});

app.get(['/api/fines/user/:userId', '/api/Fines/user/:userId'], (req, res) => {
  const userId = getParam(req.params.userId);
  const list = Array.from(fines.values()).filter(
    f => f.userId === userId || f.userId.toLowerCase() === userId.toLowerCase()
  );
  res.json(list.map(formatFine));
});

app.post(['/api/fines/pay', '/api/Fines/pay'], (req: AuthRequest, res) => {
  const fineId = parseInt(req.body.fineId || req.body.fine_id, 10);
  const fine = fines.get(fineId);
  if (!fine) return res.status(404).json({ message: 'Fine not found' });
  if (fine.isPaid) return res.status(400).json({ message: 'Fine already paid.' });

  fine.isPaid = true;
  fine.paidAt = new Date().toISOString();
  fines.set(fineId, fine);

  logActivity('Fine Paid', `Fine #${fineId} (₹${fine.amount}) paid by ${fine.userId}`, req.user?.id);
  res.json(formatFine(fine));
});

// ==================== ACTIVITY LOGS ====================
app.get(['/api/activitylog', '/api/ActivityLog'], (req, res) => {
  const page = parseInt(req.query.page as string, 10) || 1;
  const pageSize = parseInt(req.query.pageSize as string, 10) || 50;
  const start = (page - 1) * pageSize;
  const logs = activityLogs.slice(start, start + pageSize);

  res.json({
    total: activityLogs.length,
    page,
    pageSize,
    logs
  });
});

app.get(['/api/activitylog/user/:userId', '/api/ActivityLog/user/:userId'], (req, res) => {
  const userId = getParam(req.params.userId);
  const userLogs = activityLogs.filter(l => l.userId === userId || l.userId?.toLowerCase() === userId.toLowerCase());
  res.json(userLogs);
});

// ==================== OPEN LIBRARY API PROXY ====================
const curatedGlobalBooks = [
  {
    title: "Designing Data-Intensive Applications",
    authors: ["Martin Kleppmann"],
    firstPublishYear: 2017,
    isbn: "9781449373320",
    subject: "Distributed Systems & Databases",
    coverUrl: "https://covers.openlibrary.org/b/id/8314146-M.jpg"
  },
  {
    title: "Clean Architecture: A Craftsman's Guide",
    authors: ["Robert C. Martin"],
    firstPublishYear: 2017,
    isbn: "9780134494166",
    subject: "Software Architecture",
    coverUrl: "https://covers.openlibrary.org/b/id/8231856-M.jpg"
  },
  {
    title: "The Pragmatic Programmer: 20th Anniversary Edition",
    authors: ["David Thomas", "Andrew Hunt"],
    firstPublishYear: 2019,
    isbn: "9780135957059",
    subject: "Software Engineering",
    coverUrl: "https://covers.openlibrary.org/b/id/9255566-M.jpg"
  },
  {
    title: "Structure and Interpretation of Computer Programs",
    authors: ["Harold Abelson", "Gerald Jay Sussman"],
    firstPublishYear: 1996,
    isbn: "9780262510875",
    subject: "Computer Science",
    coverUrl: "https://covers.openlibrary.org/b/id/11149426-M.jpg"
  },
  {
    title: "Quantum Computation and Quantum Information",
    authors: ["Michael A. Nielsen", "Isaac L. Chuang"],
    firstPublishYear: 2010,
    isbn: "9781107002173",
    subject: "Quantum Physics & Computing",
    coverUrl: "https://covers.openlibrary.org/b/id/6979861-M.jpg"
  },
  {
    title: "Introduction to the Theory of Computation",
    authors: ["Michael Sipser"],
    firstPublishYear: 2012,
    isbn: "9781133187790",
    subject: "Theoretical Computer Science",
    coverUrl: "https://covers.openlibrary.org/b/id/8431871-M.jpg"
  },
  {
    title: "Deep Work: Rules for Focused Success",
    authors: ["Cal Newport"],
    firstPublishYear: 2016,
    isbn: "9781455586691",
    subject: "Productivity & Research",
    coverUrl: "https://covers.openlibrary.org/b/id/8091016-M.jpg"
  },
  {
    title: "Site Reliability Engineering",
    authors: ["Betsy Beyer", "Chris Jones", "Niall Richard Murphy"],
    firstPublishYear: 2016,
    isbn: "9781491929124",
    subject: "DevOps & Cloud Systems",
    coverUrl: "https://covers.openlibrary.org/b/id/8372611-M.jpg"
  }
];

app.get(['/api/openlibrary/search', '/api/OpenLibrary/search'], async (req, res) => {
  const query = ((req.query.query || req.query.q || '') as string).trim();
  const limit = parseInt(req.query.limit as string, 10) || 10;
  if (!query) return res.status(400).json({ message: 'Query is required.' });

  try {
    const url = `https://openlibrary.org/search.json?q=${encodeURIComponent(query)}&limit=${limit}&fields=title,author_name,first_publish_year,isbn,subject,cover_i`;
    const resp = await fetch(url, {
      headers: { 'User-Agent': 'LibraryManagementSystem/1.0' },
      signal: AbortSignal.timeout(4000)
    });
    if (!resp.ok) throw new Error(`OpenLibrary returned ${resp.status}`);
    const data: any = await resp.json();

    const results = (data.docs || []).map((item: any) => ({
      title: item.title || '',
      authors: item.author_name || [],
      firstPublishYear: item.first_publish_year || null,
      isbn: (item.isbn && item.isbn[0]) || null,
      subject: (item.subject && item.subject[0]) || 'General',
      coverUrl: item.cover_i ? `https://covers.openlibrary.org/b/id/${item.cover_i}-M.jpg` : null
    }));

    if (results.length > 0) {
      return res.json(results);
    }
  } catch (error) {
    // Fall back to curated global catalog if external API times out or fails
  }

  // Filter curated collection by query terms
  const qLower = query.toLowerCase();
  const matched = curatedGlobalBooks.filter(b =>
    b.title.toLowerCase().includes(qLower) ||
    b.authors.some(a => a.toLowerCase().includes(qLower)) ||
    b.subject.toLowerCase().includes(qLower) ||
    b.isbn.includes(qLower)
  );

  res.json(matched.length > 0 ? matched : curatedGlobalBooks.slice(0, limit));
});

app.get(['/api/openlibrary/isbn/:isbn', '/api/OpenLibrary/isbn/:isbn'], async (req, res) => {
  const isbn = getParam(req.params.isbn);
  try {
    const url = `https://openlibrary.org/api/books?bibkeys=ISBN:${encodeURIComponent(isbn)}&format=json&jscmd=data`;
    const resp = await fetch(url, { headers: { 'User-Agent': 'LibraryManagementSystem/1.0' } });
    const data: any = await resp.json();
    const bookData = data[`ISBN:${isbn}`];
    if (!bookData) return res.status(404).json({ message: `No book found for ISBN ${isbn}` });

    res.json({
      title: bookData.title || '',
      authors: (bookData.authors || []).map((a: any) => a.name),
      firstPublishYear: null,
      coverUrl: bookData.cover?.medium || null,
      isbn,
      subject: (bookData.subjects && bookData.subjects[0]?.name) || null
    });
  } catch (error) {
    res.status(500).json({ message: 'Error querying OpenLibrary' });
  }
});

app.post(['/api/openlibrary/import', '/api/OpenLibrary/import'], (req: AuthRequest, res: Response) => {
  const { title, author, category, publisher, publishedYear, totalCopies, isbn } = req.body;
  if (!title) return res.status(400).json({ message: 'Title is required' });

  if (isbn) {
    for (const b of books.values()) {
      if (b.isbn === isbn) {
        return res.status(400).json({ message: `Book with ISBN ${isbn} is already in the catalog (ID: #${b.id})` });
      }
    }
  }

  const id = bookIdSeq++;
  const copies = parseInt(totalCopies, 10) || 3;
  const newBook: Book = {
    id,
    title,
    author: author || 'Unknown Author',
    category: category || 'General Collection',
    publisher: publisher || 'Open Library Press',
    publishedYear: parseInt(publishedYear, 10) || new Date().getFullYear(),
    isbn: isbn || `97800000${id.toString().padStart(4, '0')}`,
    totalCopies: copies,
    availableCopies: copies
  };

  books.set(id, newBook);
  logActivity('Book Imported', `Imported "${title}" from Open Library (ISBN: ${newBook.isbn})`, req.user?.id);
  res.status(201).json(formatBook(newBook));
});

// ==================== STATIC FILES & FALLBACK ====================
// Serve static assets from wwwroot and static directories
app.use(express.static(path.join(__dirname, 'wwwroot')));
app.use('/static', express.static(path.join(__dirname, 'static')));

// Portal view routes (staff & member dashboard)
app.get(['/portal', '/classic', '/admin'], (req, res) => {
  res.sendFile(path.join(__dirname, 'static', 'index.html'));
});

// Default root view
app.get('/', (req, res) => {
  res.sendFile(path.join(__dirname, 'wwwroot', 'index.html'));
});

// Catch-all fallback
app.get('*', (req, res) => {
  if (req.path.startsWith('/api')) {
    return res.status(404).json({ error: 'Endpoint not found' });
  }
  res.sendFile(path.join(__dirname, 'wwwroot', 'index.html'));
});

app.listen(Number(PORT), '0.0.0.0', () => {
  console.log(`Library Management System server running on http://0.0.0.0:${PORT}`);
});
