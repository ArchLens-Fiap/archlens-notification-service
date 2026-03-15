using ArchLens.Notification.Infrastructure.Hubs;
using ArchLens.Notification.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;

namespace ArchLens.Notification.Tests.Services;

public class SignalRNotificationSenderTests
{
    private readonly IHubContext<AnalysisHub> _hubContext;
    private readonly IHubClients _hubClients;
    private readonly IClientProxy _analysisGroupProxy;
    private readonly IClientProxy _dashboardProxy;
    private readonly SignalRNotificationSender _sender;

    public SignalRNotificationSenderTests()
    {
        _hubContext = Substitute.For<IHubContext<AnalysisHub>>();
        _hubClients = Substitute.For<IHubClients>();
        _analysisGroupProxy = Substitute.For<IClientProxy>();
        _dashboardProxy = Substitute.For<IClientProxy>();

        _hubContext.Clients.Returns(_hubClients);
        _hubClients.Group("dashboard").Returns(_dashboardProxy);

        _sender = new SignalRNotificationSender(_hubContext);
    }

    [Fact]
    public async Task SendStatusChangedAsync_ShouldSendToAnalysisGroup()
    {
        // Arrange
        var analysisId = Guid.NewGuid();
        _hubClients.Group(analysisId.ToString()).Returns(_analysisGroupProxy);

        // Act
        await _sender.SendStatusChangedAsync(analysisId, "Pending", "Processing", DateTime.UtcNow);

        // Assert
        await _analysisGroupProxy.Received(1).SendCoreAsync(
            "StatusChanged",
            Arg.Any<object?[]>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendStatusChangedAsync_ShouldSendToDashboardGroup()
    {
        // Arrange
        var analysisId = Guid.NewGuid();
        _hubClients.Group(analysisId.ToString()).Returns(_analysisGroupProxy);

        // Act
        await _sender.SendStatusChangedAsync(analysisId, "Processing", "Completed", DateTime.UtcNow);

        // Assert
        await _dashboardProxy.Received(1).SendCoreAsync(
            "AnalysisStatusChanged",
            Arg.Any<object?[]>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendAnalysisCompletedAsync_ShouldSendToAnalysisGroup()
    {
        // Arrange
        var analysisId = Guid.NewGuid();
        var diagramId = Guid.NewGuid();
        _hubClients.Group(analysisId.ToString()).Returns(_analysisGroupProxy);

        // Act
        await _sender.SendAnalysisCompletedAsync(analysisId, diagramId, DateTime.UtcNow);

        // Assert
        await _analysisGroupProxy.Received(1).SendCoreAsync(
            "AnalysisCompleted",
            Arg.Any<object?[]>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendAnalysisCompletedAsync_ShouldSendToDashboardGroup()
    {
        // Arrange
        var analysisId = Guid.NewGuid();
        var diagramId = Guid.NewGuid();
        _hubClients.Group(analysisId.ToString()).Returns(_analysisGroupProxy);

        // Act
        await _sender.SendAnalysisCompletedAsync(analysisId, diagramId, DateTime.UtcNow);

        // Assert
        await _dashboardProxy.Received(1).SendCoreAsync(
            "AnalysisCompleted",
            Arg.Any<object?[]>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task BroadcastAsync_ShouldSendToDashboardGroup()
    {
        // Arrange
        var eventName = "CustomEvent";
        var payload = new { Message = "test" };

        // Act
        await _sender.BroadcastAsync(eventName, payload);

        // Assert
        await _dashboardProxy.Received(1).SendCoreAsync(
            eventName,
            Arg.Any<object?[]>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task BroadcastAsync_WithCancellationToken_ShouldPassToken()
    {
        // Arrange
        var eventName = "TestEvent";
        var payload = new { Data = "value" };
        using var cts = new CancellationTokenSource();

        // Act
        await _sender.BroadcastAsync(eventName, payload, cts.Token);

        // Assert
        await _dashboardProxy.Received(1).SendCoreAsync(
            eventName,
            Arg.Any<object?[]>(),
            cts.Token);
    }

    [Fact]
    public async Task SendStatusChangedAsync_WithCancellationToken_ShouldPassToken()
    {
        // Arrange
        var analysisId = Guid.NewGuid();
        _hubClients.Group(analysisId.ToString()).Returns(_analysisGroupProxy);
        using var cts = new CancellationTokenSource();

        // Act
        await _sender.SendStatusChangedAsync(analysisId, "Old", "New", DateTime.UtcNow, cts.Token);

        // Assert
        await _analysisGroupProxy.Received(1).SendCoreAsync(
            "StatusChanged",
            Arg.Any<object?[]>(),
            cts.Token);
    }

    [Fact]
    public async Task SendAnalysisCompletedAsync_WithCancellationToken_ShouldPassToken()
    {
        // Arrange
        var analysisId = Guid.NewGuid();
        var diagramId = Guid.NewGuid();
        _hubClients.Group(analysisId.ToString()).Returns(_analysisGroupProxy);
        using var cts = new CancellationTokenSource();

        // Act
        await _sender.SendAnalysisCompletedAsync(analysisId, diagramId, DateTime.UtcNow, cts.Token);

        // Assert
        await _analysisGroupProxy.Received(1).SendCoreAsync(
            "AnalysisCompleted",
            Arg.Any<object?[]>(),
            cts.Token);
    }

    [Fact]
    public async Task SendStatusChangedAsync_ShouldUseCorrectGroupName()
    {
        // Arrange
        var analysisId = Guid.NewGuid();
        _hubClients.Group(analysisId.ToString()).Returns(_analysisGroupProxy);

        // Act
        await _sender.SendStatusChangedAsync(analysisId, "Pending", "Processing", DateTime.UtcNow);

        // Assert
        _hubClients.Received().Group(analysisId.ToString());
        _hubClients.Received().Group("dashboard");
    }
}
