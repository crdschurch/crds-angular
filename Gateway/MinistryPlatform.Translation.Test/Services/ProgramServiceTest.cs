using System.Collections.Generic;
using crds_angular.App_Start;
using Crossroads.Utilities.Interfaces;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Repositories;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Moq;
using NUnit.Framework;

namespace MinistryPlatform.Translation.Test.Services
{
    [TestFixture]
    public class ProgramServiceTest
    {
        private ProgramRepository _fixture;
        private Mock<IMinistryPlatformService> _ministryPlatformService;
        private Mock<IMinistryPlatformRestRepository> _ministryPlatformRest;
        private Mock<IAuthenticationRepository> _authService;
        private Mock<IConfigurationWrapper> _configWrapper;
        private Mock<IApiUserRepository> _apiUserService;

        private const int OnlineGivingProgramsPageViewId = 1038;
        private const int ProgramsPageId = 375;


        [SetUp]
        public void SetUp()
        {
            _ministryPlatformService = new Mock<IMinistryPlatformService>();
            _ministryPlatformRest = new Mock<IMinistryPlatformRestRepository>();
            _authService = new Mock<IAuthenticationRepository>();
            _configWrapper = new Mock<IConfigurationWrapper>();
            _apiUserService = new Mock<IApiUserRepository>();

            _configWrapper.Setup(m => m.GetEnvironmentVarAsString("API_USER")).Returns("uid");
            _configWrapper.Setup(m => m.GetEnvironmentVarAsString("API_PASSWORD")).Returns("pwd");
            _configWrapper.Setup(m => m.GetConfigIntValue("OnlineGivingProgramsPageViewId")).Returns(OnlineGivingProgramsPageViewId);
            _configWrapper.Setup(m => m.GetConfigIntValue("Programs")).Returns(ProgramsPageId);
            _authService.Setup(m => m.AuthenticateClient(It.IsAny<string>(), It.IsAny<string>())).Returns(new AuthToken
            {
                AccessToken = "ABC",
                ExpiresIn = 123
            });

            AutoMapperConfig.RegisterMappings();

            _fixture = new ProgramRepository(_ministryPlatformService.Object, _authService.Object, _configWrapper.Object, _ministryPlatformRest.Object, _apiUserService.Object);
        }


        [Test]
        public void TestGetProgram()
        {
            var getRecordResponse = new Dictionary<string, object>()
            {
                {"Communication_ID", "1234"},
                {"Program_Type_ID", 4},
                {"Program_ID", 3},
                {"Program_Name", "TEst Name"},
                {"Allow_Recurring_Giving", false}
            };

            const int programId = 3;

            _ministryPlatformService.Setup(
                mocked => mocked.GetRecordDict(ProgramsPageId, programId, It.IsAny<string>(), false)).Returns(getRecordResponse);

            var program = _fixture.GetProgramById(programId);

            _ministryPlatformService.VerifyAll();
            Assert.IsNotNull(program);
            Assert.AreEqual(1234, program.CommunicationTemplateId);
        }

        [Test]
        public void TestGetProgramWithNullEmailTemplate()
        {
            var getRecordResponse = new Dictionary<string, object>()
            {
                {"Communication_ID", null},
                {"Program_Type_ID", 4},
                {"Program_ID", 3},
                {"Program_Name", "TEst Name"},
                {"Allow_Recurring_Giving", false}
            };

            const int programId = 3;

            _ministryPlatformService.Setup(
                mocked => mocked.GetRecordDict(ProgramsPageId, programId, It.IsAny<string>(), false)).Returns(getRecordResponse);

            var program = _fixture.GetProgramById(programId);

            _ministryPlatformService.VerifyAll();
            Assert.IsNotNull(program);
            Assert.AreEqual(null, program.CommunicationTemplateId);
        }

    }
}