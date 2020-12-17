function buildTest(setupConfig: TestFactory.TestSetup, resultConfig: TestFactory.TestResult): TestFactory.Test {
  const title = `Given ${setupConfig.description}; Expect status ${resultConfig.status}${resultConfig.body?.schemas ? ", valid schema":""}${resultConfig.body?.properties ? ", correct property values":""}`;

  const test: TestFactory.Test = {
    title,
    data: setupConfig.data || {},
    setup: setupConfig.setup || function() {return cy.wrap({});},
    buildRequest: function(request?: Partial<Cypress.RequestOptions>): Partial<Cypress.RequestOptions> {
      return { ...request,
        headers: this.data.headers,
        body: this.data.body
      }
    },
    result: resultConfig
  }
  return test;
}

function unzipConfig(config: TestFactory.TestConfig): TestFactory.Test[] {
  const setups = Array.isArray(config.setup) ? config.setup : [config.setup];
  const scenarios = setups.map(s => buildTest(s, config.result));
  return scenarios;
}

/**
 * Converts test configurations into tests
 * @param zippedTests Configurations to be converted into runnable Test[]
 */
export function unzipTests(zippedTests: TestFactory.TestConfig | TestFactory.TestConfig[]): TestFactory.Test[] {
  const configs = Array.isArray(zippedTests) ? zippedTests : [zippedTests];
  const tests = configs.map(c => unzipConfig(c)).flat();
  return tests;
}