using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Results;
using crds_angular.Controllers.API;
using crds_angular.Models.Crossroads.Serve;
using crds_angular.Services.Interfaces;
using Crossroads.Utilities.Messaging.Interfaces;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.Security;
using Moq;
using NUnit.Framework;

namespace crds_angular.test.controllers
{
    [TestFixture]
    public class ServeControllerTest
    {

        private ServeController _fixture;

        //private Mock<IPersonService> _personServiceMock;
        private Mock<IAuthTokenExpiryService> _authTokenExpiryService;
        private Mock<IServeService> _serveServiceMock;
        private Mock<IAuthenticationRepository> _authenticationServiceMock;
        private Mock<IMessageFactory> _messageFactoryMock;
        private Mock<IMessageQueueFactory> _messageQueryFactoryMock;
        private Mock<IConfigurationWrapper> _configurationMock;

        private string _authType;
        private string _authToken;

        [SetUp]
        public void SetUp()
        {
            //_personServiceMock = new Mock<IPersonService>();
            _authTokenExpiryService = new Mock<IAuthTokenExpiryService>();
            _serveServiceMock = new Mock<IServeService>();
            _authenticationServiceMock= new Mock<IAuthenticationRepository>();
            _messageFactoryMock = new Mock<IMessageFactory>();
            _messageQueryFactoryMock = new Mock<IMessageQueueFactory>();
            _configurationMock = new Mock<IConfigurationWrapper>();

            _fixture = new ServeController(_authTokenExpiryService.Object, 
                                           _serveServiceMock.Object, 
                                           _configurationMock.Object, 
                                           _messageFactoryMock.Object, 
                                           _messageQueryFactoryMock.Object, 
                                           new Mock<IUserImpersonationService>().Object, 
                                           new Mock<IAuthenticationRepository>().Object);

            _authType = "auth_type";
            _authToken = "auth_token";
            _fixture.Request = new HttpRequestMessage();
            _fixture.Request.Headers.Authorization = new AuthenticationHeaderValue(_authType, _authToken);
            _fixture.RequestContext = new HttpRequestContext();
        }

        [Test]
        public void GetFamilyServeSignUpTest()
        {
            const int contactId = 123456;
            var servingDays = SetUpServingDays();

            _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
            _serveServiceMock.Setup(mocked => mocked.GetServingDays(It.IsAny<string>(),contactId, It.IsAny<long>(), It.IsAny<long>()))
                .Returns(servingDays);

            IHttpActionResult result = _fixture.GetFamilyServeDays(contactId);

            Assert.NotNull(result);
            Assert.IsInstanceOf<OkNegotiatedContentResult<List<ServingDay>>>(result);
        }

        private List<ServingDay> SetUpServingDays()
        {
            var servingTeams = SetUpServingTeams();
            var servingDays = new List<ServingDay>
            {
                new ServingDay
                {
                    Day = "abc",
                    ServeTimes =
                    {
                        new ServingTime {Time = "08:30:00", ServingTeams = servingTeams},
                        new ServingTime {Time = "10:00:00", ServingTeams = servingTeams}
                    }
                }
            };

            return servingDays;
        }

        private List<ServingTeam> SetUpServingTeams()
        {
            var servingTeams = new List<ServingTeam>
            {
                new ServingTeam
                {
                    GroupId = 1,
                    Name = "team-1",
                    PrimaryContact = "me@example.com",
                    Members = new List<TeamMember>
                    {
                        new TeamMember
                        {
                            ContactId = 1,
                            Name = "memeber-1",
                            Roles =
                                new List<ServeRole>
                                {
                                    new ServeRole {Name = "Leader" },
                                    new ServeRole {Name = "Member"}
                                }
                        },
                        new TeamMember
                        {
                            ContactId = 2,
                            Name = "memeber-2",
                            Roles =
                                new List<ServeRole>
                                {
                                    new ServeRole { Name = "Admin"},
                                    new ServeRole { Name = "Member"}
                                }
                        }
                    }
                },
                new ServingTeam
                {
                    GroupId = 2,
                    Name = "team-2",
                    PrimaryContact = "me2@aol.com",
                    Members = new List<TeamMember>
                    {
                        new TeamMember
                        {
                            ContactId = 3,
                            Name = "memeber-3",
                            Roles =
                                new List<ServeRole>
                                {
                                    new ServeRole { Name = "Leader"},
                                    new ServeRole { Name = "Member"}
                                }
                        },
                        new TeamMember
                        {
                            ContactId = 2,
                            Name = "memeber-2",
                            Roles =
                                new List<ServeRole>
                                {
                                    new ServeRole { Name = "Admin"},
                                    new ServeRole { Name = "Member"}
                                }
                        }
                    }
                }
            };
            return servingTeams;
        }
    }
}
