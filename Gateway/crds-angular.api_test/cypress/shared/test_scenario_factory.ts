/**
 * Schema to define test scenarios and their response concisely
 * Use the unzipTests function with a TestConfig or TestConfig[] to generate a Test[]
 */
export interface TestConfig {
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
  /** Store any information needed in the request here. 
   * The properties "header" and "body" will be used directly in the request. */
  data?: Record<string, any>;
  /** Seed the database, generate/fetch test data to store in the data property, etc. here. Must return a chainable. */
  setup?(): Cypress.Chainable<any>;
}

interface TestResult {
  /** HTTP status code expected */
  status: number;
  /** Content of response body expected */
  body?: ResultBody;
}

export interface ResultBody {
  schemas?: any[];
  properties?: PropertyCompare[];
}

interface PropertyCompare {
  name: string;
  value: string;
  /**
   * If false, compares with 'includes'; defaults to exact match
   */
  exactMatch?: boolean;
}

/**
 * Standardized schema for naming, setting up data and creating request to run in a 
 * standard Test
 */
export interface Test {
  title: string;
  /**
   * Use this to share data between the setup and buildRequest functions.
   * data.header and data.body will be used to build the request
   */
  data: Record<string, any>;
  setup(): Cypress.Chainable<any>;
  buildRequest(request?: Partial<Cypress.RequestOptions>): Partial<Cypress.RequestOptions>;
  result: TestResult
}

function buildTest(setupConfig: TestSetup, resultConfig: TestResult): Test {
  const title = `Given ${setupConfig.description}; Expect status ${resultConfig.status}${resultConfig.body?.schemas ? ", valid schema":""}${resultConfig.body?.properties ? ", correct property values":""}`;
  function buildRequest(request?: Partial<Cypress.RequestOptions>): Partial<Cypress.RequestOptions> {
    const fullRequest = { ...request,
      headers: this.data.header,
      body: this.data.body
    }
    return fullRequest;
  }

  const test: Test = {
    title,
    data: setupConfig.data || {},
    setup: setupConfig.setup || function() {return cy.wrap({});},
    buildRequest,
    result: resultConfig
  }
  return test;
}

function unzipConfig(config: TestConfig): Test[] {
  const setups = Array.isArray(config.setup) ? config.setup : [config.setup];
  const scenarios = setups.map(s => buildTest(s, config.result));
  return scenarios;
}

/**
 * Converts test configurations into tests
 * @param zippedTests Configurations to be converted into runnable Test[]
 */
export function unzipTests(zippedTests: TestConfig | TestConfig[]): Test[] {
  const configs = Array.isArray(zippedTests) ? zippedTests : [zippedTests];
  const tests = configs.map(c => unzipConfig(c)).flat();
  return tests;
}