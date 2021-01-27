
import { addAuthorizationHeader as authorizeWithMP } from "shared/authorization/mp_user_auth";
import { addAuthorizationHeader as authorizeWithOkta } from "shared/authorization/okta_user_auth";
import { runTest, unzipScenarios } from "shared/CAT/cypress_api_tests";
import { Placeholders } from "shared/enums";
import { getContactRecord, setCanImpersonateValue } from "shared/mp_api";
import { Gatekeeper, KeeperJr } from "shared/users";
import { errorResponseContract, errorResponseProperties } from "./schemas/errorResponseSchemas";
import { getProfileContract, getProfilePropertiesSchema } from "./schemas/getProfileSchema";

// We're going to set impersonation rights before we run any tests
const Impersonator = Gatekeeper;
const NotImpersonator = KeeperJr;

function setResponsePropertyValue(response: CAT.TestResponse, propName: string, propValue: string) {
  (response.properties?.find(r => r.name === propName) as CAT.PropertyCompare).value = propValue;
}

const sharedRequest = {
  urls: ['/api/profile'],
  options: {
    method: 'GET',
    failOnStatusCode: false
  }
}

const successScenarios: CAT.CompactTestScenario = {
  sharedRequest,
  sharedResponse: {
    schemas: [getProfilePropertiesSchema, getProfileContract],
    properties: [{ name: "emailAddress", value: Placeholders.assignedInSetup }]
  },
  scenarios: [
    {
      description: "User authorized with MP",
      request: {},
      setup() {
        setResponsePropertyValue(this.response, "emailAddress", NotImpersonator.email);
        return authorizeWithMP(NotImpersonator.email, NotImpersonator.password as string, this.request)
          .then(() => this);
      },
      response: {
        status: 200
      }
    },
    {
      description: "User authorized with Okta",
      request: {},
      setup() {
        setResponsePropertyValue(this.response, "emailAddress", NotImpersonator.email);
        return authorizeWithOkta(NotImpersonator.email, NotImpersonator.password as string, this.request)
          .then(() => this);
      },
      response: {
        status: 200
      }
    },
    {
      description: "User with impersonation privileges and valid donor id query parameter",
      request: {
        qs: {
          impersonateDonorId: Placeholders.assignedInSetup
        }
      },
      setup() {
        setResponsePropertyValue(this.response, "emailAddress", Impersonator.email);
        return getContactRecord(NotImpersonator.email)
          .then((contact) => {
            // Set Params to impersonate another user
            (this.request.qs as { impersonateDonorId: MPModels.NullableNumber }).impersonateDonorId = contact.Donor_Record;

            // Authorize
            return authorizeWithMP(Impersonator.email, Impersonator.password as string, this.request)
          })
          .then(() => this);
      },
      response: {
        status: 200
      },
      preferredResponse: {
        // Sooooo, I'd expect it to actually get the info of the person we're trying to impersonate...
        status: 200,
        schemas: [getProfilePropertiesSchema, getProfileContract],
        properties: [{ name: "emailAddress", value: NotImpersonator.email }]
      }
    },
    {
      description: "User with impersonation privileges and with empty donor id query parameter",
      request: {
        url: '/api/profile',
        method: 'GET',
        failOnStatusCode: false,
        qs: {
          impersonateDonorId: ""
        }
      },
      setup() {
        setResponsePropertyValue(this.response, "emailAddress", Impersonator.email);
        return authorizeWithMP(Impersonator.email, Impersonator.password as string, this.request)
          .then(() => this);
      },
      response: {
        status: 200
      }
    }
  ]
}

const forbiddenScenarios: CAT.CompactTestScenario = {
  sharedRequest,
  sharedResponse: {
    schemas: [errorResponseProperties, errorResponseContract],
    properties: [{ name: "message", value: "User is not authorized to impersonate other users." }]
  },
  scenarios: [
    {
      description: "User without impersonation privileges",
      request: {
        qs: {
          impersonateDonorId: Placeholders.assignedInSetup
        }
      },
      setup() {
        return getContactRecord(Impersonator.email)
          .then((contact) => {
            // Set Params to impersonate another user
            (this.request.qs as { impersonateDonorId: MPModels.NullableNumber }).impersonateDonorId = contact.Donor_Record;

            // Authorize
            return authorizeWithMP(NotImpersonator.email, NotImpersonator.password as string, this.request)
          })
          .then(() => this);
      },
      response: {
        status: 403
      }
    },
    {
      description: "User without impersonation privileges and with invalid donor Id",
      request: {
        qs: {
          impersonateDonorId: 1111111
        }
      },
      setup() {
        return authorizeWithMP(NotImpersonator.email, NotImpersonator.password as string, this.request)
          .then(() => this);
      },
      response: {
        status: 403
      }
    }
  ]
}

const conflictScenarios: CAT.CompactTestScenario = {
  sharedRequest,
  scenarios: [
    {
      description: "User with impersonation privileges but with invalid donor id",
      request: {
        qs: {
          impersonateDonorId: 1111111
        }
      },
      setup() {
        return authorizeWithMP(Impersonator.email, Impersonator.password as string, this.request)
          .then(() => this);
      },
      response: {
        status: 409,
        schemas: [errorResponseProperties, errorResponseContract],
        properties: [{ name: "message", value: "Could not locate user '' to impersonate." }]
      }
    }
  ]
}

const unauthorizedScenarios: CAT.CompactTestScenario = {
  sharedRequest,
  scenarios: [
    {
      description: "Unauthorized request",
      request: {},
      response: {
        status: 401,
        bodyIsEmpty: true
      }
    },
    {
      description: "Unauthorized request with impersonate donor query parameter",
      request: {
        qs: {
          impersonateDonorId: Placeholders.assignedInSetup
        }
      },
      setup() {
        return getContactRecord(NotImpersonator.email)
          .then((contact) => {
            // Set Params to impersonate another user
            (this.request.qs as { impersonateDonorId: MPModels.NullableNumber }).impersonateDonorId = contact.Donor_Record;
          })
          .then(() => this);
      },
      response: {
        status: 401,
        bodyIsEmpty: true
      }
    }
  ]
}

describe('/profile/GetProfile()', () => {
  before(() => {
    setCanImpersonateValue(Impersonator.email, true);
    setCanImpersonateValue(NotImpersonator.email, false);
  });

  unzipScenarios(successScenarios).forEach(runTest);
  unzipScenarios(forbiddenScenarios).forEach(runTest);
  unzipScenarios(conflictScenarios).forEach(runTest);
  unzipScenarios(unauthorizedScenarios).forEach(runTest);
});