var cms_services_module = angular.module('crossroads.core');

cms_services_module.factory('SiteConfig', function ($resource) {
    return $resource(__CMS_CLIENT_ENDPOINT__ + '/api/SiteConfig/:id', { id: '@_id' }, {cache: true});
});

cms_services_module.factory('ContentBlock', function ($resource) {
    return $resource(__CMS_CLIENT_ENDPOINT__ + '/api/ContentBlock/:id', { id: '@_id' }, {cache: true});
});

cms_services_module.factory('SystemPage', function ($resource) {
    return $resource(__CMS_CLIENT_ENDPOINT__ + '/api/SystemPage/?StateName=:state', { state: '@_state' }, {cache: true});
});

cms_services_module.factory('Page', function ($resource, $location) {
  let cache = true;
  let params = { };

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