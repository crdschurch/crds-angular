"use strict()";

(function(){

  module.exports = ServeTabs;

  ServeTabs.$inject = ['$log', 'Session'];

  function ServeTabs($log,Session){
    return {
      restrict: "EA",
      transclude: true,
      templateUrl : "my_serve/serveTabs.html",
      scope : {
        opportunity: '=',
        serveDate: '='
      }
    };
  }

})();
