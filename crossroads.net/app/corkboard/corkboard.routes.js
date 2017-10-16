(function () {
  'use strict';
  module.exports = CorkboardRoutes;

  CorkboardRoutes.$inject = [
    '$stateProvider',
    '$urlRouterProvider',
    '$httpProvider',
    '$urlMatcherFactoryProvider',
    '$locationProvider',
    'CorkboardSessionProvider',
    'CorkboardListingsProvider'
  ];

  function CorkboardRoutes($stateProvider,
                           $urlRouterProvider,
                           $httpProvider,
                           $urlMatcherFactory,
                           $locationProvider,
                           CorkboardSessionProvider,
                           CorkboardListings) {

    $stateProvider
    .state('corkboard', {
      parent: 'noSideBar',
      abstract: true,
      template: '<ui-view/>',
      resolve: {
        items: function (CorkboardListings, $log, CorkboardSession, $rootScope) {
          var promise = CorkboardListings.post().query().$promise;

          promise.then(function (posts) {
            // ensure that Date does not remain undefined so that one-time data binding is possible 
            _.each(posts, function(item) {
              if (item.Date === undefined)
                item.Date = null;
            });

            CorkboardSession.posts = posts;
          }, function (error) {

            $rootScope.$emit('notify', $rootScope.MESSAGES.generalError);
          });

          return promise;
        },

        selectedItem: function () {}
      },
      data: {
        meta: {
          title: 'Corkboard',
          description: 'The Corkboard is for posting needs, FREE items, events and job opportunities.'
        }
      },
      controller: 'CorkboardController as corkboard',
    })
    .state('corkboard.root', {
      url: '/corkboard?page',
      templateUrl: 'templates/corkboard-listings.html',
      data: {
        meta: {
          title: 'Corkboard',
          description: 'The Corkboard is for posting needs, FREE items, events and job opportunities.'
        }
      },
    })
    .state('corkboard.create', {
      url: '/corkboard/create/:type',
      templateUrl: function ($stateParams) {
        return 'templates/post-' + $stateParams.type + '.html';
      },

      controller: 'CorkboardEditController as corkboardEdit',
      resolve: {
        loggedin: crds_utilities.checkLoggedin
      },
      data: {
        isProtected: true
      }
    })
    .state('corkboard.detail', {
      url: '/corkboard/detail/:id',
      templateUrl: 'templates/corkboard-listing-detail.html',
      controller: 'CorkboardController as corkboard',
      resolve: {
        selectedItem: function (items, $stateParams) {
          return _.find(items, function (item) {
            return (item._id.$oid === $stateParams.id);
          });
        },

        Meta: function (selectedItem, $state) {
          $state.next.data.meta = {
            title: 'Corkboard | ' + selectedItem.Title,
            description: selectedItem.Description,
            type: 'article',
            card: 'summary'
          };
          return $state.next.data.meta;
        }
      }
    })
    .state('corkboard.reply', {
      url: '/corkboard/reply/:id',
      params: {
        showReplySection: true
      },
      templateUrl: 'templates/corkboard-listing-detail.html',
      controller: 'CorkboardController as corkboard',
      resolve: {
        selectedItem: function (items, $stateParams) {
          return _.find(items, function (item) {
            return (item._id.$oid === $stateParams.id);
          });
        },

        loggedin: crds_utilities.checkLoggedin
      },
      data: {
        isProtected: true
      }
    })
    .state('corkboard.filtered', {
      url: '/corkboard/:type?page',
      templateUrl: 'templates/corkboard-listings.html',
      //This controller has to be here, if it's not stateparams won't be picked up
      controller: 'CorkboardController as corkboard'
    });
  }

})();
