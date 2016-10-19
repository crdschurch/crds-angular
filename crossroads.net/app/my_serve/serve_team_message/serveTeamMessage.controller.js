export default class ServeTeamMessageController {
  /* @ngInject */
  constructor(ServeTeamService, $log, $rootScope, $state) {
    console.debug('Serve Team Message controller');
    this.serveTeamService = ServeTeamService;
    this.processing = false;
    this.selection = null;
    this.individuals = [];
    this.ready = false;
    this.log = $log;
    this.rootScope = $rootScope;
    this.state = $state;
  }

  $onInit(){
    this.serveTeamService.getTeamDetailsByLeader().then((data) => {
      this.log.debug(data)
      this.teams = data;
    }).catch((err) => {
      //do something here
      this.log.debug("unable to retrieve teams")
    }).finally(() => {
      this.ready = true;
    });
    this.teamPeople = this.serveTeamService.getAllTeamMembersByLeader();
  }

  loadIndividuals($query) {
    return this.teamPeople;
  }

  cancel() {
    this.state.go('serve-signup');
  }

  submit(serveMessageForm) {
    // Validate the form - if ok, then invoke the submit callback
    if(!serveMessageForm.$valid) {
      this.rootScope.$emit('notify', this.rootScope.MESSAGES.generalError);
      return;
    }
    this.processing = true;
    this.serveTeamService.sendGroupMessage(this.selection, { Body: this.email.message, Subject: this.email.subject })
    .then((data)=>{      
      debugger;
      this.rootScope.$emit('notify', this.rootScope.MESSAGES.emailSent);
      this.state.go('serve-signup');
    })
    .catch((err)=>{
      this.rootScope.$emit('notify', this.rootScope.MESSAGES.messageSendError);
    })
    .finally(()=>{
      this.processing = false;
    });
  }
}
