using System.Net;
using Moq;
using NUnit.Framework;
using RestSharp;
using System;
using Crossroads.Web.Common.Extensions;

namespace MinistryPlatform.Translation.Test.Extensions
{
    public class RestSharpExtensionsTest
    {
        private Mock<IRestResponse> _restResponse;

        [SetUp]
        public void SetUp()
        {
            _restResponse = new Mock<IRestResponse>();
        }

        [Test]
        public void TestIsErrorGoodResponse()
        {
            _restResponse.SetupGet(mocked => mocked.ResponseStatus).Returns(ResponseStatus.Completed).Verifiable();
            _restResponse.SetupGet(mocked => mocked.StatusCode).Returns(HttpStatusCode.OK).Verifiable();

            Assert.IsFalse(_restResponse.Object.IsError());
            _restResponse.VerifyAll();
        }

        [Test]
        public void TestIsErrorResponseNotComplete()
        {
            _restResponse.SetupGet(mocked => mocked.ResponseStatus).Returns(ResponseStatus.Aborted).Verifiable();

            Assert.IsTrue(_restResponse.Object.IsError());
            _restResponse.VerifyAll();
        }

        [Test]
        public void TestIsErrorNotFoundNotError()
        {
            _restResponse.SetupGet(mocked => mocked.ResponseStatus).Returns(ResponseStatus.Completed).Verifiable();
            _restResponse.SetupGet(mocked => mocked.StatusCode).Returns(HttpStatusCode.NotFound).Verifiable();

            Assert.IsFalse(_restResponse.Object.IsError());
            _restResponse.VerifyAll();
        }

        [Test]
        public void TestIsErrorNotFoundError()
        {
            _restResponse.SetupGet(mocked => mocked.ResponseStatus).Returns(ResponseStatus.Completed).Verifiable();
            _restResponse.SetupGet(mocked => mocked.StatusCode).Returns(HttpStatusCode.NotFound).Verifiable();

            Assert.IsTrue(_restResponse.Object.IsError(true));
            _restResponse.VerifyAll();
        }

        [Test]
        public void TestIsErrorLessThan200()
        {
            _restResponse.SetupGet(mocked => mocked.ResponseStatus).Returns(ResponseStatus.Completed).Verifiable();
            _restResponse.SetupGet(mocked => mocked.StatusCode).Returns((HttpStatusCode)199).Verifiable();

            Assert.IsTrue(_restResponse.Object.IsError());
            _restResponse.VerifyAll();
        }

        [Test]
        public void TestIsErrorGreaterThan399()
        {
            _restResponse.SetupGet(mocked => mocked.ResponseStatus).Returns(ResponseStatus.Completed).Verifiable();
            _restResponse.SetupGet(mocked => mocked.StatusCode).Returns((HttpStatusCode)400).Verifiable();

            Assert.IsTrue(_restResponse.Object.IsError());
            _restResponse.VerifyAll();
        }

        [Test]
        public void TestCheckForErrorsGoodResponse()
        {
            _restResponse.SetupGet(mocked => mocked.ResponseStatus).Returns(ResponseStatus.Completed).Verifiable();
            _restResponse.SetupGet(mocked => mocked.StatusCode).Returns(HttpStatusCode.OK).Verifiable();

            _restResponse.Object.CheckForErrors("Okey Dokey");
            _restResponse.VerifyAll();
        }

        [Test]
        public void TestCheckForErrorsNotFoundNotError()
        {
            _restResponse.SetupGet(mocked => mocked.ResponseStatus).Returns(ResponseStatus.Completed).Verifiable();
            _restResponse.SetupGet(mocked => mocked.StatusCode).Returns(HttpStatusCode.NotFound).Verifiable();

            _restResponse.Object.CheckForErrors("Oh No!");
            _restResponse.VerifyAll();
        }

        [Test]
        public void TestCheckForErrorsNotFoundError()
        {
            const string errorMessage = "error message";
            const string content = "response content";
            var ex = new Exception("Doh!!");

            _restResponse.SetupGet(mocked => mocked.ResponseStatus).Returns(ResponseStatus.Completed).Verifiable();
            _restResponse.SetupGet(mocked => mocked.StatusCode).Returns(HttpStatusCode.NotFound).Verifiable();
            _restResponse.SetupGet(mocked => mocked.ErrorMessage).Returns(errorMessage).Verifiable();
            _restResponse.SetupGet(mocked => mocked.Content).Returns(content).Verifiable();
            _restResponse.SetupGet(mocked => mocked.ErrorException).Returns(ex).Verifiable();

            try
            {
                _restResponse.Object.CheckForErrors("Oh No!", true);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (RestResponseException e)
            {
                _restResponse.VerifyAll();
                Assert.AreSame(ex, e.InnerException);
                Assert.IsTrue(e.Message.Contains(errorMessage));
                Assert.IsTrue(e.Message.Contains(content));
                Assert.IsTrue(e.Message.Contains(HttpStatusCode.NotFound.ToString()));
                Assert.IsTrue(e.Message.Contains(ResponseStatus.Completed.ToString()));
            }
        }

        [Test]
        public void TestCheckForErrorsErrorResponse()
        {
            const string errorMessage = "error message";
            const string content = "response content";
            var ex = new Exception("Doh!!");

            _restResponse.SetupGet(mocked => mocked.ResponseStatus).Returns(ResponseStatus.Completed).Verifiable();
            _restResponse.SetupGet(mocked => mocked.StatusCode).Returns(HttpStatusCode.Forbidden).Verifiable();
            _restResponse.SetupGet(mocked => mocked.ErrorMessage).Returns(errorMessage).Verifiable();
            _restResponse.SetupGet(mocked => mocked.Content).Returns(content).Verifiable();
            _restResponse.SetupGet(mocked => mocked.ErrorException).Returns(ex).Verifiable();

            try
            {
                _restResponse.Object.CheckForErrors("Oh No!");
                Assert.Fail("Expected exception was not thrown");
            }
            catch (RestResponseException e)
            {
                _restResponse.VerifyAll();
                Assert.AreSame(ex, e.InnerException);
                Assert.IsTrue(e.Message.Contains(errorMessage));
                Assert.IsTrue(e.Message.Contains(content));
                Assert.IsTrue(e.Message.Contains(HttpStatusCode.Forbidden.ToString()));
                Assert.IsTrue(e.Message.Contains(ResponseStatus.Completed.ToString()));
            }
        }
    }
}
