import CONSTANTS from 'crds-constants';

export default class GroupToolCms {
  /*@ngInject*/
  constructor(Page, $state, $window, GroupService, AuthService) {
    this.page = Page;
    this.participantService = ParticipantService;
    this.state = $state;
    this.content = '';
    this.window = $window;
    this.groupService = GroupService;
    this.auth = AuthService;
  }

  $onInit() {
    this.get().then((data) => {
      if (_.get(data, 'ApprovedSmallGroupLeader', false)) {
        this.url = this.url || '/groups/leader/resources/';
Console.log(this.url);
        this.page.get({
          url: this.url
        }).$promise.then((data) => {
          if (data.pages.length > 0) {
            this.content = data.pages[0].content;
          } 
        });
      } else {
        this.groupLeaderUrl().then((segment) => {
          this.window.location.href = this.window.location.origin + segment;
        });
      }
    });
  }

  get() {
    if(this.auth.isAuthenticated()) {
      Console.log(Authenticated);
      return this.resource(__GATEWAY_CLIENT_ENDPOINT__ + 'api/participant').get().$promise; 
    } else {
      this.log.info('Unauthenticated, no participant');
      var promised = this.deferred.defer();
      promised.resolve({'ApprovedSmallGroupLeader': false});
      return promised.promise;
    }
  }

  groupLeaderUrl() {
    return this.resource(__GATEWAY_CLIENT_ENDPOINT__ + 'api/v1.0.0/group-leader/url-segment').get().$promise.then((result) => {
      Console.log("Here!");
      return result.url;
    });
  }
}