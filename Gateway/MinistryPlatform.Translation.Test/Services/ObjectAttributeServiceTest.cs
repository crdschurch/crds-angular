using System;
using System.Collections.Generic;
using System.Linq;
using Crossroads.Utilities.Interfaces;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Moq;
using NUnit.Framework;

namespace MinistryPlatform.Translation.Test.Services
{
    [TestFixture]
    public class ObjectAttributeServiceTest
    {
        private ObjectAttributeRepository _fixture;
        private Mock<IMinistryPlatformService> _ministryPlatformService;
        private Mock<IMinistryPlatformRestRepository> _ministryPlatformRest;
        private Mock<IAuthenticationRepository> _authService;
        private Mock<IConfigurationWrapper> _configWrapper;
        private Mock<IApiUserRepository> _apiUserService;

        [SetUp]
        public void SetUp()
        {
            _ministryPlatformService = new Mock<IMinistryPlatformService>();
            _authService = new Mock<IAuthenticationRepository>();
            _configWrapper = new Mock<IConfigurationWrapper>();
            _ministryPlatformRest = new Mock<IMinistryPlatformRestRepository>();

            _authService.Setup(m => m.AuthenticateClient(It.IsAny<string>(), It.IsAny<string>())).Returns(new AuthToken
            {
                AccessToken = "ABC",
                ExpiresIn = 123
            });
            _fixture = new ObjectAttributeRepository(_authService.Object, _configWrapper.Object, _ministryPlatformService.Object, _ministryPlatformRest.Object);
        }

        [Test]
        public void GetObjectAttributes()
        {
            const int contactId = 123456;

            //mock MpRestSearch
            var mockResponse = new List<MpObjectAttribute>
            {
                new MpObjectAttribute()
                {
                    EndDate = null,
                    Notes = "These are my notes",
                    ObjectAttributeId = 1,
                    StartDate = new DateTime(2014, 10, 10),
                    AttributeId = 2,
                    AttributeTypeId = 3,
                    AttributeTypeName = "AttributeType #1"
                },
                new MpObjectAttribute()
                {
                    ObjectAttributeId = 4,
                    StartDate = new DateTime(2015, 11, 11),
                    EndDate = null,
                    Notes = "",
                    AttributeId = 5,
                    AttributeTypeId = 6,
                    AttributeTypeName = "AttributeType #2"
                }
            };

            _ministryPlatformRest.Setup(m => m.UsingAuthenticationToken("fakeToken")).Returns(_ministryPlatformRest.Object);

            _ministryPlatformRest.Setup(m => m.SearchTable<MpObjectAttribute>("Contact_Attributes", It.IsAny<String>(), It.IsAny<String>(), (string) null, false))
                .Returns(mockResponse);



            var configuration = MpObjectAttributeConfigurationFactory.Contact();
            var attributes = _fixture.GetCurrentObjectAttributes("fakeToken", contactId, configuration, null);

            _ministryPlatformRest.VerifyAll();

            Assert.IsNotNull(attributes);
            Assert.AreEqual(2, attributes.Count());

            var attribute = attributes[0];
            Assert.AreEqual(1, attribute.ObjectAttributeId);
            Assert.AreEqual(new DateTime(2014, 10, 10), attribute.StartDate);
            Assert.AreEqual(null, attribute.EndDate);
            Assert.AreEqual("These are my notes", attribute.Notes);
            Assert.AreEqual(2, attribute.AttributeId);
            Assert.AreEqual(3, attribute.AttributeTypeId);

            attribute = attributes[1];
            Assert.AreEqual(4, attribute.ObjectAttributeId);
            Assert.AreEqual(new DateTime(2015, 11, 11), attribute.StartDate);
            Assert.AreEqual(null, attribute.EndDate);
            Assert.AreEqual(string.Empty, attribute.Notes);
            Assert.AreEqual(5, attribute.AttributeId);
            Assert.AreEqual(6, attribute.AttributeTypeId);
        }
    }
}