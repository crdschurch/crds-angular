
import constants from 'crds-constants';
import GroupDetailParticipantsController from '../../../../app/group_tool/group_detail/participants/groupDetail.participants.controller';
import Participant from '../../../../app/group_tool/model/participant';

describe('GroupDetailParticipantsController', () => {
    let fixture,
        groupService,
        imageService,
        state,
        rootScope,
        log,
        participantService,
        qApi;

    beforeEach(angular.mock.module(constants.MODULES.GROUP_TOOL));

    beforeEach(inject(function($injector) {
        groupService = $injector.get('GroupService'); 
        imageService = $injector.get('ImageService');
        state = $injector.get('$state');
        rootScope = $injector.get('$rootScope');
        log = $injector.get('$log');
        participantService = $injector.get('ParticipantService');
        qApi = $injector.get('$q');

        state.params = {
          groupId: 123
        };

        fixture = new GroupDetailParticipantsController(groupService, imageService, state, log, participantService, rootScope);
    }));

    describe('the constructor', () => {
        it('should initialize properties', () => {
            expect(fixture.groupId).toEqual(state.params.groupId);
            expect(fixture.ready).toBeFalsy();
            expect(fixture.error).toBeFalsy();
            expect(fixture.currentView).toEqual('List');
            expect(fixture.processing).toBeFalsy();
        });
    });

    describe('setView() function', () => {
      it('should set correct view', () => {
        fixture.setView('Remove');
        expect(fixture.currentView).toEqual('Remove');
      });
    });

    describe('beginRemoveParticipant() function', () => {
      it('should set properties', () => {
        let participant = new Participant();
        fixture.beginRemoveParticipant(participant);
        expect(fixture.deleteParticipant).toBe(participant);
        expect(fixture.deleteParticipant.deleteMessage).toEqual('');
        expect(fixture.currentView).toEqual('Delete');
      });
    });

    describe('cancelRemoveParticipant() function', () => {
      it('should unset properties', () => {
        let participant = new Participant();
        participant.deleteMessage = 'delete';
        fixture.cancelRemoveParticipant(participant);
        expect(fixture.deleteParticipant).not.toBeDefined();
        expect(participant.deleteMessage).not.toBeDefined();
        expect(fixture.currentView).toEqual('Edit');
      });
    });

    describe('removeParticipant() function', () => {
      it('should remove participant successfully', () => {
        let deferred = qApi.defer();
        deferred.resolve({});

        spyOn(groupService, 'removeGroupParticipant').and.callFake(function() {
          return(deferred.promise);
        });

        let participant = new Participant({groupParticipantId: 999});

        let participants = [
          new Participant({nickName: 'f1', lastName: 'l1', groupRoleId: constants.GROUP.ROLES.MEMBER, groupParticipantId: 987}),
          new Participant({nickName: 'f2', lastName: 'l2', groupRoleId: constants.GROUP.ROLES.LEADER, groupParticipantId: 654}),
          new Participant({nickName: 'f3', lastName: 'l3', groupRoleId: constants.GROUP.ROLES.APPRENTICE, groupParticipantId: participant.groupParticipantId})
        ];
        fixture.data = participants;

        spyOn(rootScope, '$emit').and.callFake(() => { });

        fixture.removeParticipant(participant);
        rootScope.$digest();

        expect(groupService.removeGroupParticipant).toHaveBeenCalledWith(state.params.groupId, participant);
        expect(fixture.data.length).toEqual(2);
        expect(fixture.data.find((p) => { return p.groupParticipantId === participant.groupParticipantId; })).not.toBeDefined();
        expect(fixture.processing).toBeFalsy();
        expect(fixture.ready).toBeTruthy();
        expect(fixture.currentView).toEqual('List');
        expect(fixture.deleteParticipant).not.toBeDefined();
        expect(rootScope.$emit).toHaveBeenCalledWith('notify', rootScope.MESSAGES.groupToolRemoveParticipantSuccess);
      });

      it('should set error state if problem deleting participant', () => {
        let deferred = qApi.defer();
        deferred.reject({status: 500, statusText: 'Oh no!'});

        spyOn(groupService, 'removeGroupParticipant').and.callFake(function() {
          return(deferred.promise);
        });
        
        spyOn(rootScope, '$emit').and.callFake(() => { });

        let participant = new Participant({groupParticipantId: 999});

        fixture.setView('Remove');
        fixture.removeParticipant(participant);
        rootScope.$digest();

        expect(groupService.removeGroupParticipant).toHaveBeenCalledWith(state.params.groupId, participant);
        expect(fixture.processing).toBeFalsy();
        expect(fixture.ready).toBeTruthy();
        expect(fixture.error).toBeTruthy();
        expect(fixture.currentView).toEqual('Remove');
        expect(rootScope.$emit).toHaveBeenCalledWith('notify', rootScope.MESSAGES.groupToolRemoveParticipantFailure);
      });
    });
    
    describe('$onInit() function', () => {
        it('should get group participants and set image url', () => {
          let myParticipant = {
            ParticipantId: 123
          };

          let deferredParticipant = qApi.defer();
          deferredParticipant.resolve(myParticipant);

          let participants = [
            new Participant({nickName: 'f1', lastName: 'l1', groupRoleId: constants.GROUP.ROLES.MEMBER, participantId: 99}),
            new Participant({nickName: 'f2', lastName: 'l2', groupRoleId: constants.GROUP.ROLES.LEADER, participantId: myParticipant.ParticipantId}),
            new Participant({nickName: 'f3', lastName: 'l3', groupRoleId: constants.GROUP.ROLES.APPRENTICE, participantId: 88})
          ];
          let deferredGroupParticipants = qApi.defer();
          deferredGroupParticipants.resolve(participants);

          spyOn(participantService, 'get').and.callFake(function() {
            return(deferredParticipant.promise);
          });

          spyOn(groupService, 'getGroupParticipants').and.callFake(function() {
            return(deferredGroupParticipants.promise);
          });

          fixture.$onInit();
          rootScope.$digest();

          expect(participantService.get).toHaveBeenCalled();
          expect(groupService.getGroupParticipants).toHaveBeenCalledWith(state.params.groupId);

          expect(fixture.data).toBeDefined();
          expect(fixture.data.length).toEqual(participants.length);

          // Verify that data is sorted
          expect(fixture.data[0].participantId).toEqual(participants[1].participantId);
          expect(fixture.data[0].me).toBeTruthy();
          expect(fixture.data[1].participantId).toEqual(participants[2].participantId);
          expect(fixture.data[1].me).toBeFalsy();
          expect(fixture.data[2].participantId).toEqual(participants[0].participantId);
          expect(fixture.data[2].me).toBeFalsy();

          // Verify image URL on each
          fixture.data.forEach(function(p) {
            expect(p.imageUrl).toBeDefined();
            expect(p.imageUrl).toEqual(`${imageService.ProfileImageBaseURL}${p.contactId}`);
          }, this);

          expect(fixture.ready).toBeTruthy();
          expect(fixture.error).toBeFalsy();
        });

        it('should set error state if trouble getting my participant', () => {
          let deferred = qApi.defer();
          let error = {
            status: 500,
            statusText: 'oops'
          };
          deferred.reject(error);

          spyOn(participantService, 'get').and.callFake(function() {
            return(deferred.promise);
          });

          spyOn(groupService, 'getGroupParticipants').and.callFake(function() {
            return;
          });

          fixture.$onInit();
          rootScope.$digest();

          expect(participantService.get).toHaveBeenCalled();
          expect(groupService.getGroupParticipants).not.toHaveBeenCalled();
          expect(fixture.ready).toBeTruthy();
          expect(fixture.error).toBeTruthy();
        });

        it('should set error state if trouble getting group participants', () => {
           let myParticipant = {
            ParticipantId: 123
          };

          let deferredParticipant = qApi.defer();
          deferredParticipant.resolve(myParticipant);

          let deferred = qApi.defer();
          let error = {
            status: 500,
            statusText: 'oops'
          };
          deferred.reject(error);

          spyOn(participantService, 'get').and.callFake(function() {
            return(deferredParticipant.promise);
          });

          spyOn(groupService, 'getGroupParticipants').and.callFake(function() {
            return(deferred.promise);
          });

          fixture.$onInit();
          rootScope.$digest();

          expect(participantService.get).toHaveBeenCalled();
          expect(groupService.getGroupParticipants).toHaveBeenCalledWith(state.params.groupId);
          expect(fixture.ready).toBeTruthy();
          expect(fixture.error).toBeTruthy();
        });
    });
});