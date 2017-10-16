export default class TravelInformationService {
  /* ngInject */
  constructor($resource) {
    this.currentPerson = null;
    this.profile = $resource(`${__GATEWAY_CLIENT_ENDPOINT__}api/profile`);
  }

  resetPerson() {
    this.currentPerson = null;
  }

  setPerson(person) {
    this.currentPerson = person;
  }

  getPerson() {
    return this.currentPerson;
  }
}
