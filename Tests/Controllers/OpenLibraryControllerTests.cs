using FluentAssertions;
using LibraryManagementSystem.Controllers;
using LibraryManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace LibraryManagementSystem.Tests.Controllers;

public class OpenLibraryControllerTests
{
    private readonly Mock<IOpenLibraryService> _openLibraryMock;
    private readonly OpenLibraryController _controller;

    public OpenLibraryControllerTests()
    {
        _openLibraryMock = new Mock<IOpenLibraryService>();
        _controller = new OpenLibraryController(_openLibraryMock.Object);
    }

    [Fact]
    public async Task Search_WithValidQuery_ShouldReturnOk()
    {
        // Arrange
        var books = new List<OpenLibraryBook>
        {
            new OpenLibraryBook("Clean Code", new List<string> { "Robert Martin" }, 2008, null, "978-0132350884", "Programming")
        };
        _openLibraryMock.Setup(s => s.SearchBooksAsync("clean code", 10)).ReturnsAsync(books);

        // Act
        var result = await _controller.Search("clean code", 10);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var ok = (OkObjectResult)result;
        var returned = ok.Value as List<OpenLibraryBook>;
        returned.Should().HaveCount(1);
    }

    [Fact]
    public async Task Search_WithEmptyQuery_ShouldReturnBadRequest()
    {
        // Act
        var result = await _controller.Search("", 10);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetByIsbn_WhenFound_ShouldReturnOk()
    {
        // Arrange
        var book = new OpenLibraryBook("Clean Code", new List<string> { "Robert Martin" }, 2008, null, "978-0132350884", null);
        _openLibraryMock.Setup(s => s.GetBookByIsbnAsync("978-0132350884")).ReturnsAsync(book);

        // Act
        var result = await _controller.GetByIsbn("978-0132350884");

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetByIsbn_WhenNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _openLibraryMock.Setup(s => s.GetBookByIsbnAsync("000-000")).ReturnsAsync((OpenLibraryBook?)null);

        // Act
        var result = await _controller.GetByIsbn("000-000");

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }
}
