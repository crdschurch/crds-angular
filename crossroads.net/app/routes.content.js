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

    crds_utilities.preventRouteTypeUrlEncoding($urlMatcherFactory, 'contentRouteType', /^\/.*/);

    $stateProvider
      .state('content', {
        // This url will match a slash followed by anything (including additional slashes).
        url: '{link:contentRouteType}',
        views: {
          '': {
            controller: 'ContentCtrl',
            templateProvider: function ($rootScope,
              $templateFactory,
              $stateParams,
              Page,
              PageById,
              ContentPageService,
              Session,
              $state,
              $q,
              FormBuilderResolverService,
              $location,
              $httpParamSerializer,
              $window,
              $cookies) {
              var promise;
              var redirectFlag = false;
              var link = $stateParams.link;

              if(Session.isActive() && link === "/" ) {
                link = getPersonalizedContentPath(link);
              }

              link = addTrailingSlashIfNecessary(link);

              promise = Page.get({ url: link, requiresAngular: 1 }).$promise;

              var childPromise = promise.then(function (originalPromise) {

                if (originalPromise.pages.length > 0) {
                  ContentPageService.page = originalPromise.pages[0];
                  // check if page is redirect
                  if (ContentPageService.page.pageType === "RedirectorPage") {
                    if (ContentPageService.page.redirectionType === "External") {
                      $window.location.href = ContentPageService.page.externalURL;
                      return;
                    } else {
                      redirectFlag = true;
                      return PageById.get({ id: ContentPageService.page.linkTo }).$promise;
                    }
                  } else if (ContentPageService.page.pageType === "AngularRedirectPage") {
                    $state.go(ContentPageService.page.angularRoute);
                    return;
                  }

                  return originalPromise;
                }

                const notlegacy = $cookies.get('notlegacy');

                if (notlegacy !== undefined && notlegacy === link) {
                  $cookies.remove('fwd');
                  var notFoundPromise = Page.get({ url: '/page-not-found/' }).$promise;

                  notFoundPromise.then(function (promise) {
                    if (promise.pages.length > 0) {
                      ContentPageService.page = promise.pages[0];
                    } else {
                      ContentPageService.page = {
                        content: '404 Content not found',
                        pageType: '',
                        title: 'Page not found'
                      };
                    }
                  });

                  return notFoundPromise;

                } else {
                  $cookies.put('notlegacy', link);

                  // page not found....
                  // remove the previous link from the history?
                  const queryParams = $location.search();
                  link = removeTrailingSlashIfNecessary($stateParams.link);
                  const queryParamsString = angular.equals(queryParams, {}) ? '' : `?${$httpParamSerializer(queryParams)}`;

                  // code below prevents console errors during redirect
                  ContentPageService.page = {
                    redirectType: 'RedirectorPage',
                    content: '',
                    pageType: 'NoHeaderOrFooter',
                    title: ''
                  };

                  $window.location.replace(`${link}${queryParamsString}`);                    

                  $rootScope.$destroy();
                  return;
                }
              });

              childPromise = childPromise.then(function (result) {
                if (redirectFlag && result.pages.length > 0) {
                  $location.path(result.pages[0].link);
                }

                if (ContentPageService.page.canViewType === 'LoggedInUsers') {
                  $state.next.data.isProtected = true;
                  var promise = Session.verifyAuthentication(null, $state.next.name, $state.next.data, $state.toParams);
                  return promise;
                }

                var deferred = $q.defer();
                deferred.resolve();
                return deferred.promise;
              });

              childPromise = childPromise.then(function () {
                var fields = ContentPageService.page.fields;

                if (fields && fields.length > 1) {
                  return FormBuilderResolverService.getInstance({
                    contactId: Session.exists('userId'),
                    fields: fields,
                  });
                }

                var deferred = $q.defer();
                deferred.resolve();
                return deferred.promise;
              });

              return childPromise.then(function (formBuilderServiceData) {
                ContentPageService.resolvedData = formBuilderServiceData;

                var metaDescription = ContentPageService.page.metaDescription || '';
                if (!metaDescription && ContentPageService.page.content) {
                  var content = ContentPageService.page.content;
                  var hTagRegEx = /<h1.+?>.+?<\/h1>/;
                  content = content.replace(hTagRegEx, '');
                  var openTagRegEx = /<\w[^>]*>/gm;
                  var closeTagRegEx = /<\/[^>]+>/gm;
                  content = content.replace(openTagRegEx, '').replace(closeTagRegEx, ' ');
                  var firstSentence = content.match(/[^.]*/)[0] + '.';
                  metaDescription = firstSentence;
                }

                $rootScope.meta = {
                  title: ContentPageService.page.title,
                  description: metaDescription,
                  card: ContentPageService.page.card,
                  type: ContentPageService.page.type,
                  image: ContentPageService.page.image,
                  statusCode: ContentPageService.page.errorCode
                };


                $rootScope.doRenderLegacyStyles = (typeof ContentPageService.page.legacyStyles !== 'undefined'
                  ? Boolean(parseInt(ContentPageService.page.legacyStyles))
                  : $rootScope.doRenderLegacyStyles); // revert to value set on route

                $rootScope.bodyClasses = [];
                if (typeof ContentPageService.page.bodyClasses !== 'undefined' && ContentPageService.page.bodyClasses !== null) {
                  $rootScope.bodyClasses = ContentPageService.page.bodyClasses.replace(/\s/g, '').split(',');
                }

                switch (ContentPageService.page.pageType) {
                  case 'NoHeaderOrFooter':
                    return $templateFactory.fromUrl('templates/noHeaderOrFooter.html');
                  case 'LeftSidebar':
                    return $templateFactory.fromUrl('templates/leftSideBar.html');
                  case 'RightSidebar':
                    return $templateFactory.fromUrl('templates/rightSideBar.html');
                  case 'ScreenWidth':
                    return $templateFactory.fromUrl('templates/screenWidth.html');
                  case 'HeaderOnly':
                    return $templateFactory.fromUrl('templates/headerOnly.html');
                  case 'HomePage':
                    return $templateFactory.fromUrl('templates/homePage.html');
                  case 'CenteredContentPage':
                    return $templateFactory.fromUrl('templates/centeredContentPage.html');
                  case 'GoCincinnati':
                    return $templateFactory.fromUrl('templates/goCincinnati.html');
                  case 'BraveAtHome':
                    return $templateFactory.fromUrl('templates/brave.html');
                  default:
                    return $templateFactory.fromUrl('templates/noSideBar.html');
                }
              });
            }
          },
          '@content': {
            templateUrl: 'content/content.html'
          },
          'sidebar@content': {
            templateUrl: 'content/sidebarContent.html'
          }
        }, data: {
          resolve: true
        }
      });
  }

  function addTrailingSlashIfNecessary(link) {
    if (_.endsWith(link, '/') === false) {
      return link + '/';
    }

    return link;
  }

  function removeTrailingSlashIfNecessary(link) {
    if (_.endsWith(link, '/') === true) {
      return link.substring(0, link.length - 1);
    }

    return link;
  }

  function getPersonalizedContentPath(link) {
    return "/personalized" + link;
  }

})();
