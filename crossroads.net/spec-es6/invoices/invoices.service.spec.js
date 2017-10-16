import invoicesModule from '../../app/invoices/invoices.module';

describe('Invoice Service', () => {
  // eslint-disable-next-line no-underscore-dangle
  const endpoint = `${window.__env__.CRDS_GATEWAY_CLIENT_ENDPOINT}api`;
  let invoicesService;
  let httpBackend;
  const invoiceId = 111;
  const paymentId = 222;

  beforeEach(angular.mock.module(invoicesModule));

  beforeEach(inject((_InvoicesService_, _$httpBackend_) => {
    invoicesService = _InvoicesService_;
    httpBackend = _$httpBackend_;
  }));

  it('should make the API call to get invoice', () => {
    const invoiceId = 111;
    httpBackend.expectGET(`${endpoint}/v1.0.0/invoice/${invoiceId}/details`)
      .respond(200, {});
    invoicesService.getInvoiceDetails(invoiceId);
  });

  it('should make the API call to get invoice payment details', () => {
    httpBackend.expectGET(`${endpoint}/v1.0.0/invoice/${invoiceId}/payments`)
      .respond(200, {});
    invoicesService.getPaymentDetails(invoiceId);
  });

  it('should make the API call to send payment email confirmation', () => {
    httpBackend.expectPOST(`${endpoint}/v1.0.0/invoice/${invoiceId}/payments/${paymentId}/confirmation`)
      .respond(200, {});
    invoicesService.sendPaymentConfirmation(invoiceId, paymentId);
  });

});
