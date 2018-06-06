using System;
using System.Collections.Generic;
using System.Linq;
using MinistryPlatform.Translation.Models.Rules;
using MinistryPlatform.Translation.Repositories.Interfaces;
using MinistryPlatform.Translation.Repositories.Interfaces.Rules;
using MinistryPlatform.Translation.Repositories.Rules;
using MinistryPlatform.Translation.Test.Helpers;
using Moq;
using NUnit.Framework;
using FactoryGirl.NET;
using Crossroads.Web.Common;
using Crossroads.Web.Common.MinistryPlatform;

namespace MinistryPlatform.Translation.Test.Services
{
    public class ProductRulesetsRepositoryTest
    {

        private Mock<IApiUserRepository> _apiUserRepository;
        private Mock<IRuleset> _rulesetRepository;
        private Mock<IMinistryPlatformRestRepository> _ministryPlatformRestRepository;
        private IProductRulesetsRepository _fixture;

        [SetUp]
        public void Setup()
        {
            Factories.MPProductRuleset();
            Factories.GenderRule();
            Factories.RegistrationRule();

            _apiUserRepository = new Mock<IApiUserRepository>();
            _rulesetRepository = new Mock<IRuleset>();
            _ministryPlatformRestRepository = new Mock<IMinistryPlatformRestRepository>();
            _fixture = new ProductRulesetsRepository(_apiUserRepository.Object, _ministryPlatformRestRepository.Object, _rulesetRepository.Object);
        }

        public void Teardown()
        {
            _apiUserRepository.VerifyAll();
            _ministryPlatformRestRepository.VerifyAll();
            _rulesetRepository.VerifyAll();
        }

        [Test]
        public void ShouldGetRulesetsForProduct()
        {
            const string token = "O\'Doyle Rules!";
            const int productId = 351;
            var rulesets = new List<MPProductRuleSet>
            {
               FactoryGirl.NET.FactoryGirl.Build<MPProductRuleSet>( r => { r.ProductId = r.ProductId + 2; r.RulesetId = r.RulesetId + 2; }),
               FactoryGirl.NET.FactoryGirl.Build<MPProductRuleSet>( r => { r.ProductId = r.ProductId + 3; r.RulesetId = r.RulesetId + 3; }),
               FactoryGirl.NET.FactoryGirl.Build<MPProductRuleSet>( r => { r.ProductId = r.ProductId + 4; r.RulesetId = r.RulesetId + 4; })
            };

            _apiUserRepository.Setup(m => m.GetDefaultApiClientToken()).Returns(token);
            _ministryPlatformRestRepository.Setup(m => m.UsingAuthenticationToken(token)).Returns(_ministryPlatformRestRepository.Object);
            _ministryPlatformRestRepository.Setup(m => m.Search<MPProductRuleSet>(It.IsAny<string>(), It.IsAny<string>(), null as string, false)).Returns(rulesets);

            var res = _fixture.GetProductRulesets(productId);
            Assert.AreEqual(3, res.Count);            
        }

        [Test]
        public void ShouldGetEmptyListForNoRulesets()
        {
            const string token = "O\'Doyle Rules!";
            const int productId = 351;
            var rulesets = new List<MPProductRuleSet>();

            _apiUserRepository.Setup(m => m.GetDefaultApiClientToken()).Returns(token);
            _ministryPlatformRestRepository.Setup(m => m.UsingAuthenticationToken(token)).Returns(_ministryPlatformRestRepository.Object);
            _ministryPlatformRestRepository.Setup(m => m.Search<MPProductRuleSet>(It.IsAny<string>(), It.IsAny<string>(), null as string, false)).Returns(rulesets);

            var res = _fixture.GetProductRulesets(productId);
            Assert.AreEqual(0, res.Count);
        }

        [Test]
        public void ShouldRethrowExceptionWhenExceptionIsThrown()
        {
            const string token = "O\'Doyle Rules!";
            const int productId = 351;
            var rulesets = new List<MPProductRuleSet>();

            _apiUserRepository.Setup(m => m.GetDefaultApiClientToken()).Returns(token);
            _ministryPlatformRestRepository.Setup(m => m.UsingAuthenticationToken(token)).Returns(_ministryPlatformRestRepository.Object);
            _ministryPlatformRestRepository.Setup(m => m.Search<MPProductRuleSet>(It.IsAny<string>(), It.IsAny<string>(), null as string, false)).Throws(new Exception());

            Assert.Throws<Exception>(() =>
            {
                _fixture.GetProductRulesets(productId);
            });

        }

        [Test]
        public void ShouldNotVerifyIfNoRulesetsPass()
        {
            const int ruleset1Id = 789;
            const int ruleset2Id = 9008;

            var rulesets = new List<MPProductRuleSet>
            {
               FactoryGirl.NET.FactoryGirl.Build<MPProductRuleSet>( r => { r.ProductId = r.ProductId + 2; r.RulesetId = ruleset1Id; }),
               FactoryGirl.NET.FactoryGirl.Build<MPProductRuleSet>( r => { r.ProductId = r.ProductId + 3; r.RulesetId = ruleset2Id; })
            };

            var rules1 = new List<IRule>();
            var startDate = DateTime.Today.AddDays(-30);
            rules1.Add(new GenderRule(startDate, null, 1));
            rules1.Add(new RegistrationRule(startDate, null, 1000));

            var rules2 = new List<IRule>();
            rules2.Add(new GenderRule(startDate, null, 1));
            rules2.Add(new RegistrationRule(startDate, null, 1000));

            var result1 = new MPRuleSetResult
            {
                AllRulesPass = false,
                RuleResults = new List<MPRuleResult>()
            };

            var result2 = new MPRuleSetResult
            {
                AllRulesPass = false,
                RuleResults = new List<MPRuleResult>()
            };

            _rulesetRepository.Setup(m => m.GetRulesInRuleset(ruleset1Id)).Returns(rules1);
            _rulesetRepository.Setup(m => m.GetRulesInRuleset(ruleset2Id)).Returns(rules2);
            _rulesetRepository.Setup(m => m.AllRulesPass(rules1, It.IsAny<Dictionary<string, object>>())).Returns(result1);
            _rulesetRepository.Setup(m => m.AllRulesPass(rules2, It.IsAny<Dictionary<string, object>>())).Returns(result2);
            var res = _fixture.VerifyRulesets(rulesets, new Dictionary<string, object>());
            Assert.IsFalse(res);
        }

        [Test]
        public void ShouldVerifyRulesetIfAtLeastOneRulesetPasses()
        {
            const int ruleset1Id = 789;
            const int ruleset2Id = 9008;

            var rulesets = new List<MPProductRuleSet>
            {
               FactoryGirl.NET.FactoryGirl.Build<MPProductRuleSet>( r => { r.ProductId = r.ProductId + 2; r.RulesetId = ruleset1Id; }),
               FactoryGirl.NET.FactoryGirl.Build<MPProductRuleSet>( r => { r.ProductId = r.ProductId + 3; r.RulesetId = ruleset2Id; })               
            };

            var rules1 = new List<IRule>();
            var startDate = DateTime.Today.AddDays(-30);
            rules1.Add(new GenderRule(startDate, null, 1));
            rules1.Add(new RegistrationRule(startDate, null, 1000));            

            var result1 = new MPRuleSetResult
            {
                AllRulesPass = true,
                RuleResults = new List<MPRuleResult>()
            };

            _rulesetRepository.Setup(m => m.GetRulesInRuleset(ruleset1Id)).Returns(rules1);
            _rulesetRepository.Setup(m => m.AllRulesPass(rules1, It.IsAny<Dictionary<string, object>>())).Returns(result1);
            var res = _fixture.VerifyRulesets(rulesets, new Dictionary<string, object>());
            Assert.IsTrue(res);
        }

    }


    

}
