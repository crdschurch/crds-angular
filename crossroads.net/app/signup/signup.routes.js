(function() {
  'use strict';

  module.exports = SignupRoutes;

  SignupRoutes.$inject = ['$stateProvider', '$urlMatcherFactoryProvider', '$locationProvider'];

  function SignupRoutes($stateProvider, $urlMatcherFactory, $locationProvider) {

    crds_utilities.preventRouteTypeUrlEncoding($urlMatcherFactory, 'signupRouteType', /\/sign-up\/.*$/);

    $stateProvider.state('signup', {
      parent: 'noSideBar',
      url: '{link:signupRouteType}',
      template: '<crds-signup></crds-signup>',
      data: {
        isProtected: true,
        meta: {
          title: 'Signup Page',
          description: ''
        }
      },
      resolve: {
        loggedin: crds_utilities.checkLoggedin,
        $stateParams: '$stateParams',
        SignUpForm: 'SignUpForm',
        SignupService: 'SignupService',
        $q: '$q',
        Group: 'Group',
        ServeOpportunities: 'ServeOpportunities',
        Lookup: 'Lookup',
        Family: function(SignupService, ServeOpportunities) {
          return ServeOpportunities.Family.query({}, function(data) {
            SignupService.family = data;
          }).$promise;
        },

        Locations: function(SignupService, Lookup) {
          return Lookup.query({
            table: 'crossroadslocations'
          }, function(data) {
            SignupService.locations = data;
          }).promise;
        },

        CmsInfo: function($q, SignUpForm, SignupService, Group, $stateParams) {
          var deferred = $q.defer();
          var link = addTrailingSlashIfNecessary($stateParams.link);
          SignUpForm.get({url: link}).then(function(data) {
            if (data.items.length === 0) {
              deferred.reject();
            }

            SignupService.cmsInfo = data;

            Group.Detail.get({groupId: data.items[0].fields.group}).$promise.then(function(group) {
                SignupService.group = group;
                deferred.resolve();
              },

              function() {
                deferred.reject();
              });
          },

          function() {
            deferred.reject();
          });

          return deferred.promise;
        },

      }
    });
  }

  function addTrailingSlashIfNecessary(link) {
    if (_.endsWith(link, '/') === false) {
      return link + '/';
    }

    return link;
  }

})();
