require('../../../app/common/common.module');
require('../../../app/app');

describe('PaymentService', function() {
  var sut;
  var httpBackend;
  var stripe;
  var $rootScope;
  var MESSAGES;
  var GiveTransferService;

  var card = {
    number: '4242424242424242',
    exp_month: '12',
    exp_year: '2016',
    cvc: '123'
  };
  var bankAccount = {
    country: 'US',
    currency: 'USD',
    routing_number: '110000000',
    account_number: '000123456789'
  };

  beforeEach(function() {
    angular.mock.module('crossroads.common');
    angular.mock.module('crossroads.core');
    
    angular.mock.module(function($provide) {
      $provide.value('stripe', {
        setPublishableKey: function() {},

        card: {
            createToken: function(card) {
              var last4 = card.number.slice(-4);
              return {
                then: function(callback) {return callback({id: 'tok_test', card: { last4: last4}});}
              };
            }
          },
        bankAccount:
          {
            createToken: function(bank) {
              return {
                then: function(callback) { return callback({id: 'tok_bank'});}
              };
            }
          }
      });
    });

    return null;
  });

  beforeEach(inject(function(_$injector_,
                              $httpBackend,
                              _PaymentService_,
                              _$rootScope_,
                              _MESSAGES_,
                              _GiveTransferService_) {
    var $injector = _$injector_;

    sut = _PaymentService_;
    httpBackend = $httpBackend;

    stripe = $injector.get('stripe');
    $rootScope = _$rootScope_;
    MESSAGES = _MESSAGES_;
    MESSAGES.paymentMethodProcessingError = 1;
    MESSAGES.paymentMethodDeclined = 2;

    GiveTransferService = _GiveTransferService_;
  })
  );

  afterEach(function() {
    httpBackend.verifyNoOutstandingExpectation();
    httpBackend.verifyNoOutstandingRequest();
  });

  describe('function addGlobalErrorMessage', function() {
    it('should set paymentMethodProcessingError global error message for abort', function() {
      var error = {type: 'abort'};
      var e = sut.addGlobalErrorMessage(error, 200);
      expect(e.httpStatusCode).toBe(200);
      expect(e.globalMessage).toBe(MESSAGES.paymentMethodProcessingError);
    });

    it('should set paymentMethodProcessingError global error message for card_error/processing_error', function() {
      var error = {type: 'card_error', code: 'processing_error'};
      var e = sut.addGlobalErrorMessage(error, 200);
      expect(e.httpStatusCode).toBe(200);
      expect(e.globalMessage).toBe(MESSAGES.paymentMethodProcessingError);
      expect(e.type).toBe('card_error');
    });

    it('should set paymentMethodDeclined global error message for bank_account/invalid_request_error', function() {
      var error = {param: 'bank_account', type: 'invalid_request_error'};
      var e = sut.addGlobalErrorMessage(error, 200);
      expect(e.httpStatusCode).toBe(200);
      expect(e.globalMessage).toBe(MESSAGES.paymentMethodDeclined);
      expect(e.type).toBe('invalid_request_error');
      expect(e.param).toBe('bank_account');
    });

    it('should set paymentMethodDeclined global error message for card_error/card_declined', function() {
      var error = {type: 'card_error', code: 'card_declined'};
      var e = sut.addGlobalErrorMessage(error, 200);
      expect(e.httpStatusCode).toBe(200);
      expect(e.globalMessage).toBe(MESSAGES.paymentMethodDeclined);
      expect(e.type).toBe('card_error');
      expect(e.code).toBe('card_declined');
    });

    it('should set paymentMethodDeclined global error message for card_error/incorrect_*', function() {
      var error = {type: 'card_error', code: 'incorrect_cvc'};
      var e = sut.addGlobalErrorMessage(error, 200);
      expect(e.httpStatusCode).toBe(200);
      expect(e.globalMessage).toBe(MESSAGES.paymentMethodDeclined);
      expect(e.type).toBe('card_error');
      expect(e.code).toBe('incorrect_cvc');
    });

    it('should set paymentMethodDeclined global error message for card_error/invalid_*', function() {
      var error = {type: 'card_error', code: 'invalid_cvc'};
      var e = sut.addGlobalErrorMessage(error, 200);
      expect(e.httpStatusCode).toBe(200);
      expect(e.globalMessage).toBe(MESSAGES.paymentMethodDeclined);
      expect(e.type).toBe('card_error');
      expect(e.code).toBe('invalid_cvc');
    });
  });

  describe('function getDonor', function() {
    beforeEach(function() {
      httpBackend.expectGET(window.__env__['CRDS_GATEWAY_CLIENT_ENDPOINT'] + 'api/donor/?email=me%2Byou%2Bus@here.com')
        .respond(200, 'good');
    });

    it('should encode plus signs in an email address', function() {
      var response = sut.getDonor('me+you+us@here.com');
      httpBackend.flush();
      expect(response).toBeDefined();
    });
  });

  describe('function createDonorWithCard', function() {
    var postData;
    beforeEach(function() {
      postData = {
        stripe_token_id: 'tok_test',
        email_address: 'me@here.com',
        first_name: 'fname',
        last_name: 'lname'
      };
    });

    it('should call createToken and create a new donor using the token', function() {
      spyOn(stripe.card, 'createToken').and.callFake(function(donorInfo, callback) {
        callback(200, {id: 'tok_test'});
      });

      httpBackend.expectPOST(window.__env__['CRDS_GATEWAY_CLIENT_ENDPOINT'] + 'api/donor', postData)
        .respond({
          id: '12345',
          stripe_customer_id: 'cust_test'
        });

      var errorCallback = jasmine.createSpyObj('errorCallback', ['onError']);
      sut.createDonorWithCard(card, 'me@here.com', 'fname', 'lname')
        .then(function(donor) {
          expect(donor).toBeDefined();
          expect(donor.id).toEqual('12345');
          expect(donor.stripe_customer_id).toEqual('cust_test');
        }, errorCallback.onError);
      expect(stripe.card.createToken).toHaveBeenCalledWith(card, jasmine.any(Function));
      expect(errorCallback.onError).not.toHaveBeenCalled();
      httpBackend.flush();
    });

    it('should not create a donor if createToken fails', function() {
      spyOn(stripe.card, 'createToken').and.callFake(function(donorInfo, callback) {
        callback(500, { error: { type: 'junk', } });
      });

      var successCallback = jasmine.createSpyObj('successCallback', ['onSuccess']);
      sut.createDonorWithCard(card, 'me@here.com', 'fname', 'lname')
        .then(successCallback.onSuccess,
        function(error) {
          expect(error).toBeDefined();
          expect(error.type).toEqual('junk');
        });

      expect(stripe.card.createToken).toHaveBeenCalledWith(card, jasmine.any(Function));
      expect(successCallback.onSuccess).not.toHaveBeenCalled();
    });

    it('should return error if there is problem calling donor service', function() {
      spyOn(stripe.card, 'createToken').and.callFake(function(donorInfo, callback) {
        callback(200, {id: 'tok_test'});
      });

      httpBackend.expectPOST(window.__env__['CRDS_GATEWAY_CLIENT_ENDPOINT'] + 'api/donor', postData)
        .respond(400, { error: { message: 'Token not found' } });

      var successCallback = jasmine.createSpyObj('successCallback', ['onSuccess']);
      sut.createDonorWithCard(card, 'me@here.com', 'fname', 'lname')
        .then(successCallback.onSuccess,
        function(error) {
          expect(error).toBeDefined();
          expect(error.message).toEqual('Token not found');
        });

      httpBackend.flush();
      expect(stripe.card.createToken).toHaveBeenCalledWith(card, jasmine.any(Function));
      expect(successCallback.onSuccess).not.toHaveBeenCalled();
    });
  });

  describe('function createDonorWithBankAcct', function() {
    var postData;
    beforeEach(function() {
      postData = {
        stripe_token_id: 'tok_test',
        email_address: 'me@here.com',
        first_name: 'fname',
        last_name: 'lname'
      };
    });

    it('should call createToken and create a new donor using the token', function() {
      spyOn(stripe.bankAccount, 'createToken').and.callFake(function(donorInfo, callback) {
        callback(200, {id: 'tok_test'});
      });

      httpBackend.expectPOST(window.__env__['CRDS_GATEWAY_CLIENT_ENDPOINT'] + 'api/donor', postData)
        .respond({
          id: '12345',
          stripe_customer_id: 'cust_test'
        });

      var errorCallback = jasmine.createSpyObj('errorCallback', ['onError']);
      sut.createDonorWithBankAcct(bankAccount, 'me@here.com', 'fname', 'lname')
        .then(function(donor) {
          expect(donor).toBeDefined();
          expect(donor.id).toEqual('12345');
          expect(donor.stripe_customer_id).toEqual('cust_test');
        }, errorCallback.onError);
      expect(stripe.bankAccount.createToken).toHaveBeenCalledWith(bankAccount, jasmine.any(Function));
      expect(errorCallback.onError).not.toHaveBeenCalled();
      httpBackend.flush();
    });

    it('should not create a donor if createToken fails', function() {
      spyOn(stripe.bankAccount, 'createToken').and.callFake(function(donorInfo, callback) {
        callback(500, { error: { type: 'junk', } });
      });

      var successCallback = jasmine.createSpyObj('successCallback', ['onSuccess']);
      sut.createDonorWithBankAcct(bankAccount, 'me@here.com', 'fname', 'lname')
        .then(successCallback.onSuccess,
        function(error) {
          expect(error).toBeDefined();
          expect(error.type).toEqual('junk');
        });

      expect(stripe.bankAccount.createToken).toHaveBeenCalledWith(bankAccount, jasmine.any(Function));
    });

    it('should return error if there is problem calling donor service', function() {
      spyOn(stripe.bankAccount, 'createToken').and.callFake(function(donorInfo, callback) {
        callback(200, {id: 'tok_test'});
      });

      httpBackend.expectPOST(window.__env__['CRDS_GATEWAY_CLIENT_ENDPOINT'] + 'api/donor', postData)
        .respond(400, { error: { message: 'Token not found' } });

      var successCallback = jasmine.createSpyObj('successCallback', ['onSuccess']);
      sut.createDonorWithBankAcct(bankAccount, 'me@here.com', 'fname', 'lname')
        .then(successCallback.onSuccess,
        function(error) {
          expect(error).toBeDefined();
          expect(error.message).toEqual('Token not found');
        });

      expect(stripe.bankAccount.createToken).toHaveBeenCalledWith(bankAccount, jasmine.any(Function));
      expect(successCallback.onSuccess).not.toHaveBeenCalled();
      httpBackend.flush();
    });
  });

  describe('function createRecurringGiftWithCard', function() {
    var postData;
    beforeEach(function() {
      var startDate = moment({year: 2015, month: 12, day: 31}).toDate();
      postData = {
        stripe_token_id: 'tok_test',
        amount: 100,
        program: 1,
        interval: 'week',
        start_date: moment(startDate).utc().format('YYYY-MM-DD')
      };
      GiveTransferService.amount = 100;
      GiveTransferService.program = {ProgramId: 1};
      GiveTransferService.givingType = 'week';
      GiveTransferService.recurringStartDate = startDate;
    });

    it('should call createToken and create a new recurring gift using the token', function() {
      spyOn(stripe.card, 'createToken').and.callFake(function(donorInfo, callback) {
        callback(200, {id: 'tok_test'});
      });

      httpBackend.expectPOST(window.__env__['CRDS_GATEWAY_CLIENT_ENDPOINT'] + 'api/donor/recurrence', postData)
        .respond({
          // TODO: Need to place proper response here
          dummy: ''
        });

      var errorCallback = jasmine.createSpyObj('errorCallback', ['onError']);
      var test = sut.createRecurringGiftWithCard(card)
        .then(function(recurringGift) {
          expect(recurringGift).toBeDefined();

          // expect(recurringGift.id).toEqual('12345');
          // expect(recurringGift.stripe_customer_id).toEqual('cust_test');
        }, errorCallback.onError);
      expect(stripe.card.createToken).toHaveBeenCalledWith(card, jasmine.any(Function));
      expect(errorCallback.onError).not.toHaveBeenCalled();
      httpBackend.flush();
    });

    it('should not create a recurring gift if createToken fails', function() {
      spyOn(stripe.card, 'createToken').and.callFake(function(donorInfo, callback) {
        callback(500, { error: { type: 'junk', } });
      });

      var successCallback = jasmine.createSpyObj('successCallback', ['onSuccess']);
      sut.createRecurringGiftWithCard(card)
        .then(successCallback.onSuccess,
        function(error) {
          expect(error).toBeDefined();
          expect(error.type).toEqual('junk');
        });

      expect(stripe.card.createToken).toHaveBeenCalledWith(card, jasmine.any(Function));
      expect(successCallback.onSuccess).not.toHaveBeenCalled();
    });

    it('should return error if there is problem calling donor service', function() {
      spyOn(stripe.card, 'createToken').and.callFake(function(donorInfo, callback) {
        callback(200, {id: 'tok_test'});
      });

      httpBackend.expectPOST(window.__env__['CRDS_GATEWAY_CLIENT_ENDPOINT'] + 'api/donor/recurrence', postData)
        .respond(400, { error: { message: 'Token not found' } });

      var successCallback = jasmine.createSpyObj('successCallback', ['onSuccess']);
      sut.createRecurringGiftWithCard(card)
        .then(successCallback.onSuccess,
        function(error) {
          expect(error).toBeDefined();
          expect(error.message).toEqual('Token not found');
        });

      expect(stripe.card.createToken).toHaveBeenCalledWith(card, jasmine.any(Function));
      expect(successCallback.onSuccess).not.toHaveBeenCalled();
      httpBackend.flush();
    });
  });

  describe('function createRecurringGiftWithBankAcct', function() {
    var postData;
    var startDate = moment({year: 2015, month: 12, day: 31}).toDate();
    beforeEach(function() {
      postData = {
        stripe_token_id: 'tok_test',
        amount: 100,
        program: 1,
        interval: 'week',
        start_date: moment(startDate).utc().format('YYYY-MM-DD')
      };
      GiveTransferService.amount = 100;
      GiveTransferService.program = { ProgramId: 1 };
      GiveTransferService.givingType = 'week';
      GiveTransferService.recurringStartDate = startDate;
      
    });

    it('should call createToken and create a new recurring gift using the token', function() {
      spyOn(stripe.bankAccount, 'createToken').and.callFake(function(donorInfo, callback) {
        callback(200, {id: 'tok_test'});
      });

      httpBackend.expectPOST(window.__env__['CRDS_GATEWAY_CLIENT_ENDPOINT'] + 'api/donor/recurrence', postData)
        .respond({
          // TODO: Need to place proper response here
          dummy: ''
        });

      var errorCallback = jasmine.createSpyObj('errorCallback', ['onError']);
      var test = sut.createRecurringGiftWithBankAcct(bankAccount)
        .then(function(recurringGift) {
          expect(recurringGift).toBeDefined();

          // expect(recurringGift.id).toEqual('12345');
          // expect(recurringGift.stripe_customer_id).toEqual('cust_test');
        }, errorCallback.onError);
      expect(stripe.bankAccount.createToken).toHaveBeenCalledWith(bankAccount, jasmine.any(Function));
      expect(errorCallback.onError).not.toHaveBeenCalled();
      httpBackend.flush();
    });

    it('should not create a recurring gift if createToken fails', function() {
      spyOn(stripe.bankAccount, 'createToken').and.callFake(function(donorInfo, callback) {
        callback(500, { error: { type: 'junk', } });
      });

      var successCallback = jasmine.createSpyObj('successCallback', ['onSuccess']);
      sut.createRecurringGiftWithBankAcct(bankAccount)
        .then(successCallback.onSuccess,
        function(error) {
          expect(error).toBeDefined();
          expect(error.type).toEqual('junk');
        });

      expect(stripe.bankAccount.createToken).toHaveBeenCalledWith(bankAccount, jasmine.any(Function));
      expect(successCallback.onSuccess).not.toHaveBeenCalled();
    });

    it('should return error if there is problem calling donor service', function() {
      spyOn(stripe.bankAccount, 'createToken').and.callFake(function(donorInfo, callback) {
        callback(200, {id: 'tok_test'});
      });

      httpBackend.expectPOST(window.__env__['CRDS_GATEWAY_CLIENT_ENDPOINT'] + 'api/donor/recurrence', postData)
        .respond(400, { error: { message: 'Token not found' } });

      var successCallback = jasmine.createSpyObj('successCallback', ['onSuccess']);
      sut.createRecurringGiftWithBankAcct(bankAccount)
        .then(successCallback.onSuccess,
        function(error) {
          expect(error).toBeDefined();
          expect(error.message).toEqual('Token not found');
        });

      expect(stripe.bankAccount.createToken).toHaveBeenCalledWith(bankAccount, jasmine.any(Function));
      expect(successCallback.onSuccess).not.toHaveBeenCalled();
      httpBackend.flush();
    });
  });

  describe('function donateToProgram', () => {
    it('should successfully create a donation', () => {
      GiveTransferService.campaign = {};
      GiveTransferService.campaign.pledgeDonorId = 111;
      var postData = {
        program_id: 'Program',
        pledge_campaign_id: 321,
        pledge_donor_id: GiveTransferService.campaign.pledgeDonorId,
        amount: '1234',
        donor_id: 'Donor'
      };

      httpBackend.expectPOST(window.__env__['CRDS_GATEWAY_CLIENT_ENDPOINT'] + 'api/donation', postData)
        .respond({
          amount: '1234',
          program_id: 'Program'
        });

      sut.donateToProgram('Program', 321, '1234', 'Donor')
      .then(function(confirmation) {
        expect(confirmation.program_id).toEqual('Program');
        expect(confirmation.amount).toEqual('1234');
      });

      httpBackend.flush();
    });
  });

  describe('function updateDonorWithCard', function() {
    var putData;
    var card;

    beforeEach(function() {
      putData = {
        stripe_token_id: 'tok_test',
        email_address: 'me@here.com',
        first_name: null,
        last_name: null
      };

      card = {
         number: '5555555555554444',
         exp_month: '06',
         exp_year: '2020',
         cvc: '987'
       };
    });

    it('should call createToken and update the donor using the token', function() {
      spyOn(stripe.card, 'createToken').and.callFake(function(donorInfo, callback) {
        callback(200, {id: 'tok_test'});
      });

      httpBackend.expectPUT(window.__env__['CRDS_GATEWAY_CLIENT_ENDPOINT'] + 'api/donor', putData)
        .respond({
          id: '12345',
          stripe_customer_id: 'cust_test'
        });

      var errorCallback = jasmine.createSpyObj('errorCallback', ['onError']);
      sut.updateDonorWithCard('12345', card, 'me@here.com')
        .then(function(donor) {
          expect(donor).toBeDefined();
          expect(donor.id).toEqual('12345');
          expect(donor.stripe_customer_id).toEqual('cust_test');
        }, errorCallback.onError);
      expect(stripe.card.createToken).toHaveBeenCalledWith(card, jasmine.any(Function));
      expect(errorCallback.onError).not.toHaveBeenCalled();
      httpBackend.flush();
    });

    it('should not update the donor if createToken fails', function() {
      spyOn(stripe.card, 'createToken').and.callFake(function(donorInfo, callback) {
        callback(500, { error: { type: 'junk', } });
      });

      var successCallback = jasmine.createSpyObj('successCallback', ['onSuccess']);
      sut.updateDonorWithCard('12345', card, 'me@here.com')
        .then(successCallback.onSuccess,
        function(error) {
          expect(error).toBeDefined();
          expect(error.type).toEqual('junk');
        });

      expect(stripe.card.createToken).toHaveBeenCalledWith(card, jasmine.any(Function));
      expect(successCallback.onSuccess).not.toHaveBeenCalled();
    });

    it('should return error if there is problem calling donor service', function() {
      spyOn(stripe.card, 'createToken').and.callFake(function(donorInfo, callback) {
        callback(200, {id: 'tok_test'});
      });

      httpBackend.expectPUT(window.__env__['CRDS_GATEWAY_CLIENT_ENDPOINT'] + 'api/donor', putData)
        .respond(400, { error: { message: 'Token not found' }});

      var successCallback = jasmine.createSpyObj('successCallback', ['onSuccess']);
      sut.updateDonorWithCard('12345', card, 'me@here.com')
        .then(successCallback.onSuccess,
        function(error) {
          expect(error).toBeDefined();
          expect(error.message).toEqual('Token not found');
        });

      httpBackend.flush();
      expect(stripe.card.createToken).toHaveBeenCalledWith(card, jasmine.any(Function));
      expect(successCallback.onSuccess).not.toHaveBeenCalled();
    });
  });

  describe('function updateDonorWithBankAcct', function() {
    var putData;
    var bankAccount;

    beforeEach(function() {
      putData = {
        stripe_token_id: 'tok_test',
        email_address: 'me@here.com',
        first_name: null,
        last_name: null
      };

      bankAccount = {
        country: 'US',
        currency: 'USD',
        routing_number: '110000000',
        account_number: '000123456789'
      };
    });

    it('should call createToken and update the donor using the token', function() {
      spyOn(stripe.bankAccount, 'createToken').and.callFake(function(donorInfo, callback) {
        callback(200, {id: 'tok_test'});
      });

      httpBackend.expectPUT(window.__env__['CRDS_GATEWAY_CLIENT_ENDPOINT'] + 'api/donor', putData)
        .respond({
          id: '12345',
          stripe_customer_id: 'cust_test'
        });

      var errorCallback = jasmine.createSpyObj('errorCallback', ['onError']);
      sut.updateDonorWithBankAcct('12345', bankAccount, 'me@here.com')
        .then(function(donor) {
          expect(donor).toBeDefined();
          expect(donor.id).toEqual('12345');
          expect(donor.stripe_customer_id).toEqual('cust_test');
        }, errorCallback.onError);
      expect(stripe.bankAccount.createToken).toHaveBeenCalledWith(bankAccount, jasmine.any(Function));
      expect(errorCallback.onError).not.toHaveBeenCalled();
      httpBackend.flush();
    });

    it('should not update the donor if createToken fails', function() {
      spyOn(stripe.bankAccount, 'createToken').and.callFake(function(donorInfo, callback) {
        callback(500, { error: { type: 'junk', } });
      });

      var successCallback = jasmine.createSpyObj('successCallback', ['onSuccess']);
      sut.updateDonorWithBankAcct('12345', bankAccount, 'me@here.com')
        .then(successCallback.onSuccess,
        function(error) {
          expect(error).toBeDefined();
          expect(error.type).toEqual('junk');
        });

      expect(stripe.bankAccount.createToken).toHaveBeenCalledWith(bankAccount, jasmine.any(Function));
      expect(successCallback.onSuccess).not.toHaveBeenCalled();
    });

    it('should return error if there is problem calling donor service', function() {
      spyOn(stripe.bankAccount, 'createToken').and.callFake(function(donorInfo, callback) {
        callback(200, {id: 'tok_test'});
      });

      httpBackend.expectPUT(window.__env__['CRDS_GATEWAY_CLIENT_ENDPOINT'] + 'api/donor', putData)
        .respond(400, { error: { message: 'Token not found' }});

      var successCallback = jasmine.createSpyObj('successCallback', ['onSuccess']);
      sut.updateDonorWithBankAcct('12345', bankAccount, 'me@here.com')
        .then(successCallback.onSuccess,
        function(error) {
          expect(error).toBeDefined();
          expect(error.message).toEqual('Token not found');
        });

      httpBackend.flush();
      expect(stripe.bankAccount.createToken).toHaveBeenCalledWith(bankAccount, jasmine.any(Function));
      expect(successCallback.onSuccess).not.toHaveBeenCalled();
    });
  });
});
