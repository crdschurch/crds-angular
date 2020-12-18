import { ResponseTypes } from "shared/enums";

// eslint-disable-next-line @typescript-eslint/no-var-requires
chai.use(require('chai-json-schema-ajv')
  .create({
    verbose: true
  }));

Cypress.Commands.add("verifyStatus", { prevSubject: true }, (subject: Cypress.Chainable<Cypress.Response>, status: number) => {
  expect(subject).to.have.property('status', status);
  return subject;
});


Cypress.Commands.add("itsBody", { prevSubject: true }, (subject: Cypress.Response, expectedBody: TestFactory.ResultBody) => {
  if (!expectedBody) {
    expect(subject).to.not.have.property('body');
    return undefined;
  }

  expect(subject).to.have.property('body');
  switch (expectedBody.contentType) {
    case undefined: //parse as Json
    case ResponseTypes.json: {
      const body = typeof subject.body === 'string' && subject.body.length > 0
        ? JSON.parse(subject.body)
        : subject.body;
      return body;
    }
    case ResponseTypes.text:
      return subject.body;
    default:
      console.warn(`Parsing a response with given content type "${expectedBody.contentType}" is not yet supported. Returning unparsed body.`);
      return subject.body;
  }
});

/** Chains of response.body */
Cypress.Commands.add("verifySchema", { prevSubject: true }, (subject: Cypress.Chainable<unknown>, expectedBody: TestFactory.ResultBody) => {
  if (expectedBody?.schemas) {
    expectedBody.schemas.forEach((schema) => expect(subject).to.have.jsonSchema(schema));
  }
  return subject;
});

//Warning! This assumes subject is formatted as JSON. Support for plain-text comparison should be added
// once there is a test case that needs it.
Cypress.Commands.add("verifyProperties", { prevSubject: true }, (subject: Cypress.Chainable<unknown>, expectedBody: TestFactory.ResultBody) => {
  if (expectedBody?.properties) {
    expectedBody.properties.forEach((prop) => {
      if (prop.exactMatch === false) {
        expect(subject).to.have.property(prop.name).and.include(prop.value);
      }
      else {
        expect(subject).to.have.property(prop.name).and.eq(prop.value);
      }
    });
  }
  return subject;
});