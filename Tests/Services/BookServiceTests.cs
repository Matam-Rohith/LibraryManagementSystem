using FluentAssertions;
using LibraryManagementSystem.DTOs;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Repositories.Interfaces;
using LibraryManagementSystem.Services;
using Moq;

namespace LibraryManagementSystem.Tests.Services;

public class BookServiceTests
{
    private readonly Mock<IBookRepository> _bookRepoMock;
    private readonly BookService _bookService;

    public BookServiceTests()
    {
        _bookRepoMock = new Mock<IBookRepository>();
        _bookService = new BookService(_bookRepoMock.Object);
    }

    [Fact]
    public async Task GetAllBooksAsync_ShouldReturnAllBooks()
    {
        // Arrange
        var books = new List<Book>
        {
            new Book { Id = 1, Title = "Clean Code", Author = "Robert Martin", ISBN = "978-0132350884", AvailableCopies = 3, TotalCopies = 3, Genre = "Programming" },
            new Book { Id = 2, Title = "The Pragmatic Programmer", Author = "David Thomas", ISBN = "978-0201616224", AvailableCopies = 2, TotalCopies = 2, Genre = "Programming" }
        };
        _bookRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(books);

        // Act
        var result = await _bookService.GetAllBooksAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(b => b.Title == "Clean Code");
    }

    [Fact]
    public async Task GetBookByIdAsync_WhenExists_ShouldReturnBook()
    {
        // Arrange
        var book = new Book { Id = 1, Title = "Clean Code", Author = "Robert Martin", ISBN = "978-0132350884", AvailableCopies = 3, TotalCopies = 3, Genre = "Programming" };
        _bookRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(book);

        // Act
        var result = await _bookService.GetBookByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Clean Code");
    }

    [Fact]
    public async Task GetBookByIdAsync_WhenNotExists_ShouldReturnNull()
    {
        // Arrange
        _bookRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Book?)null);

        // Act
        var result = await _bookService.GetBookByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddBookAsync_ShouldCallRepositoryAdd()
    {
        // Arrange
        var dto = new CreateBookDto
        {
            Title = "New Book",
            Author = "Test Author",
            ISBN = "978-1234567890",
            Genre = "Fiction",
            TotalCopies = 5
        };
        _bookRepoMock.Setup(r => r.AddAsync(It.IsAny<Book>())).Returns(Task.CompletedTask);

        // Act
        await _bookService.AddBookAsync(dto);

        // Assert
        _bookRepoMock.Verify(r => r.AddAsync(It.Is<Book>(b =>
            b.Title == dto.Title &&
            b.Author == dto.Author &&
            b.TotalCopies == dto.TotalCopies &&
            b.AvailableCopies == dto.TotalCopies)), Times.Once);
    }

    [Fact]
    public async Task DeleteBookAsync_WhenExists_ShouldDeleteBook()
    {
        // Arrange
        var book = new Book { Id = 1, Title = "To Delete", Author = "Author", ISBN = "111", AvailableCopies = 1, TotalCopies = 1, Genre = "Test" };
        _bookRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(book);
        _bookRepoMock.Setup(r => r.DeleteAsync(book)).Returns(Task.CompletedTask);

        // Act
        var deleted = await _bookService.DeleteBookAsync(1);

        // Assert
        deleted.Should().BeTrue();
        _bookRepoMock.Verify(r => r.DeleteAsync(book), Times.Once);
    }

    [Fact]
    public async Task DeleteBookAsync_WhenNotExists_ShouldReturnFalse()
    {
        // Arrange
        _bookRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Book?)null);

        // Act
        var deleted = await _bookService.DeleteBookAsync(999);

        // Assert
        deleted.Should().BeFalse();
    }
}
