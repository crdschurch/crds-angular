import { addAuthorizationHeader } from "shared/authorization/mp_user_auth";
import { getTempTesterEmail, getTestPassword } from "shared/data_generator";

describe("Register then get user info workflow", () =>{
  it("Registers a new user then fetches their information", () => {
    const email = getTempTesterEmail();
    const password = getTestPassword();

    const registerUserRequest: Partial<Cypress.RequestOptions> = {
      url: "/api/user",
      method: "POST",
      body: {
        firstname: "Testing",
        lastname: "Gateway",
        email,
        password
      }    
    };

    //Register new user
    cy.request(registerUserRequest)
    .its('body')
    .then((user) => {
      expect(user).to.have.property('contactId');
      const contactId = user.contactId;

      //Authorize and fetch new user data
      const getUserRequest: Partial<Cypress.RequestOptions> = {
        url: "/api/user",
        method: "GET",
        qs: {
          username: email
        }
      }
      addAuthorizationHeader(email, password, getUserRequest)
      .then(() => cy.request(getUserRequest))
      .its('body')
      .should('have.property', 'userId', contactId); //The userId property is actually the Contact Id, not the User Account id
    })
  });
})