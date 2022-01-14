namespace Microsoft.eShopOnContainers.Services.Catalog.API.IntegrationEvents.EventHandling;

public class OrderStatusChangedToPaidIntegrationEventHandler :
    IIntegrationEventHandler<OrderStatusChangedToPaidIntegrationEvent>
{
    private readonly CatalogContext _catalogContext;
    private readonly ILogger<OrderStatusChangedToPaidIntegrationEventHandler> _logger;

    public OrderStatusChangedToPaidIntegrationEventHandler(
        CatalogContext catalogContext,
        ILogger<OrderStatusChangedToPaidIntegrationEventHandler> logger)
    {
        _catalogContext = catalogContext;
        _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
    }

    public async Task Handle(OrderStatusChangedToPaidIntegrationEvent @event)
    {
        NewRelic.Api.Agent.IAgent Agent = NewRelic.Api.Agent.NewRelic.GetAgent();
        var linkingMetadata = Agent.GetLinkingMetadata();
        Serilog.Context.LogContext.PushProperty("newrelic.linkingmetadata", linkingMetadata);

        using (LogContext.PushProperty("IntegrationEventContext", $"{@event.Id}-{Program.AppName}"))
        {
            _logger.LogInformation("----- Handling integration event: {IntegrationEventId} at {AppName} - ({@IntegrationEvent})", @event.Id, Program.AppName, @event);

            //we're not blocking stock/inventory
            foreach (var orderStockItem in @event.OrderStockItems)
            {
                var catalogItem = _catalogContext.CatalogItems.Find(orderStockItem.ProductId);

                catalogItem.RemoveStock(orderStockItem.Units);
            }

            await _catalogContext.SaveChangesAsync();

        }
    }
}
