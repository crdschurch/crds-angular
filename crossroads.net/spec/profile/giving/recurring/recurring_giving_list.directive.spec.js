require('../../../../app/app');

describe('RecurringGivingList Directive', function() {
  var $compile;
  var $rootScope;
  var $httpBackend;
  var scope;
  var templateString;
  var originalRecurringGifts;
  var updatedRecurringGifts;
  var modal;

  beforeEach(angular.mock.module('crossroads.profile'));

  beforeEach(angular.mock.module(function($provide) {
    modal = {
      open: function() {
      },

      name: 'test modal',
    };

    spyOn(modal, 'open').and.callFake(function() {
      return {
        result: {
          then: function(confirmCallback, cancelCallback) {
            //Store the callbacks for later when the user clicks on the OK or Cancel button of the dialog
            this.confirmCallBack = confirmCallback;
            this.cancelCallback = cancelCallback;
          }
        },
        close: function(item) {
          //The user clicked OK on the modal dialog, call the stored confirm callback with the selected item
          this.result.confirmCallBack(item);
        },

        dismiss: function(type) {
          //The user clicked cancel on the modal dialog, call the stored cancel callback
          this.result.cancelCallback(type);
        }
      };
    });

    $provide.value('$modal', modal);
    $provide.value('$state', { get: function() {} });
  }));

  beforeEach(
      inject(function(_$compile_, _$rootScope_, _$httpBackend_, $injector) {
        $compile = _$compile_;
        $rootScope = _$rootScope_;
        $httpBackend = _$httpBackend_;

        originalRecurringGifts = [
          {
            amount: 1000,
            recurrence: 'Fridays Weekly',
            program: 'Crossroads',
            source:
            {
              type: 'CreditCard',
              brand: 'Visa',
              last4: '1000',
              expectedViewBox: '0 0 160 100',
              expectedName: 'ending in 1000',
              expectedIcon: 'cc_visa'
            }
          },
          {
            amount: 2000,
            recurrence: '8th Monthly',
            program: 'Crossroads',
            source: {
              type: 'CreditCard',
              brand: 'MasterCard',
              last4: '2000',
              expectedViewBox: '0 0 160 100',
              expectedName: 'ending in 2000',
              expectedIcon: 'cc_mastercard'
            }
          },
          {
            amount: 3000,
            recurrence: '30th Monthly',
            program: 'Crossroads',
            source: {
              type: 'CreditCard',
              brand: 'AmericanExpress',
              last4: '3000',
              expectedViewBox: '0 0 160 100',
              expectedName: 'ending in 3000',
              expectedIcon: 'cc_american_express'
            }
          },
          {
            amount: 4000,
            recurrence: '21st Monthly',
            program: 'Crossroads',
            source: {
              type: 'CreditCard',
              brand: 'Discover',
              last4: '4000',
              expectedViewBox: '0 0 160 100',
              expectedName: 'ending in 4000',
              expectedIcon: 'cc_discover'
            }
          },
          {
            amount: 5000,
            recurrence: 'Tuesdays Weekly',
            program: 'Crossroads',
            source: {
              type: 'CreditCard',
              brand: 'DinersClub',
              last4: '5000',
              expectedViewBox: '0 0 160 100',
              expectedName: 'ending in 5000',
              expectedIcon: ''
            }
          },
          {
            amount: 6000,
            recurrence: 'Mondays Weekly',
            program: 'Crossroads',
            source: {
              type: 'Bank',
              last4: '7001',
              expectedViewBox: '0 0 32 32',
              expectedName: 'ending in 7001',
              expectedIcon: 'library'
            }
          },
          {
            amount: 7000,
            recurrence: '1st Montly',
            program: 'Crossroads',
            source: {
              type: 'Bank',
              last4: '7000',
              expectedViewBox: '0 0 32 32',
              expectedName: 'ending in 7000',
              expectedIcon: 'library'
            }
          },
          {
            amount: 8000,
            recurrence: 'Sundays Weekly',
            program: 'Crossroads',
            source: {
              type: 'Bank',
              last4: '8000',
              expectedViewBox: '0 0 32 32',
              expectedName: 'ending in 8000',
              expectedIcon: 'library'
            }
          }
        ];

        updatedRecurringGifts = [
          {
            amount: 1000,
            recurrence: 'Fridays Weekly',
            program: 'Crossroads',
            source:
            {
              type: 'CreditCard',
              brand: 'Visa',
              last4: '1000',
              expectedViewBox: '0 0 160 100',
              expectedName: 'ending in 1000',
              expectedIcon: 'cc_visa'
            }
          },
          {
            amount: 2000,
            recurrence: '8th Monthly',
            program: 'Crossroads',
            source: {
              type: 'CreditCard',
              brand: 'MasterCard',
              last4: '2000',
              expectedViewBox: '0 0 160 100',
              expectedName: 'ending in 2000',
              expectedIcon: 'cc_mastercard'
            }
          },
        ];

        scope = $rootScope.$new();
        scope.recurringGiftsInput = _.cloneDeep(originalRecurringGifts);

        templateString =
            '<recurring-giving-list ' +
            ' recurring-gifts-input="recurringGiftsInput"></donation-list>';
      })
  );

  afterEach(function() {
    $httpBackend.verifyNoOutstandingExpectation();
    $httpBackend.verifyNoOutstandingRequest();
  });

});
