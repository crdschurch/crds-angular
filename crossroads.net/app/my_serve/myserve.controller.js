(function() {
  'use strict';
  var moment = require('moment');
  module.exports = MyServeController;

  MyServeController.$inject = [
    '$scope',
    '$rootScope',
    '$window',
    '$log',
    'ServeTeamFilterState',
    'Session',
    'ServeOpportunities',
    'Groups',
    'leader',
    'AUTH_EVENTS',
    'ServeTeamService',
    '$modal'
  ];

  function MyServeController(
    $scope,
    $rootScope,
    $window,
    $log,
    filterState,
    Session,
    ServeOpportunities,
    Groups,
    leader,
    AUTH_EVENTS,
    ServeTeamService,
    $modal
    ) {

    var vm = this;

    vm.convertToDate = convertToDate;
    vm.filterState = filterState;
    vm.groups = Groups;
    vm.lastDate = null;
    vm.loadMore = false;
    vm.loadNextMonth = loadNextWeek; //KD - Yeah, I know we should probably rename this to but we're trying to put in a quick fix for SU2S slowness - US7704. Bigger and better changes coming later
    vm.loadText = 'Load More';
    vm.original = [];
    vm.showButton = showButton;
    vm.showNoOpportunitiesMsg = showNoOpportunitiesMsg;
    vm.isLeader = leader.isLeader;
    vm.serveTeamService = ServeTeamService;

    vm.teams = [];

    activate();

    //////////////////////////
    // $rootScope listeners //
    //////////////////////////
    $rootScope.$on('personUpdated', personUpdateHandler);

    $rootScope.$on('filterDone', function(event, data) {
      vm.groups = data;
    });

    $rootScope.$on('filterByDates', filterByDates);

    $rootScope.$on('updateAfterSave', updateAfterSave);

    $rootScope.$on(AUTH_EVENTS.logoutSuccess, function(event, data) {
      vm.filterState.clearAll();
    });

    $rootScope.$on('$stateChangeStart', stateChangeStart);

    $window.onbeforeunload = onBeforeUnload;

    ////////////////////////////
    // Implementation Details //
    ////////////////////////////

    function activate() {
      if (vm.groups && vm.groups.length > 0) {
        vm.lastDate = formatDate(vm.groups[vm.groups.length - 1].day );
      }
      else {
        vm.lastDate= formatDate(new Date(), 42); //kd we search 6 weeks to see if we can find anything on load 
      }

      vm.serveTeamService.getTeamDetailsByLeader().then((data) => {
        vm.teams = data;
      }).catch((err) => {
        $log.debug("unable to retrieve teams")
      }).finally(() => {
        vm.ready = true;
      });
    }

    function addOneWeek(date) {
      var d = angular.copy(date);
      d.setDate(date.getDate() + 7);
      return d;
    }

    function checkChildForms() {
      var form = $scope.serveForm;
      var keys = _.keys(form);
      var dirty = [];
      _.each(keys, function(k) {
        if (_.startsWith(k, 'team')) {
          if (form[k].$dirty) {
            dirty.push(k);
          }
        }
      });

      if (dirty.length < 1) {
        $scope.serveForm.$setPristine();
      }
    }

    function convertToDate(date) {
      // date comes in as mm/dd/yyyy, convert to yyyy-mm-dd for moment to handle
      var d = new Date(date);
      return d;
    }

    function filterByDates(event, data) {
      loadOpportunitiesByDate(data.fromDate, data.toDate).then(function(opps) {
        vm.groups = opps;
        vm.original = opps;
        $rootScope.$broadcast('filterByDatesDone');
      },

      function(err) {
        $rootScope.$emit('notify', $rootScope.MESSAGES.generalError);
      });
    }

    /**
     * Takes a javascript date and returns a
     * string formated MM/DD/YYYY
     * @param date - Javascript Date
     * @param days to add - How many days to add to the original date passed in
     * @return string formatted in the way we want to display
     */
    function formatDate(date, days) {
      if (days === undefined) {
        days = 0;
      }

      var d = moment(date);
      d.add(days, 'd');
      return d.format('MM/DD/YYYY');
    }


    /**
     * This function will fetch a new set of serve opportunities between two dates
     * The dates passed in should be in epoch formatted in milliseconds
     * @param fromDate the epoch formatted beginning date
     * @param toDate the epoch formated end date
     * @returns a promise
     */
    function loadOpportunitiesByDate(fromDate, toDate) {
      return ServeOpportunities.ServeDays.query({
        id: Session.exists('userId'),
        from: fromDate / 1000,
        to: toDate / 1000
      }).$promise;
    }

    function loadNextWeek() {
      if (vm.groups[0].day !== undefined) {
        vm.loadMore = true;
        vm.loadText = 'Loading...';

        var lastDate = new Date(vm.groups[vm.groups.length - 1].day);
        lastDate.setDate(lastDate.getDate() + 1);

        var newDate = addOneWeek(new Date(lastDate));

        loadOpportunitiesByDate(lastDate.getTime(), newDate.getTime()).then(function(more) {
          if (more.length === 0) {
            $rootScope.$emit('notify', $rootScope.MESSAGES.serveSignupMoreError);
          } else {
            vm.lastDate = formatDate(newDate);
            _.each(more, function(m) {
              vm.groups.push(m);
            });
          }

          vm.loadMore = false;
          vm.loadText = 'Load More';
        }, function(e) {
          // error
          vm.loadMore = false;
          vm.loadText = 'Load More';
        });
      }
    }

    function onBeforeUnload() {
      checkChildForms();
      if ($scope.serveForm.$dirty) {
        return '';
      }
    }

    function personUpdateHandler(event, data) {
      vm.groups = angular.copy(vm.original);
      _.each(vm.groups, function(group) {
        _.each(group.serveTimes, function(serveTime) {
          _.each(serveTime.servingTeams, function(servingTeam) {
            _.each(servingTeam.members, function(member) {
              if (member.contactId === data.contactId) {
                member.name = data.nickName === null ? data.firstName : data.nickName;
                member.nickName = data.nickName;
                member.lastName = data.lastName;
                member.emailAddress = data.emailAddress;
              }
            });
          });
        });
      });

      vm.original = angular.copy(vm.groups);
      $rootScope.$broadcast('rerunFilters', vm.groups);
    }

    function showButton() {
      if (showNoOpportunitiesMsg()) {
        return false;
      } else {
        return !filterState.isActive();
      }
    }

    function showNoOpportunitiesMsg() {
      return vm.groups.length < 1 || totalServeTimesLength() === 0;
    }

    function stateChangeStart(event, toState, toParams, fromState, fromParams) {
      if ($scope.serveForm !== undefined) {
        checkChildForms();
        if ($scope.serveForm.$dirty) {
          if (!$window.confirm('Are you sure you want to leave this page?')) {
            event.preventDefault();
            return;
          }
        }
      }
    }

    function totalServeTimesLength() {
      var len = _.reduce(vm.groups, function(total, n) {
        return total + n.serveTimes.length;
      },

      0);
      return len;
    }

    function updateAfterSave(event, data) {
      _.each(vm.groups, function(group) {
        _.each(group.serveTimes, function(serveTime) {
          _.each(serveTime.servingTeams, function(servingTeam) {
            if (servingTeam.groupId === data.groupId) {
              _.each(data.eventIds, function(eventId) {
                if (servingTeam.eventId === eventId) {
                  _.each(servingTeam.members, function(member) {
                    if (member.contactId === data.member.contactId) {
                      member.serveRsvp = angular.copy(data.member.serveRsvp);
                    }
                  });
                }
              });
            }
          });
        });
      });
    }

  }

})();
