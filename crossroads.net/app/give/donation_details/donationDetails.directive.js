(function () {
  'use strict';  
  
  module.exports = DonationDetails;

  DonationDetails.$inject = ['$log'];

  function DonationDetails($log) {
        return  {
          restrict: 'EA',
          replace: true,
          scope: {
                amount: '=',
                program: '=',
                amountSubmitted: '=',
                programsIn: '=',
                showInitiative: '=',
                showFrequency: '='
            },
          templateUrl: 'donation_details/donationDetails.html',
          link: link
      };

      function link(scope, element, attrs) {

        scope.ministryShow = false;
        scope.program = scope.program === undefined ? null : scope.program;
        scope.amountError = amountError;
        scope.setProgramList = setProgramList;

        activate();
        /////////////////////////////////
        ////// IMPLMENTATION DETAILS ////
        /////////////////////////////////

        function activate(){
          if(!scope.program || !scope.program.ProgramId) {
            scope.program = scope.programsIn[0];
          }
          scope.ministryShow = scope.program.ProgramId !== scope.programsIn[0].ProgramId;
        }

        function amountError() {
            return (scope.amountSubmitted && scope.donationDetailsForm.amount.$invalid && 
                    scope.donationDetailsForm.$error.naturalNumber || 
                    scope.donationDetailsForm.$dirty && 
                    scope.donationDetailsForm.amount.$invalid && 
                    scope.amount !== undefined || 
                    scope.donationDetailsForm.$dirty && 
                    scope.amount === '');
        }

         function setProgramList (){
          return scope.ministryShow ? scope.program = '' : scope.program = scope.programsIn[0];
        }
      }
    }
})();
