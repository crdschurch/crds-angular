using System;
using System.Data.Entity.Core;
using crds_angular.Exceptions;
using crds_angular.Services;
using Crossroads.Utilities.Services;
using MinistryPlatform.Translation.Models;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Moq;
using NUnit.Framework;

namespace crds_angular.test.Services
{
    public class UserImpersonationServiceTest
    {
        private UserImpersonationService _fixture;

        private Mock<IUserRepository> _userService;
        private Mock<Func<bool>> _action;

        [SetUp]
        public void SetUp()
        {
            _userService = new Mock<IUserRepository>(MockBehavior.Strict);
            _action = new Mock<Func<bool>>(MockBehavior.Strict);

            _fixture = new UserImpersonationService(_userService.Object);
        }

        [Test]
        public void TestWithImpersonationNotAuthorized()
        {
            _userService.Setup(mocked => mocked.GetByUserId("123")).Returns(new MpUser
            {
                CanImpersonate = false
            });

            try
            {
                _fixture.WithImpersonation("123", "me@here.com", () => (_action.Object));
                Assert.Fail("Expected exception was not thrown");
            }
            catch (ImpersonationNotAllowedException e)
            {
                Assert.AreEqual("User is not authorized to impersonate other users.", e.Message);
            }
            _userService.VerifyAll();
            _action.VerifyAll();
            Assert.IsFalse(ImpersonatedUserGuid.HasValue());
        }

        [Test]
        public void TestWithImpersonationUserNotFound()
        {
            _userService.Setup(mocked => mocked.GetByUserId("123")).Returns(new MpUser
            {
                CanImpersonate = true
            });

            _userService.Setup(mocked => mocked.GetByUserId("me@here.com")).Returns((MpUser)null);

            try
            {
                _fixture.WithImpersonation("123", "me@here.com", () => (_action.Object));
                Assert.Fail("Expected exception was not thrown");
            }
            catch (ImpersonationUserNotFoundException e)
            {
                Assert.AreEqual("Could not locate user 'me@here.com' to impersonate.", e.Message);
            }
            _userService.VerifyAll();
            _action.VerifyAll();
            Assert.IsFalse(ImpersonatedUserGuid.HasValue());
        }

        [Test]
        public void TestWithImpersonation()
        {
            _userService.Setup(mocked => mocked.GetByUserId("123")).Returns(new MpUser
            {
                CanImpersonate = true
            });

            _userService.Setup(mocked => mocked.GetByUserId("me@here.com")).Returns(new MpUser
            {
                Guid = "12345"
            });

            var guid = _fixture.WithImpersonation("123", "me@here.com", () => (ImpersonatedUserGuid.Get()));
            _userService.VerifyAll();

            Assert.AreEqual("12345", guid);
            Assert.IsFalse(ImpersonatedUserGuid.HasValue());
        }
    }
}
