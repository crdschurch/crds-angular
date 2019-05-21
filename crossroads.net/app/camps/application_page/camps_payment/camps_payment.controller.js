/* @ngInject */
export default class CampPaymentController {
  constructor(CampsService, $state, $sce, $sessionStorage) {
    this.campsService = CampsService;
    this.state = $state;
    this.sce = $sce;
    this.sessionStorage = $sessionStorage;
    this.iframeSelector = '.camp-payment-widget';
    this.viewReady = false;
    this.update = false;
    this.redirectTo = undefined;
    this.minAmount = 10;
    this.paymentRemaining = 0;
    this.invoiceTotal = 0;
  }

  $onInit() {
    this.update = this.state.toParams.update || this.sessionStorage.campDeposits[`${this.state.toParams.campId}+${this.state.toParams.contactId}`];

    if (this.update && this.state.toParams.redirectTo) {
      this.redirectTo = this.state.toParams.redirectTo;
    }

    switch (__CRDS_ENV__) {
      case 'local':
        this.returnUrl = 'http://local.crossroads.net:3000/camps';
        break;
      case 'int':
        this.returnUrl = 'https://int.crossroads.net/camps';
        break;
      case 'demo':
        this.returnUrl = 'https://demo.crossroads.net/camps';
        break;
      default:
        this.returnUrl = 'https://www.crossroads.net/camps';
        break;
    }

    this.totalPrice = this.campsService.productInfo.basePrice + this.getOptionPrice();
    this.calculateDeposit();
    this.viewReady = true;
  }

  calculateDeposit() {
    if (this.update) {
      this.paymentRemaining = this.campsService.productInfo.camperInvoice.paymentLeft;
      this.invoiceTotal = this.campsService.productInfo.camperInvoice.invoiceTotal;
      this.totalPrice = this.paymentRemaining;
      this.depositPrice = this.paymentRemaining > this.minAmount ? this.minAmount : this.paymentRemaining;
    } else {
      this.depositPrice = (this.campsService.productInfo.financialAssistance) ? 50 : this.campsService.productInfo.depositPrice;
    }
  }

  buildUrl() {
    const campId = this.state.toParams.campId;
    const contactId = this.state.toParams.contactId;
    const invoiceId = this.campsService.productInfo.invoiceId;

    let url;
    if (this.redirectTo === 'mycamps') {
      /**
       * Since the `mycamps` page doesn't have '/camps' in the route
       * the following Regular Expression strips it out of `this.returnUrl`
       */
      const returnUrl = /.+(?=\/camps)/i.exec(this.returnUrl)[0];
      url = encodeURIComponent(`${returnUrl}/${this.redirectTo}`);
    } else if (this.redirectTo) {
      url = encodeURIComponent(`${this.returnUrl}/${campId}/${this.redirectTo}/${contactId}`);
    } else if (this.update) {
      url = encodeURIComponent(`${this.returnUrl}/${campId}/payment-confirmation/${contactId}`);
    } else {
      url = encodeURIComponent(`${this.returnUrl}/${campId}/confirmation/${contactId}`);
    }

    return this.sce.trustAsResourceUrl(`/give?type=payment&min_payment=${this.depositPrice}&invoice_id=${invoiceId}&total_cost=${this.totalPrice}&title=${this.campsService.campTitle}&url=${url}`);
  }

  getOptionPrice() {
    if (this.campsService.productInfo.options) {
      const now = moment();
      const currentOption = _.find(this.campsService.productInfo.options, (opt) => {
        const endDate = moment(opt.endDate);
        return endDate.isSame(now, 'day') || endDate.isAfter(now, 'day');
      });
      if (currentOption) {
        return currentOption.optionPrice;
      }
    }
    return 0;
  }
}
