using ArchLens.Contracts.Events;
using ArchLens.Notification.Infrastructure.Consumers;
using ArchLens.Notification.Infrastructure.Hubs;
using FluentAssertions;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace ArchLens.Notification.Tests.Consumers;

public class StatusChangedConsumerTests
{
    private readonly IHubContext<AnalysisHub> _hubContext;
    private readonly ILogger<StatusChangedConsumer> _logger;
    private readonly StatusChangedConsumer _consumer;
    private readonly IHubClients _hubClients;
    private readonly IClientProxy _clientProxy;

    public StatusChangedConsumerTests()
    {
        _hubContext = Substitute.For<IHubContext<AnalysisHub>>();
        _logger = Substitute.For<ILogger<StatusChangedConsumer>>();
        _hubClients = Substitute.For<IHubClients>();
        _clientProxy = Substitute.For<IClientProxy>();

        _hubContext.Clients.Returns(_hubClients);
        _hubClients.Group(Arg.Any<string>()).Returns(_clientProxy);

        _consumer = new StatusChangedConsumer(_hubContext, _logger);
    }

    [Fact]
    public async Task Consume_ShouldSendToAnalysisGroup()
    {
        // Arrange
        var analysisId = Guid.NewGuid();
        var message = new StatusChangedEvent
        {
            AnalysisId = analysisId,
            OldStatus = "Pending",
            NewStatus = "Processing",
            Timestamp = DateTime.UtcNow
        };

        var context = CreateConsumeContext(message);

        // Act
        await _consumer.Consume(context);

        // Assert
        _hubClients.Received(1).Group(analysisId.ToString());
    }

    [Fact]
    public async Task Consume_ShouldSendToDashboardGroup()
    {
        // Arrange
        var message = new StatusChangedEvent
        {
            AnalysisId = Guid.NewGuid(),
            OldStatus = "Pending",
            NewStatus = "Completed",
            Timestamp = DateTime.UtcNow
        };

        var context = CreateConsumeContext(message);

        // Act
        await _consumer.Consume(context);

        // Assert
        _hubClients.Received(1).Group("dashboard");
    }

    [Fact]
    public async Task Consume_ShouldSendStatusChangedEventToAnalysisGroup()
    {
        // Arrange
        var analysisId = Guid.NewGuid();
        var message = new StatusChangedEvent
        {
            AnalysisId = analysisId,
            OldStatus = "Pending",
            NewStatus = "Processing",
            Timestamp = DateTime.UtcNow
        };

        var context = CreateConsumeContext(message);

        // Act
        await _consumer.Consume(context);

        // Assert
        await _clientProxy.Received().SendCoreAsync(
            "StatusChanged",
            Arg.Any<object?[]>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_ShouldSendAnalysisStatusChangedEventToDashboard()
    {
        // Arrange
        var analysisId = Guid.NewGuid();
        var dashboardProxy = Substitute.For<IClientProxy>();
        _hubClients.Group("dashboard").Returns(dashboardProxy);

        var message = new StatusChangedEvent
        {
            AnalysisId = analysisId,
            OldStatus = "Processing",
            NewStatus = "Completed",
            Timestamp = DateTime.UtcNow
        };

        var context = CreateConsumeContext(message);

        // Act
        await _consumer.Consume(context);

        // Assert
        await dashboardProxy.Received().SendCoreAsync(
            "AnalysisStatusChanged",
            Arg.Any<object?[]>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_ShouldLogInformation()
    {
        // Arrange
        var message = new StatusChangedEvent
        {
            AnalysisId = Guid.NewGuid(),
            OldStatus = "Pending",
            NewStatus = "Processing",
            Timestamp = DateTime.UtcNow
        };

        var context = CreateConsumeContext(message);

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

    private static ConsumeContext<StatusChangedEvent> CreateConsumeContext(StatusChangedEvent message)
    {
        var context = Substitute.For<ConsumeContext<StatusChangedEvent>>();
        context.Message.Returns(message);
        context.CancellationToken.Returns(CancellationToken.None);
        return context;
    }
}
