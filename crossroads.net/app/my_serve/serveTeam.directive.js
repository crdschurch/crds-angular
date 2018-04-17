(function() {
  'use strict()';

  var moment = require('moment');

  module.exports = ServeTeam;

  ServeTeam.$inject = ['$rootScope',
    '$log',
    'Session',
    'ServeOpportunities',
    'OpportunityCapacityService',
    '$modal',
    'growl'
  ];

  function ServeTeam($rootScope,
      $log,
      Session,
      ServeOpportunities,
      Capacity,
      $modal,
      growl) {

    return {
      restrict: 'EA',
      transclude: true,
      templateUrl: 'my_serve/serveTeam.html',
      replace: true,
      scope: {
        team: '=',
        opportunity: '=',
        oppServeDate: '='
      },
      link: link
    };

    function link(scope, el, attr) {

      scope.buttonDisabled = false;
      scope.buttonState = buttonState;
      scope.cancel = cancel;
      scope.changeFromDate = changeFromDate;
      scope.changeToDate = changeToDate;
      scope.currentActiveTab = null;
      scope.currentMember = null;
      scope.datesDisabled = true;
      scope.dateOptions = {
        formatYear: 'yy',
        startingDay: 1,
        showWeeks: 'false'
      };
      scope.datePickers = {
        fromOpened: false,
        toOpened: false
      };
      scope.displayEmail = displayEmail;
      scope.editProfile = editProfile;
      scope.frequency = getFrequency();
      scope.format = 'MM/dd/yyyy';
      scope.formErrors = {
        role: false,
        signup: false,
        frequency: false,
        from: false,
        to: false,
        dateRange: false
      };
      scope.panelStates = { };
      scope.populateDates = populateDates;
      scope.processing = false;
      scope.isActiveTab = isActiveTab;
      scope.isCollapsed = true;
      scope.isFormValid = isFormValid;
      scope.modalInstance = {};
      scope.openFromDate = openFromDate;
      scope.openToDate = openToDate;
      scope.passedDeadlineMsg = passedDeadlineMsg;
      scope.roleChanged = roleChanged;
      scope.roles = null;
      scope.saveRsvp = saveRsvp;
      scope.selectedRole = null;
      scope.signedup = null;
      scope.showEdit = false;
      scope.showIcon = showIcon;
      scope.togglePanel = togglePanel;

      scope.isTeamTab = isTeamTab;
      scope.isTeamLeader = isTeamLeader;

      scope.capacityReady = false;

      //////////////////////////////////////

      function isTeamTab() {
        // TODO UI!!! VERIFY LOGIC
        return scope.currentMember && !!scope.currentMember.groupId;
      }

      function isTeamLeader(team) {
        // TODO UI!!! IMPLEMENT THIS
        return true;
      }

      function allowProfileEdit() {
        var cookieId = Session.exists('userId');
        if (cookieId !== undefined) {
          scope.showEdit = Number(cookieId) === scope.currentMember.contactId;
        } else {
          scope.showEdit = false;
        }
      }

      function buttonState() {
        return scope.processing || scope.buttonDisabled;
      }

      function cancel() {
        // panel is open, close it
        // but first, revert back to original state...
        if (scope.panelStates[scope.currentMember.contactId] !== undefined) {
          scope.currentMember.serveRsvp = scope.panelStates[scope.currentMember.contactId];
          scope.currentMember.showFrequency = false;
        }

        // reset panel states
        scope.panelStates[scope.currentMember.contactId] = undefined;

        // should we reset the form to pristine
        if (!isFormDirty()) {
          /*var teamFormName = 'teamForm-' + scope.team.index;*/
          /*var form = scope['teamForm-' + scope.team.index];*/
          scope.teamForm.$setPristine();
        }

        togglePanel(null);

      }

      function changeFromDate() {
        if (scope.currentMember.currentOpportunity !== undefined &&
            scope.currentMember.currentOpportunity.fromDt !== undefined) {
          var m = moment(scope.currentMember.currentOpportunity.fromDt);
          if (m.isValid()) {
            scope.formErrors.dateRange = false;
            scope.formErrors.from = false;
          }
        }
      }

      function changeToDate() {
        if (scope.currentMember.currentOpportunity !== undefined &&
            scope.currentMember.currentOpportunity.toDt !== undefined) {
          var m = moment(scope.currentMember.currentOpportunity.toDt);
          if (m.isValid()) {
            scope.formErrors.dateRange = false;
            scope.formErrors.to = false;
          }
        }

      }

      function displayEmail(emailAddress) {
        if (!emailAddress) {
          return false;
        }

        if (emailAddress.length > 0) {
          return true;
        }

        return false;
      }

      function editProfile(personToEdit) {
        var modalInstance = $modal.open({
          templateUrl: 'profile/editProfile.html',
          backdrop: true,
          controller: 'ProfileModalController as modal',
          openedClass: 'crds-legacy-styles',
          // This is needed in order to get our scope
          // into the modal - by default, it uses $rootScope
          scope: scope,
          resolve: {
            person: function() {
              return personToEdit;
            }
          }
        });
        modalInstance.result.then(function(person) {
          personToEdit.name = person.nickName === null ? person.firstName : person.nickName;
          $rootScope.$emit('personUpdated', person);
        });
      }

      function getFrequency() {
        var dateTime = moment(scope.oppServeDate + ' ' + scope.opportunity.time);
        var weeklyLabel = moment(scope.oppServeDate).format('dddd') + 's' + ' ' + dateTime.format('h:mma');

        var once = {
          value: 0,
          text: 'Once ' + dateTime.format('M/D/YYYY h:mma')
        };
        var everyWeek = {
          value: 1,
          text: 'Every Week ' + weeklyLabel
        };
        var everyOtherWeek = {
          value: 2,
          text: 'Every Other Week ' + weeklyLabel
        };

        return [once, everyWeek, everyOtherWeek];
      }

      function isActiveTab(memberName) {
        return memberName === scope.currentActiveTab;
      }

      function isFormDirty() {
        var dirty = false;

        // are there any other unsaved changes
        var possible = _.filter(scope.team.members, function(m) {
          if (m.contactId === scope.currentMember.contactId) {
            return false;
          }

          var keys = _.keys(scope.panelStates);
          if (_.indexOf(keys, '' + m.contactId) > -1) {
            return true;
          }

          return false;
        });

        _.each(possible, function(e) {
          if (e.serveRsvp !== scope.panelStates[e.contactId]) {
            dirty = true;
          }
        });

        return dirty;
      }

      function isFormValid() {
        var validForm = {
          valid: true
        };
        validForm.valid = true;
        if (scope.currentMember.serveRsvp == null) {
          validForm.valid = false;
          scope.formErrors.role = true;
        } else if (scope.currentMember.serveRsvp.roleId === undefined ||
            scope.currentMember.serveRsvp.roleId === null ||
            scope.currentMember.currentOpportunity === null) {
          validForm.valid = false;
          scope.formErrors.frequency = true;
        } else {
          if ((scope.currentMember.currentOpportunity === undefined ||
              scope.currentMember.currentOpportunity === null ||
              scope.currentMember.currentOpportunity.frequency === null ||
              scope.currentMember.currentOpportunity.frequency === undefined) && scope.currentMember.showFrequency) {
            validForm.valid = false;
            scope.formErrors.frequency = true;
          }

          if (scope.currentMember.currentOpportunity !== undefined &&
              scope.currentMember.currentOpportunity.toDt === undefined) {
            validForm.valid = false;
            scope.formErrors.to = true;
          }

          if (scope.currentMember.currentOpportunity !== undefined &&
              scope.currentMember.currentOpportunity.fromDt === undefined) {
            validForm.valid = false;
            scope.formErrors.from = true;
          }

          if (validForm.valid) {
            var startDate;
            var endDate;
            try {
              startDate = parseDate(scope.currentMember.currentOpportunity.toDt);
            } catch (ex) {
              validForm.valid = false;
              scope.formErrors.from = true;
            }

            try {
              endDate = parseDate(scope.currentMember.currentOpportunity.fromDt);
            } catch (ex) {
              validForm.valid = false;
              scope.formErrors.to = true;
            }

            if (startDate < endDate) {
              validForm.valid = false;
              scope.formErrors.dateRange = true;
            }
          }
        }

        return validForm;
      }

      function openFromDate($event) {
        $event.preventDefault();
        $event.stopPropagation();
        scope.datePickers.fromOpened = true;
      }

      function openToDate($event) {
        $event.preventDefault();
        $event.stopPropagation();
        scope.datePickers.toOpened = true;
      }

      function parseDate(stringDate) {
        var m = moment(stringDate);

        if (!m.isValid()) {
          var dateArr = stringDate.split('/');
          var dateStr = dateArr[2] + ' ' + dateArr[0] + ' ' + dateArr[1];

          // https://github.com/moment/moment/issues/1407
          // moment("2014 04 25", "YYYY MM DD"); // string with format
          m = moment(dateStr, 'YYYY MM DD');

          if (!m.isValid()) {
            throw new Error('Parse Date Failed Moment Validation');
          }
        }

        $log.debug('date: ' + m.format('X'));
        return m.format('X');
      }

      function passedDeadlineMsg(id) {
        return _.result(_.find($rootScope.MESSAGES, 'id', id), 'content');
      }

      function populateDates() {
        if (scope.currentMember !== null) {
          scope.currentMember.currentOpportunity.fromDt = scope.oppServeDate;
          switch (scope.currentMember.currentOpportunity.frequency.value) {
            case null:
              scope.currentMember.currentOpportunity.fromDt = null;
              scope.currentMember.currentOpportunity.toDT = null;
              scope.datesDisabled = true;
              break;
            case 0:

              // once...
              scope.formErrors.frequency = false;
              scope.currentMember.currentOpportunity.fromDt = scope.oppServeDate;
              scope.currentMember.currentOpportunity.toDt = scope.oppServeDate;
              scope.datesDisabled = true;
              break;
            default:

              // every  or everyother
              scope.formErrors.frequency = false;
              var roleId = (scope.currentMember.serveRsvp.roleId === 0) ?
                scope.currentMember.roles[0].roleId : scope.currentMember.serveRsvp.roleId;
              ServeOpportunities.LastOpportunityDate.get({
                id: roleId
              }, function(ret) {
                var dateNum = Number(ret.date * 1000);
                var toDate = new Date(dateNum);
                scope.currentMember.currentOpportunity.toDt =
                  (toDate.getMonth() + 1) + '/' + toDate.getDate() + '/' + toDate.getFullYear();
              });

              scope.datesDisabled = false;
              break;
          }
        }
      }

      function roleChanged(selectedRole) {
        scope.formErrors.role = false;
        scope.selectedRole = selectedRole;
        if (scope.currentMember.serveRsvp === undefined) {
          scope.currentMember.serveRsvp = {
            isSaved: false
          };
        } else {
          scope.currentMember.serveRsvp.isSaved = false;
        }

        if (scope.selectedRole === undefined) {
          scope.currentMember.serveRsvp.attending = false;
          scope.currentMember.showFrequency = false;
          scope.currentMember.currentOpportunity = {
            frequency: scope.frequency[0]
          };
          populateDates();
        } else {
          scope.currentMember.serveRsvp.attending = true;
          scope.currentMember.showFrequency = true;
        }

        determineSaveButtonState();
      }

      function savePanel(member, force) {
        if (force) {
          scope.panelStates[member.contactId] = angular.copy(member.serveRsvp);
          return true;
        }

        if (scope.panelStates[member.contactId] === undefined) {
          scope.panelStates[member.contactId] = angular.copy(member.serveRsvp);
          return true;
        }

        return false;
      }

      function saveRsvp() {
        var validForm = isFormValid();

        if (!validForm.valid) {
          $rootScope.$emit('notify', $rootScope.MESSAGES.generalError);
          return false;
        }

        scope.processing = true;
        var rsvp = {};
        rsvp.contactId = scope.currentMember.contactId;
        rsvp.opportunityId = scope.currentMember.serveRsvp.roleId;
        rsvp.opportunityIds = _.map(scope.currentMember.roles, function(role) {
          return role.roleId;
        });

        rsvp.eventTypeId = scope.team.eventTypeId;
        rsvp.endDate = parseDate(scope.currentMember.currentOpportunity.toDt);
        rsvp.startDate = parseDate(scope.currentMember.currentOpportunity.fromDt);
        if (scope.currentMember.serveRsvp.roleId !== 0) {
          rsvp.signUp = true;
        } else {
          rsvp.signUp = false;
        }

        rsvp.alternateWeeks = (scope.currentMember.currentOpportunity.frequency.value === 2);
        ServeOpportunities.SaveRsvp.save(rsvp, function(updatedEvents) {
          if (rsvp.signUp) {
            $rootScope.$emit('notify', $rootScope.MESSAGES.SU2S_Saving_Message);
          } else {
            var saveMessage = 'You have indicated that [participant] is not available for [team] on [date]';
            saveMessage = saveMessage.replace('[participant]', scope.currentActiveTab);
            saveMessage = saveMessage.replace('[team]', scope.team.name);
            saveMessage = saveMessage.replace('[date]', scope.oppServeDate);
            growl.success(saveMessage);
          }

          scope.currentMember.serveRsvp.isSaved = true;
          scope.processing = false;
          updateCapacity();
          savePanel(scope.currentMember, true);
          $rootScope.$emit('updateAfterSave',
              {member: scope.currentMember, groupId: scope.team.groupId, eventIds: updatedEvents.EventIds});

          determineSaveButtonState();

          // should we reset the form to pristine
          if (!isFormDirty()) {
            scope.teamForm.$setPristine();
          }

          return true;
        }, function(err) {

          $rootScope.$emit('notify', $rootScope.MESSAGES.generalError);
          scope.processing = false;
          determineSaveButtonState();
          return false;
        });
      }

      function setActiveTab(member) {
        // save the original state of the current tab
        savePanel(member);

        // Reset form errors
        scope.formErrors = {
          role: false,
          signup: false,
          frequency: false,
          from: false,
          to: false
        };
        scope.currentActiveTab = member.name;
        if (scope.currentMember === null || member === scope.currentMember) {
          scope.isCollapsed = !scope.isCollapsed;
        } else if (member !== scope.currentMember && scope.isCollapsed) {
          scope.isCollapsed = !scope.isCollapsed;
        }

        scope.currentMember = member;
        updateCapacity();

        if (scope.currentMember.serveRsvp !== undefined && scope.currentMember.serveRsvp !== null) {
          scope.selectedRole = _.find(scope.currentMember.roles, function(r) {
            return r.roleId === scope.currentMember.serveRsvp.roleId;
          });
        }

        allowProfileEdit();
      }

      function determineSaveButtonState() {
        if (scope.currentMember.serveRsvp) {
          if (scope.currentMember.serveRsvp.isSaved) {
            scope.buttonDisabled = true;
          } else if (scope.currentMember.serveRsvp.isSaved === undefined &&
              scope.selectedRole.roleId === scope.currentMember.serveRsvp.roleId) {
            scope.buttonDisabled = true;
          } else {
            scope.buttonDisabled = false;
          }
        } else {
          scope.buttonDisabled = false;
        }
      }

      function showIcon(member) {
        if (member.serveRsvp === undefined || member.serveRsvp === null) {
          return false;
        } else {
          if (member.serveRsvp !== null && (member.serveRsvp.isSaved || member.serveRsvp.isSaved === undefined)) {
            return true;
          } else {
            return false;
          }
        }
      }

      function togglePanel(member) {
        if (!scope.isCollapsed && (scope.currentMember === member || member === null)) {
          scope.isCollapsed = true;
          scope.currentActiveTab = null;
          return false;
        }

        //if a member wasn't passed in, use default member
        if (member === null) {
          scope.currentMember = scope.team.members[0];
          member = scope.currentMember;
        }

        // save the original state of the member
        setActiveTab(member);

        // figure out the state of the save button
        determineSaveButtonState();
      }

      function updateCapacity() {
        _.each(scope.currentMember.roles, function(r) {
          Capacity.get({
            id: r.roleId,
            eventId: scope.team.eventId,
            min: r.minimum,
            max: r.maximum
          })
          .$promise
          .then((result) => {
            r.capacity = result;
            scope.capacityReady = true;
          });
        });
      }
    }
  }
})();
