using System.Collections.Generic;
using System.Linq;
using Crossroads.Web.Common;
using Crossroads.Web.Common.MinistryPlatform;
using MinistryPlatform.Translation.Models.Rules;
using MinistryPlatform.Translation.Repositories.Interfaces;
using MinistryPlatform.Translation.Repositories.Interfaces.Rules;

namespace MinistryPlatform.Translation.Repositories.Rules
{
    public class ProductRulesetsRepository : IProductRulesetsRepository
    {
        private readonly IApiUserRepository _apiUserRepository;
        private readonly IMinistryPlatformRestRepository _ministryPlatformRestRepository;
        private readonly IRuleset _rulesetRepository;

        public ProductRulesetsRepository(IApiUserRepository apiUserRepository, IMinistryPlatformRestRepository ministryPlatformRestRepository, IRuleset rulesetRepository)
        {
            _apiUserRepository = apiUserRepository;
            _ministryPlatformRestRepository = ministryPlatformRestRepository;
            _rulesetRepository = rulesetRepository;
        }

        public List<MPProductRuleSet> GetProductRulesets(int productId)
        {
            var apiToken = _apiUserRepository.GetDefaultApiClientToken();
            var searchString = $"Product_ID_Table.[Product_ID] = {productId}";
            const string columnList = "Product_ID_Table.[Product_ID],Ruleset_ID_Table.[Ruleset_ID],cr_Product_Ruleset.[Start_Date],cr_Product_Ruleset.[End_Date]";
            return _ministryPlatformRestRepository.UsingAuthenticationToken(apiToken).Search<MPProductRuleSet>(searchString, columnList);
        }

        public bool VerifyRulesets(List<MPProductRuleSet> productRulesets, Dictionary<string, object> testData )
        {
            if (productRulesets.Any())
            {
                foreach (var productRule in productRulesets)
                {
                    var ruleset = _rulesetRepository.GetRulesInRuleset(productRule.RulesetId);
                    var result = _rulesetRepository.AllRulesPass(ruleset, testData);
                    if (result.AllRulesPass)
                    {
                        return true;
                    }
                    ;
                }
                return false;
            }
            return true;
        }
    }
}