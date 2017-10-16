/* @ngInject */
class InvoiceConfirmationController {

  constructor(InvoicesService, $rootScope, $stateParams, $sce, $q) {
    this.invoiceId = $stateParams.invoiceId;
    this.sce = $sce;
    this.invoicesService = InvoicesService;
    this.viewReady = false;
    this.q = $q;
  }

  $onInit() {
    this.q.all([
      this.invoicesService.getInvoiceDetails(this.invoiceId).then(
        (data) => {
          this.invoiceDetails = data;
        }, (err) => {
          console.error(err);
        }),
      this.invoicesService.getPaymentDetails(this.invoiceId).then(
        (data) => {
          this.paymentDetails = data;
        }, (err) => {
          console.error(err);
        })
    ]).then(value => {
      this.viewReady = true;
    });
  }
}
export default InvoiceConfirmationController;
