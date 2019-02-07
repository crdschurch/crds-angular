import applicationModule from '../../../../app/camps/application_page/application_page.module';
import campsModule from '../../../../app/camps/camps.module';
import campHelpers from '../../campHelpers';

describe('Camps Payment Component', () => {
  let fixture;
  let campsService;
  let state;
  let sce;

  const campId = 123;
  const contactId = 456;
  const invoiceId = 476543;

  beforeEach(angular.mock.module(campsModule));
  beforeEach(angular.mock.module(applicationModule));

  describe('Update flag = true', () => {
    beforeEach(inject((_$componentController_, _CampsService_, _$state_, _$sce_) => {
      campsService = _CampsService_;
      state = _$state_;
      state.toParams = {
        update: true,
        redirectTo: 'mycamps',
        campId,
        contactId
      };
      sce = _$sce_;
      spyOn(sce, 'trustAsResourceUrl').and.callThrough();

      campsService.productInfo = campHelpers().productInfo;
      campsService.sessionStorage.campDeposits = {};

      fixture = _$componentController_('campsPayment', null, {});
    }));

    // FIXME: add tests for calculateDeposit()
    it('should calculate the deposit to be min deposit when payment left greater than min payment', () => {
      fixture.$onInit();
      fixture.calculateDeposit();
      expect(fixture.depositPrice).toEqual(10);
    });

    it('should calculate the deposit to be payment left when payment left less than min payment', () => {
      fixture.campsService.productInfo.camperInvoice.paymentLeft = 5.0000;
      fixture.$onInit();
      fixture.calculateDeposit();
      expect(fixture.depositPrice).toEqual(5.0000);
    });

    it('should encode the redirect correctly to mycamps', () => {
      fixture.campsService.productInfo.invoiceId = invoiceId;
      fixture.$onInit();
      fixture.buildUrl();

      const url = 'https%3A%2F%2Fwww.crossroads.net%2Fmycamps';

      expect(sce.trustAsResourceUrl).toHaveBeenCalledWith(`/give?type=payment&min_payment=${fixture.depositPrice}&invoice_id=${invoiceId}&total_cost=${fixture.totalPrice}&title=${fixture.campsService.campTitle}&url=${url}`);
    });

    it('should redirect correctly to a custom page', () => {
      fixture.campsService.productInfo.invoiceId = invoiceId;
      state.toParams.redirectTo = 'custom';
      fixture.$onInit();
      fixture.buildUrl();

      const url = `https%3A%2F%2Fwww.crossroads.net%2Fcamps%2F${state.toParams.campId}%2F${state.toParams.redirectTo}%2F${state.toParams.contactId}`;

      expect(sce.trustAsResourceUrl).toHaveBeenCalledWith(`/give?type=payment&min_payment=${fixture.depositPrice}&invoice_id=${invoiceId}&total_cost=${fixture.totalPrice}&title=${fixture.campsService.campTitle}&url=${url}`);
    });

    it('should redirect correctly to a the payment-confirmation page', () => {
      fixture.campsService.productInfo.invoiceId = invoiceId;
      state.toParams.redirectTo = undefined;
      fixture.$onInit();
      fixture.buildUrl();

      const url = `https%3A%2F%2Fwww.crossroads.net%2Fcamps%2F${state.toParams.campId}%2Fpayment-confirmation%2F${state.toParams.contactId}`;

      expect(sce.trustAsResourceUrl).toHaveBeenCalledWith(`/give?type=payment&min_payment=${fixture.depositPrice}&invoice_id=${invoiceId}&total_cost=${fixture.totalPrice}&title=${fixture.campsService.campTitle}&url=${url}`);
    });
  });

  describe('Update flag false', () => {
    beforeEach(inject((_$componentController_, _CampsService_, _$state_, _$sce_) => {
      campsService = _CampsService_;
      state = _$state_;
      state.toParams = {
        campId,
        contactId
      };
      sce = _$sce_;
      spyOn(sce, 'trustAsResourceUrl').and.callThrough();

      campsService.productInfo = campHelpers().productInfo;
      campsService.sessionStorage.campDeposits = {};

      fixture = _$componentController_('campsPayment', null, {});
    }));

    it('should be the financial assistance price', () => {
      fixture.$onInit();
      fixture.calculateDeposit();
      expect(fixture.depositPrice).toEqual(50);
    });

    it('should be the original deposit price', () => {
      campsService.productInfo.financialAssistance = false;
      fixture.$onInit();
      fixture.calculateDeposit();
      expect(fixture.depositPrice).toEqual(campsService.productInfo.depositPrice);
    });

    it('should redirect to the confirmation page', () => {
      fixture.campsService.productInfo.invoiceId = invoiceId;
      fixture.$onInit();
      fixture.buildUrl();

      const url = `https%3A%2F%2Fcrossroads.net%2Fcamps%2F${state.toParams.campId}%2Fconfirmation%2F${state.toParams.contactId}`;

      expect(sce.trustAsResourceUrl).toHaveBeenCalledWith(`/give?type=payment&min_payment=${fixture.depositPrice}&invoice_id=${invoiceId}&total_cost=${fixture.totalPrice}&title=${fixture.campsService.campTitle}&url=${url}`);
    });
  });
});
