//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CourierShopLoginApp.DataModels
{
    using System;
    using System.Collections.Generic;
    
    public partial class OrderItems
    {
        public int item_id { get; set; }
        public Nullable<int> order_id { get; set; }
        public string product_name { get; set; }
        public Nullable<int> quantity { get; set; }
        public Nullable<decimal> price { get; set; }
    
        public virtual Orders Orders { get; set; }
    }
}
