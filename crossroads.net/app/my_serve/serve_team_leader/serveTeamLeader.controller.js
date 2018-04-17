import CONSTANTS from 'crds-constants';

export default class ServeTeamLeaderController {
    /* @ngInject */
  constructor(ServeTeamService, ServeOpportunities, $rootScope, growl, $q) {
    // this.opportunities; from component binding
    // this.oppServeDate;
    // this.oppServeTime;
    this.serveTeamService = ServeTeamService;
    this.serveOpportunities = ServeOpportunities;
    this.rootScope = $rootScope;
    this.qApi = $q;
    this.growl = growl;
    this.model = { selectedFrequency: null, selectedOpp: null };
    this.formErrors = { from: false };
    this.datesDisabled = false;
    this.individuals = [];
    this.processing = false;
  }

  $onInit() {
    this.serveTeamService.getAllTeamMembersForLoggedInLeader(this.team.groupId).then((data) => {
      this.teamMembers = data;
    });
    _.each(this.team.serveOpportunities, (opp) => {
      opp.capacity = this.serveTeamService.getCapacity(opp, this.team.eventId);
    });

    this.frequencies = this.getFrequency();
    this.model.selectedFrequency = this.frequencies[0];
    this.populateDates();
  }

  populateDates() {
    switch (this.model.selectedFrequency.value) {
      case null:
        this.model.fromDt = null;
        this.model.toDt = null;
        this.datesDisabled = true;
        this.formErrors.from = true;
        break;
      default:
        this.model.fromDt = this.oppServeDate;
        this.model.toDt = this.oppServeDate;
        this.datesDisabled = true;
        this.formErrors.from = false;
        break;
    }
  }

  getFrequency() {
    const dateTime = moment(`${this.oppServeDate} ${this.oppServeTime}`);
    const weeklyLabel = `${moment(this.oppServeDate).format('dddd')}s` + ` ${dateTime.format('h:mma')}`;

    const once = {
      value: 0,
      text: `Once ${dateTime.format('M/D/YYYY h:mma')}`
    };
    const everyWeek = {
      value: 1,
      text: `Every Week ${weeklyLabel}`
    };
    const everyOtherWeek = {
      value: 2,
      text: `Every Other Week ${weeklyLabel}`
    };

    return [once, everyWeek, everyOtherWeek];
  }

  isFormValid() {
    const validForm = {
      valid: true
    };

    if (this.individuals.length < 1) {
      validForm.valid = false;
      this.formErrors.noParticipants = true;
    }

    if (this.model.selectedOpp == null) {
      validForm.valid = false;
    }

    return validForm.valid;
  }

  saveRsvp() {
    const validForm = this.isFormValid();

    if (!validForm) {
      this.rootScope.$emit('notify', this.rootScope.MESSAGES.generalError);
      return false;
    }
    else {
      this.processing = true;

      const signUp = (this.model.selectedOpp !== 0) ? true : false;
      const promises = [];
      _.forEach(this.individuals, (person) => {
        const rsvp = {};
        rsvp.contactId = person.contactId;
        rsvp.opportunityId = this.model.selectedOpp;
        rsvp.opportunityIds = _.pluck(this.team.serveOpportunities, 'opportunityId');
        rsvp.eventTypeId = this.team.eventTypeId;
        rsvp.endDate = moment(this.model.toDt, 'MM/DD/YYYY').format('X');
        rsvp.startDate = moment(this.model.fromDt, 'MM/DD/YYYY').format('X');
        rsvp.signUp = signUp;
        rsvp.alternateWeeks = (this.model.selectedFrequency.value === 2);
        if (!signUp) {
          _.pull(rsvp.opportunityIds, 0);
        }
        promises.push(this.serveOpportunities.SaveRsvp.save(rsvp).$promise);
      });

      this.qApi.all(promises).then((results) => {
        this.updatedEvent = results[0];
        if (signUp) {
          this.rootScope.$emit('notify', this.rootScope.MESSAGES.SU2S_Saving_Message);
        }
        else {
          const saveMessage = `You have indicated that the selected participants are not available for ${this.team.name} on ${this.oppServeDate}`;
          this.growl.success(saveMessage);
        }
        _.forEach(this.individuals, (person) => {
          this.updateTeam(person, this.model.selectedOpp);
        });
        this.model.selectedOpp = null;
        this.individuals = [];
        this.teamLeaderForm.$setPristine();
        this.processing = false;
        return true;
      }, (error) => {
        this.rootScope.$emit('notify', this.rootScope.MESSAGES.generalError);
        this.processing = false;
        return false;
      });
    }
  }

  updateTeam(person, opp) {
    this.onUpdateTeam({ person, opp });
  }

  leaderCancel() {
    this.model.selectedOpp = null;
    this.individuals = [];
    this.teamLeaderForm.$setPristine();
    this.onCancel();
  }

  loadTeamMembersSearch($query) {
    return _.filter(this.teamMembers, (person) => {
      return person.displayName.toLowerCase()
                .includes($query.toLowerCase());
    });
  }
}
