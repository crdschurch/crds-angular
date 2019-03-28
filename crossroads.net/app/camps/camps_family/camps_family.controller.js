class CampsFamilyController {
  /* @ngInject */
  constructor(CampsService, $log, $rootScope, $state) {
    this.log = $log;
    this.rootScope = $rootScope;
    this.state = $state;
  }

  $onInit() {
    this.log.debug('Camps Family Controller Initialized!');
  }
  
  getIntro() {
    return (this.rootScope.MESSAGES[`campIntro_${this.state.toParams.campId}`]) ?
    this.rootScope.MESSAGES[`campIntro_${this.state.toParams.campId}`].content :
    this.rootScope.MESSAGES.summercampIntro.content;
  }
}

export default CampsFamilyController;
