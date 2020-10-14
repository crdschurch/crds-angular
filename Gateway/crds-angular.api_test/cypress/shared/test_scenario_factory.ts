export interface TestScenarioRequest {
  description: string;
  body?: any;
}

export interface TestScenarioResponse {
  status: number;
  schemas?: object[];
  properties?: { name: string, value: string }[];
}

export interface TestScenario {
  title: string;
  request: TestScenarioRequest;
  response: TestScenarioResponse;
}

/**
 * Builds a list of TestScenarios given a list of requests and their common response
 * @param requestList
 * @param response
 */
export function scenarioFactory(requestList: TestScenarioRequest[], response: TestScenarioResponse): TestScenario[] {
  const testTitle = (request: TestScenarioRequest, response: TestScenarioResponse) => {
    return `Given ${request.description}; Expect status ${response.status}${response.schemas ? ", valid schema":""}${response.properties ? ", correct property values":""}`;
  };

  let scenarios: TestScenario[] = requestList?.map((request) => ({ title: testTitle(request,response), request, response }));
  return scenarios;
}