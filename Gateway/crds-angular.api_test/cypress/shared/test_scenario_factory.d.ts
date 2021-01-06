declare namespace TestFactory {
  /**
 * Schema to define test scenarios and their response concisely
 * Use the unzipTests function with a TestConfig or TestConfig[] to generate a Test[]
 */
  interface TestConfig {
    setup: TestSetup | TestSetup[]
    result: TestResult;
    /**
     * Preferred Output values are not tested, but capture the expected output for a defect scenario, or a suggestion for improvement
     */
    preferredResult?: TestResult;
  }

  interface TestSetup {
    /** Description of the test scenario */
    description: string;
    /** Store any information needed in the request here. The Cypress.RequestOptions "headers" and "body" will be used directly in the request. */
    data: TestData;
    /** Seed the database, generate/fetch test data to store in the data property, etc. here. Must return a chainable. */
    setup?(): Cypress.Chainable<unknown>;
  }

  /** Data can contain any part of a Cypress.RequestOptions and anything else */
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  interface TestData extends Partial<Cypress.RequestOptions>, Record<string, any>{ }

  interface TestResult {
    /** HTTP status code expected */
    status: number;
    /** Content of response body expected */
    body?: ResultBody;
  }

  interface ResultBody {
    schemas?: unknown[];
    properties?: PropertyCompare[];
    contentType?: string;
  }

  interface PropertyCompare {
    name: string;
    value: any;
    /** If false, value must be a string and will 'includes' to compare; defaults to comparing with eq */
    exactMatch?: boolean;
  }

  interface Test {
    title: string;
    /**
     * Use this to share data between the setup and buildRequest functions.
     * data.header and data.body will be used to build the request
     */
    data: TestData;
    setup(): Cypress.Chainable<unknown>;
    buildRequest(request?: Partial<Cypress.RequestOptions>): Partial<Cypress.RequestOptions>;
    result: TestResult
  }
}
