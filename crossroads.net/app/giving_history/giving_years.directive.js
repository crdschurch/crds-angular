// TODO: This is a deprecated file due to the Pushpay work and is no longer in use
(function() {
  'use strict()';

  module.exports = GivingYears;

  GivingYears.$inject = ['$log', '$timeout'];

  function GivingYears($log, $timeout) {
    return {
      restrict: 'EA',
      transclude: true,
      templateUrl: 'templates/giving_years.html',
      scope: {
        selectedYear: '=',
        allYears: '=',
        onChange: '&'
      },
      link: link
    };

    function link(scope, el, attr) {
      // Wrapping the user-provided callback in a $timeout in order to make sure the digest cycle is complete and the
      // model (selectedYear) is updated in the caller's scope.
      scope.updatedYear = function() {
        $timeout(function() {
          scope.onChange();
        });
      };
    }
  }
})();