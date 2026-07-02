using FluentAssertions;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;
using Microsoft.Extensions.Configuration;
using Moq;

namespace LibraryManagementSystem.Tests.Services;

public class ActivityLogServiceTests
{
    [Fact]
    public void ActivityLog_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var log = new ActivityLog
        {
            Action = "BOOK_BORROWED",
            UserId = "user-123",
            UserEmail = "user@test.com",
            Role = "Member",
            EntityType = "Book",
            EntityId = "42",
            Details = "Borrowed 'Clean Code'"
        };

        // Assert
        log.Action.Should().Be("BOOK_BORROWED");
        log.UserId.Should().Be("user-123");
        log.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void ActivityLog_DefaultTimestamp_ShouldBeUtcNow()
    {
        // Act
        var log = new ActivityLog();

        // Assert
        log.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        log.Timestamp.Kind.Should().Be(DateTimeKind.Utc);
    }
}
