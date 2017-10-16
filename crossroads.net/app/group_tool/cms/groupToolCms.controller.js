import CONSTANTS from 'crds-constants';

export default class GroupToolCms {
  /*@ngInject*/
  constructor(Page, ParticipantService, $state, $window, GroupService) {
    this.page = Page;
    this.participantService = ParticipantService;
    this.state = $state;
    this.content = '';
    this.window = $window;
    this.groupService = GroupService;
  }

  $onInit() {
    this.participantService.get().then((data) => {
      if (_.get(data, 'ApprovedSmallGroupLeader', false)) {
        this.url = this.url || '/groups/leader/resources/';

        this.page.get({
          url: this.url
        }).$promise.then((data) => {
          if (data.pages.length > 0) {
            this.content = data.pages[0].content;
          }
        });
      } else {
        this.groupService.groupLeaderUrl().then((segment) => {
          this.window.location.href = this.window.location.origin + segment;
        });
      }
    });
  }
}