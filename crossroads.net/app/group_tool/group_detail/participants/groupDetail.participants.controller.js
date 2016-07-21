import GroupMessage from '../../model/groupMessage';

export default class GroupDetailParticipantsController {
  /*@ngInject*/
  constructor(GroupService, ImageService, $state, $log, ParticipantService, $rootScope, MessageService) {
    this.groupService = GroupService;
    this.imageService = ImageService;
    this.state = $state;
    this.log = $log;
    this.participantService = ParticipantService;
    this.rootScope = $rootScope;
    this.messageService = MessageService;

    this.groupId = this.state.params.groupId;
    this.ready = false;
    this.error = false;
    this.processing = false;

    this.setListView();
  }

  $onInit() {
    this.participantService.get().then((myParticipant) => {
      this.myParticipantId = myParticipant.ParticipantId;
      this.loadGroupParticipants();
    }, (err) => {
      this.log.error(`Unable to get my participant: ${err.status} - ${err.statusText}`);
      this.error = true;
      this.ready = true;
    });
  }

  loadGroupParticipants() {
    this.groupService.getGroupParticipants(this.groupId).then((data) => {
      this.data = data.slice().sort((a, b) => {
        return(a.compareTo(b));
      });
      this.data.forEach(function(participant) {
        participant.me = participant.participantId === this.myParticipantId;
        participant.imageUrl = `${this.imageService.ProfileImageBaseURL}${participant.contactId}`;
      }, this);

      this.ready = true;
    },
    (err) => {
      this.log.error(`Unable to get group participants: ${err.status} - ${err.statusText}`);
      this.error = true;
      this.ready = true;
    });
  }

  setDeleteView() {
    this.currentView = 'Delete';
  }

  isDeleteView() {
    return this.currentView === 'Delete';
  }

  setEditView() {
    this.currentView = 'Edit';
  }

  isEditView() {
    return this.currentView === 'Edit';
  }

  setListView() {
    this.currentView = 'List';
  }

  isListView() {
    return this.currentView === 'List';
  }

  setEmailView() {
    this.currentView = 'Email';
  }

  isEmailView() {
    return this.currentView === 'Email';
  }

  beginRemoveParticipant(participant) {
    this.deleteParticipant = participant;
    this.deleteParticipant.deleteMessage = '';
    this.setDeleteView();
  }

  cancelRemoveParticipant(participant) {
    participant.deleteMessage = undefined;
    this.deleteParticipant = undefined;
    this.setEditView();
  }

  removeParticipant(participant) {
    this.log.info(`Deleting participant: ${JSON.stringify(participant)}`);
    this.processing = true;
    this.groupService.removeGroupParticipant(this.groupId, participant).then(() => {
      _.remove(this.data, function(p) {
          return p.groupParticipantId === participant.groupParticipantId;
      });
      this.rootScope.$emit('notify', this.rootScope.MESSAGES.groupToolRemoveParticipantSuccess);
      this.setListView();
      this.deleteParticipant = undefined;
      this.ready = true;
    },
    (err) => {
      this.log.error(`Unable to remove group participant: ${err.status} - ${err.statusText}`);
      this.rootScope.$emit('notify', this.rootScope.MESSAGES.groupToolRemoveParticipantFailure);
      this.error = true;
      this.ready = true;
    }).finally(() => {
      this.processing = false;
    });
  }

  beginMessageParticipants() {
    this.groupMessage = new GroupMessage();
    this.groupMessage.groupId = '';
    this.groupMessage.subject = '';
    this.groupMessage.body = '';
    this.setEmailView();
  }

  cancelMessageParticipants(message) {
    this.groupMessage = undefined;
    this.setListView();
  }

  messageParticipants(message) {

    // TODO: Fill in implementation
    this.processing = true;

    this.messageService.sendGroupMessage(this.groupId, message).then(
        () => {
          this.groupMessage = undefined;
          this.$onInit();
          this.currentView = 'List';
          this.rootScope.$emit('notify', this.rootScope.MESSAGES.emailSent);
        },
        (error) => {
          this.rootScope.$emit('notify', this.rootScope.MESSAGES.emailSendingError);
        }
    ).finally(() => {
      this.processing = false;
    });
  }
}