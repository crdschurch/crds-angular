/* eslint-disable import/no-extraneous-dependencies, import/no-unresolved, import/extensions */
import constants from 'crds-constants';

/* ngInject */
class InvoicesService {
  constructor($resource, $rootScope, $stateParams, $log, AttributeTypeService, $sessionStorage) {
    this.log = $log;
    this.scope = $rootScope;
    this.stateParams = $stateParams;
    this.resource = $resource;
    this.invoiceDetailsResource = $resource(`${__GATEWAY_CLIENT_ENDPOINT__}api/v1.0.0/invoice/:invoiceId/details`);
    this.invoicesPaymentsResource = $resource(`${__GATEWAY_CLIENT_ENDPOINT__}api/v1.0.0/invoice/:invoiceId/payments`);
    this.invoicesPaymentConfirmationResource = $resource(`${__GATEWAY_CLIENT_ENDPOINT__}api/v1.0.0/invoice/:invoiceId/payment/:paymentId/confirmation`, { invoiceId: '@invoiceId', paymentId: '@paymentId' });
    this.invoiceDetails = {};
    this.invoicePayments = {};
  }

  getInvoiceDetails(invoiceId) {
    return this.invoiceDetailsResource.get({ invoiceId }, (invoiceDetails) => {
      this.invoiceDetails = invoiceDetails;
    }, (err) => {
      this.log.error(err);
    }).$promise;
  }

  getPaymentDetails(invoiceId) {
    return this.invoicesPaymentsResource.get({ invoiceId }, (invoicePayments) => {
      this.invoicePayments = invoicePayments;
    }, (err) => {
      this.log.error(err);
    }).$promise;
  }

  sendPaymentConfirmation(invoiceId, paymentId) {
    return this.invoicesPaymentConfirmationResource.save({ invoiceId, paymentId }, (invoicePayments) => {
    }, (err) => {
      this.log.error(err);
    }).$promise;
  }
}

export default InvoicesService;
