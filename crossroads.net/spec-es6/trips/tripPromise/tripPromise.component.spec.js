import tripsModule from '../../../app/trips/trips.module';
import helpers from '../trips.helpers';

describe('Trip I Promise Component', () => {
  let httpBackend;
  let ctrl;
  let rootScope;
  let state;
  let stateParams;
  let log;
  let tripService;

  let endpoint = `${window.__env__.CRDS_GATEWAY_CLIENT_ENDPOINT}api/v1.0.0/`;
  const { eventParticipantId } = helpers.promiseDocument;

  beforeEach(() => {
    angular.mock.module('crossroads.trips');
  });

  beforeEach(inject((_$httpBackend_, _$componentController_, _$rootScope_, _$state_, _$stateParams_, _$log_, _Trip_) => {
    httpBackend = _$httpBackend_;
    rootScope = _$rootScope_;
    state = _$state_;
    log = _$log_;
    tripService = _Trip_;

    spyOn(rootScope, '$emit').and.callThrough();
    spyOn(state, 'go');
    spyOn(log, 'error');
    spyOn(tripService.MyTripsPromise, 'get').and.callThrough();
    spyOn(tripService.MyTripsPromise, 'save').and.callThrough();

    ctrl = _$componentController_('tripPromise', null, {});
    ctrl.myTripPromise = helpers.promiseDocument;
  }));

  afterEach(() => {
    httpBackend.verifyNoOutstandingExpectation();
    httpBackend.verifyNoOutstandingRequest();
  });

  describe('Promise Controller', () => {
    it('Should create the controller', () => {
      expect(ctrl.promise).toEqual(false);
      expect(ctrl.processing).toEqual(false);
      expect(ctrl.tripPromiseForm).toEqual({});
    });

    it('Should navigate back to dashboard', () => {
      ctrl.cancel();

      expect(state.go).toHaveBeenCalledWith('mytrips');
    });

    it('Should submit document', () => {
      httpBackend.expectPOST(`${endpoint}trip/ipromise`);
      ctrl.tripPromiseForm.$valid = true;
      ctrl.submit();
      const args = tripService.MyTripsPromise.save.calls.argsFor(0)[0];
      expect(args.documentReceived).toEqual(true);
    });

    it('Should navigate to dashboard on successful submit', () => {
      httpBackend.expectPOST(`${endpoint}trip/ipromise`).respond(200);
      ctrl.tripPromiseForm.$valid = true;
      ctrl.submit();
      httpBackend.flush();

      expect(state.go).toHaveBeenCalledWith('mytrips');
    });

    it('Should emit a growl message on error', () => {
      httpBackend.expectPOST(`${endpoint}trip/ipromise`).respond(404);
      ctrl.tripPromiseForm.$valid = true;
      ctrl.submit();
      httpBackend.flush();

      expect(rootScope.$emit).toHaveBeenCalled();
    });

    it('Should emit a growl message on error', () => {
      httpBackend.expectPOST(`${endpoint}trip/ipromise`).respond(404);
      ctrl.tripPromiseForm.$valid = true;
      ctrl.submit();
      httpBackend.flush();

      expect(log.error).toHaveBeenCalled();
    });

    it('Should set processing to false on error response', () => {
      httpBackend.expectPOST(`${endpoint}trip/ipromise`).respond(404);
      ctrl.tripPromiseForm.$valid = true;
      ctrl.submit();
      httpBackend.flush();

      expect(ctrl.processing).toEqual(false);
    });
  });
});
