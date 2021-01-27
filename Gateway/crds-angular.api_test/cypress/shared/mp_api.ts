// Access to MP through their API
import { addAuthorizationHeader } from "./authorization/mp_client_auth";

export function getMPUser(email: string): Cypress.Chainable<MPModels.User> {
  const userIdRequest: Partial<Cypress.RequestOptions> = {
    url: `${Cypress.env('MP_REST_API_ENDPOINT')}/tables/dp_Users`,
    method: "GET",
    qs: {
      '$filter': `User_Name='${email}'`
    }
  };

  return addAuthorizationHeader(userIdRequest)
    .then(cy.request)
    .its('body')
    .then(body => {
      const userData = body[0];
      assert(userData, userData ? '' : `User ${email} could not be found in MP`);
      return userData;
    });
}

export function setPasswordResetToken(email: string, resetToken: string): Cypress.Chainable<Cypress.Response> {
  return getMPUser(email)
    .then(mpUser => {
      const updateResetTokenRequest: Partial<Cypress.RequestOptions> = {
        url: `${Cypress.env('MP_REST_API_ENDPOINT')}/tables/dp_Users`,
        method: "PUT",
        body: [{ User_ID: mpUser.User_ID, PasswordResetToken: resetToken }]
      };

      return addAuthorizationHeader(updateResetTokenRequest)
        .then(cy.request);
    });
}

export function setCanImpersonateValue(email: string, canImpersonate: boolean): Cypress.Chainable<Cypress.Response> {
  return getMPUser(email)
    .then(mpUser => {
      const updateCanImpersonate: Partial<Cypress.RequestOptions> = {
        url: `${Cypress.env('MP_REST_API_ENDPOINT')}/tables/dp_Users`,
        method: "PUT",
        body: [{ User_ID: mpUser.User_ID, Can_Impersonate: canImpersonate }]
      };

      return addAuthorizationHeader(updateCanImpersonate)
        .then(cy.request);
    })
}

/**
 * Returns a Contact whose username (from their User account) matches the given email.
 * @param email 
 */
export function getContactRecord(email: string): Cypress.Chainable<MPModels.Contact> {
  return getMPUser(email)
    .then(mpUser => {
      const contactId = mpUser.Contact_ID;
      const getContactRecord: Partial<Cypress.RequestOptions> = {
        url: `${Cypress.env('MP_REST_API_ENDPOINT')}/tables/Contacts/${contactId}`,
        method: "GET",
      };

      return addAuthorizationHeader(getContactRecord)
        .then(cy.request)
        .its('body')
        .then(body => {
          const contactData = body[0];
          assert(contactData, contactData ? '' : `Contact with email ${email} and contact id ${contactId} could not be found in MP`);
          return contactData;
        });
    });
}