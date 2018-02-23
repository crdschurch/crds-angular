/* ngInject */
class TripDeposit {
  constructor(GiveTransferService, GiveFlow, Session, $state) {
    this.giveTransferService = GiveTransferService;
    this.giveFlow = GiveFlow;
    this.session = Session;
    this.state = $state;   
  }

  initDefaultState(program, campaign, amount) {
    this.giveTransferService.reset();
    this.giveTransferService.program = program;
    this.giveTransferService.campaign = campaign;
    this.giveTransferService.amount = amount;
    this.giveTransferService.amountSubmitted = true;
    this.giveTransferService.givingType = 'one_time';
    this.giveTransferService.processing = false;
    this.giveTransferService.tripDeposit = true;

    // Setup the give flow service
    this.giveFlow.reset({
      account: 'tripdeposit.account',
      confirm: 'tripdeposit.confirm',
      change: 'tripdeposit.change',
      thankYou: 'tripdeposit.thanks'
    });

    this.giveTransferService.initialized = true;
    this.session.removeRedirectRoute();
  }
}

export default TripDeposit;
