require('../../app/common/common.module');
//require('../../app/give/give.module');
require('../../app/app');

describe('GiveController', function() {
  var controller;
  var $rootScope;
  var $scope;
  var $state;
  var $timeout;
  var $q;
  var httpBackend;
  var mockPaymentService;
  var mockGetResponse;
  var programList;
  var mockPaymentServiceGetPromise;
  var mockSession;
  var User;
  var AUTH_EVENTS;
  var Session;
  var GiveTransferService;
  var OneTimeGiving;
  var RecurringGiving;
  var DonationService;

  beforeEach(angular.mock.module('crossroads', function($provide) {
    $provide.value('$state', {
      go: function() {},

      get: function() {}
    });

    programList = [
      {ProgramId: 1, Name: 'Crossroads'},
      {ProgramId: 2, Name: 'Game Change'},
      {ProgramId: 3, Name: 'Old St George Building'},
    ];
    $provide.value('Programs', {
      Programs: function() {
        return ({
          get: function() {
            var deferred = $q.defer();
            deferred.resolve(programList);
            return deferred.promise;
          },
        });
      },
    });
    mockSession = jasmine.createSpyObj('Session', ['exists', 'isActive', 'removeRedirectRoute', 'addRedirectRoute']);
    mockSession.exists.and.callFake(function(something) {
      return '12345678';
    });

    $provide.value('Session', mockSession);
    mockPaymentService = jasmine.createSpyObj('PaymentService',
        ['getDonor', 'updateDonorWithCard', 'donateToProgram', 'updateDonorWithBankAcct', 'stripeErrorHandler']);
    mockPaymentService.getDonor.and.callFake(function(n) {
      return (mockPaymentServiceGetPromise.$promise);
    });

    mockPaymentService.updateDonorWithBankAcct.and.callFake(function(donorId, bankAcct, email) {
      var deferred = $q.defer();
      deferred.resolve({ donorId: donorId });
      return deferred.promise;
    });

    mockPaymentService.updateDonorWithCard.and.callFake(function(donorId, card, email) {
      var deferred = $q.defer();
      deferred.resolve({ donorId: donorId });
      return deferred.promise;
    });

    $provide.value('PaymentService', mockPaymentService);
  }));

  beforeEach(
    inject(function($injector, $controller, $httpBackend, _$q_) {
      $rootScope = $injector.get('$rootScope');
      $scope = $rootScope.$new();
      $state = $injector.get('$state');
      $timeout = $injector.get('$timeout');
      $q = _$q_;
      httpBackend = $injector.get('$httpBackend');
      Session = $injector.get('Session');
      User = $injector.get('User');
      AUTH_EVENTS = $injector.get('AUTH_EVENTS');
      GiveTransferService = $injector.get('GiveTransferService');
      OneTimeGiving = $injector.get('OneTimeGiving');
      RecurringGiving = $injector.get('RecurringGiving');
      DonationService = $injector.get('DonationService');

      $rootScope.MESSAGES = {
        failedResponse: 15
      };

      mockGetResponse = {
        id: '102030',
        Processor_ID: '123456',
        default_source:  {
          credit_card: {
            brand: 'Visa',
            last4:'9876'
          }
        }
      };

      mockPaymentServiceGetPromise = {
        $promise: {
          then: function(successCallback, errorCallback) {
            if (this._success) {
              successCallback(mockGetResponse);
            } else {
              errorCallback({httpStatusCode: this._httpStatusCode});
            }
          },

          _success: true,
          _httpStatusCode: 200,
        },
        setSuccess: function(success) {
          this.$promise._success = success;
        },

        setHttpStatusCode: function(code) {
          this.$promise._httpStatusCode = code;
        }
      };

      controller = $controller('GiveController',
        {
          $rootScope: $rootScope,
          $scope: $scope,
          $state: $state,
          $timeout: $timeout,
          giveService: OneTimeGiving,
          Session: Session,
          programList:programList,
          GiveTransferService: GiveTransferService,
          AUTH_EVENTS: AUTH_EVENTS,
          OneTimeGiving: OneTimeGiving,
          RecurringGiving: RecurringGiving
        });

      controller.service.initDefaultState();
      controller.dto.brand = '';
      controller.dto.donor = {};
      controller.dto.donorError = false;
      controller.dto.last4 = '';
      controller.programsInput = programList;

    })
  );

  describe('function confirmDonation()', function() {
    beforeEach(function() {
      spyOn(controller.service, 'goToChange');
    });

    it('should go to the thank-you page if credit card payment was accepted', function() {
      var error = {error1: '1', error2: '2'};
      controller.dto.view = 'cc';
      controller.dto.amount = 123;
      controller.dto.donor = {donorinfo: 'blah'};
      controller.dto.email = 'test@somewhere.com';
      controller.dto.program = {programId: 3, Name: 'crossroads'};
      controller.dto.processing = false;

      mockSession.isActive.and.callFake(function() {
        return true;
      });

      spyOn(DonationService, 'donate').and.callFake(
        function(program, onSuccess, onFailure) {
          $state.go(controller.service.stateName.thankYou);
        }
      );

      controller.service.confirmDonation();
      expect(DonationService.donate).toHaveBeenCalled();
      expect(controller.service.goToChange).not.toHaveBeenCalled();
    });

    it('should go to the change page if credit card payment was declined', function() {
      var error = {error1: '1', error2: '2'};
      controller.dto.view = 'cc';
      controller.dto.amount = 123;
      controller.dto.donor = {donorinfo: 'blah'};
      controller.dto.email = 'test@somewhere.com';
      controller.dto.program = {programId: 3, Name: 'crossroads'};

      mockSession.isActive.and.callFake(function() {
        return true;
      });

      spyOn(DonationService, 'donate').and.callFake(function(program, onSuccess, onFailure) {
        controller.dto.declinedPayment = true;
        onFailure(error);
      });

      spyOn($state, 'go');

      controller.service.confirmDonation();
    });

    it('should stay on the confirm page if there was a processing error', function() {
      var error = {error1: '1', error2: '2'};
      controller.dto.view = 'cc';
      controller.dto.amount = 123;
      controller.dto.donor = {donorinfo: 'blah'};
      controller.dto.email = 'test@somewhere.com';
      controller.dto.program = {programId: 3, Name: 'crossroads'};

      mockSession.isActive.and.callFake(function() {
        return true;
      });

      spyOn(DonationService, 'donate').and.callFake(function(program, onSuccess, onFailure) {
        controller.dto.declinedPayment = true;
        onFailure(error);
      });

      spyOn($state, 'go');

      controller.service.confirmDonation();
      expect($state.go).not.toHaveBeenCalled();
    });

    it('should goto give.one_time_login if session is not active', function() {
      mockSession.isActive.and.callFake(function() {
        return false;
      });

      spyOn($state, 'go');

      controller.service.confirmDonation();
      expect($state.go).toHaveBeenCalledWith('give.one_time_login');
    });
  });

  describe('function initDefaultState', function() {
    var controllerDto;

    beforeEach(function() {

      spyOn($state, 'go');
      spyOn($scope, '$on').and.callFake(function(evt, handler) {
        handler();
      });
    });

    it('should go to give.amount if starting at give', function() {

      controller.dto.initialized = false;
      controller.service.initDefaultState();

      expect($state.go).toHaveBeenCalledWith('give.amount');
      expect(Session.removeRedirectRoute).toHaveBeenCalled();
    });

    it('should go to give.amount if starting at give II', function() {
      controller.dto.initialized = false;
      controller.service.initDefaultState();

      expect($state.go).toHaveBeenCalledWith('give.amount');
      expect(controller.dto.initialized).toBeTruthy();
      expect(Session.removeRedirectRoute).toHaveBeenCalled();
    });

    it('should do nothing special if starting at give.amount', function() {

      controller.dto.initialized = false;
      controller.service.initDefaultState();

      expect($state.go).toHaveBeenCalled();
      expect(controller.dto.initialized).toBeTruthy();
      expect(Session.removeRedirectRoute).toHaveBeenCalled();
    });

    it('should go to give.amount if starting at an unknown state and not initialized', function() {

      controller.dto.initialized = false;
      controller.service.initDefaultState();

      expect($state.go).toHaveBeenCalledWith('give.amount');
      expect(controller.dto.initialized).toBeTruthy();
      expect(Session.removeRedirectRoute).toHaveBeenCalled();
    });
  });

  describe('$stateChangeStart event hook', function() {
    beforeEach(function() {
      controller.dto.processing = false;
      spyOn(OneTimeGiving, 'initDefaultState');
      spyOn(DonationService, 'transitionForLoggedInUserBasedOnExistingDonor');
    });

    it('should initialize default state if not already initialized', function() {
      $rootScope.email = 'me@here.com';
      controller.dto.email = undefined;

      controller.dto.initialized = false;
      var event = $rootScope.$broadcast('$stateChangeStart', {name: 'give.account'});

      expect(event.defaultPrevented).toBeTruthy();
      expect(controller.dto.initialized = true);
      expect(controller.dto.email).toBeUndefined();
    });

    it('should not initialize default state if already initialized', function() {
      $rootScope.email = 'me2@here.com';
      controller.dto.email = undefined;

      controller.dto.initialized = true;
      var event = $rootScope.$broadcast('$stateChangeStart', {name: 'give.amount'});

      expect(event.defaultPrevented).toBeFalsy();
      expect(controller.dto.initialized).toBeTruthy();
      expect(DonationService.transitionForLoggedInUserBasedOnExistingDonor).toHaveBeenCalled();
      expect(controller.dto.processing).toBeTruthy();
      expect(controller.dto.email).toBeUndefined();
    });

    it('should initialize default state if toState=give', function() {
      $rootScope.email = 'me@here.com';
      controller.dto.email = undefined;

      var event = $rootScope.$broadcast('$stateChangeStart', {name: 'give.amount'});

      expect(controller.dto.initialized).toBeTruthy();
      expect(DonationService.transitionForLoggedInUserBasedOnExistingDonor).toHaveBeenCalled();
      expect(controller.dto.email).toBeUndefined();
    });

    it('should not do anything if toState is not in the giving flow', function() {
      $rootScope.email = 'me@here.com';
      controller.dto.email = undefined;
      controller.dto.processing = false;

      var event = $rootScope.$broadcast('$stateChangeStart', {name: 'Ohio'});

      expect(event.defaultPrevented).toBeFalsy();
      expect(DonationService.transitionForLoggedInUserBasedOnExistingDonor).not.toHaveBeenCalled();
      expect(controller.dto.processing).toBeFalsy();
      expect(controller.dto.email).not.toBeDefined();
    });

  });

  describe('AUTH_EVENTS.logoutSuccess event hook', function() {
    it('should reset data and go home', function() {
      spyOn($state, 'go');

      $rootScope.$broadcast(AUTH_EVENTS.logoutSuccess);
      expect(controller.dto.initialized).toBe(false);
      expect($state.go).toHaveBeenCalledWith('home');
    });
  });

  describe('$stateChangeSuccess event hook', function() {
    var controllerDto;

    beforeEach(function() {
      controller.dto.processing = true;
      controller.dto.initialized = true;
    });

    it('should not un-initialize controller if toState is not thank-you', function() {
      $rootScope.meta = { title:  '' };
      $rootScope.$broadcast('$stateChangeSuccess', {name: 'give.amount'});
      expect(controller.dto.processing).toBeFalsy();
      expect(controller.dto.initialized).toBeTruthy();
    });

    it('should un-initialize controller if toState is thank-you', function() {
      $rootScope.meta = { title:  '' };
      $rootScope.$broadcast('$stateChangeSuccess', {name: 'give.thank-you'});
      expect(controller.processing).toBeFalsy();
      expect(controller.initialized).toBeFalsy();
    });
  });

  describe('function submitChangedBankInfo', function() {
    var controllerGiveForm = {
      creditCardForm: {
        $dirty: true,
      },
      bankAccountForm: {
        $dirty: true,
      },
      $valid: true,
    };

    var controllerGiveFormBank = {
      bankAccountForm: {
        $dirty: true,
      },
      $valid: true,
    };

    it('should call updateDonorWithCard with proper values when changing card info', function() {

      httpBackend.whenGET(window.__env__['CRDS_GATEWAY_CLIENT_ENDPOINT'] + 'api/authenticated').respond(200);

      controller.giveForm = controllerGiveForm;
      controller.dto.amount = 858;
      controller.dto.view = 'cc';
      controller.dto.program = {
        programId: 1,
        Name: 'crossroads'
      };
      controller.dto.email = 'tim@kriz.net';
      controller.dto.donor = {
        id: 654,
        default_source: {
          name: 'Tim Startsgiving',
          cc_number: '98765',
          exp_date: '1213',
          address_zip: '90210',
          cvc: '987',
        }
      };
      controller.dto.pymtType = 'cc';
      controller.dto.savedPayment = 'cc';

      mockSession.isActive.and.callFake(function() {
        return true;
      });

      mockPaymentService.donateToProgram.and.callFake(function(p, a, d, e, t) {
        var deferred = $q.defer();
        deferred.resolve({ amount: a, email: e });
        return deferred.promise;
      });

      mockPaymentService.updateDonorWithCard.and.callFake(function(donorId, card, email) {
        var deferred = $q.defer();
        deferred.resolve({ donorId: donorId, email: email });
        return deferred.promise;
      });

      spyOn($state, 'go').and.callFake(function(a) {
        return true;
      });

      spyOn(DonationService, 'donate');

      controller.service.submitChangedBankInfo(controller.giveForm);

      // This resolves the promise above
      $rootScope.$apply();

      expect(mockPaymentService.updateDonorWithCard).toHaveBeenCalled();
    });

    it('should call updateDonorWithBankAcct with proper values when bank account info is changed', function() {
      controller.giveForm = controllerGiveFormBank;
      controller.dto.amount = 858;
      controller.dto.view = 'bank';
      controller.dto.program = {
        programId: 1,
        Name: 'crossroads'
      };
      controller.dto.email = 'tim@kriz.net';
      controller.dto.donor = {
        id: 654,
        default_source: {
          bank_account_number: '753869',
          routing: '110000000',
        }
      };
      controller.dto.pymtType = 'bank';

      spyOn(DonationService, 'donate');

      spyOn($state, 'go').and.callFake(function(a) {
        return true;
      });

      controller.service.submitChangedBankInfo(controller.giveForm);

      // This resolves the promise above
      $rootScope.$apply();

      expect(DonationService.donate).toHaveBeenCalled();
      expect(mockPaymentService.updateDonorWithBankAcct).toHaveBeenCalled();
    });

    it('should go to give.one_time_login if session is not active', function() {
      mockSession.isActive.and.callFake(function() {
        return false;
      });

      controller.dto.program = {
        programId: 1,
        name: 'crossroads'
      };
      controller.dto.email = 'tim@kriz.net';
      controller.dto.donor = {
        id: 654,
        default_source: {
          name: 'Tim Startsgiving',
          cc_number: '98765',
          exp_date: '1213',
          address_zip: '90210',
          cvc: '987',
        }
      };

      controller.dto.pymtType = 'cc';
      controller.giveForm = controllerGiveFormBank;
      spyOn($state, 'go');
      spyOn(DonationService, 'donate');

      controller.service.submitChangedBankInfo(controller.giveForm);
      expect(DonationService.donate).not.toHaveBeenCalled();
      expect($state.go).toHaveBeenCalledWith('give.one_time_login');
    });
  });

  describe('function submitBankInfo', function() {
    var controllerGiveForm = {
      email: 'test@test.com',
      accountForm: {
        $valid: true,
      },
    };

    var controllerDto = {
      amount: 987,
      view: 'cc',
      program: {
        ProgramId: 1,
        Name: 'crossroads'
      },
      email: 'tim@kriz.net',
      donor: {
        id: 654,
        default_source: {
          name: 'Tim Startsgiving',
          last4: '98765',
          exp_date: '1213',
          address_zip: '90210',
          cvc: '987',
        }
      },
      reset: function() {},
    };

    beforeEach(function() {
      controller.giveForm = controllerGiveForm;
      controller.dto = controllerDto;
    });

    it('should call updateDonorAndDonate when there is an existing donor', function() {
      mockPaymentServiceGetPromise.setSuccess(true);
      controller.dto.amount = 987;
      controller.dto.view = 'cc';
      controller.dto.program = {
        ProgramId: 1,
        Name: 'crossroads'
      };
      controller.dto.pymtType = 'cc';
      controller.dto.email = 'tim@kriz.net';
      controller.dto.donor = {
        id: 654,
        default_source: {
          name: 'Tim Startsgiving',
          last4: '98765',
          exp_date: '1213',
          address_zip: '90210',
          cvc: '987',
        }
      };

      GiveTransferService.email = 'test@test.com';
      mockSession.isActive.and.callFake(function() {
        return true;
      });

      spyOn($state, 'go').and.callFake(function(b) {
        return true;
      });

      spyOn(DonationService, 'updateDonorAndDonate');
      controller.service.submitBankInfo(controller.giveForm);

      expect(mockPaymentService.getDonor).toHaveBeenCalledWith('test@test.com');
      expect(DonationService.updateDonorAndDonate).toHaveBeenCalled();
    });

    it('should call createDonorAndDonate when there is not an existing donor', function() {
      mockPaymentServiceGetPromise.setSuccess(false);

      controller.dto.program = {programId: 1, Name: 'crossroads'};

      GiveTransferService.email = 'test@test.com';
      mockSession.isActive.and.callFake(function() {
        return true;
      });

      spyOn(DonationService, 'createDonorAndDonate');
      spyOn(DonationService, 'updateDonorAndDonate');
      controller.service.submitBankInfo(controller.giveForm);

      expect(mockPaymentService.getDonor).toHaveBeenCalledWith('test@test.com');
      expect(DonationService.createDonorAndDonate).toHaveBeenCalled();
    });

  });

  describe('function transitionForLoggedInUserBasedOnExistingDonor', function() {
    var mockEvent = {
      preventDefault: function() {}
    };

    var mockToState = {
      name: 'give.one_time_account'
    };

    it('should not perform any transitions for an unauthenticated user', function() {
      mockSession.isActive.and.callFake(function() {
        return false;
      });

      controller.service.initDefaultState();

      spyOn($state, 'go');
      spyOn(mockEvent, 'preventDefault');

      controller.service.getLoggedInUserDonorPaymentInfo(mockEvent, mockToState);

      expect($state.go).not.toHaveBeenCalled();
      expect(mockEvent.preventDefault).not.toHaveBeenCalled();
      expect(mockPaymentService.getDonor).not.toHaveBeenCalled();
      expect(controller.dto.donorError).toBeFalsy();
    });

    it('should transition to give.one_time_account for a logged-in Giver without an existing donor', function() {
      controller.service.initDefaultState();
      mockSession.isActive.and.callFake(function() {
        return true;
      });

      spyOn($state, 'go');
      spyOn(mockEvent, 'preventDefault');

      mockPaymentServiceGetPromise.setSuccess(false);
      mockPaymentServiceGetPromise.setHttpStatusCode(404);
      controller.giveForm = {
        email: 'test@test.com'
      };
      controller.dto.email = 'test@test.com';
      controller.dto.donor = { email: 'test@test.com' };

      controller.service.getLoggedInUserDonorPaymentInfo(mockEvent, mockToState);

      expect(mockEvent.preventDefault).toHaveBeenCalled();
      expect(mockPaymentService.getDonor).toHaveBeenCalledWith('test@test.com');
      expect(controller.dto.donorError).toBeTruthy();
    });

    it('should transition to give.one_time_confirm for a logged-in Giver with an existing donor', function() {
      mockSession.isActive.and.callFake(function() {
        return true;
      });

      spyOn($state, 'go');
      spyOn(mockEvent, 'preventDefault');

      mockPaymentServiceGetPromise.setSuccess(true);
      controller.dto.email = 'test@test.com';
      controller.service.getLoggedInUserDonorPaymentInfo(mockEvent, mockToState);

      expect($state.go).toHaveBeenCalledWith('give.one_time_confirm');
      expect(mockEvent.preventDefault).toHaveBeenCalled();
      expect(mockPaymentService.getDonor).toHaveBeenCalledWith('test@test.com');
      expect(controller.dto.donorError).toBeFalsy();
      expect(controller.dto.donor.default_source.credit_card.last4).toBe('9876');
      expect(controller.dto.donor.default_source.credit_card.brand).toBe('Visa');
    });

    it('should set brand and last 4 correctly when payment type is bank', function() {
      mockGetResponse = {
        Processor_ID: '123456',
        default_source:  {
          credit_card: {
            brand: null,
            last4:null
          },
          bank_account: {
            routing: '111000222',
            last4: '6699'
          }
        }
      };

      var mockEvent = {
        preventDefault: function() {}
      };

      var mockToState = {
        name: 'give.one_time_account'
      };

      controller.dto.email = 'test@test.com';

      mockSession.isActive.and.callFake(function() {
        return true;
      });

      controller.service.getLoggedInUserDonorPaymentInfo(mockEvent, mockToState);
      expect(controller.dto.last4).toBe('6699');
      expect(controller.dto.brand).toBe('#library');
      expect(controller.dto.view).toBe('bank');
    });

    it('should set brand and last 4 correctly when payment type is cc', function() {
      mockGetResponse = {
        Processor_ID: '123456',
        default_source: {
          credit_card: {
            brand: 'Visa',
            last4: '4242'
          },
          bank_account: {
            routing: null,
            last4: null
          }
        }
      };

      var mockEvent = {
        preventDefault: function() {}
      };

      var mockToState = {
        name: 'give.one_time_account'
      };

      controller.dto.email = 'test@test.com';

      mockSession.isActive.and.callFake(function() {
        return true;
      });

      controller.service.getLoggedInUserDonorPaymentInfo(mockEvent, mockToState);
      expect(controller.dto.last4).toBe('4242');
      expect(controller.dto.brand).toBe('#cc_visa');
      expect(controller.dto.view).toBe('cc');
    });
  });

  describe('function goToChange', function() {
    beforeEach(function() {
      mockSession.isActive.and.callFake(function() {
        return true;
      });

      OneTimeGiving.initDefaultState();

      spyOn($state, 'go').and.callFake(function(state) {
        return true;
      });
    });

    it('should populate dto with appropriate values when going to the credit card change page', function() {
      controller.service.goToChange();
      expect(controller.dto.changeAccountInfo).toBeTruthy();
    });

    it('should populate dto with appropriate values when going to the bank account change page', function() {
      controller.dto.brand = '#library';
      controller.dto.amount = 123;
      controller.dto.donor = 'donor';
      controller.dto.email = 'test@here.com';
      controller.dto.program = 'program';

      controller.service.goToChange();

      expect(controller.dto.amount).toBe(123);
      expect(controller.dto.donor).toBe('donor');
      expect(controller.dto.email).toBe('test@here.com');
      expect(controller.dto.program).toBe('program');
      expect(controller.dto.view).toBe('bank');
      expect(controller.dto.changeAccountInfo).toBeTruthy();

      controller.service = RecurringGiving;
      controller.service.resetGiveFlow();
      controller.service.goToChange();

      expect(controller.dto.amount).toBe(123);
      expect(controller.dto.donor).toBe('donor');
      expect(controller.dto.email).toBe('test@here.com');
      expect(controller.dto.program).toBe('program');
      expect(controller.dto.view).toBe('bank');
      expect(controller.dto.changeAccountInfo).toBeTruthy();
    });

    it('should transition to give.one_time_login is session is not active', function() {
      mockSession.isActive.and.callFake(function() {
        return false;
      });

      mockSession.addRedirectRoute.and.callFake(function(a,b) {
        return true;
      });

      controller.service.goToChange();
      expect($state.go).toHaveBeenCalledWith('give.one_time_login');
    });

    it('should transition to give.recurring_login is session is not active', function() {
      mockSession.isActive.and.callFake(function() {
        return false;
      });

      mockSession.addRedirectRoute.and.callFake(function(a,b) {
        return true;
      });

      controller.service = RecurringGiving;
      controller.service.resetGiveFlow();
      controller.service.goToChange();
      expect($state.go).toHaveBeenCalledWith('give.recurring_login');
    });
  });

  describe('function processChange', function() {
    it('should transition to give.one_time_login is session is not active', function() {
      mockSession.isActive.and.callFake(function() {
        return false;
      });

      spyOn($state, 'go');
      controller.service.processChange();

      expect($state.go).toHaveBeenCalledWith('give.one_time_login');
    });

    it('should transition to give.recurring_login is session is not active', function() {
      mockSession.isActive.and.callFake(function() {
        return false;
      });

      spyOn($state, 'go');
      controller.service = RecurringGiving;
      controller.service.resetGiveFlow();
      controller.service.processChange();

      expect($state.go).toHaveBeenCalledWith('give.recurring_login');
    });
  });

  describe('function donate', function() {
    var callback;

    beforeEach(function() {
      callback = jasmine.createSpyObj('stripe callback', ['onSuccess', 'onFailure']);
    });

    it('should call success callback if donation is successful', function() {

      mockPaymentServiceGetPromise.setSuccess(true);
      controller.service.initDefaultState();
      controller.dto.amount = 123;
      controller.dto.donor = { donorId: '2' };
      controller.dto.email = 'test@here.com';
      controller.dto.view = 'cc';

      mockPaymentService.donateToProgram.and.callFake(
          function(programId, campaignId,  amount, donorId, email, pymtType) {
        var deferred = $q.defer();
        deferred.resolve({ });
        return deferred.promise;
      });

      DonationService.donate(
          {ProgramId: 1, Name: 'Game Change'},
          {campaignId: 321, campaignName: 'Test Campaign'},
          callback.onSuccess,
          callback.onFailure);

      $rootScope.$apply();

      expect(mockPaymentService.donateToProgram).toHaveBeenCalledWith(1, 321, 123, '2', 'test@here.com', 'cc', false, false);
      expect(callback.onSuccess).toHaveBeenCalled();
      expect(callback.onFailure).not.toHaveBeenCalled();
    });

    it('should not call success callback if donation fails', function() {

      mockPaymentService.donateToProgram.and.callFake(function(programId, amount, donorId, email, pymtType) {
        var deferred = $q.defer();
        deferred.reject('Uh oh!');
        return deferred.promise;
      });

      controller.dto.amount = undefined;
      controller.dto.program = undefined;
      controller.dto.program_name = undefined;

      DonationService.donate({
        programId: 1,
        Name: 'Game Change'
      }, undefined, callback.onSuccess, callback.onFailure);

      // This resolves the promise above
      $rootScope.$apply();

      expect(callback.onFailure).toHaveBeenCalledWith('Uh oh!');
      expect(controller.amount).toBeUndefined();
      expect(controller.program).toBeUndefined();
      expect(controller.program_name).toBeUndefined();
      expect(callback.onSuccess).not.toHaveBeenCalled();
    });
  });

  describe('Give type branching', function() {
    beforeEach(function() {
      spyOn(controller.service, 'goToAccount');
      controller.giveForm = {
        amountForm: {
          $dirty: false,
        },
        creditCardForm: {
          $dirty: true,
        },
        bankAccountForm: {
          $dirty: true,
        },
        $valid: true,
      };
    });

    it('should default give flow to one time giving', function() {
      controller.dto.givingType = 'one_time';
      controller.branchOnGivingType();

      expect(controller.service.stateName('amount')).toBe('give.amount');
      expect(controller.service.stateName('account')).toBe('give.one_time_account');
      expect(controller.service.stateName('login')).toBe('give.one_time_login');
      expect(controller.service.stateName('register')).toBe('give.register');
      expect(controller.service.stateName('confirm')).toBe('give.one_time_confirm');
      expect(controller.service.stateName('change')).toBe('give.one_time_change');
      expect(controller.service.stateName('thankYou')).toBe('give.one_time_thank-you');
    });

    it('should change give flow to recurring if selected', function() {
      controller.dto.givingType = 'recurring';
      controller.branchOnGivingType();

      expect(controller.service.stateName('amount')).toBe('give.amount');
      expect(controller.service.stateName('account')).toBe('give.recurring_account');
      expect(controller.service.stateName('login')).toBe('give.recurring_login');
      expect(controller.service.stateName('register')).toBe('give.register');
      expect(controller.service.stateName('confirm')).toBe('give.recurring_account');
      expect(controller.service.stateName('change')).toBe('give.recurring_change');
      expect(controller.service.stateName('thankYou')).toBe('give.recurring_thank-you');
    });
  });

  describe('Give convert to recurring gift', function() {
    it('should change convert to recurring', function() {
      GiveTransferService.givingType = 'one-time';
      GiveTransferService.amount = '12345';
      GiveTransferService.program = '3';
      GiveTransferService.resetForConvert();

      expect(GiveTransferService.amount).toBe('12345');
      expect(GiveTransferService.program).toBe('3');
      expect(GiveTransferService.givingType).toBe('month');
    });
  });

});
