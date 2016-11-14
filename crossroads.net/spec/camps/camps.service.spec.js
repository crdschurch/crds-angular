import campsModule from '../../app/camps/camps.module';

describe('Camp Service', () => {
  // eslint-disable-next-line no-underscore-dangle
  const endpoint = `${window.__env__.CRDS_API_ENDPOINT}api`;
  let campsService;
  let httpBackend;

  beforeEach(angular.mock.module(campsModule));

  beforeEach(inject((_CampsService_, _$httpBackend_) => {
    campsService = _CampsService_;
    httpBackend = _$httpBackend_;
  }));

  it('Should make the API call to Camp Service', () => {
    const eventId = 4525285;
    httpBackend.expectGET(`${endpoint}/camps/${eventId}`)
      .respond(200, {});
    campsService.getCampInfo(eventId);

    httpBackend.flush();
  });

  it('Should make the API call to get a Camper', () => {
    const eventId = 4525285;
    const camperId = 1234;
    httpBackend.expectGET(`${endpoint}/camps/${eventId}/campers/${camperId}`)
      .respond(200, {});
    campsService.getCamperInfo(eventId, camperId);

    httpBackend.flush();
  });

  it('should make the API call to get my dashboard', () => {
    httpBackend.expectGET(`${endpoint}/my-camp`).respond(200, []);
    campsService.getCampDashboard();
    httpBackend.flush();
  });

  it('should make the API call to get my dashboard and handle error', () => {
    httpBackend.expectGET(`${endpoint}/my-camp`).respond(500, []);
    campsService.getCampDashboard();
    httpBackend.flush();
  });

  it('should make the API call to get my camp family', () => {
    const campId = 21312;
    expect(campsService.family).toBeUndefined();
    httpBackend.expectGET(`${endpoint}/v1.0.0/camps/${campId}/family`).respond(200, []);
    campsService.getCampFamily(campId);
    httpBackend.flush();
    expect(campsService.family).toBeDefined();
  });

  it('should make the API call to get my camp family and handle error', () => {
    const campId = 21312;
    expect(campsService.family).toBeUndefined();
    httpBackend.expectGET(`${endpoint}/v1.0.0/camps/${campId}/family`).respond(500, []);
    campsService.getCampFamily(campId);
    httpBackend.flush();
    expect(campsService.family).toBeUndefined();
  });

  it('should make the API call to get my camp payment', () => {
    const invoiceId = 111;
    const paymentId = 222;
    expect(campsService.payment).toEqual({});

    httpBackend.expectGET(`${endpoint}/v1.0.0/invoice/${invoiceId}/payment/${paymentId}`).respond(200, []);
    campsService.getCampPayment(invoiceId, paymentId);
    httpBackend.flush();
    expect(campsService.payment).toBeDefined();
  });

  it('should make the API call to get my camp payment and handle error', () => {
    const invoiceId = 111;
    const paymentId = 222;

    expect(campsService.payment).toEqual({});
    httpBackend.expectGET(`${endpoint}/v1.0.0/invoice/${invoiceId}/payment/${paymentId}`).respond(500, []);
    campsService.getCampPayment(invoiceId, paymentId);
    httpBackend.flush();
    expect(campsService.payment).toEqual({});
  });

  afterEach(() => {
    httpBackend.verifyNoOutstandingExpectation();
    httpBackend.verifyNoOutstandingRequest();
  });
});
