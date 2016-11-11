﻿using System;
using System.Collections.Generic;
using MinistryPlatform.Translation.Models.Payments;
using MinistryPlatform.Translation.Repositories;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Moq;
using NUnit.Framework;

namespace MinistryPlatform.Translation.Test.Services
{
    [TestFixture]
    public class PaymentRepositoryTest
    {
        private readonly Mock<IMinistryPlatformRestRepository> _ministryPlatformRest;
        private readonly Mock<IApiUserRepository> _apiUserRepository;
        private readonly IPaymentRepository _fixture;

        private const string token = "some garbage token";

        public PaymentRepositoryTest()
        {
            _ministryPlatformRest = new Mock<IMinistryPlatformRestRepository>();
            _apiUserRepository = new Mock<IApiUserRepository>();
            _ministryPlatformRest.Setup(m => m.UsingAuthenticationToken(token)).Returns(_ministryPlatformRest.Object);
            _fixture = new PaymentRepository(_ministryPlatformRest.Object, _apiUserRepository.Object);
        }

        [Test]
        public void ShouldCreateAPaymentAndDetail()
        {
            var paymentDetail = FakePaymentInfo();

            var paymentList = new List<MpPaymentDetail> { paymentDetail };
            _ministryPlatformRest.Setup(p => p.PostWithReturn<MpPaymentDetail, MpPaymentDetailReturn>(paymentList)).Returns(
                new List<MpPaymentDetailReturn> {
                    new MpPaymentDetailReturn()
                    {
                        InvoiceDetailId = paymentDetail.InvoiceDetailId,
                        PaymentId = 1234,
                        PaymentAmount = paymentDetail.PaymentAmount
                    }
                });
            _apiUserRepository.Setup(a => a.GetToken()).Returns(token);

            var response = _fixture.CreatePaymentAndDetail(paymentDetail);
            Assert.AreEqual(true, response.Status);
        }

        [Test]
        public void ShouldNotCreateAPaymentAndDetail()
        {
            var paymentDetail = FakePaymentInfo();

            var paymentList = new List<MpPaymentDetail> { paymentDetail };
            _ministryPlatformRest.Setup(p => p.PostWithReturn<MpPaymentDetail, MpPaymentDetailReturn>(paymentList)).Returns(
                new List<MpPaymentDetailReturn>());
            _apiUserRepository.Setup(a => a.GetToken()).Returns(token);

            var response = _fixture.CreatePaymentAndDetail(paymentDetail);
            Assert.AreEqual(false, response.Status);
        }

        

        private static MpPaymentDetail FakePaymentInfo()
        {
            var payment = new MpPayment
            {
                PaymentTotal = 123.45M,
                ContactId = 3717387,
                PaymentDate = DateTime.Now,
                PaymentTypeId = 11
            };

            var paymentDetail = new MpPaymentDetail
            {
                Payment = payment,
                PaymentAmount = 123.45M,
                InvoiceDetailId = 19
            };
            return paymentDetail;
        }
    }
}

