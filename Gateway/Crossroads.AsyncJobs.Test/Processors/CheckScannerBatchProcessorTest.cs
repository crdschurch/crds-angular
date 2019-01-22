using System;
using crds_angular.Models.Crossroads;
using crds_angular.Models.Crossroads.Stewardship;
using crds_angular.Services.Interfaces;
using Crossroads.AsyncJobs.Models;
using Crossroads.AsyncJobs.Processors;
using Crossroads.Utilities.Interfaces;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Configuration;
using Moq;
using NUnit.Framework;

namespace Crossroads.AsyncJobs.Test.Processors
{
    public class CheckScannerBatchProcessorTest
    {
        private CheckScannerBatchProcessor _fixture;
        private Mock<ICheckScannerService> _checkScannerService;
        private Mock<IEmailCommunication> _emailService;
        private Mock<IConfigurationWrapper> _configuration;

        [SetUp]
        public void SetUp()
        {
            _checkScannerService = new Mock<ICheckScannerService>();
            _emailService = new Mock<IEmailCommunication>();
            _configuration = new Mock<IConfigurationWrapper>();
            _configuration.Setup(mocked => mocked.GetConfigIntValue("CheckScannerBatchSuccessTemplateId")).Returns(123);
            _configuration.Setup(mocked => mocked.GetConfigIntValue("CheckScannerBatchFailureTemplateId")).Returns(456);

            _fixture = new CheckScannerBatchProcessor(_checkScannerService.Object, _emailService.Object, _configuration.Object);
        }

        [Test]
        public void TestExecuteSuccess()
        {
            var details = new JobDetails<CheckScannerBatch>
            {
                Data = new CheckScannerBatch
                {
                    Name = "batch123",
                    ProgramId = 111,
                    MinistryPlatformContactId = 222,
                    MinistryPlatformUserId = 333
                },
                EnqueuedDateTime = DateTime.Now,
                RetrievedDateTime = DateTime.Now.AddMinutes(1)
            };

            var batchResult = new CheckScannerBatch();
            _checkScannerService.Setup(mocked => mocked.CreateDonationsForBatch(details.Data)).Returns(batchResult);
            _emailService.Setup(
                mocked => mocked.SendEmail(
                    It.Is<EmailCommunicationDTO>(
                        o =>
                            o.FromUserId == 333 && o.FromContactId == 222 && o.TemplateId == 123 && o.ToContactId == 222 && (int) o.MergeData["programId"] == 111 &&
                            o.MergeData["batchName"].Equals("batch123") && o.MergeData.ContainsKey("batch")
                        )));

            _fixture.Execute(details);
        }

        [Test]
        public void TestExecuteFailure()
        {
            var details = new JobDetails<CheckScannerBatch>
            {
                Data = new CheckScannerBatch
                {
                    Name = "batch123",
                    ProgramId = 111,
                    MinistryPlatformContactId = 222,
                    MinistryPlatformUserId = 333
                },
                EnqueuedDateTime = DateTime.Now,
                RetrievedDateTime = DateTime.Now.AddMinutes(1)
            };

            _checkScannerService.Setup(mocked => mocked.CreateDonationsForBatch(details.Data)).Throws(new Exception());
            _emailService.Setup(
                mocked => mocked.SendEmail(
                    It.Is<EmailCommunicationDTO>(
                        o =>
                            o.FromUserId == 333 && o.FromContactId == 222 && o.TemplateId == 456 && o.ToContactId == 222 && (int)o.MergeData["programId"] == 111 &&
                            o.MergeData["batchName"].Equals("batch123") && o.MergeData.ContainsKey("error")
                        )));

            _fixture.Execute(details);
        }
    }
}
