import SmallGroup from '../model/smallGroup';
import CONSTANTS from 'crds-constants';

export default class CreateGroupController {
    /*@ngInject*/
    constructor(ParticipantService, $state, $stateParams, $log, CreateGroupService, GroupService, $rootScope, $window) {
        this.log = $log;
        this.state = $state;
        this.participantService = ParticipantService;
        this.createGroupService = CreateGroupService;
        this.groupService = GroupService;
        this.rootScope = $rootScope;
        this.ready = false;
        this.approvedLeader = false;
        this.fields = [];
        this.createGroupForm = {};
        this.options = {};
        this.window = $window;

        // If the state that called this component has a specific state route for the Cancel button set it now
        // The cancel button will be hidden if there isn't a cancelSref
        this.cancelSref = $stateParams.cancelSref;
    }

    $onInit() {
        this.participantService.get().then((data) => {
                if (_.get(data, 'ApprovedSmallGroupLeader', false)) {
                    this.approvedLeader = true;
                    this.ready = true;
                } else {
                    this.groupService.groupLeaderUrl().then((segment) => {
                        this.window.location.href = this.window.location.origin + segment;
                    });
                }
            },

            (err) => {
                this.log.error(`Unable to get Participant for logged-in user: ${err.status} - ${err.statusText}`);
                    this.groupService.groupLeaderUrl().then((segment) => {
                        this.window.location.href = this.window.location.origin + segment;
                    });
            });

        this.fields = this.createGroupService.getFields();
        this.stateChangeWatcher = this.rootScope.$on('$stateChangeStart', (event, toState, toParams, fromState, fromParams) => {
            if (!toState.name.startsWith('grouptool.create')) {
                if (this.createGroupForm.$dirty) {
                    if (!this.window.confirm('Are you sure you want to leave this page?')) {
                        event.preventDefault();
                        return;
                    } else {
                        this.createGroupService.reset();
                        this.stateChangeWatcher();
                        return;
                    }
                }
                this.createGroupService.reset();
                this.stateChangeWatcher();
                return;
            }
        });
    }

    previewGroup() {
        if (this.createGroupForm.$valid) {
            this.state.go('grouptool.create.preview');
        } else {
            this.rootScope.$emit('notify', this.rootScope.MESSAGES.generalError);
        }
    }
}