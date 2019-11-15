using crds_angular.Models.Crossroads.Profile;
using Moq;
using NUnit.Framework;
using MPServices = MinistryPlatform.Translation.Repositories.Interfaces;
using crds_angular.Services.Interfaces;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using Crossroads.Service.Identity.Tests.Repositories;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;

namespace crds_angular.test.Services
{
    class PersonServiceIdentityIntegrationTest
    {
        [Test]
        public void UpdateUsernameOrPasswordIfNeeded_WhenGivenValidPersonWithUNUpdate_RunCleanly()
        {
            MockRepository mockRepository = new MockRepository(MockBehavior.Loose);
            Mock<MPServices.IContactRepository> mockContactService = mockRepository.Create<MPServices.IContactRepository>();
            Mock<IObjectAttributeService> mockObjectAttribute = mockRepository.Create<IObjectAttributeService>();
            Mock<IApiUserRepository> mockApiUserService = mockRepository.Create<IApiUserRepository>();
            Mock<MPServices.IParticipantRepository> mockParticipantService = mockRepository.Create<MPServices.IParticipantRepository>();
            Mock<MPServices.IUserRepository> mockUserService = mockRepository.Create<MPServices.IUserRepository>();
            Mock<IAuthenticationRepository> mockAuthenticationService = mockRepository.Create<IAuthenticationRepository>();
            Mock<IAddressService> mockAddressService = mockRepository.Create<IAddressService>();
            Mock<IAnalyticsService> mockAnalyticsService = mockRepository.Create<IAnalyticsService>();
            Mock<IConfigurationWrapper> mockConfigurationWrapper = mockRepository.Create<IConfigurationWrapper>();               
            mockAuthenticationService.Setup(a => a.AuthenticateUser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(new AuthToken(){ AccessToken = "123"});
            mockUserService.Setup(u => u.UpdateUser(It.IsAny<Dictionary<string, object>>()));
            FakeHttpClientFactory httpClientFactory = new FakeHttpClientFactory(mockRepository);
            List<HttpRequestMessage> sentRequestMessages = new List<HttpRequestMessage>();
            httpClientFactory.SetupSendAsync().ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK
            }).Callback((HttpRequestMessage msg, CancellationToken ct) => sentRequestMessages.Add(msg));
            IsolatedPersonService mockIsolatedPersonService = new IsolatedPersonService(mockContactService.Object, mockObjectAttribute.Object, mockApiUserService.Object, mockParticipantService.Object, mockUserService.Object, mockAuthenticationService.Object, mockAddressService.Object, mockAnalyticsService.Object, mockConfigurationWrapper.Object, httpClientFactory.httpClient);
            var testPerson = new Person()
            {
                EmailAddress = "test2@crossroads.net",
                OldEmail = "test@crossroads.net",
                OldPassword = "Pass123",
                NewPassword = "Pass1234"
            };

            mockIsolatedPersonService._UpdateUsernameOrPasswordIfNeeded(testPerson, "1234");
        }

    }
}
