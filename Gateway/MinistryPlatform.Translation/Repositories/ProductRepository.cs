using System.Collections.Generic;
using System.Linq;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using MinistryPlatform.Translation.Models.Product;
using MinistryPlatform.Translation.Repositories.Interfaces;

namespace MinistryPlatform.Translation.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly IConfigurationWrapper _configurationWrapper;
        private readonly IMinistryPlatformRestRepository _ministryPlatformRest;
        private readonly IApiUserRepository _apiUserRepository;

        public ProductRepository(IConfigurationWrapper configurationWrapper,
                                 IMinistryPlatformRestRepository ministryPlatformRest, 
                                 IApiUserRepository apiUserRepository)
        {
            _configurationWrapper = configurationWrapper;
            _ministryPlatformRest = ministryPlatformRest;
            _apiUserRepository = apiUserRepository;
        }

        public MpProduct GetProduct(int productId)
        {
            var apiToken = _apiUserRepository.GetDefaultApiClientToken();
            return _ministryPlatformRest.UsingAuthenticationToken(apiToken).Get<MpProduct>(productId);
        }
        public MpProduct GetProductForEvent(int eventId)
        {
            var apiToken = _apiUserRepository.GetDefaultApiClientToken();
            var productId = GetProductIdForEvent(eventId);
            return _ministryPlatformRest.UsingAuthenticationToken(apiToken).Search<MpProduct>($"Product_ID = {productId}").FirstOrDefault();
        }
        public MpProductOptionPrice GetProductOptionPrice(int productOptionPriceId)
        {
            var apiToken = _apiUserRepository.GetDefaultApiClientToken();
            return _ministryPlatformRest.UsingAuthenticationToken(apiToken).Get<MpProductOptionPrice>(productOptionPriceId);
        }
        public List<MpProductOptionPrice> GetProductOptionPricesForProduct(int productId)
        {
            var productOptionPrices = new List<MpProductOptionPrice>();
            var apiToken = _apiUserRepository.GetDefaultApiClientToken();

            //get the product option groups
            var productOptionGroups = _ministryPlatformRest.UsingAuthenticationToken(apiToken).Search<MpProductOptionGroup>($"Product_ID = {productId}");

            //get product option prices
            foreach (var group in productOptionGroups)
            {
                var prices = _ministryPlatformRest.UsingAuthenticationToken(apiToken).Search<MpProductOptionPrice>($"Product_Option_Group_ID = {group.ProductOptionGroupId}");
                productOptionPrices.AddRange(prices);
            }

            return productOptionPrices;
        }

        private int GetProductIdForEvent(int eventId)
        {
            var apiToken = _apiUserRepository.GetDefaultApiClientToken();
            return _ministryPlatformRest.UsingAuthenticationToken(apiToken).Search<int>("Events", $"Event_ID = {eventId}", "Online_Registration_Product", null, false);
        }
    }
}
