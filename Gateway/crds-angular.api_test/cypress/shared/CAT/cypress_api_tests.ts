// eslint-disable-next-line @typescript-eslint/no-var-requires
chai.use(require('chai-json-schema-ajv')
  .create({
    verbose: true
  }));

/**
 * Converts a compact request config into individual test requests 
 * @param sharedRequest 
 */
function unzipSharedRequest(sharedRequest?: CAT.SharedRequest): CAT.TestRequest[] {
  if(!sharedRequest) return [];

  // Expand urls
  if (Array.isArray(sharedRequest.urls)) {
    return sharedRequest.urls.map((url) => ({ ...sharedRequest.options, url }));
  }
  return [{ ...sharedRequest.options, url: sharedRequest.urls }];
}

/**
 * Merges a request into a scenario's existing request. Values in the scenario's request will take priority.
 * @param scenario 
 * @param request 
 */
function mergeRequestAndScenario(scenario: CAT.TestScenario, request: CAT.TestRequest): CAT.TestScenario {
  return { ...scenario, request: { ...request, ...scenario.request } }
}

/**
 * Merges a response into a scenario's existing response. Non-array values in the scenario's response will take priority,
 *  array values will be combined (duplicate array values will *not* be removed).
 * @param scenario 
 * @param response 
 */
function mergeResponseAndScenario(scenario: CAT.TestScenario, response: CAT.ResponseBody): void {
  const mergedResponse: CAT.TestResponse = {...response, ...scenario.response }

  //Combine lists
  if (response.schemas && scenario.response.schemas) {
    mergedResponse.schemas = scenario.response.schemas.concat(response.schemas)
  }
  if (response.properties && scenario.response.properties){
    mergedResponse.properties = scenario.response.properties.concat(response.properties)
  }

  scenario.response = mergedResponse;
}

/**
 * Converts compact test configurations into runnable tests
 * @param compactScenario Configurations to be converted into runnable Test[]
 */
export function unzipScenarios(compactScenario: CAT.CompactTestScenario): CAT.TestScenario[] {
  const sharedRequests = unzipSharedRequest(compactScenario.sharedRequest);
  const sharedResponse = compactScenario.sharedResponse;
  const scenarios = compactScenario.scenarios;

  const expandedScenarios: CAT.TestScenario[] = [];
  scenarios.forEach((scenario) => {
    //Add shared response components
    if(sharedResponse) {
      mergeResponseAndScenario(scenario, sharedResponse);
    }

    //Generate new scenarios from shared requests
    if(sharedRequests.length < 1){
      expandedScenarios.push(scenario)
    }
    else {
      sharedRequests.forEach((request) => {
        expandedScenarios.push(mergeRequestAndScenario(scenario, request))
      })
    }
  });

  return expandedScenarios;
}

function buildTitle(scenario: CAT.TestScenario): string {
  return `Given ${scenario.description}; ${scenario.request.method} ${scenario.request.url} returns ${scenario.response.status}${scenario.response.schemas ? ", valid schema" : ""}${scenario.response.properties ? ", correct property values" : ""}`
}

function runSetup(scenario: CAT.TestScenario): Cypress.Chainable<CAT.TestScenario> {
  return scenario.setup ? scenario.setup() : cy.wrap(scenario);
}

function verifyProperties(properties: CAT.PropertyVerify[], body: any){
  properties.forEach((prop) => {
    if(prop.exactValue) {
      expect(body).to.have.property(prop.name).and.eq(prop.exactValue);
    }
    
    if(prop.satisfies){
      console.log("using valueCompare")
      expect(body).to.have.property(prop.name).and.satisfy(prop.satisfies)
    }
  });
}

// eslint-disable-next-line @typescript-eslint/explicit-module-boundary-types
export function runTest(scenario: CAT.TestScenario) {
  it(buildTitle(scenario), () => {
    runSetup(scenario) //Arrange
      .then((test) => {
        cy.request(test.request)//Act
          .then((response) => {
            //Assert
            expect(response).to.have.property('status', test.response.status);

            // Parse body as JSON if indicated
            const body = test.response.parseAsJSON && response.body ? JSON.parse(response.body) : response.body;

            if(test.response.bodyIsEmpty) {
              expect(body).to.be.empty;
            }

            if(test.response.schemas) {
              test.response.schemas.forEach((schema) => expect(body).to.have.jsonSchema(schema));
            }

            if(test.response.properties){
              //verify properties
              verifyProperties(test.response.properties, body);
            }            
          })
      });
  });
}