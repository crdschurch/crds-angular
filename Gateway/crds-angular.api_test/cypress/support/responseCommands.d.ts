declare namespace Cypress {
  interface Chainable<Subject = any> {
    verifyStatus(status: number): Chainable<Response>
    itsBody(expectedBody: TestFactory.ResultBody | undefined): Chainable<JSON | undefined>
    verifySchema(expectedBody: TestFactory.ResultBody | undefined): Chainable<JSON | undefined>
    verifyProperties(expectedBody: TestFactory.ResultBody | undefined): Chainable<JSON | undefined>
  }
}
