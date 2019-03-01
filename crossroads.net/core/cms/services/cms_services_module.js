var cms_services_module = angular.module('crossroads.core');
const contentful = require('contentful');

var contentfulClient = contentful.createClient({
    space: __CONTENTFUL_SPACE_ID__,
    accessToken: __CONTENTFUL_ACCESS_TOKEN__,
    environment: __CONTENTFUL_ENV__
});

cms_services_module.factory('SiteConfig', function ($resource) {
    return $resource(__CMS_CLIENT_ENDPOINT__ + 'api/SiteConfig/:id', { id: '@_id' }, { cache: true });
});

cms_services_module.factory('ContentBlock', function ($resource) {
    return $resource(__CMS_CLIENT_ENDPOINT__ + 'api/contentblock/:id', { id: '@_id' }, { cache: true });
});

cms_services_module.factory('SystemPage', function ($resource, $q) {
    var get = function (state) {
        var SystemPagesResource = $resource('/system-pages.json', { cache: true });
        var SystemPagesQuery = SystemPagesResource.get().$promise;

        return $q(function (resolve, reject) {
            SystemPagesQuery.then(function (response) {
                var page = response.systemPages.filter(page => page.stateName == state.state)[0];
                resolve(page);
            }).catch(function (response) {
                reject(response);
            });
        })
    }

    return {
        get: get
    }
});

cms_services_module.factory('SignUpForm', function ($location) {
    var get = function (state) {
        var params = {
            limit: 1,
            'content_type': 'sign_up_form',
            'fields.link': state.url
        };

        return contentfulClient.getEntries(params);
    }
    return { get: get }
});

cms_services_module.factory('Page', function ($resource, $location) {
    let cache = true;
    let params = {};

    const stageParam = $location.search().stage;
    if (stageParam) {
        params.stage = stageParam;
        cache = false;
    }

    return $resource(__CMS_CLIENT_ENDPOINT__ + 'api/Page?link=:url', params, { cache });
});

cms_services_module.factory('PageById', function ($resource, $location) {
    var stageParam = $location.search()['stage'];
    if (stageParam) {
        return $resource(__CMS_CLIENT_ENDPOINT__ + '/api/Page/?id=:id&STAGE=:stage', { id: '@_id', stage: stageParam }, { cache: false });
    }

    return $resource(__CMS_CLIENT_ENDPOINT__ + '/api/Page/?id=:id', { id: '@_id' }, { cache: true });
})
