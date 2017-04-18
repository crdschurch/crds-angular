(function () {
'use strict';

module.exports = CorkboardListings;

CorkboardListings.$inject = ['$resource', '$http', '$cacheFactory'];
var postEndpoint = __CORKBOARD_API_ENDPOINT__ + 'api/posts/';

function CorkboardListings($resource, $http, $cacheFactory, $log) {
  return {
      InvalidateCache: function () {
          $cacheFactory.get('$http').remove(postEndpoint);
        },

      post: function (post) {
          return $resource(postEndpoint,
          post,
          {
              get: { method: 'GET', cache: true },
              query: { method: 'GET', cache: true, isArray: true },
              save: { method: 'POST' }
            });
        },

      flag: function () {
          return $resource(postEndpoint + 'flag/:id',
          { id: '@id' },
          {
              post: { method: 'POST', params: { id: '@id' } }
            });
        }
    };

}

})();
