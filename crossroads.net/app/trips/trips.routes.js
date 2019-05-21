(function() {
  'use strict';

  var attributes = require('crds-constants').ATTRIBUTE_TYPE_IDS;

  module.exports = TripRoutes;

  TripRoutes.$inject = ['$stateProvider', '$urlMatcherFactoryProvider'];

  function TripRoutes($stateProvider, $urlMatcherFactory) {

    $urlMatcherFactory.strictMode(false);
  
    $stateProvider
      .state('tripsearch', {
        parent: 'noSideBar',
        url: '/trips/search',
        controller: 'TripSearchCtrl as tripSearch',
        templateUrl: 'tripsearch/tripsearch.html',
        data: {
          meta: {
            title: 'Trip Search',
            description: 'Search Crossroads Trips'
          }
        }
      })
     

      .state('tripgiving', {
        parent: 'noSideBar',
        url: '/trips/giving/:eventParticipantId',
        controller: 'TripGivingController as tripGiving',
        templateUrl: 'tripgiving/tripgiving.html',
        resolve: {
          Trip: 'Trip',
          $stateParams: '$stateParams',
          $state: '$state',
          TripParticipant: function(Trip, $stateParams) {
            return Trip.TripParticipant.get({
              tripParticipantId: $stateParams.eventParticipantId
            }).$promise;
          },

          Meta: function(TripParticipant, $state) {
            TripParticipant.$promise.then(
              function() {
                $state.next.data.meta.title = TripParticipant.participantName +
                  ' - ' + TripParticipant.trips[0].tripName;
              });
          }
        }
      })
      .state('tripgiving.amount', {
        templateUrl: 'tripgivingTemplates/amount.html'
      })
      .state('tripgiving.login', {
        controller: 'LoginController',
        templateUrl: 'tripgivingTemplates/login.html'
      })
      .state('tripgiving.register', {
        controller: 'RegisterController',
        templateUrl: 'tripgivingTemplates/register.html'
      })
      .state('tripgiving.confirm', {
        templateUrl: 'tripgivingTemplates/confirm.html'
      })
      .state('tripgiving.account', {
        templateUrl: 'tripgivingTemplates/account.html'
      })
      .state('tripgiving.change', {
        templateUrl: 'tripgivingTemplates/change.html'
      })
      .state('tripgiving.thank-you', {
        templateUrl: 'tripgivingTemplates/thank_you.html'
      })
      .state('tripdeposit', {
        parent: 'noSideBar',
        url: '/trips/:campaignId/deposit/:contactId?invite',
        template: '<trip-deposit> </trip-deposit>',
        data: {
          isProtected: true,
          meta: {
            title: 'Trip Deposit',
            description: ''
          }
        },
        resolve: {
          loggedin: crds_utilities.checkLoggedin,
          TripsSignupService: 'TripsSignupService',
          $stateParams: '$stateParams',
          CampaignInfo: getCampaignInfo
        }
      })
      .state('tripdeposit.account', {
        templateUrl: 'templates/tripDepositAccount.html'
      })
      .state('tripdeposit.change', {
        templateUrl: 'templates/tripDepositChange.html'
      })
      .state('tripdeposit.confirm', {
        templateUrl: 'templates/tripDepositConfirm.html'
      })
      .state('tripdeposit.thanks', {
        templateUrl: 'templates/tripDepositThanks.html'
      })
      .state('mytrips', {
        parent: 'noSideBar',
        url: '/trips/mytrips',
        controller: 'MyTripsController as tripsController',
        templateUrl: 'mytrips/mytrips.html',
        data: {
          isProtected: true,
          meta: {
            title: 'My Trips',
            description: ''
          }
        },
        resolve: {
          loggedin: crds_utilities.checkLoggedin,
          Trip: 'Trip',
          $cookies: '$cookies',
          $state: '$state',
          MyTrips(Trip) {
            return Trip.MyTrips.get().$promise;
          }
        }
      })
      .state('travelinformation', {
        parent: 'noSideBar',
        url: '/trips/mytrips/update',
        template: '<travel-information></travel-information>',
        data: {
          isProtected: true,
          meta: {
            title: 'My Trips',
            description: ''
          }
        },
        resolve: {
          loggedin: crds_utilities.checkLoggedin,

          $cookies: '$cookies',
          $stateParams: '$stateParams',
          Profile: 'Profile',
          TravelInformationService: 'TravelInformationService',
          $q: '$q',
          Person: (Profile, $stateParams, $cookies, $q, TravelInformationService) => {
            const deferred = $q.defer();
            let cid = $cookies.get('userId');
            if ($stateParams.contactId) {
              cid = $stateParams.contactId;
            }

            Profile.Person.get({ contactId: cid }, (p) => {
              TravelInformationService.setPerson(p);
              deferred.resolve(p);
            }, () => {
              deferred.reject();
            });
            return deferred.promise;
          }
        }
      })
      .state('tripwaiver', {
        parent: 'centeredContentPage',
        url: '/trips/mytrips/waiver/:waiverId',
        template: '<trip-waiver></trip-waiver>',
        data: {
          isProtected: true,
          meta: {
            title: 'Trip Waiver',
            description: ''
          }
        },
        resolve: {
          $stateParams: '$stateParams',
          loggedin: crds_utilities.checkLoggedin,
          Trip: 'Trip',
          Waiver($stateParams, Trip) {
            return Trip.Waiver.get({ waiverId: $stateParams.waiverId }).$promise;
          }
        }
      })
      .state('trippromise', {
        parent: 'centeredContentPage',
        url: '/trips/mytrips/promise/:eventParticipantId',
        template: '<trip-promise my-trip-promise="MyTripsPromise"></trip-promise>',
        controller($scope, MyTripsPromise) {
          $scope.MyTripsPromise = MyTripsPromise;
        },
        data: {
          isProtected: true,
          meta: {
            title: 'I Promise',
            description: ''
          }
        },
        resolve: {
          loggedin: crds_utilities.checkLoggedin,
          $rootScope: '$rootScope',
          $state: '$state',
          $stateParams: '$stateParams',
          $log: '$log',
          Trip: 'Trip',
          MyTripsPromise(Trip, $stateParams) {
            return Trip.MyTripsPromise.get({ eventParticipantId: $stateParams.eventParticipantId }).$promise;
          }
        }
      })
      .state('tripsignup', {
        parent: 'noSideBar',
        url: '/trips/:campaignId?invite',
        templateUrl: 'page0/page0.html',
        controller: 'Page0Controller as page0',
        data: {
          isProtected: true,
          meta: {
            title: 'Trip Signup',
            description: 'Select the family member you want to signup for a trip'
          }
        },
        resolve: {
          loggedin: crds_utilities.checkLoggedin,
          $cookies: '$cookies',
          Trip: 'Trip',
          $stateParams: '$stateParams',
          Campaign: function(Trip, $stateParams) {
            return Trip.Campaign.get({campaignId: $stateParams.campaignId}).$promise;
          },

          Family: function(Trip, $stateParams) {
            return Trip.Family.query({pledgeCampaignId: $stateParams.campaignId}).$promise;
          },

        }
      })
      .state('tripsignup.application', {
        parent: 'noSideBar',
        url: '/trips/:campaignId/signup/:contactId?invite',
        templateUrl: 'signup/signupPage.html',
        controller: 'TripsSignupController as tripsSignup',
        data: {
          isProtected: true,
          meta: {
            title: 'Trip Signup',
            description: ''
          }
        },
        resolve: {
          loggedin: crds_utilities.checkLoggedin,
          $cookies: '$cookies',
          contactId: function($cookies) {
            return $cookies.get('userId');
          },

          Trip: 'Trip',
          $stateParams: '$stateParams',
          Campaign: function(Trip, $stateParams) {
            return Trip.Campaign.get({campaignId: $stateParams.campaignId}).$promise;
          },

          Profile: 'Profile',
          Person: function(Profile, $stateParams, $cookies) {
            var cid = $cookies.get('userId');
            if ($stateParams.contactId) {
              cid = $stateParams.contactId;
            }

            return Profile.Person.get({ contactId: cid }).$promise;
          },

          pageId: function() {
            return 0;
          },

          Scholarshipped: isScholarshipped
        }
      })
      .state('tripsignup.application.thankyou', {
        url: '/thankyou',
        templateUrl: 'pageTemplates/thankYou.html',
      })
      .state('tripsignup.application.page', {
        url: '/:stepId',
        templateUrl: function($stateParams) {
          var template = 'pageTemplates/signupPage' + $stateParams.stepId + '.html';
          return template;
        },
        controller: 'SignupStepController as signupStep',
        resolve: {
          AttributeTypeService: 'AttributeTypeService',
          $stateParams: '$stateParams',

          TshirtSizes: function(AttributeTypeService, $stateParams) {
            if ($stateParams.stepId === '2') {
              return AttributeTypeService.AttributeTypes().get({ id: attributes.TSHIRT_SIZES }).$promise;
            }

            return;
          },

          InternationalExperience: function(AttributeTypeService, $stateParams) {
            if ($stateParams.stepId === '6') {
              return AttributeTypeService.AttributeTypes().get({ id: attributes.INTERNATIONAL_EXPERIENCE }).$promise;
            }

            return;
          },

          AbuseHistory: function(AttributeTypeService, $stateParams) {
            if ($stateParams.stepId === '6') {
              return AttributeTypeService.AttributeTypes().get({ id: attributes.ABUSE_HISTORY }).$promise;
            }

            return;
          },

          Trip: 'Trip',
          WorkTeams: function(Trip) {
            return Trip.WorkTeams.query().$promise;
          },

          Lookup: 'Lookup',
          Locations: function(Lookup) {
            return Lookup.query({
              table: 'crossroadslocations'
            }, function(data) {
              return data;
            });
          }

        }
      });

      function getCampaignInfo(TripsSignupService, $stateParams, $q) {
        var deferred = $q.defer();
        var promise = TripsSignupService.CampaignInfo.get({
          contactId: $stateParams.contactId,
          campaignId: $stateParams.campaignId
        }).$promise;
        promise.then( (data) => {
          if (TripsSignupService.campaign !== undefined) {
            TripsSignupService.campaign.name = data.campaignName;
            TripsSignupService.campaign.campaignId = data.campaignId;
            TripsSignupService.campaign.nickname = data.campaignNickname;
            TripsSignupService.depositAmount = data.deposit;
            TripsSignupService.donorId = data.donorId;
            TripsSignupService.programId = data.programId;
            TripsSignupService.programName = data.programName;
            TripsSignupService.pledgeAmount = data.pledgeAmount;
          }
          deferred.resolve();
        }, (err) => {
          console.log(err);
          deferred.reject();
        });
        return deferred.promise;
      }

      function isScholarshipped($q, $stateParams, Trip, TripsSignupService) {
        var deferred = $q.defer();
        let promise = Trip.TripScholarship.get(
          {campaignId: $stateParams.campaignId, contactId: $stateParams.contactId}
        ).$promise;
        promise.then(() => {
          TripsSignupService.isScholarshipped = true;
          deferred.resolve();
        }, () => {
          TripsSignupService.isScholarshipped = false;
          deferred.resolve();
        });
        return deferred.promise;
      }
  }

})();
