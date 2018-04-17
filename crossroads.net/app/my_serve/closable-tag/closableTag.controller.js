import CONSTANTS from 'crds-constants';

export default class ClosableTagController {
  /*@ngInject*/
  constructor() {
    this.maxSVAge = CONSTANTS.SERVING.MAXSTUDENTVOLUNTEERAGE;
    this.svText = CONSTANTS.SERVING.STUDENTVOLUNTEERTEXT;
    this.title = 'Remove Team Member';
    this.message = `Are you sure you want to remove ${this.name}?`;
    this.confirmText = "Yes <i class='glyphicon glyphicon-ok'>";
    this.cancelText = "No <i class='glyphicon glyphicon-remove'>";
  }

  click() {
    this.onClick({});
  }

  close() {
    this.opportunityId = 0;
    this.onClose({});
  }
}
