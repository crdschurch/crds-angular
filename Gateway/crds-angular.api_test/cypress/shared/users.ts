export interface TestUser {
  email: string;
  password?: string;
  firstName?: string;
}

/**
 * Ben is a test user with the same MP contact ID in every environment.
 * It is safe to authenticate with either MP or Okta, and will be safe post-cutover.
 * DO NOT change his email, password or anything that may prevent him from logging in,
 *   even temporarily. His account is used by many automated tests.
 */
export const Ben: TestUser = {
  email: "mpcrds+auto+2@gmail.com",
  password: Cypress.env("BEN_KENOBI_PW"),
  firstName: 'Ben'
};

/**
 * Sue is a test user with the same MP contact ID in every environment.
 * It is safe to authenticate with either MP or Okta, and will be safe post-cutover.
 * DO NOT change her email, password or anything that may prevent her from logging in,
 *   even temporarily.
 */
export const Sue: TestUser = {
  email: "mpcrds+auto+suesmith@gmail.com",
  password: Cypress.env("TEST_USER_PW"), 
  firstName: 'Sue'
};

/**
 * Gate is a test user in non-prod environments.
 * They cannot be authenticated through Okta in Demo.
 * Their password in MP may be different than in Okta
 */
export const Gatekeeper: TestUser = {
  firstName: "Gate",
  email: "mpcrds+auto+gatekeeper@testmail.com",
  password: Cypress.env("TEST_GATEKEEPER_PW") //This may be inaccurate
};


/**
 * Jr is a test user in non-prod environments.
 * They cannot be authenticated through Okta in Demo.
 * They have an email address that is a subset of another user's email address.
 */
export const KeeperJr: TestUser = {
  email: "auto+gatekeeper@testmail.com",
  password: Cypress.env("TEST_GATEKEEPERJR_PW"),
  firstName: "Gate"
};


/**
 * Load only has an MP Contact record, but they are in each environment.
 * They cannot be authenticated.
 */
export const Load: TestUser = {
  email: "mpcrds+LoadTest_98@gmail.com",
  password: undefined
};

/**
 * Luke is a test user in non-prod environments.
 * They can only be authenticated through Okta.
 */
export const Luke: TestUser = {
  email: "mpcrds+auto+child1@gmail.com",
  password: Cypress.env("TEST_USER_PW")
}