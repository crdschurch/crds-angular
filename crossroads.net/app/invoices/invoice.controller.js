/* @ngInject */
class InvoiceController {

  constructor(InvoicesService, $rootScope, $stateParams, $sce, $q) {
    this.invoiceId = $stateParams.invoiceId;
    this.sce = $sce;
    this.invoicesService = InvoicesService;
    this.viewReady = false;
    this.q = $q;
    this.alreadyPaid = false;
  }

  $onInit() {
    this.setGatewayUrls();
    this.q.all([
      this.invoicesService.getInvoiceDetails(this.invoiceId),
      this.invoicesService.getPaymentDetails(this.invoiceId).then(
        (data) => {
          this.alreadyPaid = (data.paymentLeft === 0)
          this.url = this.buildUrl(this.invoiceId, data.paymentLeft, data.paymentLeft);
        }, (err) => {
          console.error(err);
        })
    ]).then(value => {
      this.viewReady = true;
    });
  }

  setGatewayUrls() {
    switch (__CRDS_ENV__) {
      case 'local':
        this.baseUrl = 'http://localhost:8080';
        this.returnUrl = `http://local.crossroads.net:3000/invoices/${this.invoiceId}/email`;
        break;
      case 'int':
        this.baseUrl = 'https://embedint.crossroads.net';
        this.returnUrl = `https://int.crossroads.net/invoices/${this.invoiceId}/email`;
        break;
      case 'demo':
        this.baseUrl = 'https://embeddemo.crossroads.net';
        this.returnUrl = `https://demo.crossroads.net/invoices/${this.invoiceId}/email`;
        break;
      default:
        this.baseUrl = 'https://embed.crossroads.net';
        this.returnUrl = `https://crossroads.net/invoices/${this.invoiceId}/email`;
        break;
    }
  }

  buildUrl(invoiceId, totalCost, minPayment) {
    return this.sce.trustAsResourceUrl(`${this.baseUrl}/?type=payment&invoice_id=${invoiceId}&total_cost=${totalCost}&min_payment=${minPayment}&url=${this.returnUrl}`);
  }
}
export default InvoiceController;
