import CONSTANTS from 'crds-constants';

export default class AddEventcontroller {
  /* @ngInject */
  constructor($log, AddEvent, Lookup, Programs, StaffContact, Validation, Session, LookupService) {
    this.log = $log;
    this.addEvent = AddEvent;
    this.lookup = Lookup;
    this.programsLookup = Programs;
    this.lookupService = LookupService;
    this.staffContact = StaffContact;
    this.validation = Validation;
    this.endDateOpened = false;
    this.startDateOpened = false;
    this.childcareSelectedFlag = false;
    this.session = Session;
    this.ready = false;
  }

  $onInit() {
    this.eventTypes = this.lookupService.EventTypesForEventTool.query({}, (types) => {
      if (this.eventData && this.eventData.eventType && this.eventData.eventType.dp_RecordID) {
        this.eventData.eventType = _.find(types, (type) => {
          return type.dp_RecordID === this.eventData.eventType.dp_RecordID;
        });

        this.eventTypeChanged();
      }
    });
    this.reminderDays = this.lookup.query({ table: 'reminderdays' });
    this.programs = this.programsLookup.ProgramsForEventTool.query();
    // Get the congregations
    this.lookup.query({ table: 'crossroadslocations' }, (locations) => {
      this.crossroadsLocations = locations;
    });
    if (_.isEmpty(this.eventData)) {
      const startDate = new Date();
      startDate.setMinutes(0);
      startDate.setSeconds(0);
      const endDate = new Date(startDate);
      endDate.setHours(startDate.getHours() + 1);
      this.eventData = {
        donationBatch: 0,
        sendReminder: 0,
        minutesSetup: 0,
        minutesCleanup: 0,
        startDate: new Date(),
        endDate: new Date(),
        startTime: startDate,
        endTime: endDate
      };
    }
    else {
      this.eventTypeChanged();
    }

    this.staffContact.query({}, (contacts) => {
      this.staffContacts = contacts;
      if (this.eventData.primaryContact == null) {
        this.eventData.primaryContact = _.findWhere(this.staffContacts, { contactId: parseInt(this.session.exists('userId')) });
      }
      this.ready = true;
    });
  }

  endDateOpen($event) {
    $event.preventDefault();
    $event.stopPropagation();
    this.endDateOpened = true;
  }

  resetRooms() {
    if (!this.addEvent.editMode) {
      this.addEvent.eventData.rooms = [];
      this.rooms = [];
    }
  }

  startDateOpen($event) {
    $event.preventDefault();
    $event.stopPropagation();
    this.startDateOpened = true;
  }

  childcareSelected() {
    return this.childcareSelectedFlag;
  }

  checkMinMax(form) {
    if (this.eventData.minimumChildren === undefined || this.eventData.maximumChildren === undefined) {
      return false;
    }

    // set the proper error state
    if (this.eventData.minimumChildren > this.eventData.maximumChildren) {
      form.maximumChildren.$error.minmax = true;
      form.maximumChildren.$valid = false;
      form.maximumChildren.$invalid = true;
      form.maximumChildren.$dirty = true;
      form.$valid = false;
      form.$invalid = true;
      return true;
    }
    else {
      form.maximumChildren.$error.minmax = false;
      form.maximumChildren.$error.endDate = false;
      form.maximumChildren.$valid = true;
      form.maximumChildren.$invalid = false;
      return false;
    }
  }

  eventTypeChanged() {
    // if childcare is selected then show additional fields
    // constrain congregations
    if (this.eventData.eventType.dp_RecordName === 'Childcare') {
      this.childcareSelectedFlag = true;
      this.lookupService.ChildcareLocations.query({}, (locations) => { this.crossroadsLocations = locations; });
    }
    else {
      this.childcareSelectedFlag = false;
      this.lookup.query({ table: 'crossroadslocations' }, (locations) => { this.crossroadsLocations = locations; });
    }
  }

  validDateRange(form) {
    if (form === undefined) {
      return false;
    }
    // verify that dates are valid;
    let start;
    let end;
    try {
      start = this.addEvent.dateTime(this.eventData.startDate, this.eventData.startTime);
      if (this.eventData.eventType && this.eventData.eventType.Allow_Multiday_Event !== undefined && !this.eventData.eventType.Allow_Multiday_Event) {
        this.eventData.endDate = this.eventData.startDate;
      }
      end = this.addEvent.dateTime(this.eventData.endDate, this.eventData.endTime);
    } catch (err) {
      console.log(err);
      form.endDate.$error.endDate = true;
      form.endDate.$valid = false;
      form.endDate.$invalid = true;
      form.endDate.$dirty = true;
      form.$valid = false;
      form.$invalid = true;
      return true;
    }

    if (moment(start) <= moment(end)) {
      form.endDate.$error.endDate = false;
      form.endDate.$valid = true;
      form.endDate.$invalid = false;
      return false;
    }

    // set the endDate Invalid...
    form.endDate.$error.endDate = true;
    form.endDate.$valid = false;
    form.endDate.$invalid = true;
    form.endDate.$dirty = true;
    form.$valid = false;
    form.$invalid = true;
    return true;
  }

  formatContact(contact) {
    const displayName = contact.displayName;
    const email = contact.email;
    return `${displayName} - ${email}`;
  }

}

