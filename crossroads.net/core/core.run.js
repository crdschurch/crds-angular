/* eslint-disable no-param-reassign */
(() => {
  function AppRun(Session,
    $rootScope,
    MESSAGES,
    $http,
    $log,
    $state,
    $timeout,
    $location,
    $cookies,
    $document,
    ContentSiteConfigService,
    SiteConfig,
    ContentPageService,
    Impersonate
  ) {
    function setupMetaData() {
      const title = ContentSiteConfigService.getTitle();
      const titleSuffix = ` | ${title}`;
      $rootScope.meta.siteconfig = ContentSiteConfigService.siteconfig;
      if ($rootScope.meta.title.indexOf(titleSuffix, $rootScope.meta.title.length - titleSuffix.length) === -1) {
        $rootScope.meta.title += titleSuffix;
      }
      $rootScope.meta.url = $location.absUrl();
      if (!$rootScope.meta.statusCode) {
        $rootScope.meta.statusCode = '200';
      }
      if (!$rootScope.meta.image || $rootScope.meta.image.filename === '/assets/') {
        $rootScope.meta.image = {
          filename: 'http://crds-cms-uploads.imgix.net/content/images/cr-social-sharing-still-bg.jpg'
        };
      }
    }

    function setupHeaderAndFooter() {
      svg4everybody();
      $('html, body').removeClass('noscroll');
      $('.collapse.in').removeClass('in');
      $('body:not(.modal-open) .modal-backdrop.fade').remove();
      
      var sharedOptions = {
        cmsEndpoint: __CMS_CLIENT_ENDPOINT__,
        appEndpoint: __APP_CLIENT_ENDPOINT__,
        imgEndpoint: __IMG_ENDPOINT__,
        crdsCookiePrefix: __CRDS_ENV__,
        contentBlockTitle: __HEADER_CONTENTBLOCK_TITLE__,
        contentBlockCategories: ['common']
      };
      // header options
      var headerOptions = {
        el: '[data-header]',
        ...sharedOptions
      };
      // footer options
      var footerOptions = {
        el: '[data-footer]',
        ...sharedOptions
      };
      setTimeout(() => {
        if ($('[data-header] [data-mobile-menu]').length == 0 &&
            $('[data-header]').length > 0) {
          new CRDS.SharedHeader(headerOptions).render();
        }
        if ($('[data-footer]')){
          new CRDS.SharedFooter(footerOptions);
        }
      }, 100);
    }

    function setOriginForCmsPreviewPane($injectedDocument) {
      const document = $injectedDocument[0];
      // work-around for displaying cr.net inside preview pane for CMS
      const domain = document.domain;
      const parts = domain.split('.');
      if (parts.length === 4) {
        // possible ip address
        const firstChar = parts[0].charAt(0);
        if (firstChar >= '0' && firstChar <= '9') {
          // ip address
          document.domain = domain;
          return;
        }
      }
      while (parts.length > 2) {
        parts.shift();
      }
      document.domain = parts.join('.');
    }

    Impersonate.clear();
    $rootScope.MESSAGES = MESSAGES;
    setOriginForCmsPreviewPane($document);
    fastclick.attach(document.body);

    // Detect Browser Agent. use for browser targeting in CSS
    const doc = document.documentElement;
    doc.setAttribute('data-useragent', navigator.userAgent);
    doc.setAttribute('data-platform', navigator.platform);

    $rootScope.$on('$stateChangeStart', (event, toState, toParams, fromState, fromParams) => {
      ContentPageService.reset();
      if (toState.name === 'logout') {
        if ((fromState.data === undefined || !fromState.data.isProtected) &&
          (fromState.name !== undefined && fromState.name !== '')) {
          Session.addRedirectRoute(fromState.name, fromParams);
        } else if (!Session.hasRedirectionInfo()) {
          Session.addRedirectRoute('content', { link: '/' });
        }

        return;
      }

      // Navigated here from outside the app
      // Remove redirect info if it's not going to be used
      if (toState.name !== 'login' && fromState.name === '' && Session.hasRedirectionInfo()) {
        Session.removeRedirectRoute();
      }

      // Only set redirect if we're going to login from an angular route
      // and only if there is not already a protected route in the redirect cookie
      if (toState.name === 'login' && fromState.name !== '' && !Session.hasRedirectionInfo()) {
        Session.addRedirectRoute(fromState.name, fromParams);
      }

      if (toState.data !== undefined && toState.data.preventRouteAuthentication) {
        return;
      }
      Session.verifyAuthentication(event, toState.name, toState.data, toParams);
    });

    $rootScope.$on('$stateChangeSuccess', (event, toState) => {
      if (toState.data && toState.data.meta) {
        $rootScope.meta = toState.data.meta;
      }
      setupMetaData();
      setupHeaderAndFooter();
    });
  }

  AppRun.$inject = [
    'Session',
    '$rootScope',
    'MESSAGES',
    '$http',
    '$log',
    '$state',
    '$timeout',
    '$location',
    '$cookies',
    '$document',
    'ContentSiteConfigService',
    'SiteConfig',
    'ContentPageService',
    'Impersonate'
  ];

  angular.module('crossroads.core').run(AppRun);
})();
