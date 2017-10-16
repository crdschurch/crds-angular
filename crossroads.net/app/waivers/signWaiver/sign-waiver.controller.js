import confirmationModal from './confirmationModal.html';

/* @ngInject */
export default class SignWaiverController {
  constructor($log, $state, $rootScope, WaiversService, $modal) {
    this.log = $log;
    this.state = $state;
    this.rootScope = $rootScope;
    this.waiversService = WaiversService;
    this.modal = $modal;
    this.title = this.state.params.title || 'Trips Waiver';
    this.eventName = this.state.params.eventName || '';
  }

  $onInit() {
    this.viewReady = false;
    this.processing = false;

    this.waiversService.getWaiver(this.state.params.waiverId).then((res) => {
      this.waiver = res;
      this.viewReady = true;
    }).catch((err) => {
      this.viewReady = true;
      this.log.error(err);
      this.rootScope.$emit('notify', this.rootScope.MESSAGES.generalError);
    });
  }

  submit() {
    this.processing = true;

    const { waiverId, eventParticipantId } = this.state.params;

    this.waiversService.sendAcceptEmail(parseInt(waiverId, 10), eventParticipantId).then(() => {
      const modal = this.modal.open({
        animation: true,
        template: confirmationModal,
        backdrop: 'static',
        openedClass: 'crds-legacy-styles',
        controller: ($scope, $modalInstance) => {
          // eslint-disable-next-line
          $scope.ok = () => {
            $modalInstance.close();
          };
        },
      });
      modal.result.then(() => {
        this.processing = false;
        this.state.go('mytrips');
      });
    }).catch((err) => {
      this.processing = false;
      this.log.error(err);
      this.rootScope.$emit('notify', this.rootScope.MESSAGES.generalError);
    });
  }

  cancel() {
    this.state.go('mytrips');
  }
}
