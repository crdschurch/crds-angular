/**
 * Schema to define test scenarios and their response concisely
 * Use the unzipTests function with a TestConfig or TestConfig[] to generate a TestCase[]
 */
export interface TestConfig {
  setup: TestSetup | TestSetup[];
  result: TestResult;
  /**
   * Preferred Output values are not tested, but capture the expected output for a defect scenario, or a suggestion for improvement
   */
  preferredResult?: TestResult;
}

export interface TestCase {
  title: string;
  setup: TestSetup;
  result: TestResult;
}

export interface TestSetup {
  description: string;
  body?: any;
  header?: any;
}

export interface TestResult {
  status: number;
  body?: ResultBody;
}

export interface ResultBody {
  schemas?: object[];
  properties?: PropertyCompare[];
}

export interface PropertyCompare {
  name: string;
  value: string;
  /**
   * If false, compares with 'includes'; defaults to exact match
   */
  exactMatch?: boolean;
 }

function unzip(config: TestConfig): TestCase[]{
  const testTitle = (setup: TestSetup, result: TestResult) => {
    return `Given ${setup.description}; Expect status ${result.status}${result.body?.schemas ? ", valid schema":""}${result.body?.properties ? ", correct property values":""}`;
  };

  let scenarios: TestCase[];
  if(Array.isArray(config.setup)){
    //handle individual
    scenarios = config.setup.map((setup) => ({ title: testTitle(setup, config.result), setup, result: config.result }));
  }
  else {
    scenarios = [{ title: testTitle(config.setup, config.result), setup: config.setup, result: config.result }];
  }

  return scenarios;
}

export function unzipTests(zippedTests: TestConfig | TestConfig[]): TestCase[] {
  if(Array.isArray(zippedTests)){
    return zippedTests.map((config) => (unzip(config)))
    .reduce((accumulator, value) => accumulator.concat(value), []); //TODO use .flat
  }
  else {
    return unzip(zippedTests);
  }
}