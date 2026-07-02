using FluentAssertions;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Tests.Services;

public class ActivityLogServiceTests
{
    [Fact]
    public void ActivityLog_ShouldHaveCorrectProperties()
    {
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

        log.Action.Should().Be("BOOK_BORROWED");
        log.UserId.Should().Be("user-123");
        log.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void ActivityLog_DefaultTimestamp_ShouldBeUtcNow()
    {
        var log = new ActivityLog();

        log.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        log.Timestamp.Kind.Should().Be(DateTimeKind.Utc);
    }
}
