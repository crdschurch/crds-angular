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

        private const int OnlineGivingProgramsPageViewId = 1038;
        private const int ProgramsPageId = 375;


        [SetUp]
        public void SetUp()
        {
            _ministryPlatformService = new Mock<IMinistryPlatformService>();
            _ministryPlatformRest = new Mock<IMinistryPlatformRestRepository>();
            _authService = new Mock<IAuthenticationRepository>();
            _configWrapper = new Mock<IConfigurationWrapper>();

            _configWrapper.Setup(m => m.GetEnvironmentVarAsString("API_USER")).Returns("uid");
            _configWrapper.Setup(m => m.GetEnvironmentVarAsString("API_PASSWORD")).Returns("pwd");
            _configWrapper.Setup(m => m.GetConfigIntValue("OnlineGivingProgramsPageViewId")).Returns(OnlineGivingProgramsPageViewId);
            _configWrapper.Setup(m => m.GetConfigIntValue("Programs")).Returns(ProgramsPageId);
            _authService.Setup(m => m.AuthenticateUser(It.IsAny<string>(), It.IsAny<string>())).Returns(new AuthToken
            {
                AccessToken = "ABC",
                ExpiresIn = 123
            });

            AutoMapperConfig.RegisterMappings();

            _fixture = new ProgramRepository(_ministryPlatformService.Object, _authService.Object, _configWrapper.Object, _ministryPlatformRest.Object);
        }

        [Test]
        public void TestGetOnlineGivingPrograms()
        {
            var getPageViewRecordsResponse = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>()
                {
                    {"Program_Name", "Test Program Name"},
                    {"Program_ID", 20},
                    {"Communication_ID", "1234"},
                    {"Program_Type_ID", 4},
                    {"Allow_Recurring_Giving", false}
                },
                new Dictionary<string, object>()
                {
                    {"Program_Name", "Test Fund Name"},
                    {"Program_ID", 22},
                    {"Communication_ID", "1234"},
                    {"Program_Type_ID", 4},
                    {"Allow_Recurring_Giving", false}
                }
            };

            _ministryPlatformService.Setup(
                mocked => mocked.GetPageViewRecords(OnlineGivingProgramsPageViewId, It.IsAny<string>(), ",,,1", "", 0)).Returns(getPageViewRecordsResponse);

            var programs = _fixture.GetOnlineGivingPrograms(1);

            _ministryPlatformService.VerifyAll();
            Assert.IsNotNull(programs);

            Assert.AreEqual("Test Program Name", programs[0].Name);
            Assert.AreEqual(22, programs[1].ProgramId);
        }

        [Test]
        public void TestGetOnlineGivingProgramsNullResponse()
        {
            _ministryPlatformService.Setup(
                mocked => mocked.GetPageViewRecords(OnlineGivingProgramsPageViewId, It.IsAny<string>(), ",,,1", "", 0)).Returns((List<Dictionary<string, object>>) null);

            var programs = _fixture.GetOnlineGivingPrograms(1);

            _ministryPlatformService.VerifyAll();
            Assert.IsNotNull(programs);
            Assert.AreEqual(0, programs.Count);
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