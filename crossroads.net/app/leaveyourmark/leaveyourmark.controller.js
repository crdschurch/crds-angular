(function() {
    'use strict';

    module.exports = LeaveYourMarkController;

    LeaveYourMarkController.$inject = [
        '$rootScope',
        '$filter',
        '$log',
        '$state',
        'LeaveYourMark'
    ];

    function LeaveYourMarkController(
        $rootScope,
        $filter,
        $log,
        $state,
        LeaveYourMark
    ) {
        var vm = this;

        vm.campaigns = Array;

        for (let i=0; i<=1; i++) {
          vm.campaigns[i] = new Array;
        }

        vm.viewReady = false;

        activate();

        function activate() {
            LeaveYourMark.campaignSummary
                         .query({pledgeCampaignId: 1103})
                         .$promise
                         .then((data) => {
                             if(data && data.length && data.length>0) {
                               data.forEach(function(element, i) {
                                 vm.viewReady = true;
                                 vm.campaigns[i]['currentDay'] = element.currentDay;
                                 vm.campaigns[i]['totalDays'] = element.totalDays;
                                 vm.campaigns[i]['given'] = element.totalGiven;
                                 vm.campaigns[i]['committed'] = element.totalCommitted;
                                 vm.campaigns[i]['givenPercentage'] = $filter('number')(vm.campaigns[i]['given']  / vm.campaigns[i]['committed'] * 100, 0);
                                 vm.campaigns[i]['notStartedPercent'] = element.notStartedPercent;
                                 vm.campaigns[i]['behindPercent'] = element.behindPercent;
                                 vm.campaigns[i]['onPacePercent'] = element.onPacePercent;
                                 vm.campaigns[i]['aheadPercent'] = element.aheadPercent;
                                 vm.campaigns[i]['completedPercent'] = element.completedPercent;
                                });

                             }



                         })
                         .catch((err) => {
                            vm.viewReady = true;
                            console.error(err);
                         });
        }
    }
})();
