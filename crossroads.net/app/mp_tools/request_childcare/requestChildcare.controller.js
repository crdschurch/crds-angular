import moment from 'moment';
/* jshint unused: false */
import * as recur from 'moment-recur';

class RequestChildcareController {
  /*@ngInject*/
  constructor($rootScope,
              MPTools,
              CRDS_TOOLS_CONSTANTS,
              $log,
              RequestChildcareService,
              Validation,
              $cookies,
              $window) {
    this.allowAccess = MPTools.allowAccess(CRDS_TOOLS_CONSTANTS.SECURITY_ROLES.ChildcareRequestTool);
    this.congregations = RequestChildcareService.getCongregations();
    this.currentRequest = Number(MPTools.getParams().recordId);
    this.datesList = [];
    this.customSessionSelected = false;
    this.customSessionTime = 'Customize My Childcare Session...';
    this.loadingGroups = false;
    this.log = $log;
    this.ministries = RequestChildcareService.getMinistries();
    this.minDate = new Date();
    this.minDate.setDate(this.minDate.getDate() + 7);
    this.name = 'request-childcare';
    this.requestChildcareService = RequestChildcareService;
    this.rootScope = $rootScope;
    this.runDateGenerator = true;
    this.startTime = new Date();
    this.startTime.setHours(9);
    this.startTime.setMinutes(30);
    this.endTime = this.startTime;
    this.uid = $cookies.get('userId');
    this.validation = Validation;
    this.viewReady = true;
    this.window = $window;
    this.datesSelected = true;
  }

  generateDateList() {
    if (this.runDateGenerator) {
      this.datesSelected = true;
      let dayOfWeek = this.choosenPreferredTime.Meeting_Day;
      if (this.choosenPreferredTime.dp_RecordID === -1) {
        dayOfWeek = this.dayOfWeek;
      }
      const start = moment(this.startDate);
      const end = moment(this.endDate);
      if (this.choosenFrequency === 'Weekly') {
        let weekly = moment().recur(start, end).every(dayOfWeek).daysOfWeek();
        this.datesList = weekly.all().map( (d) => {
          return { 
            unix: d.unix(),
            date: d,
            selected: true
          };
        });
      } else if (this.choosenFrequency === 'Monthly') {
        let weekOfMonth = this.getWeekOfMonth(start);
        let monthly = moment().recur(start, end)
          .every(dayOfWeek).daysOfWeek()
          .every(weekOfMonth -1).weeksOfMonthByDay();
        this.datesList = monthly.all().map( (d) => {
          return {
            unix: d.unix(),
            date: d,
            selected: true
          };
        });
      } else {
        // use the startDate and make sure it aligns with the day
        if (start.day() === moment().day(dayOfWeek).day()) {
          this.datesList = [{ unix: start.unix(), date: start, selected: true}];
        } else {
          this.rootScope.$emit('notify', this.rootScope.MESSAGES.daysDoNotMatch);
          this.datesList = [];
        }
      }
      this.runDateGenerator = false;
    }
  }

  getGroups() {
    if (this.choosenCongregation && this.choosenMinistry) {
      this.loadingGroups = true;
      this.groups = this.requestChildcareService
        .getGroups(this.choosenCongregation.dp_RecordID, this.choosenMinistry.dp_RecordID);
      this.groups.$promise
        .then(() => this.loadingGroups = false, () => this.loadingGroups = false);
      this.preferredTimes = this.requestChildcareService.getPreferredTimes( this.choosenCongregation.dp_RecordID);
      this.preferredTimes.$promise.then(() => {
        this.preferredTimes = [...this.preferredTimes, {
          Childcare_Start_Time: null,
          Childcare_End_Time: null,
          Meeting_Day: null, dp_RecordID: -1,
          Deactivate_Date: null
        }];
        this.filteredTimes = this.preferredTimes;
      });
    }
  }

  getWeekOfMonth(startDate) {
    return Math.ceil(startDate.date() / 7);
  }

  onEndDateChange(endDate) {
    this.endDate = endDate;
    this.runDateGenerator = true;
  }

  onFrequencyChange() {
    this.runDateGenerator = true;
  }

  onDateSelectionChange() {
    let datesSelected = this.datesList.filter( (d) => { return d.selected; });
    this.datesSelected = datesSelected.length > 0;
  }

  onDayChange() {
    this.runDateGenerator = true;
  }

  onStartDateChange(startDate) {
    this.runDateGenerator = true;
    this.filteredTimes = this.preferredTimes.filter((time) => {
      if (time.Deactivate_Date === null) { return true; }

      var preferredStart = moment(startDate);
      var deactivateDate = moment(time.Deactivate_Date);
      return preferredStart.isBefore(deactivateDate) || preferredStart.isSame(deactivateDate);
    });
  }

  showGaps() {
    if (this.choosenPreferredTime &&
        (this.choosenPreferredTime.Meeting_Day !== null || this.dayOfWeek) &&
        this.choosenFrequency &&
        this.startDate &&
        this.endDate) {
      const start = this.startDate.getTime();
      const end = this.endDate.getTime();
      if (start < end || start === end) {
        this.generateDateList();
        if (this.choosenFrequency === 'Once') {
          return false;
        }

        return this.datesList.length > 0;
      }

      return false;
    }

    return false;
  }

  showGroups() {
    return this.choosenCongregation && this.choosenMinistry && this.groups.length > 0;
  }

  preferredTimeChanged() {
    if (this.choosenPreferredTime.dp_RecordID === -1) {
      this.customSessionSelected = true;
    } else {
      this.customSessionSelected = false;
    }
    this.runDateGenerator = true;
  }

  filterTimes(time) {
    let t = time;
    if (time.Childcare_Start_Time === undefined && Number(this.choosenPreferredTime) !== -1) {
      t = _.find(this.filteredTimes, (tm) => {
        return tm.dp_RecordID === Number(time);
      });
    }
    return t;
  }

  formatPreferredTime(time) {
    if (time.dp_RecordID === -1) {
            return this.customSessionTime;
    } else {
      time = this.filterTimes(time);
      const startTimeArr = time['Childcare_Start_Time'].split(':');
      const endTimeArr = time['Childcare_End_Time'].split(':');
      const startTime = moment().set(
        {'hour': parseInt(startTimeArr[0]), 'minute': parseInt(startTimeArr[1])});
      const endTime = moment().set(
        {'hour': parseInt(endTimeArr[0]), 'minute': parseInt(endTimeArr[1])});
      const day = time['Meeting_Day'];
      return `${day}, ${startTime.format('h:mmA')} - ${endTime.format('h:mmA')}`;
    }
  }

  submit() {
    this.saving = true;
    if (this.childcareRequestForm.$invalid) {
      this.saving = false;
      return false;
    } else if (this.datesList.length < 1) {
      this.rootScope.$emit('notify', this.rootScope.MESSAGES.noDatesChosen);
      return false;
    } else {
      let time = this.formatPreferredTime(this.choosenPreferredTime);
      if (this.choosenPreferredTime.dp_RecordID === -1) {
        let start = moment(this.startTime);
        let end = moment(this.endTime);
        time = `${this.dayOfWeek}, ${start.format('h:mmA')} - ${end.format('h:mmA')}`;
      }
      const dto = {
        requester: this.uid,
        site: this.choosenCongregation.dp_RecordID,
        ministry: this.choosenMinistry.dp_RecordID,
        group: this.choosenGroup.dp_RecordID,
        startDate: moment(this.startDate).utc(),
        endDate: moment(this.endDate).utc(),
        frequency: this.choosenFrequency,
        timeframe: time,
        notes: this.notes,
        dates: this.datesList.filter( (d) => { return d.selected === true;}).map( (d) => { return d.date; })
      };
      const save = this.requestChildcareService.saveRequest(dto);
      save.$promise.then(() => {
        this.log.debug('saved!');
        this.saving = false;
        this.window.close();
      }, () => {
        this.saving = false;
        this.log.error('error!');
        this.saving = false;
      });
    }
  }

  validateField(fieldName) {
    return this.validation.showErrors(this.childcareRequestForm, fieldName);
  }
    
  validateDateSelection() {
      return !this.datesSelected;
  }

  validTimeRange(form) {
    if (form === undefined) {
      return false;
    }

    //verify that times are valid;
    var start;
    var end;
    try {
      start =  moment(this.startTime);
      end = moment(this.endTime);
    } catch (err) {
      form.endTime.$error.invalidEnd = true;
      form.endTime.$valid = false;
      form.endTime.$invalid = true;
      form.endTime.$dirty = true;
      form.$valid = false;
      form.$invalid = true;
      return true;
    }

    if (start <= end) {
      form.endTime.$error.invalidEnd = false;
      form.endTime.$valid = true;
      form.endTime.$invalid = false;
      return false;
    }

    // set the endTime Invalid...
    form.endTime.$error.invalidEnd = true;
    form.endTime.$valid = false;
    form.endTime.$invalid = true;
    form.endTime.$dirty = true;
    form.$valid = false;
    form.$invalid = true;
    return true;
  }
}
export default RequestChildcareController;
