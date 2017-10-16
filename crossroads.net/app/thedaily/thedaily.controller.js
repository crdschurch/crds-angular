(function() {
    'use strict';

    module.exports = TheDailyController;

    TheDailyController.$inject = [
        '$rootScope',
        '$log',
        '$state'
    ];

    function TheDailyController(
        $rootScope,
        $log,
        $state
    ) {

        $log.debug('Inside The Daily Controller');
        var vm = this;
    }
})();
