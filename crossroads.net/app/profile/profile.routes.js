(function() {
  'use strict';

  module.exports = ProfileRoutes;

  ProfileRoutes.$inject = [
    '$stateProvider',
    '$urlMatcherFactoryProvider',
    '$locationProvider'
  ];

  function ProfileRoutes($stateProvider,
                         $urlMatcherFactoryProvider,
                         $locationProvider) {

    $stateProvider
        .state('profile', {
          parent: 'noSideBar',
          abstract: true,
          url: '/profile',
          templateUrl: 'profile/profile.html',
          controller: 'ProfileController as profile',
          resolve: {
            loggedin: crds_utilities.checkLoggedin,

            AttributeTypeService: 'AttributeTypeService',
            Profile: 'Profile',
            $stateParams: '$stateParams',
            $cookies: '$cookies',
            Lookup: 'Lookup',

            AttributeTypes: function(AttributeTypeService) {
              return AttributeTypeService.AttributeTypes().query().$promise;
            },

            Person: function(Profile, $stateParams, $cookies) {
              var cid = $cookies.get('userId');
              if ($stateParams.contactId) {
                cid = $stateParams.contactId;
              }

              return Profile.Person.get({contactId: cid}).$promise;
            },

            Locations: function(Lookup) {
              return Lookup.query({
                table: 'crossroadslocations'
              }, function(data) {
                return data;
              });
            },

            Subscriptions: function(Profile) {
              return Profile.Subscriptions.query().$promise;
            },

            Statement: function(Profile) {
              return Profile.Statement.get().$promise;
            },

          },
          data: {
            isProtected: true,
            meta: {
              title: 'Profile',
              description: ''
            }
          }
        })
        .state('profile.personal', {
          url: '/personal',
          templateUrl: 'personal/profilePersonal.html',
          data: {resolve: true}
        })
        .state('profile.communications', {
          url: '/communications',
          templateUrl: 'communications/profileCommunications.html',
          controller: 'ProfileCommunicationsController as profileCommunications',
          data: {resolve: true}
        })
        .state('profile.skills', {
          url: '/skills',
          templateUrl: 'skills/profileSkills.html',
          controller: 'ProfileSkillsController as profileSkills',
          data: {resolve: true}
        })
    ;
  }
})();
