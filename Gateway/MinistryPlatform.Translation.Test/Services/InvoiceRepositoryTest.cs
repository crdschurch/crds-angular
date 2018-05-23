using System.Collections.Generic;
using Crossroads.Web.Common.Configuration;
using Crossroads.Web.Common.MinistryPlatform;
using MinistryPlatform.Translation.Models.Payments;
using MinistryPlatform.Translation.Repositories;
using MinistryPlatform.Translation.Repositories.Interfaces;
using Moq;
using NUnit.Framework;

namespace MinistryPlatform.Translation.Test.Services
{
  [TestFixture]
  public class InvoiceRepositoryTest
  {
    private Mock<IMinistryPlatformRestRepository> _ministryPlatformRest;
    private Mock<IApiUserRepository> _apiUserRepository;
    private Mock<IProductRepository> _productRepository;
    private Mock<IConfigurationWrapper> _configurationWrapper;
    private IInvoiceRepository _fixture;

    [SetUp]
    public void SetUp()
    {
      _ministryPlatformRest = new Mock<IMinistryPlatformRestRepository>();
      _apiUserRepository = new Mock<IApiUserRepository>();
      _productRepository = new Mock<IProductRepository>();
      _configurationWrapper = new Mock<IConfigurationWrapper>();

      _configurationWrapper.Setup(m => m.GetConfigIntValue("InvoiceCancelled")).Returns(4);

      _fixture = new InvoiceRepository(_ministryPlatformRest.Object, _apiUserRepository.Object, _productRepository.Object, _configurationWrapper.Object);
    }

    [TearDown]
    public void Teardown()
    {
      _ministryPlatformRest.VerifyAll();
      _apiUserRepository.VerifyAll();
      _productRepository.VerifyAll();
      _configurationWrapper.VerifyAll();
    }

    [Test]
    public void CancelledInvoiceShouldNotShowAsExists()
    {
      const int invoiceId = 123445;
      const string token = "letmein";
      var filter = $"Invoice_ID={invoiceId} AND Invoice_Status_ID!={4}";

      _apiUserRepository.Setup(m => m.GetDefaultApiClientToken()).Returns(token);
      _ministryPlatformRest.Setup(m => m.UsingAuthenticationToken(token)).Returns(_ministryPlatformRest.Object);
      _ministryPlatformRest.Setup(m => m.Search<MpInvoice>(filter, null as string, null, false)).Returns(new List<MpInvoice>());

      var result = _fixture.InvoiceExists(invoiceId);
      Assert.IsFalse(result);
    }

    [Test]
    public void ShouldGetInvoiceForCamperAndProductAndEvent()
    {
      const int camperId = 12333;
      const int productId = 3455;
      const int eventId = 1111;
      const int expectedInvoiceId = 192837;
      const string token = "letmein";
      var invoiceDetail = InvoiceDetail(productId, camperId, expectedInvoiceId);

      _apiUserRepository.Setup(m => m.GetDefaultApiClientToken()).Returns(token);
      _ministryPlatformRest.Setup(m => m.UsingAuthenticationToken(token)).Returns(_ministryPlatformRest.Object);
      _ministryPlatformRest.Setup(m => m.Search<MpInvoiceDetail>(
                                    $"Recipient_Contact_ID_Table.[Contact_ID]={camperId} AND Product_ID_Table.[Product_ID]={productId} AND Event_Participant_ID_Table_Event_ID_Table.[Event_ID]={eventId} AND Invoice_Status_ID!=4",
                                    "Invoice_ID_Table.[Invoice_ID]",
                                    null,
                                    false)).Returns(new List<MpInvoiceDetail>() {invoiceDetail});
      var result = _fixture.GetInvoiceDetailsForProductAndCamper(productId, camperId, eventId);
      Assert.IsTrue(result.Status);
      Assert.AreEqual(expectedInvoiceId, result.Value.InvoiceId);
    }

    [Test]
    public void ShouldFailIfMultipleInvoicesForCamperAndProductAndEvent()
    {
      const int camperId = 12333;
      const int productId = 3455;
      const int eventId = 1111;
      const int expectedInvoiceId = 192837;
      const string token = "letmein";
      var invoiceDetail = InvoiceDetail(productId, camperId, expectedInvoiceId);

      _apiUserRepository.Setup(m => m.GetDefaultApiClientToken()).Returns(token);
      _ministryPlatformRest.Setup(m => m.UsingAuthenticationToken(token)).Returns(_ministryPlatformRest.Object);
      _ministryPlatformRest.Setup(m => m.Search<MpInvoiceDetail>(
                                    $"Recipient_Contact_ID_Table.[Contact_ID]={camperId} AND Product_ID_Table.[Product_ID]={productId} AND Event_Participant_ID_Table_Event_ID_Table.[Event_ID]={eventId} AND Invoice_Status_ID!=4",
                                    "Invoice_ID_Table.[Invoice_ID]",
                                    null,
                                    false)).Returns(new List<MpInvoiceDetail>() {invoiceDetail, invoiceDetail});
      var result = _fixture.GetInvoiceDetailsForProductAndCamper(productId, camperId, eventId);
      Assert.IsFalse(result.Status);
      Assert.AreEqual($"Found multiple invoices for camper: {camperId}, product: {productId} and event: {eventId}", result.ErrorMessage);
    }

    [Test]
    public void ShouldFailIfNoInvoicesForCamperAndProductAndEvent()
    {
      const int camperId = 12333;
      const int productId = 3455;
      const int eventId = 1111;
      const string token = "letmein";

      _apiUserRepository.Setup(m => m.GetDefaultApiClientToken()).Returns(token);
      _ministryPlatformRest.Setup(m => m.UsingAuthenticationToken(token)).Returns(_ministryPlatformRest.Object);
      _ministryPlatformRest.Setup(m => m.Search<MpInvoiceDetail>(
                                    $"Recipient_Contact_ID_Table.[Contact_ID]={camperId} AND Product_ID_Table.[Product_ID]={productId} AND Event_Participant_ID_Table_Event_ID_Table.[Event_ID]={eventId} AND Invoice_Status_ID!=4",
                                    "Invoice_ID_Table.[Invoice_ID]",
                                    null,
                                    false)).Returns(new List<MpInvoiceDetail>());
      var result = _fixture.GetInvoiceDetailsForProductAndCamper(productId, camperId, eventId);
      Assert.IsFalse(result.Status);
      Assert.AreEqual($"No invoice details for camper: {camperId}, product: {productId} and event: {eventId}", result.ErrorMessage);
    }

    private static MpInvoiceDetail InvoiceDetail(int productId, int camperId, int invoiceId = 90)
    {
      return new MpInvoiceDetail
      {
        EventParticipantId = 1234,
        InvoiceDetailId = 111,
        InvoiceId = invoiceId,
        InvoiceStatusId = 2,
        ProductId = productId,
        RecipientContactId = camperId
      };
    }
  }
}