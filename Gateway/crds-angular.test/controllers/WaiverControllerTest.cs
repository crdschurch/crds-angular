using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Results;
using crds_angular.Controllers.API;
using crds_angular.Models.Crossroads.Waivers;
using crds_angular.Services.Interfaces;
using Crossroads.Web.Common.Security;
using Moq;
using NUnit.Framework;

namespace crds_angular.test.controllers
{

    [TestFixture]
    public class WaiverControllerTest
    {
        private Mock<IAuthTokenExpiryService> _authTokenExpiryService;
        private readonly Mock<IWaiverService> _waiverService;
        private readonly Mock<IUserImpersonationService> _userImpersonationService;
        private readonly Mock<IAuthenticationRepository> _authenticationService;
        private readonly WaiverController _fixture;

        private const string authToken = "authtoken";
        private const string authType = "authtype";


        public WaiverControllerTest()
        {
            _authTokenExpiryService = new Mock<IAuthTokenExpiryService>();
            _waiverService = new Mock<IWaiverService>();
            _userImpersonationService = new Mock<IUserImpersonationService>();
            _authenticationService = new Mock<IAuthenticationRepository>();
            _fixture = new WaiverController(_authTokenExpiryService.Object, 
                                            _waiverService.Object, 
                                            _userImpersonationService.Object, 
                                            _authenticationService.Object)
            {
                Request = new HttpRequestMessage(),
                RequestContext = new HttpRequestContext()
            };
            _fixture.Request.Headers.Authorization = new AuthenticationHeaderValue(authType, authToken);
            ;
        }

        public void Teardown()
        {
            _waiverService.VerifyAll();
            _userImpersonationService.VerifyAll();
            _authenticationService.VerifyAll();
        }

        [Test]
        public async Task ShouldGetEventWaivers()
        {
            const int eventId = 769;

            var waiverDto1 = new WaiverDTO
            {
                WaiverId = 23,
                WaiverName = "You won't read it anyways",
                WaiverText = "You agree to give me everything you own",            
            };

            var waiverDto2 = new WaiverDTO
            {
                WaiverId = 24,
                WaiverName = "You may read it this one",
                WaiverText = "You agree to give some of your stuff"
            };

            _waiverService.Setup(m => m.EventWaivers(eventId, $"{authType} {authToken}")).Returns(Observable.Create<WaiverDTO>(observer =>
            {
                observer.OnNext(waiverDto1);
                observer.OnNext(waiverDto2);
                observer.OnCompleted();
                return Disposable.Empty;
            }));
            var response = await _fixture.GetEventWaivers(eventId);
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<OkNegotiatedContentResult<IList<WaiverDTO>>>(response);
            var r = (OkNegotiatedContentResult<IList<WaiverDTO>>) response;
            Assert.IsNotNull(r.Content);
            Assert.AreEqual(r.Content.Count, 2);
        }

        [Test]
        public void ShouldHandleEventWaiverFailure()
        {
            const int eventId = 769;
            var waiverDto1 = new WaiverDTO
            {
                WaiverId = 23,
                WaiverName = "You won't read it anyways",
                WaiverText = "You agree to give me everything you own"
            };

            var waiverDto2 = new WaiverDTO
            {
                WaiverId = 24,
                WaiverName = "You may read it this one",
                WaiverText = "You agree to give some of your stuff"
            };

            _waiverService.Setup(m => m.EventWaivers(eventId, $"{authType} {authToken}")).Returns(Observable.Create<WaiverDTO>(observer =>
            {
                observer.OnNext(waiverDto1);
                observer.OnNext(waiverDto2);
                observer.OnError(new Exception("Something bad happened"));
                return Disposable.Empty;
            }));


            Assert.Throws<HttpResponseException>(async () =>
            {
                await _fixture.GetEventWaivers(eventId);
            });       
        }

        [Test]
        public async Task ShouldGetWaiver()
        {
            const int waiverId = 23;

            var waiverDto1 = new WaiverDTO
            {
                WaiverId = 23,
                WaiverName = "You won't read it anyways",
                WaiverText = "You agree to give me everything you own"
            };

            _waiverService.Setup(m => m.EventWaivers(waiverId, $"{authType} {authToken}")).Returns(Observable.Create<WaiverDTO>(observer =>
            {
                observer.OnNext(waiverDto1);
                observer.OnCompleted();
                return Disposable.Empty;
            }));
            var response = await _fixture.GetEventWaivers(waiverId);
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<OkNegotiatedContentResult<IList<WaiverDTO>>>(response);
            var r = (OkNegotiatedContentResult<IList<WaiverDTO>>)response;
            Assert.IsNotNull(r.Content);
        }

        [Test]
        public void ShouldHandleGetWaiverFailure()
        {
            const int waiverId = 23;
            
            _waiverService.Setup(m => m.GetWaiver(waiverId)).Returns(Observable.Create<WaiverDTO>(observer =>
            {                
                observer.OnError(new Exception("Something bad happened"));
                return Disposable.Empty;
            }));


            Assert.Throws<HttpResponseException>(async () =>
            {
                await _fixture.GetWaiver(waiverId);
            });
        }


    }
}