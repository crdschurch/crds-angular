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

        vm.currentDay = undefined;
        vm.totalDays = undefined;
        vm.given = undefined;
        vm.committed = undefined;
        vm.givenGercentage = undefined
        vm.notStartedPercent = undefined
        vm.behindPercent = undefined
        vm.onPacePercent = undefined
        vm.aheadPercent = undefined
        vm.completedPercent = undefined
        vm.viewReady = false;

        activate();

        function activate() {
            LeaveYourMark.campaignSummary
                         .get({pledgeCampaignId: 1103})
                         .$promise
                         .then((data) => {
                             if(data && data.length && data.length>0) {
                                 data = data[0]; //TODO: Temporary fix until UI is updated to handle the array
                             }
                            vm.viewReady = true;
                            vm.currentDay = data.currentDay;
                            vm.totalDays = data.totalDays;
                            vm.given = data.totalGiven;
                            vm.committed = data.totalCommitted;
                            vm.givenPercentage = $filter('number')(vm.given / vm.committed * 100, 0);
                            vm.notStartedPercent = data.notStartedPercent;
                            vm.behindPercent = data.behindPercent;
                            vm.onPacePercent = data.onPacePercent;
                            vm.aheadPercent = data.aheadPercent;
                            vm.completedPercent = data.completedPercent;
                         })
                         .catch((err) => {
                            vm.viewReady = true;
                            console.error(err);
                         });
        }
    }
})();
