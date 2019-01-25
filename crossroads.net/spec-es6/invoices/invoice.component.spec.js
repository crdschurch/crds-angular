import invoicesModule from '../../app/invoices/invoices.module';
import InvoiceController from '../../app/invoices/invoice.controller';

describe('Invoice Component', () => {
  let $componentController,
    fixture,
    invoicesService,
    rootScope,
    stateParams,
    sce,
    q;
  const invoiceId = 344;

  beforeEach(angular.mock.module(invoicesModule));

  beforeEach(inject(function (_$rootScope_, $injector, $sce) {
    invoicesService = $injector.get('InvoicesService');
    sce = $injector.get('$sce');
    rootScope = $injector.get('$rootScope');
    stateParams = $injector.get('$stateParams');
    stateParams.invoiceId = invoiceId;
    q = $injector.get('$q');
    fixture = new InvoiceController(invoicesService, rootScope, stateParams, sce, q);
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

    it('should set urls, set the view as ready', () => {
      expect(fixture.returnUrl).toBe(`https://www.crossroads.net/invoices/${invoiceId}/email`);
      expect(fixture.viewReady).toBeTruthy();
    });

    it('should set invoiceId, get invoice', () => {
      expect(fixture.invoiceId).toBe(invoiceId);
      expect(invoicesService.getInvoiceDetails).toHaveBeenCalledWith(invoiceId);
    });

  });
});
