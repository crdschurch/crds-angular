using System;
using System.Collections.Generic;
using Crossroads.Web.Common;
using Crossroads.Web.Common.MinistryPlatform;
using MinistryPlatform.Translation.Models;
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
            _apiUserRepository.Setup(a => a.GetDefaultApiClientToken()).Returns(token);

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
            _apiUserRepository.Setup(a => a.GetDefaultApiClientToken()).Returns(token);

            var response = _fixture.CreatePaymentAndDetail(paymentDetail);
            Assert.AreEqual(false, response.Status);
        }

        [Test]
        public void CreatePaymentBatchShouldThrow()
        {
            const string batchName = "name";
            var setupDate = DateTime.Now;

            var batchRecord = new MpBatch
            {
                BatchName = batchName,
                SetupDate = setupDate,
                BatchTotal = (decimal)123.45,
                ItemCount = 2,
                BatchEntryTypeId = 1,
                DepositId = 777,
                FinalizeDate = setupDate,
                ProcessorTransferId = "philiscool"
            };

            _ministryPlatformRest.Setup(p => p.PostWithReturn<MpBatch, MpBatch>(new List<MpBatch>() { batchRecord })).Returns(new List<MpBatch>());
            
            Assert.Throws<ApplicationException>(() => _fixture.CreatePaymentBatch(batchName, setupDate, (decimal) 123.45, 2, 1, 777, DateTime.Now, "philiscool"));
        }

        [Test]
        public void AddPaymentToBatchShouldThrow()
        {
            var paymentId = 1;
            var batchId = 2;
            var parms = new Dictionary<string, object>
            {
                {"Payment_ID", paymentId},
                {"Batch_ID", batchId}
            };
            var parmList = new List<Dictionary<string, object>> { parms };
            var e = new Exception("bad things happened");
            
            _ministryPlatformRest.Setup(p => p.Put("Payments", parmList)).Throws(e);

            Assert.Throws<ApplicationException> (() => _fixture.AddPaymentToBatch(batchId,paymentId));     
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

