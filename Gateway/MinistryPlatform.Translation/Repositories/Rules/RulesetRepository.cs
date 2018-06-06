using System.Collections.Generic;
using System.Linq;
using Crossroads.Web.Common;
using Crossroads.Web.Common.MinistryPlatform;
using MinistryPlatform.Translation.Models.Rules;
using MinistryPlatform.Translation.Repositories.Interfaces;
using MinistryPlatform.Translation.Repositories.Interfaces.Rules;

namespace MinistryPlatform.Translation.Repositories.Rules
{
    public class RulesetRepository : IRuleset
    {
        private readonly IApiUserRepository _apiUserRepository;
        private readonly IMinistryPlatformRestRepository _ministryPlatformRestRepository;

        public RulesetRepository(IApiUserRepository apiUserRepository, IMinistryPlatformRestRepository ministryPlatformRestRepository)
        {
            _apiUserRepository = apiUserRepository;
            _ministryPlatformRestRepository = ministryPlatformRestRepository;
        }

        public MPRuleSet GetRulesetFromMP(int ruleSetId)
        {
            var apiToken = _apiUserRepository.GetDefaultApiClientToken();
            return _ministryPlatformRestRepository.UsingAuthenticationToken(apiToken).Get<MPRuleSet>(ruleSetId);
        }

        public List<IRule> GetRulesInRuleset(int ruleSetId)
        {
            var rules = new List<IRule>();

            rules.AddRange(GetGenderRules(ruleSetId));
            rules.AddRange(GetRegistrantRules(ruleSetId));

            return rules;
        }

        public MPRuleSetResult AllRulesPass(List<IRule> rules, Dictionary<string, object> testData)
        {
            var result = new MPRuleSetResult();
            if (!rules.Any())
                return new MPRuleSetResult()
                {
                    AllRulesPass = true,
                    RuleResults = new List<MPRuleResult>
                    {
                        new MPRuleResult()
                        {
                            Message = "No Rules Present",
                            RulePassed = true
                        }
                    }
                };
            foreach (var rule in rules)
            {
                result.RuleResults.Add(rule.RulePasses(testData));
            }
            result.AllRulesPass = result.RuleResults.All(r => r.RulePassed);
            return result;
        }

        private IEnumerable<GenderRule> GetGenderRules(int ruleSetId)
        {
            var apiToken = _apiUserRepository.GetDefaultApiClientToken();
            var searchString = $"Ruleset_ID = {ruleSetId}";
            var genderRules = _ministryPlatformRestRepository.UsingAuthenticationToken(apiToken).Search<MPGenderRule>(searchString);
            return genderRules.Select(g => new GenderRule(g.StartDate, g.EndDate, g.AllowedGenderId));
        }

        private IEnumerable<RegistrationRule> GetRegistrantRules(int ruleSetId)
        {
            var apiToken = _apiUserRepository.GetDefaultApiClientToken();
            var searchString = $"Ruleset_ID = {ruleSetId}";
            var registrantRules = _ministryPlatformRestRepository.UsingAuthenticationToken(apiToken).Search<MPRegistrationRule>(searchString);
            return registrantRules.Select(g => new RegistrationRule(g.StartDate, g.EndDate, g.MaximumRegistrants));
        }
    }
}
