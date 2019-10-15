﻿using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.ProductCatalogModel
{
    public interface IProductCatalogService:IService
    {
        Task<Product[]> GetAllProductsAsync();
        Task AddProductAsync(Product product);
        Task<Product> GetProduct(Guid key);
    }
}
