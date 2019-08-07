using crds_angular.Services;
using Crossroads.Web.Common.Configuration;
using log4net;
using Moq;
using NUnit.Framework;
using System;

namespace crds_angular.test.Services
{
    [TestFixture]
    [Category("IntegrationTests")]
    public class TwilioServiceTest
    {
        private TwilioService _fixture;
        private Mock<IConfigurationWrapper> _configurationMock;
        private Mock<ILog> _mockLogger;

        [SetUp]
        public void Setup()
        {
            _configurationMock = new Mock<IConfigurationWrapper>();
            _configurationMock.Setup(mock => mock.GetConfigValue("TwilioAccountSid")).Returns("AC051651a7abfd7ec5209ad22273a24390");
            _configurationMock.Setup(mock => mock.GetConfigValue("TwilioFromPhoneNumber")).Returns("+15005550006");
            Environment.SetEnvironmentVariable("TWILIO_AUTH_TOKEN", "TWILIO_TEST_AUTH_TOKEN");
            _fixture = new TwilioService(_configurationMock.Object);
            _mockLogger = new Mock<ILog>();
            _fixture.SetLogger(_mockLogger.Object);
        }

        [Test]
        public void TestTextSucceeds()
        {
            _mockLogger.Setup(mock => mock.Error(It.IsAny<string>())).Verifiable();
            _fixture.SendTextMessage("+15005550006", "Hi");
            _mockLogger.Verify(mock => mock.Error(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void TestTextFails()
        {
            _mockLogger.Setup(mock => mock.Error(It.IsAny<string>())).Verifiable();
            _fixture.SendTextMessage("+15005550001", "Hi");
            _mockLogger.Verify(mock => mock.Error(It.IsAny<string>()), Times.Once);
        }
    }
}
