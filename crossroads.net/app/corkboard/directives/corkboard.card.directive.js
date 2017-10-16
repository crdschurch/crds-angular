(function () {
  'use strict';

  module.exports = CorkboardCard;

  CorkboardCard.$inject = [];

  function CorkboardCard() {
    return {
      restrict: 'EA',
      templateUrl: 'templates/corkboard.card.html',
      scope: {
        item: '=',
        showText: '=',
        postTypes: '='
      }
    };
  }
})();
