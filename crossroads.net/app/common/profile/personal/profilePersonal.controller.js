(function() {
  'use strict';

  var moment = require('moment');

  module.exports = ProfilePersonalController;

  ProfilePersonalController.$inject = [
    '$rootScope',
    '$scope',
    '$log',
    '$timeout',
    '$location',
    '$anchorScroll',
    'MESSAGES',
    'ProfileReferenceData',
    'Profile',
    'Validation',
    '$sce',
    '$modal',
    'PasswordService',
    'Session',
    'emailChange',
    'AuthService',
    'CRDS_TOOLS_CONSTANTS'
  ];

  function ProfilePersonalController(
      $rootScope,
      $scope,
      $log,
      $timeout,
      $location,
      $anchorScroll,
      MESSAGES,
      ProfileReferenceData,
      Profile,
      Validation,
      $sce,
      $modal,
      PasswordService,
      Session,
      emailChange,
      AuthService,
      CRDS_TOOLS_CONSTANTS
      ) {

    var vm = this;
    var attributeTypeIds = require('crds-constants').ATTRIBUTE_TYPE_IDS;
    var now = new Date();

    vm.allowAccess = allowAccess;
    vm.allowPasswordChange = angular.isDefined(vm.allowPasswordChange) ?  vm.allowPasswordChange : 'true';
    vm.allowSave = angular.isDefined(vm.allowSave) ? vm.allowSave : 'true';
    vm.closeModal = closeModal;
    vm.convertHomePhone = convertHomePhone;
    vm.convertPhone = convertPhone;
    vm.crossroadsStartDate = new Date(1994, 0, 1);
    vm.currentPassword = '';
    vm.dateFormat = /^(0[1-9]|1[012])[- /.](0[1-9]|[12][0-9]|3[01])[- /.]((19|20)\d\d)$/;
    vm.editingOtherProfile = editingOtherProfile;
    vm.emailRequired = emailRequired;
    vm.formatAnniversaryDate = formatAnniversaryDate;
    vm.householdForm = {};
    vm.householdInfo = {};
    vm.householdPhoneFocus = householdPhoneFocus;
    vm.isCrossroadsAttendee = isCrossroadsAttendee;
    vm.isHouseholdCollapsed = true;
    vm.hstep = 1;
    vm.isDobError = isDobError;
    vm.isMeridian = true;
    vm.loading = true;
    vm.maxBirthdate = new Date(now.getFullYear(), now.getMonth(), now.getDate());
    vm.initDate = new Date(now.getFullYear(), now.getMonth(), now.getDate());
    vm.mstep = 15;
    vm.oldEmail = '';
    vm.oneHundredFiftyYearsAgo = new Date(now.getFullYear() - 150, now.getMonth(), now.getDate());
    vm.openBirthdatePicker = openBirthdatePicker;
    vm.openStartAttendingDatePicker = openStartAttendingDatePicker;
    vm.password = '';
    vm.passwordPrefix = 'account-page';
    vm.phoneFormat = /^\(?(\d{3})\)?-(\d{3})-(\d{4})$/;
    vm.requireEmail = true;
    vm.requireMobilePhone = angular.isDefined(vm.requireMobilePhone) ? vm.requireMobilePhone : 'false';
    vm.resetCredentialsEntered = false;
    vm.savePersonal = savePersonal;
    vm.setOldEmail = setOldEmail;
    vm.showMobilePhoneError = showMobilePhoneError;
    vm.showPasswordConfirmModal = showPasswordConfirmModal;
    vm.submitted = false;
    vm.today = moment();
    vm.underThirteen = underThirteen;
    vm.validation = Validation;
    vm.verifyPasswordAttempt = '';
    vm.viewReady = false;
    vm.zipFormat = /^(\d{5}([\-]\d{4})?)$/;
    vm.scrollToAnchor= scrollToAnchor;

    // TODO: Remove this hack and move the promises below into resolves on the directive.
    // Hack to overcome issue where Bootstrap UI Datepicker picks up the date value as string and can't parse it correctly, so it raises validation error.
    // After the promises are resolved below setAttendanceStartDateToJSDate() gets called. The previous code set the date to a JS date, but the validation was not re-firing leaving the field invalid.
    // This hack temporarily captures the value for attendanceStartDate, sets the models value to a valid date, and when everything is resolved sets the value to the real date
    var originalAttendanceStartDate = _.get(vm, 'profileData.person.attendanceStartDate');
    if (originalAttendanceStartDate) {
      vm.profileData.person.attendanceStartDate = new Date();
    }

    // variables for email and password reset
    vm.emailSet = false;
    vm.passwordSet = false;

    activate();

    ////////////////////////////////
    //// IMPLEMENTATION DETAILS ////
    ////////////////////////////////

    function activate() {
      // fix for de3046
      // emailChange is a singleton object, so everytime when controller actives
      // the serivice object doesn't reconstruct
      // the first time when user goes to my-profile page,
      // this line of code emailChange.setEmail(vm.profileData.person.emailAddress) will
      // cause emailChange.isSet to be set to true, then when you go to other pages and come back to my-profile page
      // emailChange.isSet will not change (since it's a singleton object), which cause setOldEmail() do nothing
      // emailChange.reset() will only be called for a successful update, but really it should be called
      // everytime when the controller actives
      emailChange.reset();

      if (vm.enforceAgeRestriction) {
        vm.maxBirthdate.setFullYear(vm.maxBirthdate.getFullYear() - vm.enforceAgeRestriction);
        vm.initDate.setFullYear(vm.initDate.getFullYear() - vm.enforceAgeRestriction);
      }

      ProfileReferenceData.getInstance().then(function(response) {
        vm.genders = response.genders;
        vm.maritalStatuses = response.maritalStatuses;
        vm.serviceProviders = response.serviceProviders;
        vm.states = response.states;
        vm.countries = response.countries;
        vm.crossroadsLocations = response.crossroadsLocations;
        if (!vm.profileData) {
          Profile.Person.get({contactId: vm.contactId}).$promise.then (function(data) {
            vm.profileData = { person: data };

            // TODO: This is a continuation of the hack above. Remove this as part of fixing that hack.
            originalAttendanceStartDate = _.get(vm, 'profileData.person.attendanceStartDate');
            setAttendanceStartDateToJSDate();

            underThirteen();
            setOldEmail();
            emailChange.setEmail(vm.profileData.person.emailAddress);
            vm.viewReady = true;
          });
        } else {
          configurePerson();
          setAttendanceStartDateToJSDate();
          underThirteen();
          setOldEmail();
          emailChange.setEmail(vm.profileData.person.emailAddress);
          vm.viewReady = true;
        }
      });

      vm.buttonText = vm.buttonText !== undefined ? vm.buttonText : 'Save';
    }

    function editingOtherProfile() {
      return Number(vm.contactId) !== parseInt(Session.exists('userId'));
    }

    function setAttendanceStartDateToJSDate() {
      if (originalAttendanceStartDate) {
        vm.profileData.person.attendanceStartDate = new Date(originalAttendanceStartDate);
      }
    }

    function scrollToAnchor(anchor) {
      if (anchor != null && vm.modalInstance == null) {
        $anchorScroll(anchor.$name);
        // need to scroll up by toast message height
        window.scrollBy(0, -100);
      }
    }


    function allowAccess() {
      return (AuthService.isAuthenticated() && AuthService.isAuthorized(CRDS_TOOLS_CONSTANTS.SECURITY_ROLES.ServeSignupTool));
    }

    function configurePerson() {

      vm.profileData.person.dateOfBirth = convertDate(vm.profileData.person.dateOfBirth);

      vm.ethnicities = vm.profileData.person.attributeTypes[attributeTypeIds.ETHNICITY].attributes;
      vm.startAttendReason = vm.profileData.person.singleAttributes[attributeTypeIds.START_ATTEND_REASON];
      vm.startAttendReasons = _.find(vm.attributeTypes, function(attributeType) {
        return attributeType.attributeTypeId === attributeTypeIds.START_ATTEND_REASON;
      });
    }

    function convertDate(date) {
      if ((date !== undefined) && (date !== '')) {
        if (typeof date === 'string' ||
            date instanceof String) {
          var newDate = date.replace(vm.dateFormat, '$3 $1 $2');
          var mDate = moment(newDate, 'YYYY MM DD');
          return mDate.format('MM/DD/YYYY');
        } else {
          var formatedDate = moment(date);
          return formatedDate.format('MM/DD/YYYY');
        }
      }
    }

    function convertHomePhone() {
      if (vm.pform['home-phone'].$valid) {
        vm.profileData.person.homePhone = vm.profileData.person.homePhone.replace(vm.phoneFormat, '$1-$2-$3');
      }
    }

    function closeModal(success) {
      if (success) {
        vm.updatedPerson.emailAddress = vm.profileData.person.emailAddress;
        vm.updatedPerson.firstName = vm.profileData.person.firstName;
        vm.updatedPerson.nickName =
            vm.profileData.person.nickName === '' ?
                vm.profileData.person.firstName :
                vm.profileData.person.nickName;
        vm.updatedPerson.lastName = vm.profileData.person.lastName;
      }

      vm.modalInstance.close(vm.updatedPerson);
    }

    function convertPhone() {
      if (vm.pform['mobile-phone'].$valid) {
        vm.profileData.person.mobilePhone = vm.profileData.person.mobilePhone.replace(vm.phoneFormat, '$1-$2-$3');
      }
    }

    function formatAnniversaryDate(anniversaryDate) {
      var tmp = moment(anniversaryDate);
      var month = tmp.month() + 1;
      var year = tmp.year();
      return month + '/' + year;
    }

    function householdPhoneFocus() {
      vm.isHouseholdCollapsed = false;
      $location.hash('homephonecont');
      $timeout(function() {
        $anchorScroll();
      }, 500);
    }

    function isDobError() {
      return (vm.pform.birthdate.$touched ||
          vm.pform.$submitted) &&
          vm.pform.birthdate.$invalid;
    }

    function openBirthdatePicker($event) {
      $event.preventDefault();
      $event.stopPropagation();

      vm.birthdateOpen = true;
    }

    function openStartAttendingDatePicker($event) {
      $event.preventDefault();
      $event.stopPropagation();

      vm.startAttendingOpen = true;
    }

    function emailRequired() {
      if (!editingOtherProfile() || (!emailChange.originalEmail)) {
        return vm.requireEmail;
      }

      return false;
    }

    function savePersonal() {

      //force genders field to be dirty
      vm.pform.$submitted = true;
      vm.householdForm.$submitted = true;

      $timeout(function() {
        vm.submitted = true;

        if (vm.pform.$invalid) {
          $rootScope.$emit('notify', $rootScope.MESSAGES.generalError);
          vm.isHouseholdCollapsed = false;
          if (vm.pform.$error.required[0].$name === 'household.householdForm') {
            vm.scrollToAnchor(vm.householdForm.$error.required[0]);
          } else {
            vm.scrollToAnchor(vm.pform.$error.required[0]);
          }
          vm.submitted = false;
          return;
        }

        // length 0 check supports if a user starts to change their pw, then decides not to
        if (vm.pform['passwd.passwordForm'] !== undefined) {
          if (vm.pform['passwd.passwordForm'].password.$touched &&
              _.size(vm.pform['passwd.passwordForm'].password.$modelValue) > 0) {
            vm.passwordSet = true;

            // set the fields before saving
            vm.profileData.person.newPassword = vm.password;
          }
        }

        if (vm.pform.email !== undefined) {
          if (vm.pform.email.$touched === true) {
            if (vm.forTrips) {
              if (vm.oldEmail !== null && vm.oldEmail !== '') {
                vm.emailSet = true;
              }
            } else {
              vm.emailSet = true;
            }
          }
        }

        // if either of these fields are dirty, we need to ask the user for password verification before continuing
        if (vm.emailSet || vm.passwordSet) {
          if (vm.resetCredentialsEntered === false) {
            showPasswordConfirmModal();
            vm.submitted = true;
            return;
          }
        }

        vm.profileData.person.oldEmail = vm.oldEmail;
        vm.profileData.person.oldPassword = vm.currentPassword;

        // DE3068: convert to string and exclude time to avoid timezone/DST issues
        if (_.has(vm.profileData.person, 'dateOfBirth'))
          vm.profileData.person.dateOfBirth = convertDate(vm.profileData.person.dateOfBirth);

        vm.profileData.person['State/Region'] = vm.profileData.person.State;
        if (vm.submitFormCallback !== undefined) {
          vm.submitFormCallback({profile: vm.profileData});
        } else {
          vm.profileData.person.$save(function() {
                vm.submitted = false;
                $rootScope.$emit('notify', $rootScope.MESSAGES.profileUpdated);
                $log.debug('person save successful');
                if (vm.profileParentForm) {
                  vm.profileParentForm.$setPristine();

                  // do so we make sure to set the dialog to show
                  vm.resetCredentialsEntered = false;
                }

                if (vm.modalInstance !== undefined) {
                  vm.closeModal(true);
                }

                // do this to avoid blanking out the password if it was auto-filled from
                // the browser
                if (vm.passwordSet === true) {
                  vm.password = '';
                  vm.currentPassword = '';
                  vm.profileData.person.oldPassword = '';
                  vm.profileData.person.newPassword = '';
                }

                // update the email here, if it was changed
                if (vm.emailSet === true) {
                  vm.oldEmail = vm.profileData.person.emailAddress;
                }
                emailChange.reset();

                vm.pform.email.$setUntouched();
                vm.emailSet = false;
                vm.passwordSet = false;
              },

              function() {
                $rootScope.$emit('notify', $rootScope.MESSAGES.generalError);
                $log.debug('person save unsuccessful');
                vm.submitted = false;
              });
        }
      }, 550);

      vm.emailSet = false;
      vm.passwordSet = false;

    }

    function showMobilePhoneError() {
      var show = vm.validation.showErrors(vm.pform, 'mobile-phone') && vm.requireMobilePhone;
      return show;
    }

    function underThirteen() {
      if (vm.profileData.person.dateOfBirth !== '') {
        var birthdate = crds_utilities.convertStringToDate(vm.profileData.person.dateOfBirth);
        if (birthdate) {
          var thirteen = new Date();
          thirteen.setFullYear(thirteen.getFullYear() - 13);
          vm.requireEmail = birthdate.getTime() < thirteen.getTime();
        } else {
          vm.requireEmail = true;
        }
      } else {
        vm.requireEmail = true;
      }
    }

    function isCrossroadsAttendee() {
      var nonCrossroadsLocations = require('crds-constants').NON_CROSSROADS_LOCATIONS;
      return vm.profileData.person.congregationId &&
          vm.profileData.person.congregationId !== nonCrossroadsLocations.I_DO_NOT_ATTEND_CROSSROADS &&
          vm.profileData.person.congregationId !== nonCrossroadsLocations.NOT_SITE_SPECIFIC;
    }

    // set the old email address
    function setOldEmail() {
      if (!emailChange.isSet) {
        vm.oldEmail = vm.profileData.person.emailAddress;
      }
    }

    function showPasswordConfirmModal() {

      var modalType = '';

      if ((vm.emailSet === true) && (vm.passwordSet === false)) {
        modalType = 'emailOnly';
      } else if (vm.passwordSet === true) {
        modalType = 'notEmailOnly';
      }

      var modalInstance = $modal.open({
        templateUrl: 'personal/confirmPassword.html',
        controller: 'ConfirmPasswordCtrl as pwModal',
        openedClass: 'crds-legacy-styles',
        backdrop: 'static',
        resolve: {
          modalTypeItem: function() {
            return modalType;
          }
        }
      });

      modalInstance.result.then(function(currentPassword) {

        if (currentPassword !== undefined) {
          vm.currentPassword = currentPassword;
          vm.resetCredentialsEntered = true;
          vm.savePersonal();
        } else {
          vm.submitted = false;
        }

      });
    }

  }

})();
