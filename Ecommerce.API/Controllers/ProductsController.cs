using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.API.Models;
using ECommerce.ProductCatalogModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;

namespace Ecommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductCatalogService _service;

        public ProductsController()
        {
            var proxyFactory = new ServiceProxyFactory(c => new FabricTransportServiceRemotingClientFactory());
            _service = proxyFactory.CreateServiceProxy<IProductCatalogService>(
                new Uri("fabric:/Ecommerce/Ecommerce.ProductCatalog"),
                new ServicePartitionKey(0));
        }
        [HttpGet]
        public async Task<IEnumerable<ApiProduct>> GetAsync()
        {
            return (await _service.GetAllProductsAsync()).Select(c => new ApiProduct
            {
                Description = c.Description,
                Id = c.Id,
                IsAvailable = c.Availability > 0,
                Name = c.Name,
                Price = c.Price
            });
        }

        [HttpPost]
        public async Task PostAsync( ApiProduct product)
        {
            var productObj = new Product
            {
                Price = product.Price,
                Name = product.Name,
                Availability = 100,
                Description = product.Description,
                Id = product.Id
            };

            await _service.AddProductAsync(productObj);
        }
    }
}