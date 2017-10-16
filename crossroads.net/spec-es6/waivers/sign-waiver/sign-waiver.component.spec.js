import WaiversModule from '../../../app/waivers/waivers.module';

describe('Waivers', () => {
  let log;
  let state;
  let rootScope;
  let ctrl;
  let waiversService;
  let q;
  let modal;

  const waiverId = 8889;
  const eventParticipantId = 98711;

  beforeEach(() => {
    angular.mock.module(WaiversModule);
  });

  beforeEach(inject((_$log_, _$state_, _$rootScope_, _WaiversService_, _$q_, _$modal_) => {
    log = _$log_;
    modal = _$modal_;
    state = _$state_;
    state.params = {
      waiverId,
      eventParticipantId
    };

    q = _$q_;
    rootScope = _$rootScope_;
    waiversService = _WaiversService_;
  }));

  describe('Sign Waiver Component', () => {
    beforeEach(() => {
      spyOn(log, 'error');
      spyOn(state, 'go').and.callFake(() => true);
      spyOn(modal, 'open').and.callFake(() => {
        const deferred = q.defer();
        deferred.resolve();
        return deferred.promise;
      });

      spyOn(rootScope, '$emit');
    });

    beforeEach(inject((_$componentController_) => {
      ctrl = _$componentController_('signWaiver', null, {});
    }));

    it('Should get the waiver to be signed', () => {
      spyOn(waiversService, 'getWaiver').and.callFake(() => {
        const deferred = q.defer();
        deferred.resolve([{}]);
        return deferred.promise;
      });

      ctrl.$onInit();
      rootScope.$apply();
      expect(waiversService.getWaiver).toHaveBeenCalled();
      expect(ctrl.viewReady).toBeTruthy();
    });

    it('Should display an error on failure to get waiver', () => {
      spyOn(waiversService, 'getWaiver').and.callFake(() => {
        const deferred = q.defer();
        deferred.reject([{}]);
        return deferred.promise;
      });

      ctrl.$onInit();
      rootScope.$apply();
      expect(rootScope.$emit).toHaveBeenCalled();
    });

    it('Should log an error on failure to get waiver', () => {
      spyOn(waiversService, 'getWaiver').and.callFake(() => {
        const deferred = q.defer();
        deferred.reject([{}]);
        return deferred.promise;
      });
      ctrl.$onInit();
      rootScope.$apply();
      expect(log.error).toHaveBeenCalled();
    });

    it('Should navigate back on Cancel', () => {
      ctrl.cancel();
      expect(state.go).toHaveBeenCalledWith('mytrips');
    });

    it('Should send a confirmation email on Send', () => {
      spyOn(waiversService, 'sendAcceptEmail').and.callFake(() => {
        const deferred = q.defer();
        deferred.resolve();
        return deferred.promise;
      });
      ctrl.submit();
      rootScope.$apply();
      expect(waiversService.sendAcceptEmail).toHaveBeenCalled();
    });

    it('Should display a modal on successful Send', () => {
      spyOn(waiversService, 'sendAcceptEmail').and.callFake(() => {
        const deferred = q.defer();
        deferred.resolve();
        return deferred.promise;
      });
      ctrl.submit();
      rootScope.$apply();
      expect(modal.open).toHaveBeenCalled();
    });

    it('Should display an error on failed Send', () => {
      spyOn(waiversService, 'sendAcceptEmail').and.callFake(() => {
        const deferred = q.defer();
        deferred.reject();
        return deferred.promise;
      });
      ctrl.submit();
      rootScope.$apply();
      expect(modal.open).not.toHaveBeenCalled();
      expect(rootScope.$emit).toHaveBeenCalled();
    });

    it('Should log an error on failed Send', () => {
      spyOn(waiversService, 'sendAcceptEmail').and.callFake(() => {
        const deferred = q.defer();
        deferred.reject();
        return deferred.promise;
      });
      ctrl.submit();
      rootScope.$apply();
      expect(modal.open).not.toHaveBeenCalled();
      expect(log.error).toHaveBeenCalled();
    });
  });
});
