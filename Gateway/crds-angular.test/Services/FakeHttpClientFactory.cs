using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Language.Flow;
using Moq.Protected;

//Use this class whenever a fake httpClient is needed
namespace Crossroads.Service.Identity.Tests.Repositories
{
    public class FakeHttpClientFactory
    {
        public Mock<HttpMessageHandler> mockMessageHandler { get; }
        public HttpClient httpClient { get; }

        public FakeHttpClientFactory(MockRepository mockRepository)
        {
            //HttpClient can't be mocked directly, so mock what handles tasks
            mockMessageHandler = mockRepository.Create<HttpMessageHandler>();
            httpClient = new HttpClient(mockMessageHandler.Object)
            {
                BaseAddress = new Uri("https://int.crossroads.net")
            };
        }
        
        public ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupSendAsync()
        {
            return mockMessageHandler.Protected().Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }
    }
}