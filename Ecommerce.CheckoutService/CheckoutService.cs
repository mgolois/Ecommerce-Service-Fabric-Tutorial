using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ecommerce.CheckoutService.Model;
using ECommerce.ProductCatalogModel;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using UserActor.Interfaces;

namespace Ecommerce.CheckoutService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class CheckoutService : StatefulService, ICheckoutService
    {
        public CheckoutService(StatefulServiceContext context)
            : base(context)
        { }

        public async Task<CheckoutSummary> CheckoutAsync(string userId)
        {
            var result = new CheckoutSummary();
            result.Date = DateTime.UtcNow;
            result.Products = new List<CheckoutProduct>();

            //get user basket 
            var userActor = GetUserActor(userId);
            Dictionary<Guid, int> basket = await userActor.GetBasket();

            //get catalogClient
            IProductCatalogService catalogService = GetProductCatalogService();

            foreach (var item in basket)
            {
                Product product = await catalogService.GetProduct(item.Key);
                result.Products.Add(
                    new CheckoutProduct
                    {
                        Product = product,
                     Price = product.Price,
                     Quantity = item.Value
                    });
            }
            await userActor.ClearBasket();
            await AddToHistoryAsync(result);
            return result;
        }

        private IProductCatalogService GetProductCatalogService()
        {
            var proxyFactory = new ServiceProxyFactory(c => new FabricTransportServiceRemotingClientFactory());
            return proxyFactory.CreateServiceProxy<IProductCatalogService>(
                new Uri("fabric:/Ecommerce/Ecommerce.ProductCatalog"),
                new ServicePartitionKey(0));
        }

        private async Task AddToHistoryAsync(CheckoutSummary checkout)
        {
            var history = await StateManager.GetOrAddAsync<IReliableDictionary<DateTime, CheckoutSummary>>("history");
            using(ITransaction tx = StateManager.CreateTransaction())
            {
                await history.AddAsync(tx, checkout.Date, checkout);
                await tx.CommitAsync();
            }
        }
        public async Task<CheckoutSummary[]> GetOrderHistoryAsync(string userId)
        {
            var history = await StateManager.GetOrAddAsync<IReliableDictionary<DateTime, CheckoutSummary>>("history");
            var result = new List<CheckoutSummary>();

            using(ITransaction tx = StateManager.CreateTransaction())
            {
                using (var enumerator = (await history.CreateEnumerableAsync(tx, EnumerationMode.Unordered)).GetAsyncEnumerator())
                {
                    while(await enumerator.MoveNextAsync(CancellationToken.None))
                    {
                        var current = enumerator.Current;
                        result.Add(current.Value);
                    }
                }
            }
            return result.ToArray();
        }

        private IUserActor GetUserActor(string userId)
        {
            return ActorProxy.Create<IUserActor>(new ActorId(userId), new Uri("fabric:/Ecommerce/UserActorService"));
        }
        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new[]
            {
                new ServiceReplicaListener(context => new FabricTransportServiceRemotingListener(context, this))
            };
        }

    }
}
