using System;
using System.Net;
using System.Reflection;
using System.Web.Http.Results;
using crds_angular.Controllers.API;
using crds_angular.Models.Crossroads.Stewardship;
using crds_angular.Models.Json;
using crds_angular.Services;
using crds_angular.Services.Interfaces;
using Crossroads.ClientApiKeys;
using Crossroads.Utilities.Messaging.Interfaces;
using Crossroads.Web.Common.Configuration;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace crds_angular.test.controllers
{
    public class StripeEventControllerTest
    {
        private StripeEventController _fixture;
        private Mock<IStripeEventService> _stripeEventService;
        private Mock<IMessageQueueFactory> _messageQueueFactory;
        private Mock<IMessageFactory> _messageFactory;

        [SetUp]
        public void SetUp()
        {
            var configuration = new Mock<IConfigurationWrapper>();
            configuration.Setup(mocked => mocked.GetConfigValue("StripeWebhookLiveMode")).Returns("true");
            configuration.Setup(mocked => mocked.GetConfigValue("StripeWebhookAsynchronousProcessingMode")).Returns("false");
            configuration.Setup(mocked => mocked.GetConfigValue("StripeWebhookEventQueueName")).Returns("queue name");

            _stripeEventService = new Mock<IStripeEventService>();
            _messageFactory = new Mock<IMessageFactory>();
            _messageQueueFactory = new Mock<IMessageQueueFactory>();

            _fixture = new StripeEventController(configuration.Object, _stripeEventService.Object, _messageQueueFactory.Object, _messageFactory.Object);
        }

        [Test]
        public void TestProcessStripeEventNullEvent()
        {
            var result = _fixture.ProcessStripeEvent(null);
            Assert.IsInstanceOf<InvalidModelStateResult>(result);
            _stripeEventService.VerifyAll();
            _messageQueueFactory.VerifyAll();
            _messageFactory.VerifyAll();
        }

        [Test]
        public void TestProcessStripeEventInvalidModelState()
        {
            _fixture.ModelState.AddModelError("Data", new Exception("invalid"));
            var result = _fixture.ProcessStripeEvent(new StripeEvent());
            Assert.IsInstanceOf<InvalidModelStateResult>(result);
            _stripeEventService.VerifyAll();
            _messageQueueFactory.VerifyAll();
            _messageFactory.VerifyAll();
        }

        [Test]
        public void TestProcessStripeEventLiveModeMismatch()
        {
            var e = new StripeEvent
            {
                LiveMode = false
            };

            var result = _fixture.ProcessStripeEvent(e);
            Assert.IsInstanceOf<OkResult>(result);
            _stripeEventService.VerifyAll();
            _messageQueueFactory.VerifyAll();
            _messageFactory.VerifyAll();
        }

        [Test]
        public void TestProcessEventSynchronously()
        {
            var e = new StripeEvent
            {
                LiveMode = true,
                Type = "charge.succeeded",
                Created = DateTime.Now.AddDays(-1),
                Data = new StripeEventData
                {
                    Object = JObject.FromObject(new StripeCharge
                    {
                        Id = "9876"
                    })
                }
            };

            var expectedResponse = new StripeEventResponseDTO();
            _stripeEventService.Setup(mocked => mocked.ProcessStripeEvent(e)).Returns(expectedResponse);
            var result = _fixture.ProcessStripeEvent(e);
            Assert.IsInstanceOf<RestHttpActionResult<StripeEventResponseDTO>>(result);
            Assert.AreEqual(HttpStatusCode.OK, ((RestHttpActionResult<StripeEventResponseDTO>)result).StatusCode);
            var dto = ((RestHttpActionResult<StripeEventResponseDTO>) result).Content;
            Assert.IsNotNull(dto);
            Assert.AreSame(expectedResponse, dto);
            _stripeEventService.VerifyAll();
            _messageQueueFactory.VerifyAll();
            _messageFactory.VerifyAll();

        }

        [Test]
        public void TestControllerIgnoresClientApiKeys()
        {
            Assert.IsNotNull(_fixture.GetType().GetCustomAttribute<IgnoreClientApiKeyAttribute>(), $"{_fixture.GetType().FullName} should specify the IgnoreClientApiKey Attribute at the class level");
        }

        // This test unfortunately does not work, as you cannot mock Message and MessageQueue.
        // A possible refactor to wrap those into our own interfaces would allow for more thorough testing
        //
        //[Test]
        //public void TestProcessEventAsynchronously()
        //{
        //    var configuration = new Mock<IConfigurationWrapper>();
        //    configuration.Setup(mocked => mocked.GetConfigValue("StripeWebhookLiveMode")).Returns("true");
        //    configuration.Setup(mocked => mocked.GetConfigValue("StripeWebhookAsynchronousProcessingMode")).Returns("true");
        //    configuration.Setup(mocked => mocked.GetConfigValue("StripeWebhookEventQueueName")).Returns("queue name");
        //
        //    var mq = new Mock<MessageQueue>();
        //
        //    _messageQueueFactory.Setup(mocked => mocked.CreateQueue("queue name", QueueAccessMode.Send, null))
        //        .Returns(mq.Object);
        //
        //    _fixture = new StripeEventController(configuration.Object, _stripeEventService.Object, _messageQueueFactory.Object, _messageFactory.Object);
        //
        //    var e = new StripeEvent
        //    {
        //        LiveMode = true,
        //        Type = "charge.succeeded",
        //        Created = DateTime.Now.AddDays(-1),
        //        Data = new StripeEventData
        //        {
        //            Object = JObject.FromObject(new StripeCharge
        //            {
        //                Id = "9876"
        //            })
        //        }
        //    };
        //
        //    var msg = new Mock<Message>();
        //    _messageFactory.Setup(mocked => mocked.CreateMessage(e, null)).Returns(msg.Object);
        //    mq.Setup(mocked => mocked.Send(msg.Object, MessageQueueTransactionType.None));
        //
        //    var result = _fixture.ProcessStripeEvent(e);
        //    Assert.IsInstanceOf<RestHttpActionResult<StripeEventResponseDTO>>(result);
        //    Assert.AreEqual(HttpStatusCode.OK, ((RestHttpActionResult<StripeEventResponseDTO>)result).StatusCode);
        //    var dto = ((RestHttpActionResult<StripeEventResponseDTO>)result).Content;
        //    Assert.IsNotNull(dto);
        //    Assert.AreEqual("Queued event for asynchronous processing", dto.Message);
        //    _stripeEventService.VerifyAll();
        //    _messageQueueFactory.VerifyAll();
        //    _messageFactory.VerifyAll();
        //}

    }


}
