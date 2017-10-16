import invoicesModule from '../../app/invoices/invoices.module';
import InvoiceConfirmationController from '../../app/invoices/invoice-confirmation.controller';

describe('Invoice Confirmation Component', () => {
  let $componentController,
    fixture,
    invoicesService,
    rootScope,
    stateParams,
    sce,
    q;
  const invoiceId = 456;

  beforeEach(angular.mock.module(invoicesModule));

  beforeEach(inject(function (_$rootScope_, $injector, $sce) {
    invoicesService = $injector.get('InvoicesService');
    sce = $injector.get('$sce');
    rootScope = $injector.get('$rootScope');
    q = $injector.get('$q');
    stateParams = $injector.get('$stateParams');
    stateParams.invoiceId = invoiceId;
    fixture = new InvoiceConfirmationController(invoicesService, rootScope, stateParams, sce, q);
  }));

  describe('#onInit', () => {
    beforeEach(() => {
      spyOn(invoicesService, 'getPaymentDetails').and.callFake(() => {
        const deferred = q.defer();
        deferred.resolve({ status: 200 });
        return deferred.promise;
      });
      spyOn(invoicesService, 'getInvoiceDetails').and.callFake(() => {
        const deferred = q.defer();
        deferred.resolve({ status: 200 });
        return deferred.promise;
      });
      fixture.$onInit();
      rootScope.$digest();
    });

    it('should set view as ready', () => {
      expect(fixture.viewReady).toBeTruthy();
    });

    it('set invoiceId, get invoice payment details', () => {
      expect(fixture.invoiceId).toBe(invoiceId);
      expect(invoicesService.getPaymentDetails).toHaveBeenCalledWith(invoiceId);
    });

  });

});
