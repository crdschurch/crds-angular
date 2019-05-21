using System;
using System.Messaging;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using crds_angular.Exceptions;
using crds_angular.Exceptions.Models;
using crds_angular.Models.Crossroads.Stewardship;
using crds_angular.Models.Json;
using crds_angular.Security;
using crds_angular.Services.Interfaces;
using Crossroads.Utilities.Interfaces;
using Crossroads.Utilities.Messaging.Interfaces;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Crossroads.ApiVersioning;
using Crossroads.ClientApiKeys;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.Security;

namespace crds_angular.Controllers.API
{
    // TODO - Once Ez-Scan has been updated to send a client API key (US7764), remove the IgnoreClientApiKey attribute
    [IgnoreClientApiKey]
    public class CheckScannerController : ImpersonateAuthBaseController
    {
        private readonly bool _asynchronous;
        private readonly IContactRepository _contactRepository;
        private readonly ICheckScannerService _checkScannerService;
        private readonly ICommunicationRepository _communicationService;
        private readonly MessageQueue _donationsQueue;
        private readonly IMessageFactory _messageFactory;
        private readonly ICryptoProvider _cryptoProvider;

        private readonly int _crossroadsFinanceClerkContactId;

        public CheckScannerController(IAuthTokenExpiryService authTokenExpiryService,
                                      IConfigurationWrapper configuration,
                                      ICheckScannerService checkScannerService,
                                      IAuthenticationRepository authenticationService,
                                      IContactRepository contactRepository,
                                      ICommunicationRepository communicationService,
                                      ICryptoProvider cryptoProvider,
                                      IUserImpersonationService userImpersonationService,
                                      IMessageQueueFactory messageQueueFactory = null,
                                      IMessageFactory messageFactory = null) 
                                      : base(authTokenExpiryService, userImpersonationService, authenticationService)
        {
            _checkScannerService = checkScannerService;
            _contactRepository = contactRepository;
            _communicationService = communicationService;
            _cryptoProvider = cryptoProvider;

            var b = configuration.GetConfigValue("CheckScannerDonationsAsynchronousProcessingMode");
            _asynchronous = b != null && bool.Parse(b);

            var id = configuration.GetConfigValue("CrossroadsFinanceClerkContactId");
            _crossroadsFinanceClerkContactId = (id == null ? -1 : int.Parse(id));

            if (_asynchronous)
            {
                var donationsQueueName = configuration.GetConfigValue("CheckScannerDonationsQueueName");
                // ReSharper disable once PossibleNullReferenceException
                _donationsQueue = messageQueueFactory.CreateQueue(donationsQueueName, QueueAccessMode.Send);
                _messageFactory = messageFactory;
            }
        }

        [RequiresAuthorization]
        [VersionedRoute(template: "check-scanner/batches", minimumVersion: "1.0.0")]
        [Route("checkscanner/batches")]
        public IHttpActionResult GetBatches([FromUri(Name = "onlyOpen")] bool onlyOpen = true)
        {
            return (Authorized(authDto =>
            {
                var batches = _checkScannerService.GetBatches(onlyOpen);
                return (Ok(batches));
            }));
        }

        [RequiresAuthorization]
        [VersionedRoute(template: "check-scanner/batches/{batchName}/checks", minimumVersion: "1.0.0")]
        [Route("checkscanner/batches/{batchName}/checks")]
        public IHttpActionResult GetChecksForBatch(string batchName)
        {
            return (Authorized(authDto =>
            {
                var checks = _checkScannerService.GetChecksForBatch(batchName);
                return (Ok(checks));
            }));
        }

        [RequiresAuthorization]
        [VersionedRoute(template: "check-scanner/batches", minimumVersion: "1.0.0")]
        [Route("checkscanner/batches")]
        [HttpPost]
        public IHttpActionResult CreateDonationsForBatch([FromBody] CheckScannerBatch batch)
        {
            return (Authorized(authDto =>
            {
                if (!_asynchronous)
                {
                    return (Ok(_checkScannerService.CreateDonationsForBatch(batch)));
                }

                // US6745 - Only finance person receives email instead of the user who imports the batch
                if (_crossroadsFinanceClerkContactId > 0)
                {
                    batch.MinistryPlatformContactId = _crossroadsFinanceClerkContactId;
                    batch.MinistryPlatformUserId = _communicationService.GetUserIdFromContactId(batch.MinistryPlatformContactId.Value);

                    var message = _messageFactory.CreateMessage(batch);

                    _donationsQueue.Send(message);
                }

                return (Ok(batch));
            }));
        }
        /// <summary>
        /// Takes in the encrypted account and routing number which is then used to locate an existing donor.
        /// If an existing donor is found, then the address data is returned.
        /// If an existing donor is not found, then a 404 will be returned
        /// </summary>
        /// <param name="checkAccount">This is the encrypted account and routing number from EZ Scan.</param>
        /// <returns>The created or updated donor record.</returns>
        [RequiresAuthorization]
        [ResponseType(typeof(EZScanDonorDetails))]
        [VersionedRoute(template: "check-scanner/get-donor", minimumVersion: "1.0.0")]
        [Route("checkscanner/getdonor")]
        [HttpPost]
        public IHttpActionResult GetDonorForCheck([FromBody] CheckAccount checkAccount)
        {
            return (Authorized(authDto =>
            {
                var donorDetail = _checkScannerService.GetContactDonorForCheck(checkAccount.AccountNumber, checkAccount.RoutingNumber);
                if (donorDetail == null)
                {
                    return NotFound();
                }
                return (Ok(donorDetail));
            }));
        }

        /// <summary>
        /// Creates a donor record in Ministry Platform based off of the check details passed in.
        /// </summary>
        /// <param name="checkDetails" type="FromBody">The Check Details from the check that was scanned.</param>
        /// <returns>The created donor record.</returns>
        [RequiresAuthorization]
        [ResponseType(typeof(MpContactDonor))]
        [VersionedRoute(template: "check-scanner/donor", minimumVersion: "1.0.0")]
        [Route("checkscanner/donor")]
        [HttpPost]
        public IHttpActionResult CreateDonor([FromBody] CheckScannerCheck checkDetails)
        {
            return (Authorized(authDto =>
            {
                try
                {
                    var result = _checkScannerService.CreateDonor(checkDetails);
                    return (Ok(result));
                }
                catch (PaymentProcessorException e)
                {
                    return (RestHttpActionResult<ApiErrorDto>.WithStatus(HttpStatusCode.MethodNotAllowed, new ApiErrorDto("Could not create checking account at payment processor", e)));
                }
            }));
        }

        //the following was created for testing purposes only
        //QA needed the ability encrypt account and routing numbers for testing
        [RequiresAuthorization]
        [ResponseType(typeof(EncryptValue))]
        [VersionedRoute(template: "check-scanner/encrypt/{*value}", minimumVersion: "1.0.0")]
        [Route("checkscanner/encrypt/{*value}")]
        public IHttpActionResult GetEncrypted(string value = "")
        {
            return (Authorized(authDto =>
            {
                var encryptValue = _cryptoProvider.EncryptValueToString(value);
                return (Ok(encryptValue));
            }));

        }
    }
}
