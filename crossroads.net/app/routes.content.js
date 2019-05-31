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

              link = addTrailingSlashIfNecessary(link);

              function redirectOutsideAngular() {
                const queryParams = $location.search();
                link = removeTrailingSlashIfNecessary($stateParams.link);
                if (isAngularRoute(link)) $window.location.replace(`${__APP_SERVER_ENDPOINT__}404`);
                const queryParamsString = angular.equals(queryParams, {}) ? '' : `?${$httpParamSerializer(queryParams)}`;
                ContentPageService.page = {
                  redirectType: 'RedirectorPage',
                  content: '',
                  pageType: 'NoHeaderOrFooter',
                  title: ''
                };
                $window.location.replace(`${link}${queryParamsString}`);
                $rootScope.$destroy();
              }
              /** Hard coding undivided pages here to remove dependency on legacy CMS */
              if (link.match(new RegExp('^\/undivided\/participant'))) {
                promise = new Promise((resolve, reject) => {
                  resolve({ "pages": [{ "id": 2521, "submitButtonText": null, "clearButtonText": null, "onCompleteMessage": "<p>Thank you for your Request to Participate. You'll receive a confirmation email with more details shortly.<\/p>", "showClearButton": "0", "disableSaveSubmissions": "0", "enableLiveValidation": "0", "hideFieldLabels": "0", "displayErrorMessagesAtTop": "0", "disableAuthenicatedFinishAction": "0", "disableCsrfSecurityToken": "0", "pageType": "CenteredContentPage", "link": "\/undivided\/participant\/", "metaKeywords": null, "bodyClasses": null, "legacyStyles": "1", "migrated": "0", "richContent": "0", "requiresAngular": "1", "type": "website", "card": "summary", "inheritSideBar": "0", "uRLSegment": "participant", "title": "UNDIVIDED Participant Sign-up", "menuTitle": null, "content": "<h1 class=\"page-header\">UNDIVIDED\u2122 Participant Sign-up<\/h1>", "metaDescription": null, "extraMeta": null, "showInMenus": "1", "showInSearch": "1", "sort": "1", "hasBrokenFile": "0", "hasBrokenLink": "0", "reportClass": null, "canViewType": "LoggedInUsers", "canEditType": "Inherit", "version": "9", "sideBar": 1044, "parent": 484, "fields": [{ "id": 169, "name": "pageOne", "title": "First Page", "default": null, "sort": "1", "required": "0", "customErrorMessage": null, "customRules": null, "customSettings": null, "migrated": "1", "extraClass": null, "rightTitle": null, "showOnLoad": "1", "model": null, "header": "<p>This is a SIGN-UP REQUEST ONLY. We want to balance the racial make-up of groups and we're limited by capacity at sites, so registration is first-come, first-served. This is simply a request to participate. We'll send you a confirmation in the next few weeks if you're selected.<\/p><p><label>Important:<\/label> Please complete one form per person.<\/p><hr>", "description": null, "label": null, "footer": "<small>If you have any questions or need additional assistance, please email us at <a href=\"mailto:undivided@crossroads.net\" target=\"_blank\">undivided@crossroads.net<\/a><\/small>", "buttonText": null, "version": "22", "parent": 2521, "created": "2016-06-16T00:00:00-04:00", "className": "EditableFormStep" }, { "id": 170, "templateType": "Name", "name": "firstLastName", "title": "FirstLastName", "default": null, "sort": "2", "required": "0", "customErrorMessage": null, "customRules": null, "customSettings": null, "migrated": "1", "extraClass": null, "rightTitle": null, "showOnLoad": "1", "model": null, "header": null, "description": null, "label": "Full Name", "footer": null, "buttonText": null, "version": "18", "parent": 2521, "created": "2016-06-16T00:00:00-04:00", "className": "ProfileField" }, { "id": 171, "templateType": "Email", "name": "email", "title": "Email", "default": null, "sort": "3", "required": "0", "customErrorMessage": null, "customRules": null, "customSettings": null, "migrated": "1", "extraClass": null, "rightTitle": null, "showOnLoad": "1", "model": null, "header": null, "description": null, "label": "Email", "footer": null, "buttonText": null, "version": "18", "parent": 2521, "created": "2016-06-16T00:00:00-04:00", "className": "ProfileField" }, { "id": 172, "templateType": "Gender", "name": "gender", "title": "Gender", "default": null, "sort": "4", "required": "1", "customErrorMessage": null, "customRules": null, "customSettings": null, "migrated": "1", "extraClass": null, "rightTitle": null, "showOnLoad": "1", "model": null, "header": null, "description": null, "label": "What is your gender?", "footer": null, "buttonText": null, "version": "18", "parent": 2521, "created": "2016-06-16T00:00:00-04:00", "className": "ProfileField" }, { "id": 173, "templateType": "Ethnicity", "name": "ethnicity", "title": "Ethnicity", "default": null, "sort": "5", "required": "1", "customErrorMessage": null, "customRules": null, "customSettings": null, "migrated": "1", "extraClass": null, "rightTitle": null, "showOnLoad": "1", "model": null, "header": null, "description": null, "label": "What is your ethnicity?", "footer": null, "buttonText": null, "version": "18", "parent": 2521, "created": "2016-06-16T00:00:00-04:00", "className": "ProfileField" }, { "id": 174, "templateType": "Birthday", "name": "birthdate", "title": "Birthdate", "default": null, "sort": "6", "required": "1", "customErrorMessage": null, "customRules": null, "customSettings": null, "migrated": "1", "extraClass": null, "rightTitle": null, "showOnLoad": "1", "model": null, "header": null, "description": "<label><span class=\"small text-muted\">We will look for age diversity in the groups as well. <\/span><\/label>", "label": "What is your birthdate?", "footer": null, "buttonText": null, "version": "22", "parent": 2521, "created": "2016-06-16T00:00:00-04:00", "className": "ProfileField" }, { "id": 175, "templateType": "Location", "name": "site", "title": "Site", "default": null, "sort": "7", "required": "1", "customErrorMessage": null, "customRules": null, "customSettings": null, "migrated": "1", "extraClass": null, "rightTitle": null, "showOnLoad": "1", "model": null, "header": null, "description": null, "label": "What is your Crossroads site?", "footer": null, "buttonText": null, "version": "18", "parent": 2521, "created": "2016-06-16T00:00:00-04:00", "className": "ProfileField" }, { "id": 176, "templateType": "GroupsUndivided", "name": "undividedSession", "title": "UndividedSession", "default": null, "sort": "8", "required": "1", "customErrorMessage": null, "customRules": null, "customSettings": null, "migrated": "1", "extraClass": null, "rightTitle": null, "showOnLoad": "1", "model": null, "header": null, "description": "<label><span class=\"small text-muted\">Childcare is available at limited locations, please see below for details.<\/span><\/label>", "label": "What is your preferred UNDIVIDED Session?", "footer": null, "buttonText": null, "version": "24", "parent": 2521, "created": "2016-06-16T00:00:00-04:00", "className": "GroupParticipantField" }, { "id": 178, "templateType": "CoParticipant", "name": "CoParticipant", "title": "CoParticipant", "default": null, "sort": "10", "required": "0", "customErrorMessage": null, "customRules": null, "customSettings": null, "migrated": "1", "extraClass": null, "rightTitle": null, "showOnLoad": "1", "model": null, "header": null, "description": "<label><span class=\"small text-muted\">If yes, list them in priority order below. We'll do our best to honor requests while maintaining the desired mix of races within the group. The person or people you list must also sign up separately. We are unable to honor requests for facilitators at this time.<\/span><\/label>", "label": "<strong>Do you want to participate with people you know?<\/strong>", "footer": null, "buttonText": null, "version": "22", "parent": 2521, "created": "2016-06-16T00:00:00-04:00", "className": "GroupParticipantField" }, { "id": 179, "templateType": "Member", "name": "Member", "title": "Member", "default": null, "sort": "11", "required": "0", "customErrorMessage": null, "customRules": null, "customSettings": null, "migrated": "1", "extraClass": null, "rightTitle": null, "showOnLoad": "1", "model": null, "header": null, "description": null, "label": null, "footer": null, "buttonText": null, "version": "18", "parent": 2521, "created": "2016-06-16T00:00:00-04:00", "className": "GroupParticipantField" }], "created": "2016-06-16T00:00:00-04:00", "className": "CenteredContentPage" }] });
                });
              } else if (link.match(new RegExp('^\/undivided/facilitator'))) {
                promise = new Promise((resolve, reject) => {
                  resolve({ "pages": [{ "id": 2522, "submitButtonText": null, "clearButtonText": null, "onCompleteMessage": "<p>Thank you for your Request to Facilitate. You'll receive a confirmation email with more details shortly.<\/p>", "showClearButton": "0", "disableSaveSubmissions": "0", "enableLiveValidation": "0", "hideFieldLabels": "0", "displayErrorMessagesAtTop": "0", "disableAuthenicatedFinishAction": "0", "disableCsrfSecurityToken": "0", "pageType": "CenteredContentPage", "link": "\/undivided\/facilitator\/", "metaKeywords": null, "bodyClasses": null, "legacyStyles": "1", "migrated": "0", "richContent": "0", "requiresAngular": "1", "type": "website", "card": "summary", "inheritSideBar": "0", "uRLSegment": "facilitator", "title": "UNDIVIDED Facilitator Sign-up", "menuTitle": null, "content": "<h1 class=\"page-header\">UNDIVIDED\u2122 Facilitator Sign-up<\/h1>", "metaDescription": null, "extraMeta": null, "showInMenus": "1", "showInSearch": "1", "sort": "2", "hasBrokenFile": "0", "hasBrokenLink": "0", "reportClass": null, "canViewType": "LoggedInUsers", "canEditType": "Inherit", "version": "12", "sideBar": 1045, "parent": 484, "fields": [{ "id": 180, "name": "pageOne", "title": "First Page", "default": null, "sort": "1", "required": "0", "customErrorMessage": null, "customRules": null, "customSettings": null, "migrated": "1", "extraClass": null, "rightTitle": null, "showOnLoad": "1", "model": null, "header": "<p>This is a SIGN-UP REQUEST ONLY. We want to balance the racial make-up of groups and we're limited by capacity at sites, so registration is first-come, first-served. This is simply a request to participate. We'll send you a confirmation in the next few weeks if you're selected.<\/p><p><label>Important:<\/label> Please complete one form per person.<\/p><p><\/p><p><\/p><hr>", "description": null, "label": null, "footer": "<small>If you have any questions or need additional assistance, please email us at <a href=\"mailto:undivided@crossroads.net\" target=\"_blank\">undivided@crossroads.net<\/a><\/small>", "buttonText": null, "version": "28", "parent": 2522, "created": "2016-06-16T00:00:00-04:00", "className": "EditableFormStep" }, { "id": 181, "templateType": "Name", "name": "firstLastName", "title": "FirstLastName", "default": null, "sort": "2", "required": "0", "customErrorMessage": null, "customRules": null, "customSettings": null, "migrated": "1", "extraClass": null, "rightTitle": null, "showOnLoad": "1", "model": null, "header": null, "description": null, "label": "Full Name", "footer": null, "buttonText": null, "version": "22", "parent": 2522, "created": "2016-06-16T00:00:00-04:00", "className": "ProfileField" }, { "id": 182, "templateType": "Email", "name": "email", "title": "Email", "default": null, "sort": "3", "required": "0", "customErrorMessage": null, "customRules": null, "customSettings": null, "migrated": "1", "extraClass": null, "rightTitle": null, "showOnLoad": "1", "model": null, "header": null, "description": null, "label": "Email", "footer": null, "buttonText": null, "version": "22", "parent": 2522, "created": "2016-06-16T00:00:00-04:00", "className": "ProfileField" }, { "id": 183, "templateType": "Gender", "name": "gender", "title": "Gender", "default": null, "sort": "4", "required": "1", "customErrorMessage": null, "customRules": null, "customSettings": null, "migrated": "1", "extraClass": null, "rightTitle": null, "showOnLoad": "1", "model": null, "header": null, "description": null, "label": "What is your gender?", "footer": null, "buttonText": null, "version": "22", "parent": 2522, "created": "2016-06-16T00:00:00-04:00", "className": "ProfileField" }, { "id": 184, "templateType": "Ethnicity", "name": "ethnicity", "title": "Ethnicity", "default": null, "sort": "5", "required": "1", "customErrorMessage": null, "customRules": null, "customSettings": null, "migrated": "1", "extraClass": null, "rightTitle": null, "showOnLoad": "1", "model": null, "header": null, "description": null, "label": "What is your ethnicity?", "footer": null, "buttonText": null, "version": "22", "parent": 2522, "created": "2016-06-16T00:00:00-04:00", "className": "ProfileField" }, { "id": 185, "templateType": "Birthday", "name": "birthdate", "title": "Birthdate", "default": null, "sort": "6", "required": "1", "customErrorMessage": null, "customRules": null, "customSettings": null, "migrated": "1", "extraClass": null, "rightTitle": null, "showOnLoad": "1", "model": null, "header": null, "description": "<label><span class=\"small text-muted\">We will look for age diversity in the groups as well. Kids can participate in the MSM and HS ministry versions of Undivided at the same time.<\/span><\/label>", "label": "What is your birthdate?", "footer": null, "buttonText": null, "version": "24", "parent": 2522, "created": "2016-06-16T00:00:00-04:00", "className": "ProfileField" }, { "id": 186, "templateType": "Location", "name": "site", "title": "Site", "default": null, "sort": "7", "required": "1", "customErrorMessage": null, "customRules": null, "customSettings": null, "migrated": "1", "extraClass": null, "rightTitle": null, "showOnLoad": "1", "model": null, "header": null, "description": null, "label": "What is your Crossroads site?", "footer": null, "buttonText": null, "version": "22", "parent": 2522, "created": "2016-06-16T00:00:00-04:00", "className": "ProfileField" }, { "id": 187, "templateType": "GroupsUndivided", "name": "undividedSession", "title": "UndividedSession", "default": null, "sort": "8", "required": "1", "customErrorMessage": null, "customRules": null, "customSettings": null, "migrated": "1", "extraClass": null, "rightTitle": null, "showOnLoad": "1", "model": null, "header": null, "description": "Childcare is available at limited locations, please see below for details.", "label": "What is your preferred UNDIVIDED Session?", "footer": null, "buttonText": null, "version": "28", "parent": 2522, "created": "2016-06-16T00:00:00-04:00", "className": "GroupParticipantField" }, { "id": 189, "templateType": "FacilitatorTraining", "name": "facilitatorTraining", "title": "facilitatorTraining", "default": null, "sort": "10", "required": "1", "customErrorMessage": null, "customRules": null, "customSettings": null, "migrated": "1", "extraClass": null, "rightTitle": null, "showOnLoad": "1", "model": null, "header": null, "description": "<label><span class=\"small text-muted\"><mark>Required<\/mark> for all facilitators. Childcare will NOT be available for training sessions.<\/span><\/label>", "label": "Choose the appropriate facilitator training based on your experience.", "footer": null, "buttonText": null, "version": "24", "parent": 2522, "created": "2016-06-16T00:00:00-04:00", "className": "GroupParticipantField" }, { "id": 190, "templateType": "CoFacilitator", "name": "CoFacilitator", "title": "CoFacilitator", "default": null, "sort": "11", "required": "0", "customErrorMessage": null, "customRules": null, "customSettings": null, "migrated": "1", "extraClass": null, "rightTitle": null, "showOnLoad": "1", "model": null, "header": null, "description": "<label><span class=\"small text-muted\">If you know who you'd like to facilitate with, list them below. They must be of a different race from you. They must also sign up separately.<\/span><\/label>", "label": "<strong>Would you like to facilitate with someone you know?<\/strong>", "footer": null, "buttonText": null, "version": "26", "parent": 2522, "created": "2016-06-16T00:00:00-04:00", "className": "GroupParticipantField" }, { "id": 191, "templateType": "Leader", "name": "Leader", "title": "Leader", "default": null, "sort": "12", "required": "0", "customErrorMessage": null, "customRules": null, "customSettings": null, "migrated": "1", "extraClass": null, "rightTitle": null, "showOnLoad": "1", "model": null, "header": null, "description": null, "label": null, "footer": null, "buttonText": null, "version": "22", "parent": 2522, "created": "2016-06-16T00:00:00-04:00", "className": "GroupParticipantField" }], "created": "2016-06-16T00:00:00-04:00", "className": "CenteredContentPage" }] });
                });
              }
              else {
                redirectOutsideAngular()
              }

              var childPromise = promise.then(function (originalPromise) {

                if (originalPromise.pages.length > 0 && link !== '/') {
                  ContentPageService.page = originalPromise.pages[0];
                  // check if page is redirect
                  if (ContentPageService.page.pageType === 'RedirectorPage') {
                    if (ContentPageService.page.redirectionType === 'External') {
                      $window.location.href = ContentPageService.page.externalURL;
                      return;
                    } else {
                      redirectFlag = true;
                      return PageById.get({ id: ContentPageService.page.linkTo }).$promise;
                    }
                  } else if (ContentPageService.page.pageType === 'AngularRedirectPage') {
                    $state.go(ContentPageService.page.angularRoute);
                    return;
                  } else if (ContentPageService.page.requiresAngular === '0' && __IN_MAESTRO__ === '1') {
                    redirectOutsideAngular();
                    return;
                  }
                  return originalPromise;
                }

                redirectOutsideAngular();

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

  function isAngularRoute(link) {
    const angularRoutes = ['mycamps', 'camps', 'childcare', 'corkboard', 'invoices', 'undivided/participant', 'undivided/facilitator', 'mptools', 'reset-password', 'volunteer-sign-up', 'sign-up', 'trips'];
    return angularRoutes.find(route => link.includes(route));
  }
})();
