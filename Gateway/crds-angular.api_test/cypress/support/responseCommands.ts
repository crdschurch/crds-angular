import { ResultBody } from "cypress/shared/test_scenario_factory";
chai.use(require('chai-json-schema-ajv')
  .create({
    verbose: true
  }));

//TODO add/fix Typescript definition for all these
Cypress.Commands.add("verifyStatus", { prevSubject: true }, (subject: Cypress.Chainable<Cypress.Response>, status: number) => {
  expect(subject).to.have.property('status', status);
  return subject;
});


//TODO it would be good if we could type the output as something we could require for "verifySchema" and "verifyProperties"
Cypress.Commands.add("itsBody", { prevSubject: true }, (subject: Cypress.Chainable<Cypress.Response>, expectedBody: ResultBody) => {
  if (!expectedBody) {
    expect(subject).to.not.have.property('body');
    return undefined;
  }
  else {
    expect(subject).to.have.property('body');
    const body = typeof subject.body === 'string' && subject.body.length > 0
      ? JSON.parse(subject.body)
      : subject.body;
    return body;
  }
});


/** Chains of response.body */
Cypress.Commands.add("verifySchema", { prevSubject: true }, (subject: Cypress.Chainable<any>, expectedBody: ResultBody) => {
  if (expectedBody?.schemas) {
    expectedBody.schemas.forEach((schema) => expect(subject).to.have.jsonSchema(schema));
  }
  return subject;
});

Cypress.Commands.add("verifyProperties", { prevSubject: true }, (subject: Cypress.Chainable<any>, expectedBody: ResultBody) => {
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