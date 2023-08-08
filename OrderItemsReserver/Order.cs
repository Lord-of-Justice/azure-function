using Newtonsoft.Json;
using System.Collections.Generic;

namespace OrderItemsReserver;

public class Order
{
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; }

    [JsonProperty(PropertyName = "buyerId")]
    public string BuyerId { get; set; }

    public Address ShipToAddress { get; set; }
    public IEnumerable<OrderItem> OrderItems { get; set; }
    public decimal Total { get; private set; }
    public Order(string id, string buyerId, Address shipToAddress, IEnumerable<OrderItem> items, decimal total)
    {
        Id = $"{buyerId}.{id}";
        BuyerId = buyerId;
        ShipToAddress = shipToAddress;
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

public class Address
{
    public string Street { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string Country { get; set; }
    public string ZipCode { get; set; }
}
