(function() {
  'use strict';

  module.exports = StaffContact;

  StaffContact.$inject = ['$resource'];

  function StaffContact($resource) {
    return $resource(__GATEWAY_CLIENT_ENDPOINT__ + 'api/primarycontacts');
  }
})();
