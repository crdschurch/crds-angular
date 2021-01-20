using crds_angular.Models.Json;
using NUnit.Framework;
using System.Net;
using System.Net.Http;
using System.Threading;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace crds_angular.test.Models.Json
{
    public class RestHttpActionResultTest
    {
        private JsonModel _jsonModel;
        [SetUp]
        public void SetUp()
        {
            _jsonModel = new JsonModel
            {
                S1 = "a property",
                S2 = "another property"
            };
        }

        [Test]
        public void TestOk()
        {
            var result = RestHttpActionResult<JsonModel>.Ok(_jsonModel);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.AreSame(_jsonModel, result.Content);
        }

        [Test]
        public void TestClientError()
        {
            var result = RestHttpActionResult<JsonModel>.ClientError(_jsonModel);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.AreSame(_jsonModel, result.Content);
        }

        [Test]
        public void TestServerError()
        {
            var result = RestHttpActionResult<JsonModel>.ServerError(_jsonModel);
            Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
            Assert.AreSame(_jsonModel, result.Content);
        }

        [Test]
        public void TestWithStatus()
        {
            var result = RestHttpActionResult<JsonModel>.WithStatus(HttpStatusCode.Conflict, _jsonModel);
            Assert.AreEqual(HttpStatusCode.Conflict, result.StatusCode);
            Assert.AreSame(_jsonModel, result.Content);
        }

        [Test]
        public void TestExecuteAsync()
        {
            var result = RestHttpActionResult<JsonModel>.Ok(_jsonModel);
            var task = result.ExecuteAsync(new CancellationToken());
            task.Wait();
            var response = task.Result;
            Assert.NotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var content = response.Content;
            Assert.AreEqual(MediaTypeHeaderValue.Parse("application/json"), response.Content.Headers.ContentType);
            Assert.IsInstanceOf<StringContent>(content);
            var stringContentTask = content.ReadAsStringAsync();
            stringContentTask.Wait();
            Assert.AreEqual(JsonConvert.SerializeObject(_jsonModel), stringContentTask.Result);
        }
    }

    public class JsonModel
    {
        public string S1 { get; set; }
        public string S2 { get; set; }
    }
}