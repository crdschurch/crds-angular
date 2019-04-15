(function () {
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
          Meta: ['SystemPage', '$state', '$rootScope', function (SystemPage, $state, $rootScope) {
            return SystemPage.get({
              state: $state.next.name
            }).then(
              function (systemPage) {
                if (systemPage) {
                  if (!$state.next.data) {
                    $state.next.data = {};
                  }

                  $rootScope.doRenderLegacyStyles = (typeof systemPage.legacyStyles !== 'undefined'
                    ? Boolean(parseInt(systemPage.legacyStyles))
                    : true); // revert to value set on route

                  $state.params.bodyClasses = [];
                  if (typeof systemPage.bodyClasses !== 'undefined' && systemPage.bodyClasses !== null) {
                    $state.params.bodyClasses = systemPage.bodyClasses.replace(/\s/g, '').split(',');
                  }

                  $state.next.data.meta = systemPage;
                }
              });
          }],

          SiteConfig: ['SiteConfig', 'ContentSiteConfigService', function (SiteConfig, ContentSiteConfigService) {
            return SiteConfig.get().then(function (result) {
              console.log(ContentSiteConfigService);
              ContentSiteConfigService.siteconfig = result;
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
          TokenStatus: function (PasswordService, $stateParams) {
            var token = { token: $stateParams.token };
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
          CmsInfo: ['SignUpForm', '$stateParams', function (SignUpForm, $stateParams) {
            var link = addTrailingSlashIfNecessary($stateParams.link);
            return SignUpForm.get({
              url: link
            });
          }]
        }
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
  }

  function addTrailingSlashIfNecessary(link) {
    if (_.endsWith(link, '/') === false) {
      return link + '/';
    }

    return link;
  }

})();
