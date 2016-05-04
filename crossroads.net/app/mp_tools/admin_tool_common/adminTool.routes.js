(function() {

  'use strict';

  module.exports = AdminToolRoutes;

  AdminToolRoutes.$inject = ['$httpProvider', '$stateProvider'];

  /**
   * This holds all of common AdminTool routes
   */
  function AdminToolRoutes($httpProvider, $stateProvider) {

    $httpProvider.defaults.useXDomain = true;

    //TODO: I think this is done globally, not needed here, I think the above needs to be done globally
    $httpProvider.defaults.headers.common['X-Use-The-Force'] = true;

    $stateProvider
      .state('tools.adminGivingHistoryTool', {
        // This is a "launch" page for the tool, it will check access, etc, then forward
        // on to the actual page with the history.
        url: '/adminGivingHistoryTool',
        controller: 'AdminToolController as AdminToolController',
        templateUrl: 'admin_tool_common/adminTool.html',
        resolve: {
          $state: '$state',
          CRDS_TOOLS_CONSTANTS: 'CRDS_TOOLS_CONSTANTS',
          GivingHistoryService: 'GivingHistoryService',
          role: function(CRDS_TOOLS_CONSTANTS) {
            return CRDS_TOOLS_CONSTANTS.SECURITY_ROLES.FinanceTools;
          },

          goToFunction: function(GivingHistoryService, $state) {
            return function(donorId) {
              GivingHistoryService.impersonateDonorId = donorId;
              $state.go('tools.adminGivingHistory');
            };
          }
        }
      })
      .state('tools.adminGivingHistory', {
        url: '/adminGivingHistory',
        controller: 'GivingHistoryController as admin_giving_history_controller',
        templateUrl: 'admin_giving_history/adminGivingHistory.html',
        data: {
          isProtected: true,
          meta: {
            title: 'Giving History - Admin View',
            description: ''
          }
        }
      })
      .state('tools.adminRecurringGiftTool', {
        // This is a "launch" page for the tool, it will check access, etc, then forward
        // on to the actual page with the history.
        url: '/adminRecurringGiftTool',
        controller: 'AdminToolController as AdminToolController',
        templateUrl: 'admin_tool_common/adminTool.html',
        resolve: {
          $state: '$state',
          CRDS_TOOLS_CONSTANTS: 'CRDS_TOOLS_CONSTANTS',
          GiveTransferService: 'GiveTransferService',
          role: function(CRDS_TOOLS_CONSTANTS) {
            return CRDS_TOOLS_CONSTANTS.SECURITY_ROLES.FinanceTools;
          },

          goToFunction: function(GiveTransferService, $state) {
            return function(donorId) {
              GiveTransferService.impersonateDonorId = donorId;
              $state.go('tools.adminManageRecurringGifts');
            };
          }
        }
      })
      .state('tools.adminManageRecurringGifts', {
        url: '/adminManageRecurringGifts',
        controller: 'AdminRecurringGiftController as recurringGift',
        templateUrl: 'templates/adminManageRecurringGifts.html',
        data: {
          isProtected: true,
          meta: {
            title: 'Manage Recurring Gifts - Admin',
            description: ''
          }
        }
      })
      .state('tools.adminCheckinDashboard', {
        // This is a "launch" page for the tool, it will check access, etc, then forward
        // on to the actual page with the history.
        url: '/adminCheckinDashboard',
        controller: 'AdminToolController as AdminToolController',
        templateUrl: 'admin_tool_common/adminTool.html',
        resolve: {
          $state: '$state',
          CRDS_TOOLS_CONSTANTS: 'CRDS_TOOLS_CONSTANTS',
          role: function(CRDS_TOOLS_CONSTANTS) {
            return CRDS_TOOLS_CONSTANTS.SECURITY_ROLES.KidsClubTools;
          },
          goToFunction: function(GiveTransferService, $state) {
            return function(donorId) {
              GiveTransferService.impersonateDonorId = donorId;
              $state.go('tools.adminManageCheckinDashboard');
            }
          }
        }
      })
      .state('tools.adminManageCheckinDashboard', {
        url: '/adminManageCheckinDashboard',
        controller: 'AdminCheckinDashboardController as checkinDashboard',
        templateUrl: 'templates/adminCheckinDashboard.html',
        data: {
          isProtected: true,
          meta: {
            title: 'Checkin Dashboard - Admin',
            description: ''
          }
        }
      })
      .state('tools.adminEventSetup', {
        // This is a "launch" page for the tool, it will check access, etc, then forward
        // on to the actual page with the history.
        url: '/adminEventSetup',
        controller: 'AdminToolController as AdminToolController',
        templateUrl: 'admin_tool_common/adminTool.html',
        resolve: {
          $state: '$state',
          CRDS_TOOLS_CONSTANTS: 'CRDS_TOOLS_CONSTANTS',
          role: function(CRDS_TOOLS_CONSTANTS) {
            return CRDS_TOOLS_CONSTANTS.SECURITY_ROLES.KidsClubTools;
          },
          goToFunction: function($state) {
            return function() {
              $state.go('tools.eventSetup');
            };
          }
        }
      })
      .state('tools.eventSetup', {
        url: '/eventSetup',
        controller: 'EventSetupController as EventSetupCtrl',
        templateUrl: 'event_setup_tool/eventSetup.html',
        data: {
          isProtected: true,
          meta: {
            title: 'Manage Event Setup - Admin',
            description: ''
          }
        }
      });
  }

})();
