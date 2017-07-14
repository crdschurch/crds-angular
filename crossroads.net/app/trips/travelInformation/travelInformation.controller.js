var attributeTypes = require('crds-constants').ATTRIBUTE_TYPE_IDS;
var attributeIds = require('crds-constants').ATTRIBUTE_IDS;

export default class TravelInformationController {
  /* @ngInject() */
  constructor($rootScope) {
    this.$rootScope = $rootScope;

    this.now = null;
    this.initDate = null;

    this.destination = null;
    this.person = {};
    this.frequentFlyers = [];
    this.validPassport = false;

    this.maxPassportExpireDate = null;
    this.minPassportExpireDate = null;
    this.passportExpireDateOpen = false;

    this.processing = false;
  }

  $onInit() {
    this.destination = 'South Africa';
    this.frequentFlyers = [
      {
        "attributeId": 3958,
        "name": "Delta Airlines",
        "description": null,
        "selected": false,
        "startDate": "0001-01-01T00:00:00",
        "endDate": null,
        "notes": null,
        "sortOrder": 0,
        "category": null,
        "categoryId": null,
        "categoryDescription": null,
        "attributeTypeId": null
      },
      {
        "attributeId": 3959,
        "name": "South Africa Airlines",
        "description": null,
        "selected": false,
        "startDate": "0001-01-01T00:00:00",
        "endDate": null,
        "notes": null,
        "sortOrder": 0,
        "category": null,
        "categoryId": null,
        "categoryDescription": null,
        "attributeTypeId": null
      },
      {
        "attributeId": 4623,
        "name": "Southwest",
        "description": null,
        "selected": false,
        "startDate": "0001-01-01T00:00:00",
        "endDate": null,
        "notes": null,
        "sortOrder": 0,
        "category": null,
        "categoryId": null,
        "categoryDescription": null,
        "attributeTypeId": null
      },
      {
        "attributeId": 3960,
        "name": "United Airlines",
        "description": null,
        "selected": false,
        "startDate": "0001-01-01T00:00:00",
        "endDate": null,
        "notes": null,
        "sortOrder": 0,
        "category": null,
        "categoryId": null,
        "categoryDescription": null,
        "attributeTypeId": null
      },
      {
        "attributeId": 3980,
        "name": "US Airways",
        "description": null,
        "selected": false,
        "startDate": "0001-01-01T00:00:00",
        "endDate": null,
        "notes": null,
        "sortOrder": 0,
        "category": null,
        "categoryId": null,
        "categoryDescription": null,
        "attributeTypeId": null
      }
    ];

    this.now = new Date();
    this.initDate = new Date(this.now.getFullYear(), this.now.getMonth(), this.now.getDate());
    this.maxPassportExpireDate = new Date(this.now.getFullYear() + 150, this.now.getMonth(), this.now.getDate());
    this.minPassportExpireDate = new Date(this.now.getFullYear(), this.now.getMonth(), this.now.getDate());
  }

  showFrequentFlyer(airline) {
    if (airline.attributeId === attributeIds.SOUTHAFRICA_FREQUENT_FLYER) {
      if (this.isSouthAfrica()) {
        return true;
      }
      return false;
    }

    if (airline.attributeId === attributeIds.US_FREQUENT_FLYER) {
      if (this.isNica()) {
        return true;
      }

      return false;
    }

    return true;
  }

  isNica() {
    if (this.destination === 'Nicaragua') {
      return true;
    }

    return false;
  }

  isSouthAfrica() {
    if (this.destination === 'South Africa') {
      return true;
    }

    return false;
  }

  passportInvalidContent() {
    let message = this.$rootScope.MESSAGES.TripNoPassport.content;
    switch (this.destination) {
      case 'South Africa':
        message = this.$rootScope.MESSAGES.TripNoPassportSouthAfrica.content;
        break;
      case 'India':
        message = this.$rootScope.MESSAGES.TripNoPassportIndia.content;
        break;
      case 'Nicaragua':
        message = this.$rootScope.MESSAGES.TripNoPassportNicaragua.content;
        break;
    }
    return message;
  }

  openPassportExpireDatePicker($event) {
    $event.preventDefault();
    $event.stopPropagation();
    this.passportExpireDateOpen = true;
  }

  submit() {
    this.processing = true;
  }

  cancel() {
  }
}
