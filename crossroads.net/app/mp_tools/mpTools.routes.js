(function() {
  'use strict';

  module.exports = MPToolsRoutes;

  MPToolsRoutes.$inject = ['$stateProvider'];

  function MPToolsRoutes($stateProvider) {
    $stateProvider
    .state('tools', {
      parent: 'noHeaderOrFooter',
      abstract: true,
      url: '/mptools',
      templateUrl: 'mp_tools/tools.html',
      data: {
        hideMenu: true,
        isProtected: true,
        meta: {
          title: 'Tools',
          description: ''
        }
      },
      resolve: {
        loggedin: crds_utilities.checkLoggedin
      }
    })
    .state('tools.su2s', {
      url: '/su2s',
      controller: 'SignupToServeController as su2s',
      templateUrl: 'signup_to_serve/su2s.html'
    })
    .state('tools.kcApplicant', {
      url: '/kcapplicant',
      controller: 'KCApplicantController as applicant',
      templateUrl: 'kc_applicant/applicant.html',
      data: {
        isProtected: true,
        meta: {
          title: 'Kids Club Application',
          description: ''
        }
      },
      resolve: {
        loggedin: crds_utilities.checkLoggedin,
        MPTools: 'MPTools',
        Page: 'Page',
        CmsInfo: function(Page) {
          return Page.get({
            url: '/volunteer-application/kids-club/'
          });
        }
      }
    })
    .state('tools.tripParticipants', {
      url: '/tripParticipants',
      controller: 'TripParticipantController as trip',
      templateUrl: 'trip_participants/trip.html',
      resolve: {
        MPTools: 'MPTools',
        Trip: 'Trip',
        PageInfo: function(MPTools, Trip) {
          var params = MPTools.getParams();
          return Trip.TripFormResponses.get({
            selectionId: params.selectedRecord,
            selectionCount: params.selectedCount,
            recordId: params.recordId
          }).$promise.then(function(data) {
                // promise fulfilled
                return data;
              }, function(error) {
                // promise rejected, could log the error with: console.log('error', error);
                var data = {};
                data.errors = error;
                return error;
              });
        }
      }
    })
    .state('tools.tripPrivateInvite', {
      url: '/tripPrivateInvite',
      controller: 'TripPrivateInviteController as invite',
      templateUrl: 'trip_private_invite/invite.html',
      resolve: {
        MPTools: 'MPTools',
        Trip: 'Trip'
      }
    })
    .state('tools.createEvent', {
      url: '/create-event',
      template: '<add-event-tool></add-event-tool>',
      resolve: {
        MPTools: 'MPTools'
      }
    })
    .state('tools.volunteerContact', {
      url: '/volunteer-contact',
      template: '<volunteer-contact></volunteer-contact>',
      resolve: {
        MPTools: 'MPTools'
      }
    })
    .state('tools.checkBatchProcessor', {
      url: '/checkBatchProcessor',
      controller: 'CheckBatchProcessor as checkBatchProcessor',
      templateUrl: 'check_batch_processor/checkBatchProcessor.html',
      data: {
        isProtected: true,
        meta: {
          title: 'Check Batch Processor',
          description: ''
        }
      }
    })
    .state('tools.gpExport', {
      url: '/gpExport',
      controller: 'GPExportController as gpExport',
      templateUrl: 'gp_export/gpExport.html'
    })
    .state('tools.requestChildcare', {
      url: '/requestchildcare',
      template: '<request-childcare> </request-childcare>',
      resolve: { 
        LookupService: 'LookupService',
        Congregations: fetchCongregations,
        Ministries: fetchMinistries,
        MPTools: 'MPTools'
      }
    })
	.state('tools.childcareDecision', {
      url: '/childcaredecision',
      template: '<childcare-decision> </childcare-decision>',
    })
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
          GiveTransferService: 'GiveTransferService',
          role: function(CRDS_TOOLS_CONSTANTS) {
            return CRDS_TOOLS_CONSTANTS.SECURITY_ROLES.KidsClubTools;
          },

          goToFunction: function(GiveTransferService, $state) {
            return function(donorId) {
              GiveTransferService.impersonateDonorId = donorId;
              $state.go('tools.adminManageCheckinDashboard');
            };
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

      function fetchCongregations(LookupService, MPTools, $q) {
        var deferred = $q.defer();
        var lkups = LookupService.Congregations.query();
        lkups.$promise.then( (data) => {
          MPTools.congregations = data;
          deferred.resolve();
        }, (err) => {
          console.log(err);
          deferred.reject();
        });
        return deferred.promise;
      }

      function fetchMinistries(LookupService, MPTools, $q) {
        var deferred = $q.defer();
        var lkups = LookupService.Ministries.query();
        lkups.$promise.then( (data) => {
          MPTools.ministries = data;
          deferred.resolve();
        }, (err) => {
          console.log(err);
          deferred.reject();
        });
        return deferred.promise;
      }

  }
})();
