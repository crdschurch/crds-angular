(function() {
  'use strict';

  module.exports = AppConfig;

  AppConfig.$inject = ['$stateProvider',
    '$urlRouterProvider',
    '$httpProvider',
    '$urlMatcherFactoryProvider',
    '$locationProvider'
  ];

  function AppConfig($stateProvider,
                     $urlRouterProvider,
                     $httpProvider,
                     $urlMatcherFactory,
                     $locationProvider) {

    crds_utilities.preventRouteTypeUrlEncoding($urlMatcherFactory, 'volunteerRouteType', /\/volunteer-sign-up\/.*$/);

    $stateProvider
        .state('root', {
          abstract: true,
          template: '<ui-view/>',
          resolve: {
            Meta: ['SystemPage', '$state', '$rootScope', function(SystemPage, $state, $rootScope) {
              return SystemPage.get({
                state: $state.next.name
              }).$promise.then(
                  function(systemPage) {
                    if (systemPage.systemPages[0]) {
                      if (!$state.next.data) {
                        $state.next.data = {};
                      }

                      $rootScope.doRenderLegacyStyles = (typeof systemPage.systemPages[0].legacyStyles !== 'undefined'
                        ? Boolean(parseInt(systemPage.systemPages[0].legacyStyles))
                        : true); // revert to value set on route

                      $state.params.bodyClasses = [];
                      if (typeof systemPage.systemPages[0].bodyClasses !== 'undefined' && systemPage.systemPages[0].bodyClasses !== null) {
                        $state.params.bodyClasses = systemPage.systemPages[0].bodyClasses.replace(/\s/g, '').split(',');
                      }

                      $state.next.data.meta = systemPage.systemPages[0];
                    }
                  });
            }],

            SiteConfig: ['SiteConfig', 'ContentSiteConfigService', function(SiteConfig, ContentSiteConfigService) {
              return SiteConfig.get({id: 1}).$promise.then(function(result) {
                    ContentSiteConfigService.siteconfig = result.siteConfig;
                  }
              );
            }]
          }
        })
        .state('noSideBar', {
          parent: 'root',
          abstract: true,
          templateUrl: 'templates/noSideBar.html'
        })
        .state('leftSidebar', {
          parent: 'root',
          abstract: true,
          templateUrl: 'templates/leftSidebar.html'
        })
        .state('rightSidebar', {
          parent: 'root',
          abstract: true,
          templateUrl: 'templates/rightSidebar.html'
        })
        .state('screenWidth', {
          parent: 'root',
          abstract: true,
          templateUrl: 'templates/screenWidth.html'
        })
        .state('headerOnly', {
          parent: 'root',
          abstract: true,
          templateUrl: 'templates/headerOnly.html'
        })
        .state('centeredContentPage', {
          parent: 'root',
          abstract: true,
          templateUrl: 'templates/centeredContentPage.html'
        })
        .state('noHeaderOrFooter', {
          parent: 'root',
          abstract: true,
          templateUrl: 'templates/noHeaderOrFooter.html'
        })
        .state('goCincinnati', {
          parent: 'root',
          abstract: true,
          templateUrl: 'templates/goCincinnati.html'
        })
        .state('giving_history', {
          parent: 'noSideBar',
          url: '/givinghistory',
          templateUrl: 'giving_history/history.html',
          controller: 'GivingHistoryController as giving_history_controller',
          data: {
            isProtected: true,
            meta: {
              title: 'Personal Giving History',
              description: ''
            }
          },
          resolve: {
            loggedin: crds_utilities.checkLoggedin
          }
        })
        .state('login', {
          parent: 'headerOnly',
          url: '/signin',
          templateUrl: 'login/login_page.html',
          controller: 'LoginController',
          data: {
            isProtected: false,
            renderLegacyStyles: false,
            meta: {
              title: 'Sign In',
              description: ''
            }
          }
        })
        .state('logout', {
          url: '/signout',
          controller: 'LogoutController',
          data: {
            isProtected: false,
            meta: {
              title: 'Sign out',
              description: ''
            }
          }
        })
        .state('register', {
          parent: 'headerOnly',
          url: '/register',
          templateUrl: 'register/register_page.html',
          data: {
            renderLegacyStyles: false,
            meta: {
              title: 'Register',
              description: ''
            }
          }
        })
        .state('forgotPassword', {
          parent: 'noSideBar',
          url: '/forgot-password',
          templateUrl: 'login/forgot_password.html',
          controller: 'PasswordController as pwController',
          data: {
            renderLegacyStyles: false,
            isProtected: false
          }
        })
        .state('resetPassword', {
          parent: 'noSideBar',
          url: '/reset-password?token',
          templateUrl: 'login/reset_password.html',
          controller: 'ResetPasswordController as resetPwController',
          data: {
            renderLegacyStyles: false,
            isProtected: false
          },
          resolve: {
            PasswordService: 'PasswordService',
            $stateParams: '$stateParams',
            TokenStatus: function(PasswordService, $stateParams) {
              var token = {token: $stateParams.token};
              return PasswordService.VerifyResetToken.get(token).$promise;
            }
          }
        })
        .state('impersonate', {
          parent: 'noSideBar',
          templateUrl: 'impersonate/impersonate.html',
          url: '/impersonate',
          controller: 'ImpersonateController as impersonate',
          data: {
            isProtected: true
          }
        })
        .state('adbox', {
          parent: 'noSideBar',
          url: '/adbox',
          controller: 'AdboxCtrl as adbox',
          templateUrl: 'adbox/adbox-index.html'
        })
        .state('serve-one-time', {
          parent: 'noSideBar',
          url: '/serve-one-time',
          templateUrl: 'my_serve/one_time_serve_mockup.html'
        })
        .state('event-registration', {
          parent: 'noSideBar',
          url: '/event-registration',
          templateUrl: 'my_serve/event_registration_mockup.html'
        })
        .state('event-registration-desired', {
          parent: 'noSideBar',
          url: '/event-registration-desired',
          templateUrl: 'my_serve/event_registration_mockup_desired.html'
        })
        .state('styleguide', {
          parent: 'noHeaderOrFooter',
          url: '/styleguide',
          controller: 'StyleguideCtrl as styleguide',
          templateUrl: 'styleguide/styleguide.html'
        })
        .state('go_trip_giving_results', {
          parent: 'noSideBar',
          url: '/go_trip_giving_results',
          controller: 'TripGivingCtrl as gotripresults',
          templateUrl: 'tripgiving/tripgivingresults.html'
        })
        .state('/demo/go-trip-giving', {
          parent: 'noSideBar',
          url: '/demo/go-trip-giving',
          templateUrl: 'trip_giving/give.html'
        })
        .state('volunteer-request', {
          parent: 'noSideBar',
          url: '{link:volunteerRouteType}',
          controller: 'VolunteerController as volunteer',
          templateUrl: 'volunteer_signup/volunteer_signup_form.html',
          data: {
            isProtected: true,
            meta: {
              title: 'Volunteer Signup',
              description: ''
            }
          },
          resolve: {
            loggedin: crds_utilities.checkLoggedin,
            CmsInfo: ['Page', '$stateParams', function(Page, $stateParams) {
              var link = addTrailingSlashIfNecessary($stateParams.link);
              return Page.get({
                url: link
              }).$promise;
            }]
          }
        })
        .state('volunteer-application', {
          parent: 'noSideBar',
          url: '/volunteer-application/:appType/:id',
          controller: 'VolunteerApplicationController as volunteer',
          templateUrl: 'volunteer_application/volunteerApplicationForm.html',
          data: {
            isProtected: true,
            meta: {
              title: 'Volunteer Signup',
              description: ''
            }
          },
          resolve: {
            loggedin: crds_utilities.checkLoggedin,
            Page: 'Page',
            PageInfo: ['$q', 'Profile', 'Page', '$stateParams', function($q, Profile, Page, $stateParams) {
              var deferred = $q.defer();
              var contactId = $stateParams.id;

              Profile.Person.get({
                contactId: contactId
              }).$promise.then(
                  function(contact) {
                    var age = contact.age;
                    var cmsPath = '/volunteer-application/adult-applicant-form/';
                    if ((age >= 10) && (age <= 15)) {
                      cmsPath = '/volunteer-application/student-applicant-form/';
                    }

                    Page.get({
                      url: cmsPath
                    }).$promise.then(function(cmsInfo) {
                          deferred.resolve({
                            contact, cmsInfo
                          });
                        }
                    );
                  });

              return deferred.promise;
            }],

            Volunteer: 'VolunteerService',
            Family: function(Volunteer) {
              return Volunteer.Family.query({
                contactId: crds_utilities.getCookie('userId')
              }).$promise;
            }
          }
        })
        .state('thedaily', {
          parent: 'noSideBar',
          url: '/thedaily',
          controller: 'TheDailyController as dailyCtrl',
          templateUrl: 'thedaily/thedaily.html'
        })
        .state('leaveyourmark', {
          parent: 'screenWidth',
          url: '/leaveyourmark',
          controller: 'LeaveYourMarkController as leaveYourMarkCtrl',
          templateUrl: 'leaveyourmark/leaveyourmark.html',
          data: {
            meta: {
              title: 'Leave Your Mark',
              description: ''
            }
          }
        });

    //Leave the comment below.  Once we have a true 404 page hosted in the same domain, this is how we
    //will handle the routing.
    //.state('404', {
    //    templateUrl: __CMS_CLIENT_ENDPOINT__ + '/page-not-found/'
    //});

    $urlRouterProvider.otherwise('/');
  }

  function addTrailingSlashIfNecessary(link) {
    if (_.endsWith(link, '/') === false) {
      return link + '/';
    }

    return link;
  }

})();
