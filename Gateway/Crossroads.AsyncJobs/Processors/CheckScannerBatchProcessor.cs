using crds_angular.Models.Crossroads.Stewardship;
using crds_angular.Services.Interfaces;
using Crossroads.AsyncJobs.Interfaces;
using Crossroads.AsyncJobs.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using crds_angular.Models.Crossroads;
using Crossroads.Utilities.Interfaces;
using Crossroads.Web.Common;
using Crossroads.Web.Common.Configuration;
using Newtonsoft.Json;

namespace Crossroads.AsyncJobs.Processors
{
    public class CheckScannerBatchProcessor : IJobExecutor<CheckScannerBatch>
    {
        private readonly ICheckScannerService _checkScannerService;
        private readonly IEmailCommunication _emailService;
        private readonly int _checkScannerBatchSuccessTemplateId;
        private readonly int _checkScannerBatchFailureTemplateId;

        private readonly ILog _logger = LogManager.GetLogger(typeof(CheckScannerBatch));

        public CheckScannerBatchProcessor(ICheckScannerService checkScannerService, IEmailCommunication emailService, IConfigurationWrapper configuration)
        {
            _checkScannerService = checkScannerService;
            _emailService = emailService;
            _checkScannerBatchSuccessTemplateId = configuration.GetConfigIntValue("CheckScannerBatchSuccessTemplateId");
            _checkScannerBatchFailureTemplateId = configuration.GetConfigIntValue("CheckScannerBatchFailureTemplateId");
        }

        public void Execute(JobDetails<CheckScannerBatch> details)
        {
            var batch = details.Data;
            try
            {
                _logger.Info(string.Format("Received check scanner batch {0} at {1} (queued at {2})", batch.Name, details.RetrievedDateTime, details.EnqueuedDateTime));
                var result = _checkScannerService.CreateDonationsForBatch(batch);
                result.MinistryPlatformContactId = batch.MinistryPlatformContactId;
                result.MinistryPlatformUserId = batch.MinistryPlatformUserId;
                SendEmail(result, null);
            }
            catch (Exception e)
            {
                var msg = "Unexpected error processing check scanner batch " + batch.Name;
                _logger.Error(msg, e);

                SendEmail(batch, e);
            }
        }

        private void SendEmail(CheckScannerBatch batch, Exception error)
        {
            if (batch.MinistryPlatformContactId == null || batch.MinistryPlatformUserId == null)
            {
                return;
            }

            var email = new EmailCommunicationDTO
            {
                FromContactId = batch.MinistryPlatformContactId.Value,
                FromUserId = batch.MinistryPlatformUserId,
                ToContactId = batch.MinistryPlatformContactId.Value,
                TemplateId = error == null ? _checkScannerBatchSuccessTemplateId : _checkScannerBatchFailureTemplateId,
                MergeData = new Dictionary<string, object>
                    {
                        { "batchName", batch.Name },
                        { "programId", batch.ProgramId },
                    }
            };

            if (error != null)
            {
                email.MergeData["exception"] = error;
            }
            else
            {
                email.MergeData["batch"] = batch.EmailMsg;
            }


            try
            {
                _emailService.SendEmail(email);
            }
            catch(Exception e)
            {
                _logger.Error(string.Format("Could not send email {0} for batch", JsonConvert.SerializeObject(email, Formatting.Indented)), e);
            }
        }
    }
}