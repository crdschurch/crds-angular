using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Results;
using crds_angular.App_Start;
using NUnit.Framework;
using crds_angular.Controllers.API;
using crds_angular.Models.Crossroads;
using Moq;
using crds_angular.Services.Interfaces;
using Crossroads.Web.Common.Security;

namespace crds_angular.test.controllers
{
    [TestFixture]
    public class ProgramControllerTest

    {
        private ProgramController _fixture;

          private Mock<IAuthTokenExpiryService> _authTokenExpiryService;
          private Mock<IProgramService> _programServiceMock;


        [SetUp]
        public void SetUp()
        {
            _authTokenExpiryService = new Mock<IAuthTokenExpiryService>();
            _programServiceMock = new Mock<IProgramService>();
            _fixture = new ProgramController(_authTokenExpiryService.Object, 
                                             _programServiceMock.Object, 
                                             new Mock<IUserImpersonationService>().Object, 
                                             new Mock<IAuthenticationRepository>().Object)
            {
                Request = new HttpRequestMessage(),
                RequestContext = new HttpRequestContext()
            };

            AutoMapperConfig.RegisterMappings();
        }

        [Test]
        public void TestGetProgramsByType()
        {
            int programType = 1;
            var programList = new List<ProgramDTO>();
            ProgramDTO program = new ProgramDTO
            {
                Name = "Test Fund",
                ProgramId = 1
            };
            programList.Add(program);

            _programServiceMock.Setup(mocked => mocked.GetOnlineGivingPrograms(programType)).Returns(programList);

            var httpResult = _fixture.GetProgramsByType(programType);
            var result = (OkNegotiatedContentResult<List<ProgramDTO>>) httpResult;
            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(OkNegotiatedContentResult<List<ProgramDTO>>), result);
            _programServiceMock.VerifyAll();

            Assert.AreEqual(result.Content[0].Name, program.Name);
            Assert.AreEqual(result.Content[0].ProgramId, program.ProgramId);
        }

        [Test]
        public void TestGetAllPrograms()
        {
            var programList = new List<ProgramDTO>
            {
                new ProgramDTO
                {
                    Name = "Program 1",
                    ProgramId = 1,
                    ProgramType = 1
                },
                new ProgramDTO
                {
                    Name = "Program 3",
                    ProgramId = 3,
                    ProgramType = 3
                },
                new ProgramDTO
                {
                    Name = "Program 2",
                    ProgramId = 2,
                    ProgramType = 2
                }
            };
            _programServiceMock.Setup(mocked => mocked.GetOnlineGivingPrograms(null)).Returns(programList);

            var httpResult = _fixture.GetAllPrograms(new[] {1, 2});
            _programServiceMock.VerifyAll();

            Assert.IsNotNull(httpResult);
            Assert.IsInstanceOf(typeof(OkNegotiatedContentResult<List<ProgramDTO>>), httpResult);
            var result = (OkNegotiatedContentResult<List<ProgramDTO>>)httpResult;
            Assert.IsNotNull(result.Content);
            Assert.AreEqual(1, result.Content.Count);
            Assert.AreEqual(result.Content[0].Name, "Program 3");
            Assert.AreEqual(result.Content[0].ProgramId, 3);
        }
    }
}
