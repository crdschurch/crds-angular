export default class ServeTeamMessageController {
  /* @ngInject */
  constructor($state) {
    console.debug('Serve Team Message controller');
    this.groupId;
    this.ready = false;
    this.state = $state;
  }

  $onInit() {
    this.groupId = this.state.params.groupId;
    this.ready = true;
  }

}
