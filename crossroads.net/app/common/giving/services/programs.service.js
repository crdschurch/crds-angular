(function() {
  'use strict';
  module.exports = Programs;

  Programs.$inject = ['$resource'];

  function Programs($resource) {
    return {
      Programs: $resource(__GATEWAY_CLIENT_ENDPOINT__ + 'api/programs/:programType', {programType: '@programType'}, {
        get: { method: 'GET', isArray: true }
      }),
      ProgramsForEventTool: $resource(__GATEWAY_CLIENT_ENDPOINT__ + 'api/programs/event-tool')
    };
  }

})();
