using crds_angular.Controllers.API;
using crds_angular.Services.Interfaces;
using Crossroads.Utilities.Interfaces;
using Crossroads.Utilities.Messaging.Interfaces;
using Moq;
using NUnit.Framework;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Web.Http.Controllers;
using Crossroads.ClientApiKeys;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Translation.Repositories.Interfaces;

namespace crds_angular.test.controllers
{
    public class CheckScannerControllerTest
    {
        private CheckScannerController _fixture;

        private Mock<IAuthTokenExpiryService> _authTokenExpiryService;
        private Mock<IConfigurationWrapper> _configuration;
        private Mock<ICheckScannerService> _checkScannerService;
        private Mock<IAuthenticationRepository> _authenticationService;
        private Mock<IContactRepository> _contactRepository;
        private Mock<ICommunicationRepository> _communicationService;
        private Mock<IMessageQueueFactory> _messageQueueFactory;
        private Mock<IMessageFactory> _messageFactory;
        private Mock<ICryptoProvider> _cryptoProvider; 

        private const string AuthType = "auth_type";
        private const string AuthToken = "auth_token";

        [SetUp]
        public void SetUp()
        {
            _authTokenExpiryService = new Mock<IAuthTokenExpiryService>();
            _configuration = new Mock<IConfigurationWrapper>();
            _contactRepository = new Mock<IContactRepository>();
            _checkScannerService = new Mock<ICheckScannerService>(MockBehavior.Strict);
            _authenticationService = new Mock<IAuthenticationRepository>();
            _communicationService = new Mock<ICommunicationRepository>();
            _messageQueueFactory = new Mock<IMessageQueueFactory>(MockBehavior.Strict);
            _messageFactory = new Mock<IMessageFactory>(MockBehavior.Strict);
            _cryptoProvider = new Mock<ICryptoProvider>();

            _configuration.Setup(mocked => mocked.GetConfigValue("CheckScannerDonationsAsynchronousProcessingMode")).Returns("false");
            _configuration.Setup(mocked => mocked.GetConfigValue("CheckScannerDonationsQueueName")).Returns("CheckScannerBatchQueue");

            _fixture = new CheckScannerController(_authTokenExpiryService.Object, 
                                                  _configuration.Object, 
                                                  _checkScannerService.Object, 
                                                  _authenticationService.Object, 
                                                  _contactRepository.Object, 
                                                  _communicationService.Object, 
                                                  _cryptoProvider.Object, 
                                                  new Mock<IUserImpersonationService>().Object, 
                                                  _messageQueueFactory.Object, 
                                                  _messageFactory.Object);

            _fixture.Request = new HttpRequestMessage();
            _fixture.Request.Headers.Authorization = new AuthenticationHeaderValue(AuthType, AuthToken);
            _fixture.RequestContext = new HttpRequestContext();
        }

        //[Test]
        //public void TestGetOpenBatches()
        //{
        //    var batches = new List<CheckScannerBatch>
        //    {
        //        new CheckScannerBatch
        //        {
        //            Id = 1,
        //            Name = "Name 1",
        //            ScanDate = DateTime.Now,
        //            Status = BatchStatus.NotExported
        //        },
        //        new CheckScannerBatch
        //        {
        //            Id = 2,
        //            Name = "Name 2",
        //            ScanDate = DateTime.Now,
        //            Status = BatchStatus.NotExported
        //        }
        //    };

        //    _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
        //    _checkScannerService.Setup(mocked => mocked.GetBatches(true)).Returns(batches);

        //    var result = _fixture.GetBatches();
        //    _checkScannerService.VerifyAll();
        //    Assert.NotNull(result);
        //    Assert.IsInstanceOf<OkNegotiatedContentResult<List<CheckScannerBatch>>>(result);
        //    var okResult = (OkNegotiatedContentResult<List<CheckScannerBatch>>) result;
        //    Assert.IsNotNull(okResult.Content);
        //    Assert.AreSame(batches, okResult.Content);
        //}

        //[Test]
        //public void TestGetAllBatches()
        //{
        //    var batches = new List<CheckScannerBatch>
        //    {
        //        new CheckScannerBatch
        //        {
        //            Id = 1,
        //            Name = "Name 1",
        //            ScanDate = DateTime.Now,
        //            Status = BatchStatus.NotExported
        //        },
        //        new CheckScannerBatch
        //        {
        //            Id = 2,
        //            Name = "Name 2",
        //            ScanDate = DateTime.Now,
        //            Status = BatchStatus.NotExported
        //        }
        //    };

        //    _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
        //    _checkScannerService.Setup(mocked => mocked.GetBatches(false)).Returns(batches);

        //    var result = _fixture.GetBatches(false);
        //    _checkScannerService.VerifyAll();
        //    Assert.NotNull(result);
        //    Assert.IsInstanceOf<OkNegotiatedContentResult<List<CheckScannerBatch>>>(result);
        //    var okResult = (OkNegotiatedContentResult<List<CheckScannerBatch>>)result;
        //    Assert.IsNotNull(okResult.Content);
        //    Assert.AreSame(batches, okResult.Content);
        //}

        //[Test]
        //public void TestGetChecksForBatch()
        //{
        //    var checks = new List<CheckScannerCheck>
        //    {
        //        new CheckScannerCheck
        //        {
        //            AccountNumber = "123",
        //            RoutingNumber = "456",
        //            Amount = 111M
        //        },
        //        new CheckScannerCheck
        //        {
        //            AccountNumber = "789",
        //            RoutingNumber = "012",
        //            Amount = 222M
        //        },
        //    };

        //    _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
        //    _checkScannerService.Setup(mocked => mocked.GetChecksForBatch("batch123")).Returns(checks);

        //    var result = _fixture.GetChecksForBatch("batch123");
        //    _checkScannerService.VerifyAll();
        //    Assert.NotNull(result);
        //    Assert.IsInstanceOf<OkNegotiatedContentResult<List<CheckScannerCheck>>>(result);
        //    var okResult = (OkNegotiatedContentResult<List<CheckScannerCheck>>)result;
        //    Assert.IsNotNull(okResult.Content);
        //    Assert.AreSame(checks, okResult.Content);
        //}

        //[Test]
        //public void TestCreateDonationsForBatch()
        //{
        //    var batch = new CheckScannerBatch
        //    {
        //        Name = "batch123",
        //        ProgramId = 987
        //    };

        //    var batchResult = new CheckScannerBatch
        //    {
        //        Name = "batch123",
        //        ProgramId = 987,
        //    };
        //    batchResult.Checks.Add(new CheckScannerCheck
        //    {
        //        AccountNumber = "111",
        //        RoutingNumber = "222"
        //    });
        //    batchResult.Checks.Add(new CheckScannerCheck
        //    {
        //        AccountNumber = "333",
        //        RoutingNumber = "444"
        //    });

        //   _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
        //   _checkScannerService.Setup(mocked => mocked.CreateDonationsForBatch(batch)).Returns(batchResult);

        //    var result = _fixture.CreateDonationsForBatch(batch);
        //    _checkScannerService.VerifyAll();
        //    _messageFactory.VerifyAll();
        //    _messageQueueFactory.VerifyAll();
        //    Assert.NotNull(result);
        //    Assert.IsInstanceOf<OkNegotiatedContentResult<CheckScannerBatch>>(result);
        //    var okResult = (OkNegotiatedContentResult<CheckScannerBatch>)result;
        //    Assert.IsNotNull(okResult.Content);
        //    Assert.AreSame(batchResult, okResult.Content);
        //}

        //[Test]
        //public void TestGetContactDonorForCheck()
        //{
        //    const string encryptedAccountNumber = "P/H+3ccB0ZssORkd+YyJzA==";
        //    const string encryptedRoutingNumber = "TUbiKZ/Vw1l6uyGCYIIUMg==";

        //    var checkAccount = new CheckAccount
        //    {
        //        AccountNumber = encryptedAccountNumber,
        //        RoutingNumber = encryptedRoutingNumber
        //    };

        //    var donorDetail = new EZScanDonorDetails
        //    {
        //        DisplayName = "Peyton Manning",
        //        Address = new MpPostalAddress()
        //        {
        //            Line1 = "1 Superbowl Way",
        //            Line2 = "Suite 1000",
        //            City = "Denver",
        //            State = "CO",
        //            PostalCode = "11111-2222"
        //          }
        //    };

        //    _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
        //    _checkScannerService.Setup(mocked => mocked.GetContactDonorForCheck(encryptedAccountNumber, encryptedRoutingNumber)).Returns(donorDetail);

        //    var result = _fixture.GetDonorForCheck(checkAccount);
        //    _checkScannerService.VerifyAll();
            
        //    Assert.NotNull(result);
       
        //    Assert.IsInstanceOf<OkNegotiatedContentResult<EZScanDonorDetails>>(result);
        //    var okResult = (OkNegotiatedContentResult<EZScanDonorDetails>)result;
        //    Assert.IsNotNull(okResult.Content);
        //    Assert.AreSame(donorDetail, okResult.Content);
        //}

        //[Test]
        //public void TestGetDonorForCheckUnauthenticated()
        //{
        //    _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
        //    _contactRepository.Setup(mocked => mocked.GetContactId(It.IsAny<string>())).Throws<Exception>();
        //    var result = _fixture.GetDonorForCheck(new CheckAccount
        //    {
        //        AccountNumber = "123",
        //        RoutingNumber = "456"
        //    });

        //    _authenticationService.VerifyAll();
        //    _checkScannerService.VerifyAll();

        //    Assert.IsNotNull(result);
        //    Assert.IsInstanceOf<RestHttpActionResult<ApiErrorDto>>(result);
        //    var restResult = (RestHttpActionResult<ApiErrorDto>) result;
        //    Assert.AreEqual(HttpStatusCode.Unauthorized, restResult.StatusCode);
        //    Assert.AreEqual("Could not authenticate to MinistryPlatform", restResult.Content.Message);
        //}

        //[Test]
        //public void TestCreateDonorUnauthenticated()
        //{
        //    _authTokenExpiryService.Setup(a => a.IsAuthtokenCloseToExpiry(It.IsAny<HttpRequestHeaders>())).Returns(true);
        //    _contactRepository.Setup(mocked => mocked.GetContactId(It.IsAny<string>())).Throws<Exception>();
        //    var result = _fixture.CreateDonor(new CheckScannerCheck());

        //    _authenticationService.VerifyAll();
        //    _checkScannerService.VerifyAll();

        //    Assert.IsNotNull(result);
        //    Assert.IsInstanceOf<RestHttpActionResult<ApiErrorDto>>(result);
        //    var restResult = (RestHttpActionResult<ApiErrorDto>)result;
        //    Assert.AreEqual(HttpStatusCode.Unauthorized, restResult.StatusCode);
        //    Assert.AreEqual("Could not authenticate to MinistryPlatform", restResult.Content.Message);
        //}

        //[Test]
        //public void TestCreateDonorStripeFails()
        //{
        //    _contactRepository.Setup(mocked => mocked.GetContactId(It.IsAny<string>())).Returns(1);

        //    _checkScannerService.Setup(mocked => mocked.CreateDonor(It.IsAny<CheckScannerCheck>()))
        //        .Throws(new PaymentProcessorException(HttpStatusCode.BadGateway, "aux message", "type", "message", "code", "decline code", "param"));

        //    var result = _fixture.CreateDonor(new CheckScannerCheck());
        //    _authenticationService.VerifyAll();
        //    _checkScannerService.VerifyAll();

        //    Assert.IsNotNull(result);
        //    Assert.IsInstanceOf<RestHttpActionResult<ApiErrorDto>>(result);
        //    var restResult = (RestHttpActionResult<ApiErrorDto>)result;
        //    Assert.AreEqual(HttpStatusCode.MethodNotAllowed, restResult.StatusCode);
        //    Assert.AreEqual("Could not create checking account at payment processor", restResult.Content.Message);
        //}

        [Test]
        public void TestControllerIgnoresClientApiKeys()
        {
            Assert.IsNotNull(_fixture.GetType().GetCustomAttribute<IgnoreClientApiKeyAttribute>(), $"{_fixture.GetType().FullName} should specify the IgnoreClientApiKey Attribute at the class level");
        }

    }
}
