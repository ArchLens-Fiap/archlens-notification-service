using ArchLens.Contracts.Events;
using ArchLens.Notification.Infrastructure.Consumers;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace ArchLens.Notification.Tests.Consumers;

public class UserAccountDeletedConsumerTests
{
    private readonly ILogger<UserAccountDeletedConsumer> _logger;
    private readonly UserAccountDeletedConsumer _consumer;

    public UserAccountDeletedConsumerTests()
    {
        _logger = Substitute.For<ILogger<UserAccountDeletedConsumer>>();
        _consumer = new UserAccountDeletedConsumer(_logger);
    }

    [Fact]
    public async Task Consume_ShouldLogInformation()
    {
        // Arrange
        var message = new UserAccountDeletedEvent
        {
            UserId = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow
        };

        var context = Substitute.For<ConsumeContext<UserAccountDeletedEvent>>();
        context.Message.Returns(message);

        // Act
        await _consumer.Consume(context);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Consume_ShouldCompleteSuccessfully()
    {
        // Arrange
        var message = new UserAccountDeletedEvent
        {
            UserId = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow
        };

        var context = Substitute.For<ConsumeContext<UserAccountDeletedEvent>>();
        context.Message.Returns(message);

        // Act
        var act = () => _consumer.Consume(context);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Consume_WithEmptyUserId_ShouldStillComplete()
    {
        // Arrange
        var message = new UserAccountDeletedEvent
        {
            UserId = Guid.Empty,
            Timestamp = DateTime.UtcNow
        };

        var context = Substitute.For<ConsumeContext<UserAccountDeletedEvent>>();
        context.Message.Returns(message);

        // Act
        var act = () => _consumer.Consume(context);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Consume_ShouldReturnCompletedTask()
    {
        // Arrange
        var message = new UserAccountDeletedEvent
        {
            UserId = Guid.NewGuid(),
            Timestamp = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc)
        };

        var context = Substitute.For<ConsumeContext<UserAccountDeletedEvent>>();
        context.Message.Returns(message);

        // Act
        var task = _consumer.Consume(context);

        // Assert
        task.IsCompleted.Should().BeTrue();
    }
}
