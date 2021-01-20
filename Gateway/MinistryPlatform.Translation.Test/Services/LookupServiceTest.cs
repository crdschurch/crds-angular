using System;
using System.Collections.Generic;
using System.Linq;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Models.Lookups;
using MinistryPlatform.Translation.Repositories;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Moq;
using NUnit.Framework;

namespace MinistryPlatform.Translation.Test.Services
{
    [TestFixture]
    class LookupServiceTest
    {
        private LookupRepository _fixture;
        private Mock<IMinistryPlatformService> _ministryPlatformService;
        private Mock<IMinistryPlatformRestRepository> _ministryPlatformRest;
        private Mock<IConfigurationWrapper> _configurationWrapper;
        private Mock<IAuthenticationRepository> _authenticationService;
        private Mock<IApiUserRepository> _apiUserService;


        [SetUp]
        public void Setup()
        {
            _ministryPlatformService = new Mock<IMinistryPlatformService>();
            _ministryPlatformRest = new Mock<IMinistryPlatformRestRepository>();
            _authenticationService = new Mock<IAuthenticationRepository>();
            _configurationWrapper = new Mock<IConfigurationWrapper>();
            _apiUserService = new Mock<IApiUserRepository>();
        _fixture = new LookupRepository(_authenticationService.Object, _configurationWrapper.Object, _ministryPlatformService.Object, _ministryPlatformRest.Object, _apiUserService.Object);

        }

        [Test]
        public void ShouldReturnWorkTeamList()
        {

            var wt = WorkTeams();

            _ministryPlatformService.Setup(m => m.GetLookupRecords(It.IsAny<int>(), It.IsAny<string>())).Returns(wt);
            var returnVal = _fixture.GetList<MpWorkTeams>();

            Assert.IsInstanceOf<IEnumerable<MpWorkTeams>>(returnVal);
            Assert.AreEqual(wt.Count, returnVal.Count());

        }

        [Test]
        public void ShouldReturnOtherOrgList()
        {

            var oo = OtherOrgs();

            _ministryPlatformService.Setup(m => m.GetLookupRecords(It.IsAny<int>(), It.IsAny<string>())).Returns(oo);
            var returnVal = _fixture.GetList<MpOtherOrganization>();

            Assert.IsInstanceOf<IEnumerable<MpOtherOrganization>>(returnVal);
            Assert.AreEqual(oo.Count, returnVal.Count());

        }

        [Test]
        public void ShouldReturnSites()
        {
            string _tokenValue = "ABC";
            _authenticationService.Setup(m => m.AuthenticateClient(It.IsAny<string>(), It.IsAny<string>())).Returns(new AuthToken
            {
                AccessToken = _tokenValue,
                ExpiresIn = 123
            });
            _fixture = new LookupRepository(_authenticationService.Object, _configurationWrapper.Object, _ministryPlatformService.Object, _ministryPlatformRest.Object, _apiUserService.Object);
            var sites = CrossroadsSites();
            _ministryPlatformService.Setup(m => m.GetLookupRecords(It.IsAny<int>(), It.IsAny<String>())).Returns(sites);
            var returnVal = _fixture.CrossroadsLocations();
            Assert.IsInstanceOf<List<Dictionary<string, object>>>(returnVal);
            Assert.AreEqual(sites.Count, returnVal.Count());

        }

        private List<Dictionary<string, object>> OtherOrgs()
        {
            return new List<Dictionary<string, object>>()
            {
                new Dictionary<string, object>()
                {
                    {"dp_RecordID", 15},
                    {"dp_RecordName", "namworkteam"}
                },
                new Dictionary<string, object>()
                {
                    {"dp_RecordID", 12},
                    {"dp_RecordName", "name or workteam"}
                }
            };
        }

        private List<Dictionary<string, object>> CrossroadsSites()
        {
            return new List<Dictionary<string, object>>()
            {
                new Dictionary<string, object>()
                {
                    {"dp_RecordID", 15},
                    {"dp_RecordName", "Anywhere"}
                },
                new Dictionary<string, object>()
                {
                    {"dp_RecordID", 7},
                    {"dp_RecordName", "Florence"}
                }
            };
        }

        private List<Dictionary<string, object>> WorkTeams()
        {
            return new List<Dictionary<string, object>>()
            {
                new Dictionary<string, object>()
                {
                    {"dp_RecordID", 15},
                    {"dp_RecordName", "namworkteam"}
                },
                new Dictionary<string, object>()
                {
                    {"dp_RecordID", 12},
                    {"dp_RecordName", "name or workteam"}
                }
            };
        }

    }
}
