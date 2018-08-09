(function() {
  'use strict';

  module.exports = PasswordService;

  PasswordService.$inject = ['$resource'];

  function PasswordService($resource) {
    return {
      ResetRequest: $resource(__GATEWAY_CLIENT_ENDPOINT__ + 'api/requestpasswordreset'),
      VerifyResetToken: $resource(__GATEWAY_CLIENT_ENDPOINT__ + 'api/verifyresettoken/:token/'),
      EmailExists: $resource(__GATEWAY_CLIENT_ENDPOINT__ + 'api/lookup/0/find/'),
      ResetPassword: $resource(__GATEWAY_CLIENT_ENDPOINT__ + 'api/resetpassword'),
      VerifyPassword: $resource(__GATEWAY_CLIENT_ENDPOINT__ + 'api/verifypassword')
    };
  }

})();
