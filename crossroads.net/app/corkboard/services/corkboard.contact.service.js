(function () {
  'use strict';

  module.exports = ContactAboutPost;
  var emailEndpoint = __GATEWAY_CLIENT_ENDPOINT__ + 'api/v1.0.0/send-email/';

  ContactAboutPost.$inject = ['$resource', '$http'];

  function ContactAboutPost($resource, $http) {

    return {
      post: function (post) {
        return $resource(emailEndpoint,
          post,
          {
            post: { method: 'POST' }
          });
      }
    };
  };
})();
