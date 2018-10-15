(function() {
  'use strict';

  module.exports = TripGivingController;

  TripGivingController.$inject = [
    '$rootScope',
    '$state',
    '$timeout',
    'Session',
    'DonationService',
    'GiveTransferService',
    'GiveFlow',
    'AUTH_EVENTS',
    'TripGiving',
    'TripParticipant'];

  function TripGivingController(
      $rootScope,
      $state,
      $timeout,
      Session,
      DonationService,
      GiveTransferService,
      GiveFlow,
      AUTH_EVENTS,
      TripGiving, TripParticipant) {

    var vm = this;
    vm.activeSession = activeSession;
    vm.donationService = DonationService;
    vm.dto = GiveTransferService;
    vm.emailAlreadyRegisteredGrowlDivRef = 1000;
    vm.dto.emailPrefix = 'give';
    vm.giveFlow = GiveFlow;
    vm.initDefaultState = initDefaultState;
    vm.onEmailFound = onEmailFound;
    vm.onEmailNotFound = onEmailNotFound;
    vm.program = null;
    vm.tripParticipant = TripParticipant;
    vm.showNameInput = showNameInput;

    $rootScope.$on('$stateChangeStart', function(event, toState, toParams) {
      // Short-circuit this handler if we're not transitioning TO a give state
      if (toState && !/^tripgiving.*/.test(toState.name)) {
        return;
      }

      GiveTransferService.processing = true;

      if (!vm.dto.initialized || toState.name === 'tripgiving') {
         return;
      }

      vm.donationService.transitionForLoggedInUserBasedOnExistingDonor(event, toState);
    });

    $rootScope.$on(AUTH_EVENTS.logoutSuccess, function(event) {
      vm.dto.reset();
      $state.go('home');
    });

    $rootScope.$on('$stateChangeSuccess', function(event, toState, toParams) {
      // Short-circuit this handler if we're not transitioning TO a give state
      if (toState && !/^tripgiving.*/.test(toState.name)) {
        return;
      }

      vm.dto.processing = false;
      if ((!vm.dto.initialized || toState.name === 'tripgiving') && toState.name !== GiveFlow.thankYou) {
        event.preventDefault();
        initDefaultState();
        return;
      }
      if (toState.name === GiveFlow.thankYou) {
        vm.dto.initialized = false;
      }
    });

    $rootScope.$on('$stateChangeError', function(event, toState, toParams) {
      GiveTransferService.processing = false;
    });

    activate();

    ////////////////////////////////
    //// IMPLEMENTATION DETAILS ////
    ////////////////////////////////
    function activate() {
      vm.tripParticipant.showGiveButton = false;
      vm.tripParticipant.showShareButtons = true;
    }

    function activeSession() {
      return (Session.isActive());
    }

    function initDefaultState() {
      var program = {
        ProgramId: TripParticipant.trips[0].programId,
        Name: TripParticipant.trips[0].programName
      };

      TripGiving.initDefaultState(program);
      GiveTransferService.campaign = {
        campaignId: TripParticipant.trips[0].campaignId,
        campaignName: TripParticipant.trips[0].campaignName,
        pledgeDonorId: TripParticipant.trips[0].pledgeDonorId
      };
    }

    function onEmailFound() {
      $rootScope.$emit(
          'notify',
          $rootScope.MESSAGES.donorEmailAlreadyRegistered,
          vm.emailAlreadyRegisteredGrowlDivRef,
          -1 // Indicates that this message should not time out
          );
      $timeout(function() {
        var closeButton = document.querySelector('#existingEmail .close');
        if (closeButton) {
          closeButton.tabIndex = -1;
        }
      }, 11);
    }

    function onEmailNotFound() {
      var closeButton = document.querySelector('#existingEmail .close');
      if (closeButton !== undefined) {
        $timeout(function() {
          angular.element(closeButton).triggerHandler('click');
        }, 0);
      }
    }

    function showNameInput() {
      if (this.dto.anonymous === true || activeSession()) {
        return false;
      }

      return true;
    }

  }
})();
