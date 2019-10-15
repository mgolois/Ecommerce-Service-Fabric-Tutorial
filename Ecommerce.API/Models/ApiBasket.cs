using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.API.Models
{
    public class ApiBasketAddRequest
    {
        [JsonProperty]
        public Guid ProductId { get; set; }
        [JsonProperty]
        public int Quantity { get; set; }
    }
    public class ApiBasket
    {
        [JsonProperty]
        public string UserId { get; set; }
        [JsonProperty]
        public ApiBasketItem[] Items { get; set; }
    }
    public class ApiBasketItem
    {
        [JsonProperty]
        public string ProductId { get; set; }
        [JsonProperty]
        public int Quantity { get; set; }
    }
}
