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
          this.alreadyPaid = (data.paymentLeft === 0);
          this.url = this.buildUrl(this.invoiceId, data.paymentLeft, data.paymentLeft);
          console.log(this.url);
        },
        (err) => {
          console.log('error in invoice controller ', err);
          console.error(err); // eslint-disable-line no-console
        })
    ]).then(() => {
      this.viewReady = true;
      console.log ('view is ready ', this.viewRedy);
    });
  }

  setGatewayUrls() {
    switch (__CRDS_ENV__) {
      case 'local':
        this.returnUrl = `http://local.crossroads.net:3000/invoices/${this.invoiceId}/email`;
        break;
      case 'int':
        this.returnUrl = `https://int.crossroads.net/invoices/${this.invoiceId}/email`;
        break;
      case 'demo':
        this.returnUrl = `https://demo.crossroads.net/invoices/${this.invoiceId}/email`;
        break;
      default:
        this.returnUrl = `https://crossroads.net/invoices/${this.invoiceId}/email`;
        break;
    }
  }

  buildUrl(invoiceId, totalCost, minPayment) {
    return this.sce.trustAsResourceUrl(`/give?type=payment&invoice_id=${invoiceId}&total_cost=${totalCost}&min_payment=${minPayment}&url=${this.returnUrl}`);
  }
}
export default InvoiceController;
