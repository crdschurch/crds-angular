/*ngInject*/
class TripDepositController {
  constructor($scope, $rootScope, $state,
              DonationService, GiveTransferService,
              GiveFlow, AUTH_EVENTS,
              TripDeposit, TripsSignupService, $stateParams, $window){
    this.viewReady = false;
    this.scope = $scope;
    this.rootScope = $rootScope;
    this.state = $state;
    this.donationService = DonationService;
    this.dto = GiveTransferService;
    this.giveFlow = GiveFlow;
    this.AUTH_EVENTS = AUTH_EVENTS;
    this.tripDeposit = TripDeposit;
    this.signupService = TripsSignupService;
    this.stateParams = $stateParams;
    this.loadingDonor = false;
    this.window = $window;
    this.initialized = false; 
  }

  $onDestroy() {
    this.window.onbeforeunload = null;
  }

  $onInit() {

    this.window.onbeforeunload = () => {
        return 'Data will be lost if you leave the page, are you sure?';
    };

    this.initialized = true;

    // make sure that an application exists and is valid
    if(!this.signupService.applicationValid) {
      this.state.go('tripsignup', { campaignId: this.stateParams.campaignId });
      return;
    }

    /*jshint unused:false */
    this.scope.$on('$stateChangeStart', (event, toState, toParams) => {

      if (toState && !/^tripdeposit.*/.test(toState.name)) {
        return;
      }
      this.dto.processing = true;
      if (!this.dto.initialized || toState.name === 'tripdeposit') {
         return;
      }

      if (!this.loadingDonor) {
        this.loadingDonor = true;
        var promise = this.donationService.transitionForLoggedInUserBasedOnExistingDonor(event, toState);
        promise.finally( () => {
          this.loadingDonor = false;
          this.viewReady = true;
        });
      }
    });

    /*jshint unused:false */
    this.scope.$on(this.AUTH_EVENTS.logoutSuccess, (event) =>  {
      this.dto.reset();
      this.state.go('home');
    });

    this.scope.$on('$stateChangeSuccess', (event, toState, toParams, fromState, fromParams) => {

      if (toState && !/^tripdeposit.*/.test(toState.name)) {
        return;
      }
      if (fromState.name === 'tripsignup.application.page' &&
          toState.name === 'tripdeposit' &&
          fromParams.stepId &&
          Number(fromParams.stepId) < 5
          ) {
        event.preventDefault();
        this.state.go('tripsignup', { campaignId: this.stateParams.campaignId });
        return;
      }
      if (fromState.name !== 'tripsignup.application.page' &&
          fromState.name !== 'tripdeposit.confirm' &&
            toState.name === 'tripdeposit' ) {
        this.state.go('tripsignup', { campaignId: this.stateParams.campaignId });
        return;
      }
      this.dto.processing = false;
      if ((!this.dto.initialized || toState.name === 'tripdeposit') &&
          toState.name !== this.giveFlow.thankYou) {
        event.preventDefault();
        this.initDefaultState();
        return;
      }
      if (toState.name === this.giveFlow.thankYou) {
        this.signupService.pageId = 'thanks';
        this.dto.initialized = false;
        this.window.onbeforeunload = null;
      }
    });

    this.scope.$on('$stateChangeError', (event, toState, toParams) => {
      this.dto.processing = false;
    });

    this.initDefaultState();
  }

  initDefaultState() {

    let program = {
      ProgramId: this.signupService.programId,
      Name: this.signupService.programName
    };

    let campaign = {
      campaignId: this.signupService.campaign.id,
      campaignName: this.signupService.campaign.name,
      pledgeDonorId: this.signupService.donorId
    };

    this.tripDeposit.initDefaultState(program, campaign, this.signupService.depositAmount);
    this.state.go('tripdeposit.account',
                  {campaignId: this.stateParams.campaignId, contactId: this.stateParams.contactId});
  }

  remainingAmount() {
    return this.signupService.pledgeAmount - this.signupService.depositAmount;
  }

  getPaymentType() {
    if (this.dto.view === 'cc') {
      return 'Credit Card';
    }
    else if (this.dto.view === 'bank') {
      return 'Bank';
    }
    else {
      return 'Unknown';
    }
  }

  saveApplication(shouldSubmitBank = '') {
    this.dto.bankinfoSubmitted = true;
    if (!this.tripForm.$valid){
      this.rootScope.$emit('notify', this.rootScope.MESSAGES.generalError);
      return;
    }

    this.dto.processing = true;
    this.signupService.paymentMethod = this.getPaymentType();
   
    this.signupService.saveApplication((data) => {     
      this.dto.campaign.pledgeDonorId = data.donorId;
      this.saveDeposit(shouldSubmitBank);
    }, (err) => {
      this.dto.processing = false;
      if (err.status === 409) {
        this.rootScope.$emit('notify', this.rootScope.MESSAGES.tripIsFull);
      } else {
        this.rootScope.$emit('notify', this.rootScope.MESSAGES.generalError);
      }
    });   
  }

  saveDeposit(shouldSubmitBank) {
    /*jshint unused:false */   
    if (shouldSubmitBank === 'submit')
    {
      this.donationService.submitBankInfo(this.tripForm);
    } else if (shouldSubmitBank === 'changed') {
      this.donationService.submitChangedBankInfo(this.tripForm);
    } else {
      this.donationService.confirmDonation();    
    }
  }

}
export default TripDepositController;
