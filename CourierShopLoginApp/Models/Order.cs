using System;

namespace CourierShopLoginApp.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public int? CustomerId { get; set; }
        public int? CourierId { get; set; }
        public DateTime OrderDate { get; set; }
        public string DeliveryAddress { get; set; }
        public int StatusId { get; set; }
        public string StatusName { get; set; }
        public decimal TotalAmount { get; set; }
        
        // Navigation properties for display
        public string CustomerName { get; set; }
        public string CourierName { get; set; }
        
        // Additional properties
        public DateTime? DeliveryDate { get; set; }
        public string Comment { get; set; }
    }
}