using System;
namespace Microsoft.eShopOnContainers.Web.Shopping.HttpAggregator.Infrastructure;

public class GrpcExceptionInterceptor : Interceptor
{
    private readonly ILogger<GrpcExceptionInterceptor> _logger;

    public GrpcExceptionInterceptor(ILogger<GrpcExceptionInterceptor> logger)
    {
        _logger = logger;
    }

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        Console.WriteLine("{0} $$$ AsyncUnaryCall start pelim... request:{1} context:{2}, continuation:{3}", DateTime.Now.ToLocalTime().ToString(), request, context, continuation);

        NewRelic.Api.Agent.IAgent Agent = NewRelic.Api.Agent.NewRelic.GetAgent();
        var linkingMetadata = Agent.GetLinkingMetadata();
        Serilog.Context.LogContext.PushProperty("newrelic.linkingmetadata", linkingMetadata);

        var traceId = "";
        var spanId = "";
        var entityName = "";
        var entityType = "";
        foreach (KeyValuePair<string, string> kvp in linkingMetadata)
        {
            if (kvp.Key == "trace.id") traceId = kvp.Value;
            else if (kvp.Key == "span.id") spanId = kvp.Value;
            else if (kvp.Key == "entity.name") entityName = kvp.Value;
            else if (kvp.Key == "entity.type") entityType = kvp.Value;
            //Console.WriteLine("{0} @1@ Key = {1}, Value = {2}", DateTime.Now.ToLocalTime().ToString(), kvp.Key, kvp.Value);
        }
        Console.WriteLine("{0} $$$ AsyncUnaryCall entity.name = {1}({2}) trace.id = {3} span.id = {4}", 
            DateTime.Now.ToLocalTime().ToString(), entityName, entityType, traceId, spanId);
        _logger.LogInformation("{0} $$$ AsyncUnaryCall start pelim... request:{1} context:{2}, continuation:{3}", DateTime.Now.ToLocalTime().ToString(), request, context, continuation);

        //Console.WriteLine("{0} @@@ linkingMetadata = {1}", DateTime.Now.ToLocalTime().ToString(), linkingMetadata);
        //Console.WriteLine("{0} $$$ AsyncUnaryCall end prelim... request:{1} context:{2}, continuation:{3}", DateTime.Now.ToLocalTime().ToString(), request, context, continuation);

        var call = continuation(request, context);

        return new AsyncUnaryCall<TResponse>(HandleResponse(call.ResponseAsync), call.ResponseHeadersAsync, call.GetStatus, call.GetTrailers, call.Dispose);
    }

    private async Task<TResponse> HandleResponse<TResponse>(Task<TResponse> task)
    {
        try
        {
            var response = await task;
            return response;
        }
        catch (RpcException e)
        {
            _logger.LogInformation("$$$ 1 Error calling via grpc: {Status} - {Message}", e.Status, e.Message);
            _logger.LogInformation("$$$ 2 Error calling via grpc: exception:{0}", e);
            _logger.LogError("Error calling via grpc: {Status} - {Message}", e.Status, e.Message);
            return default;
        }
    }
}
