// eslint-disable-next-line import/no-extraneous-dependencies,import/no-unresolved
const attributeType = require('crds-constants').ATTRIBUTE_TYPE_IDS.FREQUENT_FLYERS;

export default class TravelInformationController {
  /* @ngInject() */
  constructor($rootScope, Validation, AttributeTypeService, TravelInformationService, $state) {
    this.$rootScope = $rootScope;
    this.validation = Validation;
    this.travelInformation = TravelInformationService;
    this.attributeTypeService = AttributeTypeService;
    this.state = $state;

    this.now = null;
    this.initDate = null;

    this.destination = null;
    this.person = {};
    this.travelInfoForm = {};
    this.frequentFlyers = [];
    this.validPassport = null;
    this.preferredAirport = null;
    this.otherAirport = null;

    this.maxPassportExpireDate = null;
    this.minPassportExpireDate = null;
    this.passportExpireDateOpen = false;

    this.processing = false;
  }

  $onInit() {
    this.now = new Date();
    this.initDate = new Date(this.now.getFullYear(), this.now.getMonth(), this.now.getDate());
    this.maxPassportExpireDate = new Date(this.now.getFullYear() + 150, this.now.getMonth(), this.now.getDate());
    this.minPassportExpireDate = new Date(this.now.getFullYear(), this.now.getMonth(), this.now.getDate());

    this.person = this.travelInformation.getPerson();

    if (this.person.passportNumber) {
      this.validPassport = 'true';
    }

    this.attributeTypeService.AttributeTypes().get({ id: attributeType }, (data) => {
      this.frequentFlyers = data.attributes.map((ff) => {
        const exists = this.frequentFlyerValue(ff.attributeId);
        if (exists) {
          const newFF = Object.assign({}, ff, { selected: true, notes: exists, startDate: new Date() });
          return newFF;
        }
        return ff;
      });
    }, (err) => {
      this.error = err;
    });
  }

  passportInvalidContent() {
    const message = this.$rootScope.MESSAGES.TripNoPassport.content;
    return message;
  }

  openPassportExpireDatePicker($event) {
    $event.preventDefault();
    $event.stopPropagation();
    this.passportExpireDateOpen = true;
  }

  frequentFlyerValue(id) {
    const frequentFlyerTypes = this.person.attributeTypes[attributeType];
    if (frequentFlyerTypes !== null && frequentFlyerTypes.attributes) {
      const currff = frequentFlyerTypes.attributes.find(ff => ff.attributeId === id);
      if (currff.selected) {
        return currff.notes;
      }
    }
    return null;
  }

  buildFrequentFlyers() {
    return this.frequentFlyers.map((ff) => {
      if (ff.notes) {
        return Object.assign({}, ff, { selected: true, startDate: new Date() });
      }
      return Object.assign({}, ff, { selected: false, startDate: new Date() });
    });
  }

  submit() {
    this.processing = true;
    let preferredAirport = this.preferredAirport === 'Other' ? this.otherAirport : this.preferredAirport;
    if (this.travelInfoForm.$valid) {
      // set the selected attribute on frequent flyer..
      const flyers = this.buildFrequentFlyers();
      this.person.attributeTypes[attributeType] = {
        attributes: flyers
      };
      // save the info
      this.travelInformation.profile.save(this.person, () => {
        // clear the current user from TravelInformationService
        this.processing = false;
        this.travelInformation.resetPerson();
        this.state.go('mytrips');
        this.$rootScope.$emit('notify', this.$rootScope.MESSAGES.profileUpdated);
      }, () => {
        this.$rootScope.$emit('notify', this.$rootScope.MESSAGES.generalError);
        this.processing = false;
      });
    } else {
      // show error message on page
      this.$rootScope.$emit('notify', this.$rootScope.MESSAGES.generalError);
      this.processing = false;
    }
  }

  cancel() {
    this.state.go('mytrips');
  }
}
