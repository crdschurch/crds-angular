require('../../app/app');

describe('DonationList Directive', function () {
  var $compile;
  var $rootScope;
  var $httpBackend;
  var scope;
  var templateString;
  var originalDonations;

  beforeEach(angular.mock.module('crossroads'));

  beforeEach(angular.mock.module(function ($provide) {
    $provide.value('$state', { get: function () {} });
  }));

  beforeEach(
      inject(function (_$compile_, _$rootScope_, _$httpBackend_) {
        $compile = _$compile_;
        $rootScope = _$rootScope_;
        $httpBackend = _$httpBackend_;

        originalDonations = [
          {
            amount: 1000,
            status: 'Succeeded',
            date: '2015-03-31T09:52:44.873',
            distributions: [
              {
                program_name: 'Crossroads',
                amount: 1000
              }
            ],
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
            status: 'Succeeded',
            date: '2015-03-31T09:52:44.873',
            distributions: [
              {
                program_name: 'Crossroads',
                amount: 2000
              }
            ],
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
            status: 'Succeeded',
            date: '2015-03-31T09:52:44.873',
            distributions: [
              {
                program_name: 'Crossroads',
                amount: 3000
              }
            ],
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
            status: 'Succeeded',
            date: '2015-03-31T09:52:44.873',
            distributions: [
              {
                program_name: 'Crossroads',
                amount: 4000
              }
            ],
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
            status: 'Succeeded',
            date: '2015-03-31T09:52:44.873',
            distributions: [
              {
                program_name: 'Crossroads',
                amount: 5000
              }
            ],
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
            status: 'Succeeded',
            date: '2015-03-31T09:52:44.873',
            distributions: [
              {
                program_name: 'Crossroads',
                amount: 6000
              }
            ],
            source: {
              type: 'Cash',
              name: 'cash',
              expectedViewBox: '0 0 34 32',
              expectedName: 'cash',
              expectedIcon: 'money'
            }
          },
          {
            amount: 7000,
            status: 'Succeeded',
            date: '2015-03-31T09:52:44.873',
            distributions: [
              {
                program_name: 'Crossroads',
                amount: 7000,
              }
            ],
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
            status: 'Succeeded',
            date: '2015-03-31T09:52:44.873',
            distributions: [
              {
                program_name: 'Crossroads',
                amount: 8000
              }
            ],
            source: {
              type: 'Check',
              last4: '8000',
              check_number: '1234',
              expectedViewBox: '0 0 32 32',
              expectedName: 'ending in 8000 | Check #1234',
              expectedIcon: 'library'
            }
          },
          {
            amount: 8000,
            status: 'Succeeded',
            date: '2015-03-31T09:52:44.873',
            distributions: [
              {
                program_name: 'Crossroads',
                amount: 8000
              }
            ],
            source: {
              type: 'Check',
              check_number: '5678',
              expectedViewBox: '0 0 32 32',
              expectedName: 'Check #5678',
              expectedIcon: ''
            }
          }
        ];

        scope = $rootScope.$new();
        scope.donationsInput = _.cloneDeep(originalDonations);
        scope.totalAmount = 12300;
        scope.statementTotalAmount = 45600;

        templateString =
            '<donation-list ' +
            ' donations-input="donationsInput"' +
            ' donation-total-amount="totalAmount"' +
            ' donation-statement-total-amount="statementTotalAmount"></donation-list>';
      })
  );

});
