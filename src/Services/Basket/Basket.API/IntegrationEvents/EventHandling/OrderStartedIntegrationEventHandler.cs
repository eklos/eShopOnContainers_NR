namespace Basket.API.IntegrationEvents.EventHandling;

public class OrderStartedIntegrationEventHandler : IIntegrationEventHandler<OrderStartedIntegrationEvent>
{
    private readonly IBasketRepository _repository;
    private readonly ILogger<OrderStartedIntegrationEventHandler> _logger;

    public OrderStartedIntegrationEventHandler(
        IBasketRepository repository,
        ILogger<OrderStartedIntegrationEventHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(OrderStartedIntegrationEvent @event)
    {
        NewRelic.Api.Agent.IAgent Agent = NewRelic.Api.Agent.NewRelic.GetAgent();
        var linkingMetadata = Agent.GetLinkingMetadata();
        Serilog.Context.LogContext.PushProperty("newrelic.linkingmetadata", linkingMetadata);

        using (LogContext.PushProperty("IntegrationEventContext", $"{@event.Id}-{Program.AppName}"))
        {
            _logger.LogInformation("----- Handling integration event: {IntegrationEventId} at {AppName} - ({@IntegrationEvent})", @event.Id, Program.AppName, @event);

            await _repository.DeleteBasketAsync(@event.UserId.ToString());
        }
    }
}




