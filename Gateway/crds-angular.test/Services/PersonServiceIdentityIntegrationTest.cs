using System;
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
using Moq.Language.Flow;
using Newtonsoft.Json.Linq;

namespace crds_angular.test.Services
{
    class PersonServiceIdentityIntegrationTest
    {
        [Test]
        public void UpdateUsernameOrPasswordIfNeeded_WhenPasswordUpdated_RequestMadeToExpectedEndpoint()
        {
            //Arrange
            string fakeAccessToken = "123";
            string fakeFullyQualifiedApiUrl = "https://fakeurl.crossroads.net/";
            string currentEmail = "test@crossroads.net";

            var mockRepository = new MockRepository(MockBehavior.Loose);
            var isolatedPersonServiceFactory = new IsolatedPersonServiceFactory(mockRepository);
            isolatedPersonServiceFactory.SetupFakeIdentityServiceUrl(fakeFullyQualifiedApiUrl);
            isolatedPersonServiceFactory.SetupFakeAuthentication(fakeAccessToken);
            isolatedPersonServiceFactory.SetupFakeUpdateUser();

            //Capture requests
            var sentRequestMessages = new List<HttpRequestMessage>();
            isolatedPersonServiceFactory.fakeHttpClientFactory.SetupSendAsync().ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK
            }).Callback((HttpRequestMessage msg, CancellationToken ct) => sentRequestMessages.Add(msg));

            IsolatedPersonService isolatedPersonService = isolatedPersonServiceFactory.Build();

            //Act
            var testPerson = new Person()
            {
                EmailAddress = currentEmail,
                OldPassword = "Pass123",
                NewPassword = "Pass1234"
            };
            isolatedPersonService.IsolatedUpdateUsernameOrPasswordIfNeeded(testPerson, fakeAccessToken);

            //Assert
            Assert.AreEqual(1, sentRequestMessages.Count);
            var passwordRequest = sentRequestMessages[0];
            Assert.AreEqual($"{fakeFullyQualifiedApiUrl}{currentEmail}/password", passwordRequest.RequestUri.ToString());
        }

        [Test]
        public void UpdateUsernameOrPasswordIfNeeded_WhenEmailUpdated_RequestMadeWithEmailUpdateData()
        {
            //Arrange
            string fakeAccessToken = "123";
            string fakeFullyQualifiedApiUrl = "https://fakeurl.crossroads.net/";
            string currentEmail = "test@crossroads.net";

            var mockRepository = new MockRepository(MockBehavior.Loose);
            var isolatedPersonServiceFactory = new IsolatedPersonServiceFactory(mockRepository);
            isolatedPersonServiceFactory.SetupFakeIdentityServiceUrl(fakeFullyQualifiedApiUrl);
            isolatedPersonServiceFactory.SetupFakeAuthentication(fakeAccessToken);
            isolatedPersonServiceFactory.SetupFakeUpdateUser();

            //Capture requests
            var sentRequestMessages = new List<HttpRequestMessage>();
            isolatedPersonServiceFactory.fakeHttpClientFactory.SetupSendAsync().ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK
            }).Callback((HttpRequestMessage msg, CancellationToken ct) => sentRequestMessages.Add(msg));

            IsolatedPersonService isolatedPersonService = isolatedPersonServiceFactory.Build();

            //Act
            var testPerson = new Person()
            {
                EmailAddress = "test2@crossroads.net",
                OldEmail = currentEmail
            };
            isolatedPersonService.IsolatedUpdateUsernameOrPasswordIfNeeded(testPerson, fakeAccessToken);

            //Assert
            Assert.AreEqual(1, sentRequestMessages.Count);
            var emailRequest = sentRequestMessages[0];
            Assert.AreEqual($"{fakeFullyQualifiedApiUrl}{currentEmail}/email", emailRequest.RequestUri.ToString());
        }

        [Test]
        public void UpdateUsernameOrPasswordIfNeeded_WhenEmailAndPasswordUpdated_EmailUpdatedBeforePassword()
        {
            //Arrange
            string fakeAccessToken = "123";
            string fakeFullyQualifiedApiUrl = "https://fakeurl.crossroads.net/";
            string currentEmail = "test@crossroads.net";
            string newEmail = "test2@crossroads.net";

            var mockRepository = new MockRepository(MockBehavior.Loose);
            var isolatedPersonServiceFactory = new IsolatedPersonServiceFactory(mockRepository);
            isolatedPersonServiceFactory.SetupFakeIdentityServiceUrl(fakeFullyQualifiedApiUrl);
            isolatedPersonServiceFactory.SetupFakeAuthentication(fakeAccessToken);
            isolatedPersonServiceFactory.SetupFakeUpdateUser();

            //Capture requests
            var sentRequestMessages = new List<HttpRequestMessage>();
            isolatedPersonServiceFactory.fakeHttpClientFactory.SetupSendAsync().ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK
            }).Callback((HttpRequestMessage msg, CancellationToken ct) => sentRequestMessages.Add(msg));

            IsolatedPersonService isolatedPersonService = isolatedPersonServiceFactory.Build();

            //Act
            var testPerson = new Person()
            {
                EmailAddress = newEmail,
                OldEmail = currentEmail,
                OldPassword = "Pass123",
                NewPassword = "Pass1234"

            };
            isolatedPersonService.IsolatedUpdateUsernameOrPasswordIfNeeded(testPerson, fakeAccessToken);

            //Assert
            Assert.AreEqual(2, sentRequestMessages.Count);

            var emailRequest = sentRequestMessages[0];
            var passwordRequest = sentRequestMessages[1];
            
            Assert.AreEqual($"{fakeFullyQualifiedApiUrl}{currentEmail}/email", emailRequest.RequestUri.ToString());
            Assert.AreEqual($"{fakeFullyQualifiedApiUrl}{newEmail}/password", passwordRequest.RequestUri.ToString());
        }
    }

    
    class IsolatedPersonServiceFactory
    {
        public Mock<MPServices.IContactRepository> fakeContactService { get; }
        public Mock<IObjectAttributeService> fakeObjectAttribute { get; }
        public Mock<IApiUserRepository> fakeApiUserService { get; }
        public Mock<MPServices.IParticipantRepository> fakeParticipantService { get; }
        public Mock<MPServices.IUserRepository> fakeUserRepository { get; }
        public Mock<IAuthenticationRepository> fakeAuthenticationService { get; }
        public Mock<IAddressService> fakeAddressService { get; }
        public Mock<IAnalyticsService> fakeAnalyticsService { get; }
        public Mock<IConfigurationWrapper> fakeConfigurationWrapper { get; }
        public FakeHttpClientFactory fakeHttpClientFactory { get; }
        
        public IsolatedPersonServiceFactory(MockRepository mockRepository)
        {
            fakeContactService = mockRepository.Create<MPServices.IContactRepository>();
            fakeObjectAttribute = mockRepository.Create<IObjectAttributeService>();
            fakeApiUserService = mockRepository.Create<IApiUserRepository>();
            fakeParticipantService = mockRepository.Create<MPServices.IParticipantRepository>();
            fakeUserRepository = mockRepository.Create<MPServices.IUserRepository>();
            fakeAuthenticationService = mockRepository.Create<IAuthenticationRepository>();
            fakeAddressService = mockRepository.Create<IAddressService>();
            fakeAnalyticsService = mockRepository.Create<IAnalyticsService>();
            fakeConfigurationWrapper = mockRepository.Create<IConfigurationWrapper>();
            fakeHttpClientFactory = new FakeHttpClientFactory(mockRepository);
        }
        
        public IsolatedPersonService Build()
        {
            return new IsolatedPersonService(
                fakeContactService.Object,
                fakeObjectAttribute.Object,
                fakeApiUserService.Object,
                fakeParticipantService.Object,
                fakeUserRepository.Object,
                fakeAuthenticationService.Object,
                fakeAddressService.Object,
                fakeAnalyticsService.Object,
                fakeConfigurationWrapper.Object,
                fakeHttpClientFactory.httpClient);
        }

        public void SetupFakeAuthentication(string fakeAccessToken)
        {
            fakeAuthenticationService.Setup(a => a.AuthenticateUser(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>())).Returns(new AuthToken() { AccessToken = fakeAccessToken });
        }

        public void SetupFakeIdentityServiceUrl(string fakeIdentityServiceUrl)
        {
            var serviceUrlEnvVar = "IDENTITY_SERVICE_URL";
            fakeConfigurationWrapper
                .Setup(cr => cr.GetEnvironmentVarAsString(serviceUrlEnvVar)).Returns(fakeIdentityServiceUrl);
        }

        public ISetup<MPServices.IUserRepository> SetupFakeUpdateUser()
        {
            return fakeUserRepository.Setup(u => u.UpdateUser(It.IsAny<Dictionary<string, object>>()));
        }
    }
}
