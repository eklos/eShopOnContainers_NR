﻿namespace Microsoft.eShopOnContainers.Services.Ordering.SignalrHub.IntegrationEvents.EventHandling;

public class OrderStatusChangedToCancelledIntegrationEventHandler : IIntegrationEventHandler<OrderStatusChangedToCancelledIntegrationEvent>
{
    private readonly IHubContext<NotificationsHub> _hubContext;
    private readonly ILogger<OrderStatusChangedToCancelledIntegrationEventHandler> _logger;

    public OrderStatusChangedToCancelledIntegrationEventHandler(
        IHubContext<NotificationsHub> hubContext,
        ILogger<OrderStatusChangedToCancelledIntegrationEventHandler> logger)
    {
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }


    public async Task Handle(OrderStatusChangedToCancelledIntegrationEvent @event)
    {
        NewRelic.Api.Agent.IAgent Agent = NewRelic.Api.Agent.NewRelic.GetAgent();
        var linkingMetadata = Agent.GetLinkingMetadata();
        Serilog.Context.LogContext.PushProperty("newrelic.linkingmetadata", linkingMetadata);

        using (LogContext.PushProperty("IntegrationEventContext", $"{@event.Id}-{Program.AppName}"))
        {
            _logger.LogInformation("----- Handling integration event: {IntegrationEventId} at {AppName} - ({@IntegrationEvent})", @event.Id, Program.AppName, @event);

            await _hubContext.Clients
                .Group(@event.BuyerName)
                .SendAsync("UpdatedOrderState", new { OrderId = @event.OrderId, Status = @event.OrderStatus });
        }
    }
}