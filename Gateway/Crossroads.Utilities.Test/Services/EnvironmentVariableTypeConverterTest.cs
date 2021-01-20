using Moq;
using NUnit.Framework;
using System.ComponentModel;
using System.Globalization;
using Crossroads.Web.Common.Configuration;

namespace Crossroads.Utilities.Test.Services
{
    class EnvironmentVariableTypeConverterTest
    {
        private EnvironmentVariableTypeConverter fixture;
        private Mock<IConfigurationWrapper> configurationWrapper;
        private Mock<ITypeDescriptorContext> typeDescriptorContext;
        private CultureInfo cultureInfo;

        [SetUp]
        public void SetUp()
        {
            configurationWrapper = new Mock<IConfigurationWrapper>();
            fixture = new EnvironmentVariableTypeConverter(configurationWrapper.Object);

            typeDescriptorContext = new Mock<ITypeDescriptorContext>();
            cultureInfo = CultureInfo.CurrentCulture;
        }

        [Test]
        public void testCanConvertFrom()
        {
            Assert.IsTrue(fixture.CanConvertFrom(typeof(string)));
            Assert.IsFalse(fixture.CanConvertFrom(typeof(int)));
        }

        [Test]
        public void testConvertFrom()
        {
            configurationWrapper.Setup(mocked => mocked.GetEnvironmentVarAsString("EnvVarToGet")).Returns("TheValue");

            var val = fixture.ConvertFrom(typeDescriptorContext.Object, cultureInfo, "EnvVarToGet");
            configurationWrapper.VerifyAll();

            Assert.IsNotNull(val);
            Assert.AreEqual("TheValue", val);
        }



    }
}
