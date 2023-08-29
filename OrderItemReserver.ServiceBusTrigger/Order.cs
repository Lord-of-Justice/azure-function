using Newtonsoft.Json;
using System.Collections.Generic;

namespace OrderItemReserver.ServiceBusTrigger;

public class Order
{
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; }
    public IEnumerable<OrderItem> OrderItems { get; set; }
    public decimal Total { get; private set; }

    public Order(string id, IEnumerable<OrderItem> items, decimal total)
    {
        Id = id;
        OrderItems = items;
        Total = total;
    }
}

public class OrderItem
{
    public string Id { get; set; }
    public CatalogItemOrdered ItemOrdered { get; set; }
    public decimal UnitPrice { get; set; }
    public int Units { get; set; }
}

public class CatalogItemOrdered
{
    public int CatalogItemId { get; set; }
    public string ProductName { get; set; }
    public string PictureUri { get; set; }
}
