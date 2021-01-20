using System;
using System.Collections.Generic;
using Crossroads.Utilities.FunctionalHelpers;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories;
using MinistryPlatform.Translation.Repositories.Interfaces;
using MinistryPlatform.Translation.Test.Helpers;
using Moq;
using NUnit.Framework;

namespace MinistryPlatform.Translation.Test.Services
{
    [TestFixture]
    public class CongregationRepositoryTest
    {

        private Mock<IMinistryPlatformService> _ministryPlatformService;
        private Mock<IMinistryPlatformRestRepository> _ministryPlatformRest;        
        private Mock<IAuthenticationRepository> _authRepository;
        private Mock<IConfigurationWrapper> _configWrapper;
        private ICongregationRepository _fixture;
        private Mock<IApiUserRepository> _apiUserService;

        [SetUp]
        public void Setup()
        {
            Factories.MpCongregation();

            _ministryPlatformService = new Mock<IMinistryPlatformService>();
            _ministryPlatformRest = new Mock<IMinistryPlatformRestRepository>();
            _authRepository = new Mock<IAuthenticationRepository>();
            _configWrapper = new Mock<IConfigurationWrapper>();
            _apiUserService = new Mock<IApiUserRepository>();
            _fixture = new CongregationRepository(_ministryPlatformService.Object, _ministryPlatformRest.Object, _authRepository.Object, _configWrapper.Object, _apiUserService.Object);
        }

        [TearDown]
        public void Teardown()
        {
            _ministryPlatformService.VerifyAll();
            _ministryPlatformRest.VerifyAll();
            _authRepository.VerifyAll();
            _configWrapper.VerifyAll();
        }

        [Test]
        public void ShouldGetCongregationByName()
        {
            const string congregationName = "Oakley";
            const string token = "token";
            var congregation = FactoryGirl.NET.FactoryGirl.Build<MpCongregation>(c => c.Name = "Oakley");
            var congregationList = new List<MpCongregation>
            {
                congregation
            };

            _ministryPlatformRest.Setup(m => m.UsingAuthenticationToken(token)).Returns(_ministryPlatformRest.Object);
            _ministryPlatformRest.Setup(m => m.Search<MpCongregation>(It.IsAny<String>(), null as string, null, false)).Returns(congregationList);

            var result = _fixture.GetCongregationByName(congregationName, token);
            Assert.IsInstanceOf<Ok<MpCongregation>>(result);
            Assert.AreEqual(congregation.CongregationId, result.Value.CongregationId);
        }

        [Test]
        public void ShouldNotFindCongregationByName()
        {
            const string congregationName = "Oakley";
            const string token = "token";
            var congregationList = new List<MpCongregation>();            

            _ministryPlatformRest.Setup(m => m.UsingAuthenticationToken(token)).Returns(_ministryPlatformRest.Object);
            _ministryPlatformRest.Setup(m => m.Search<MpCongregation>(It.IsAny<String>(), null as string, null, false)).Returns(congregationList);

            var result = _fixture.GetCongregationByName(congregationName, token);
            Assert.IsInstanceOf<Err<MpCongregation>>(result);
            Assert.IsNotEmpty(result.ErrorMessage);            
        }
    }
}
