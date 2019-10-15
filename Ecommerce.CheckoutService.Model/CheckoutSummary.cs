using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ecommerce.CheckoutService.Model
{
    public class CheckoutSummary
    {
        public List<CheckoutProduct> Products { get; set; }
        public double TotalPrice => Products.Sum(c => c.Quantity * c.Price);
        public DateTime Date { get; set; }

    }
}
