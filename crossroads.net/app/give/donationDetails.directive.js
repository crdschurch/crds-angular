require('./donation-details.html');﻿

(function () {
    angular
    .module("donation-details",[])
    .directive("donationDetails", ['$log','getPrograms', donationDetails]);

    function donationDetails($log , getPrograms) {
        var directive = {
          restrict: 'EA',
          replace: true,
          scope: {
                progType: "=progtype",
            },
          templateUrl: 'give/donation-details.html',
          link: link
      };
      return directive;

      function link(scope, element, attrs) {
        console.log(scope);
        getPrograms.Programs.get({programType: scope.progType}).$promise.then(function(response){
          scope.programs = response;
        });
      }
    }
})()
