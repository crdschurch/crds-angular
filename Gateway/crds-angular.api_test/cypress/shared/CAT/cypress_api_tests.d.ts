declare namespace CAT {
  /** Config */
  type TestRequest = Partial<Cypress.RequestOptions>;
  interface TestResponse extends ResponseBody {
    /** HTTP status code expected */
    status: number;
  }

  interface ResponseBody {
    schemas?: unknown[];
    properties?: PropertyCompare[];
    bodyIsEmpty?: boolean;
    /** Force a response to be parsed if it is not done automatically */
    parseAsJSON?: boolean;
  }

  interface PropertyCompare {
    name: string;
    value: any;
    /** If false, value must be a string and will 'includes' to compare; defaults to comparing with eq */
    exactMatch?: boolean;
  }
  
  
  interface TestScenario {
    description: string
    request: TestRequest
    setup?(): Cypress.Chainable<TestScenario>
    response: TestResponse
    preferredResponse?: Partial<TestResponse> //These values are *not* tested, but capture the expected output for a defect scenario, or a suggestion for improvement
  }
  

  /** Config more concisely */
  //Lock this down to non-objects
  interface SharedRequest {
    urls?: string[]
    options: {
      method: string
      failOnStatusCode?: boolean
    }
  }

  interface CompactTestScenario {
    sharedRequest?: SharedRequest
    sharedResponse?: ResponseBody
    scenarios: TestScenario[]
  }
}