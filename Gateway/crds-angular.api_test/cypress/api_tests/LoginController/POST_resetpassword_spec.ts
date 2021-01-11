import { setPasswordResetToken } from "shared/mp_api";
import { Gatekeeper, KeeperJr } from 'shared/users';
import { getTestPassword, getUUID } from "shared/data_generator";
import { runTest, unzipScenarios } from "shared/CAT/cypress_api_tests";
import { Placeholders } from "shared/enums";

// Data Setup
const sharedRequest = {
  urls: ["/api/resetpassword", "/api/v1.0.0/reset-password"],
  options: {
    method: "POST",
    failOnStatusCode: false
  }
}
const successScenarios: CAT.CompactTestScenario  ={
  sharedRequest,
  scenarios: [
    {
      description: "Valid Request",
      request: {
        body: {
          password: getTestPassword(),
          token: Placeholders.assignedInSetup
        }
      },
      setup() {
        const token = getUUID();
        (this.request.body as {token:string}).token = token;
        return setPasswordResetToken(Gatekeeper.email, token)
        .then(() => this);
      },
      response: {
        status: 200
      }
    },
    {
      description: "New Password is Current Password",
      request: {
        body: {
          password: KeeperJr.password,
          token: Placeholders.assignedInSetup
        }
      },
      setup() {
        //TODO should set password first to guarantee this scenario
        const token = getUUID();
        (this.request.body as {token:string}).token = token;
        return setPasswordResetToken(KeeperJr.email, token)
        .then(() => this);
      },
      response: {
        status: 200
      }
    }
  ]
}

const serverErrorScenario: CAT.CompactTestScenario = {
  sharedRequest,
  scenarios: [
    {
      description: "Reset Token that Does Not Match any Reset Request",
      request: {
        body: {
          password: getTestPassword(),
          token: getUUID()
        }
      },
      setup() {
        return setPasswordResetToken(Gatekeeper.email, "").then(() => this)
      },
      response: {status: 500}
    },
    {
      description: "Reset Token Value is Substring of Existing Reset Request",
      request: {
        body: {
          password: getTestPassword(),
          token: Placeholders.assignedInSetup
        }
      },
      setup() {
        const token = getUUID();
        (this.request.body as {token:string}).token = token;
        return setPasswordResetToken(Gatekeeper.email, `${token}9`).then(() => this);
      },
      response: {status: 500}
    },
    {
      description: "Request is Missing Reset Token",
      request: {
        body: {
          password: getTestPassword()
        }
      },
      setup() {
        return setPasswordResetToken(Gatekeeper.email, getUUID()).then(() => this);
      },
      response: {status: 500}
    },
    {
      description: "Request is Missing Password",
      request: {
        body: {
          token: Placeholders.assignedInSetup
        }
      },
      setup() {
        const token = getUUID();
        (this.request.body as {token:string}).token = token;
        return setPasswordResetToken(Gatekeeper.email, token).then(() => this);
      },
      response: {status: 500}
    }
  ]
}

describe("/Login/ResetPassword()", () => {
  unzipScenarios(successScenarios).forEach(runTest)
  unzipScenarios(serverErrorScenario).forEach(runTest)
})