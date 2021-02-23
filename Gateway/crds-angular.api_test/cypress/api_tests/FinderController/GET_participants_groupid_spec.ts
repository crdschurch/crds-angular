import { runTest, unzipScenarios } from "shared/CAT/cypress_api_tests";
import { Group } from "shared/enums";
import { errorResponseContract, errorResponseProperties } from "./schemas/errorResponseSchemas";
import { groupParticipantListContract, groupParticipantListProperties } from "./schemas/groupParticipantListSchemas";

function setUrl(groupId: string, request: Partial<Cypress.RequestOptions>) {
  request.url = request.url?.replace('{groupId}', groupId);
}

// This is only used here
const emptyArray = {
  type: "array",
  maxItems: 0
}

const sharedRequest = {
  urls: ['/api/finder/participants/{groupId}', '/api/v1.0.0/finder/participants/{groupId}'],
  options: {
    method: 'GET',
    failOnStatusCode: false
  }
}

const successScenarios: CAT.CompactTestScenario = {
  sharedRequest,
  scenarios: [
    {
      description: "Group with participants",
      request: {},
      setup() {
        setUrl(`${Group.groupWithParticipants}`, this.request);
        return cy.wrap(this);
      },
      response: {
        status: 200,
        schemas: [groupParticipantListProperties, groupParticipantListContract]
      }
    },
    {
      description: "Group with no participants",
      request: {},
      setup() {
        setUrl(`${Group.emptyGroup}`, this.request);
        return cy.wrap(this);
      },
      response: {
        status: 200,
        schemas: [emptyArray]
      }
    },
    {
      description: "Group doesn't exist",
      request: {},
      setup() {
        setUrl('000', this.request);
        return cy.wrap(this);
      },
      response: {
        status: 200,
        schemas: [emptyArray]
      }
    },
    {
      description: "Group has ended",
      request: {},
      setup() {
        setUrl(`${Group.endedGroup}`, this.request);
        return cy.wrap(this);
      },
      response: {
        status: 200,
        schemas: [emptyArray]
      }
    }
  ]
}

const notFoundScenario: CAT.CompactTestScenario = {
  sharedRequest,
  scenarios: [
    {
      description: "Request missing group id",
      request: {},
      setup() {
        setUrl('', this.request);
        return cy.wrap(this);
      },
      response: {
        status: 404
      }
    }
  ]
}

const badRequestScenarios: CAT.CompactTestScenario = {
  sharedRequest,
  scenarios: [
    {
      description: "Group name instead of id",
      request: {},
      setup() {
        setUrl('FI Oakley Team', this.request);
        return cy.wrap(this);
      },
      response: {
        status: 400,
        schemas: [errorResponseProperties, errorResponseContract],
        properties: [
          { name: "Message", exactValue: "The request is invalid." }
        ]
      }
    }
  ]
}

describe('/Finder/GetParticipantsForGroup()', () => {
  unzipScenarios(successScenarios).forEach(runTest);
  unzipScenarios(notFoundScenario).forEach(runTest);
  unzipScenarios(badRequestScenarios).forEach(runTest);
})