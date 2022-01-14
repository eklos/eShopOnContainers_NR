namespace Microsoft.eShopOnContainers.Web.Shopping.HttpAggregator.Services;

public class BasketService : IBasketService
{
    private readonly Basket.BasketClient _basketClient;
    private readonly ILogger<BasketService> _logger;

    public BasketService(Basket.BasketClient basketClient, ILogger<BasketService> logger)
    {
        _basketClient = basketClient;
        _logger = logger;
    }

    [NewRelic.Api.Agent.Trace]
    public async Task<BasketData> GetByIdAsync(string id)
    {
        _logger.LogInformation("$$$ Here I am $$$ request = {@id}", id);
        _logger.LogDebug("grpc client created, request = {@id}", id);
        var response = await _basketClient.GetBasketByIdAsync(new BasketRequest { Id = id });
        _logger.LogDebug("grpc response {@response}", response);

        return MapToBasketData(response);
    }

    [NewRelic.Api.Agent.Trace]
    public async Task UpdateAsync(BasketData currentBasket)
    {
        _logger.LogInformation("$$$ Here I am $$$ update basket currentBasket {@currentBasket}", currentBasket);
        var num = 1;
        foreach(BasketDataItem item in currentBasket.Items)
        {
            _logger.LogInformation("$$$ Basketdata({0}) Id:{1} ProductId: {2} ProductName: {3} UnitPrice: {4} OldUnitPrice: {5} Quantity: {6} PictureUrl: {7}",
            num++, item.Id, item.ProductId, item.ProductName, item.UnitPrice, item.OldUnitPrice, item.Quantity, item.PictureUrl);

            //try
            //{
                if(item.Quantity == 17)
                {
                    throw  new ArgumentOutOfRangeException();
                }
                else if(item.Quantity == 18)
                {
                    var zero = 0;
                    num = num / zero;
                }
            //}
            //catch(DivideByZeroException ex)
            //{
            //    NewRelic.Api.Agent.NewRelic.NoticeError(ex);
            //    throw;
            //}
            //catch (Exception)
            //{
            //    throw;
            //}
        }
        _logger.LogDebug("Grpc update basket currentBasket {@currentBasket}", currentBasket);
        var request = MapToCustomerBasketRequest(currentBasket);
        _logger.LogDebug("Grpc update basket request {@request}", request);

        await _basketClient.UpdateBasketAsync(request);
    }

    [NewRelic.Api.Agent.Trace]
    private BasketData MapToBasketData(CustomerBasketResponse customerBasketRequest)
    {
        if (customerBasketRequest == null)
        {
            return null;
        }

        var map = new BasketData
        {
            BuyerId = customerBasketRequest.Buyerid
        };

        customerBasketRequest.Items.ToList().ForEach(item =>
        {
            if (item.Id != null)
            {
                map.Items.Add(new BasketDataItem
                {
                    Id = item.Id,
                    OldUnitPrice = (decimal)item.Oldunitprice,
                    PictureUrl = item.Pictureurl,
                    ProductId = item.Productid,
                    ProductName = item.Productname,
                    Quantity = item.Quantity,
                    UnitPrice = (decimal)item.Unitprice
                });
            }
        });

        return map;
    }

    [NewRelic.Api.Agent.Trace]
    private CustomerBasketRequest MapToCustomerBasketRequest(BasketData basketData)
    {
        if (basketData == null)
        {
            return null;
        }

        var map = new CustomerBasketRequest
        {
            Buyerid = basketData.BuyerId
        };

        basketData.Items.ToList().ForEach(item =>
        {
            if (item.Id != null)
            {
                map.Items.Add(new BasketItemResponse
                {
                    Id = item.Id,
                    Oldunitprice = (double)item.OldUnitPrice,
                    Pictureurl = item.PictureUrl,
                    Productid = item.ProductId,
                    Productname = item.ProductName,
                    Quantity = item.Quantity,
                    Unitprice = (double)item.UnitPrice
                });
            }
        });

        return map;
    }
}
