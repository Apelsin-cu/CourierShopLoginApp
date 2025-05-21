using System;

namespace CourierShopLoginApp.Models
{
    public class Delivery
    {
        public int DeliveryId { get; set; }
        public int OrderId { get; set; }
        public int CourierId { get; set; }
        public DateTime AssignmentDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public int StatusId { get; set; }
        public string StatusName { get; set; }
        public string Comment { get; set; }
        
        // Navigation properties
        public string CourierName { get; set; }
        public string CustomerName { get; set; }
        public string DeliveryAddress { get; set; }
    }
}