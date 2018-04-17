'use strict()';
(function() {

  var formatDate = crds_utilities.formatDate;

  module.exports = RefineDirective;

  RefineDirective.$inject = ['$rootScope', 'ServeTeamFilterState', 'screenSize', '$modal'];

  function RefineDirective($rootScope, filterState, screenSize, $modal) {
    return {
      restrict: 'E',
      replace: true,
      templateUrl: 'refine/refineList.html',
      scope: {
        'servingDays': '=servingDays',
        'original': '=?original',
        'filterBoxes': '=?filterBoxes',
        'lastDate': '=lastDate'
      },
      link: link
    };

    function link(scope, el, attr) {
      scope.applyFamilyFilter = applyFamilyFilter;
      scope.applySignUpFilter = applySignUpFilter;
      scope.applyTeamFilter = applyTeamFilter;
      scope.applyTimeFilter = applyTimeFilter;
      scope.clearFilters = clearFilters;
      scope.filterAll = filterAll;
      scope.filterFromDate = formatDate(new Date());
      scope.format = 'MM/dd/yyyy';
      scope.getUniqueMembers = getUniqueMembers;
      scope.getUniqueSignUps = getUniqueSignUps;
      scope.getUniqueTeams = getUniqueTeams;
      scope.getUniqueTimes = getUniqueTimes;
      scope.isDateCollapsed = $rootScope.mobile;
      scope.isFilterCollapsed = $rootScope.mobile;
      scope.isFilterSet = isFilterSet;
      scope.open = openDateModal;
      scope.toggleDateCollapse = toggleDateCollapse;
      scope.toggleFilterCollapse = toggleFilterCollapse;
      scope.toggleFamilyMember = toggleFamilyMember;
      scope.toggleSignedUp = toggleSignedUp;
      scope.toggleTeam = toggleTeam;
      scope.toggleTime = toggleTime;
      scope.uniqueDays = [];
      scope.uniqueMembers = [];
      scope.uniqueSignUps = [];
      scope.uniqueTeams = [];
      scope.uniqueTimes = [];

      activate();

      $rootScope.$on('rerunFilters', function(event, data) {
        // Update the entire data with the new data
        scope.servingDays = data;
        initServeArrays();
        filter(data, false);
        $rootScope.$emit('filterDone', scope.servingDays);
      });

      //////////////////////////////////

      function activate() {
        initServeArrays();
        filter(scope.servingDays);
      }

      function applyFamilyFilter() {
        if (filterState.memberIds.length > 0) {
          var serveDay = [];
          _.each(scope.servingDays, function(day) {
            var serveTimes = [];
            _.each(day.serveTimes, function(serveTime) {
              var servingTeams = [];
              _.each(serveTime.servingTeams, function(team) {
                var theTeam = team;
                var members = _.filter(team.members, function(m) {
                  return _.find(filterState.memberIds, function(familyMember) {
                    return familyMember === m.contactId;
                  });
                });

                if (members.length > 0) {
                  theTeam.members = members;
                  servingTeams.push(theTeam);
                }
              });
              if (servingTeams.length > 0) {
                serveTimes.push({
                  time: serveTime.time,
                  'servingTeams': servingTeams
                });
              }
            });
            if (serveTimes.length > 0) {
              serveDay.push({
                day: day.day,
                eventType: day.eventType,
                eventTypeId: day.eventTypeId,
                serveTimes: serveTimes
              });
            }
          });
          if (serveDay.length > 0) {
            scope.servingDays = serveDay;
          }
        }
      }

      function applySignUpFilter() {
        if (filterState.signUps.length > 0) {
          var serveDay = [];
          _.each(scope.servingDays, function(day) {
            var serveTimes = [];
            _.each(day.serveTimes, function(serveTime) {
              var servingTeams = [];
              _.each(serveTime.servingTeams, function(team) {
                var theTeam = team;
                var members = _.filter(team.members, function(m) {
                  return _.find(filterState.signUps, function(signUp) {
                    if ((m.serveRsvp == null) && (signUp.attending == null)) {
                      return true;
                    } else {
                      if (m.serveRsvp != null) {
                        return signUp.attending === m.serveRsvp.attending;
                      } else {
                        return false;
                      }
                    }
                  });
                });
                if (members.length > 0) {
                  theTeam.members = members;
                  servingTeams.push(theTeam);
                }
              });
              if (servingTeams.length > 0) {
                serveTimes.push({
                  time: serveTime.time,
                  'servingTeams': servingTeams
                });
              }
            });
            if (serveTimes.length > 0) {
              serveDay.push({
                day: day.day,
                eventType: day.eventType,
                eventTypeId: day.eventTypeId,
                serveTimes: serveTimes
              });
            }
          });
          scope.servingDays = serveDay;
        }
      }

      function applyTimeFilter() {
        if (filterState.times.length > 0) {
          var serveDay = [];
          _.each(scope.servingDays, function(day) {
            var times = _.filter(day.serveTimes, function(serveTime) {
              return _.find(filterState.times, function(ftimes) {
                return ftimes === serveTime.time;
              });
            });
            if (times.length > 0) {
              serveDay.push({
                day: day.day,
                eventType: day.eventType,
                eventTypeId: day.eventTypeId,
                serveTimes: times
              });
            };
          });
          scope.servingDays = serveDay;
        }
      }

      function applyTeamFilter() {
        if (filterState.teams.length > 0) {
          var serveDay = [];
          _.each(scope.servingDays, function(day) {
            var serveTimes = [];
            _.each(day.serveTimes, function(serveTime) {
              var servingTeams = _.filter(serveTime.servingTeams, function(team) {
                return _.find(filterState.teams, function(t) {
                  return team.groupId === t;
                });
              });
              if (servingTeams.length > 0) {
                serveTimes.push({
                  time: serveTime.time,
                  'servingTeams': servingTeams
                });
              }
            });
            serveDay.push({
              day: day.day,
              eventType: day.eventType,
              eventTypeId: day.eventTypeId,
              serveTimes: serveTimes
            });
          });
          scope.servingDays = serveDay;
        }
      };

      function clearFilters() {
        filterState.clearAll();
        _.each(scope.uniqueMembers, function(member) {
          member.selected = false;
        });
        _.each(scope.uniqueTeams, function(team) {
          team.selected = false;
        });
        _.each(scope.uniqueTimes, function(time) {
          time.selected = false;
        });
        _.each(scope.uniqueSignUps, function(signUp) {
          signUp.selected = false;
        })
        filterAll();
      }

      function filter(data, copyScope = true) {
        filterTimes();
        filterTeams();
        filterFamily();
        getUniqueMembers();
        getUniqueSignUps();
        getUniqueTeams();
        getUniqueTimes();
        initCheckBoxes();
        if (copyScope) {
          scope.original = angular.copy(data);
        }
        filterAll(copyScope);
      }

      function filterAll(copyScope = true) {
        if (copyScope) {
          scope.servingDays = angular.copy(scope.original);
        }
        applyFamilyFilter();
        applySignUpFilter();
        applyTeamFilter();
        applyTimeFilter();
      }

      function filterFamily() {
        _.each(scope.serveTeams, function(serveTeam) {
          _.each(serveTeam.members, function(member) {
            scope.serveMembers.push(member);
          });
        });
      }

      function filterTeams() {
        _.each(scope.times, function(serveTime) {
          _.each(serveTime.servingTeams, function(serveTeam) {
            scope.serveTeams.push(serveTeam);
          });
        });
      }

      function filterTimes() {
        _.each(scope.servingDays, function(servingDay) {
          _.each(servingDay.serveTimes, function(serveTime) {
            scope.times.push(serveTime);
          });
        });
      }

      function getUniqueMembers() {
        scope.uniqueMembers = _.chain(scope.serveMembers).map(function(m) {
          return {
            name: m.name,
            lastName: m.lastName,
            contactId: m.contactId
          };
        }).uniq('contactId').value();
      }

      function getUniqueSignUps() {
        //var signUps = [];
        var yes = {
          'name': 'Yes',
          'id': 1,
          'selected': false,
          'attending': true
        };
        var no = {
          'name': 'No',
          'id': 2,
          'selected': false,
          'attending': false
        };
        var nada = {
          'name': 'Nada',
          'id': 3,
          'selected': false,
          'attending': null
        };
        scope.uniqueSignUps.push(yes);
        scope.uniqueSignUps.push(no);
        scope.uniqueSignUps.push(nada);
      }

      function getUniqueTeams() {
        scope.uniqueTeams = _.chain(scope.serveTeams).map(function(team) {
          return {
            'name': team.name,
            'groupId': team.groupId
          };
        }).uniq('groupId').value();
      }

      function getUniqueTimes() {
        scope.uniqueTimes = _.chain(scope.times).map(function(time) {
          return {
            time: time.time
          };
        }).uniq('time').sortBy(function(n) {
          return n.time;
        }).value();
      }

      function initCheckBoxes() {
        _.each(scope.uniqueMembers, function(member) {
          var found = filterState.findMember(member.contactId);
          if (found !== undefined) {
            member.selected = true;
          }
        });
        _.each(scope.uniqueTeams, function(team) {
          var found = filterState.findTeam(team.groupId);
          if (found !== undefined) {
            team.selected = true;
          }
        });
        _.each(scope.uniqueTimes, function(time) {
          var found = filterState.findTime(time.time);
          if (found !== undefined) {
            time.selected = true;
          }
        });
      }

      function initServeArrays() {
        scope.serveMembers = [];
        scope.serveTeams = [];
        scope.times = [];
      }

      function isFilterSet() {
        return filterState.isActive();
      }
      // Modals
      function openDateModal () {
        var modalInstance = $modal.open({
          templateUrl: 'refine/serveModalContent.html',
          backdrop: true,
          size: 'sm',
          controller: 'ServeModalController as modal',
          openedClass: 'crds-legacy-styles',
          resolve: {
            dates: function () {
              return {
                'fromDate': scope.filterFromDate,
                'toDate': scope.lastDate
              };
            }
          }
        });
        modalInstance.result.then(function (dates) {
          scope.filterFromDate = dates.fromDate;
          scope.lastDate = dates.toDate;
          filterAll();
        });
      }

      function toggleFilterCollapse() {
        if ($rootScope.mobile) {
          scope.isFilterCollapsed = !scope.isFilterCollapsed;
        }
      }

      function toggleDateCollapse() {
        if ($rootScope.mobile) {
          scope.isDateCollapsed = !scope.isDateCollapsed;
        }
      }

      function toggleFamilyMember(member) {
        if (member.selected) {
          filterState.addFamilyMember(member.contactId);
        } else {
          filterState.removeFamilyMember(member.contactId);
        }
        filterAll();
      }

      function toggleSignedUp(signUp) {
        if (signUp.selected) {
          filterState.addSignUp(signUp);
        } else {
          filterState.removeSignUp(signUp);
        }
        filterAll();
      }

      function toggleTeam(team) {
        if (team.selected) {
          filterState.addTeam(team.groupId);
        } else {
          filterState.removeTeam(team.groupId);
        }
        filterAll();
      }

      function toggleTime(time) {
        if (time.selected) {
          filterState.addTime(time.time);
        } else {
          filterState.removeTime(time.time);
        }
        filterAll();
      }
    }
  }
})();
