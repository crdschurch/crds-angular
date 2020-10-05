# API tests with Cypress
## Quick Start
0. Install Node.js
1. Set environment variables
2. Run `npm install` to install packages
3. Run `npm run test` to run tests against Int in the command line OR run `npm run test_ui` to open the Cypress Test Runner and run or debug tests with a UI.
- To run tests against a different environment see the [config files](#Config-Files) section.

## Environment variables
The following environment variables need to be available in the terminal you're running commands in. They need to have access to the environment you want to run tests against.
```bash
VAULT_ROLE_ID
VAULT_SECRET_ID
```

Want to run on Docker? Set the variables above and checkout the [readme](./docker/README.md) for additional setup.

## Config Files

A config file has been created for each live environment and for localhost and stored in `/cypress/config` folder. All environment-specific non-sensitive variables should be stored here.
Sensitive variables should be stored in Vault, and added to the `/cypress/config/vault_config.json` file for retrieval. 
Run Cypress with a config file using the `--config-file` flag
```bash
npx cypress open --config-file ./cypress/config/demo_crossroads.json
npx cypress run --config-file ./cypress/config/localhost.json
```

## Naming Test Files

Cypress looks for test files in the `/cypress/integration` folder. Test files must end with `spec.ts`, other files in that folder/subfolders will be ignored by the Cypress runner. 
To avoid running tests that will modify data in Prod, a test file must be whitelisted for production by using the `prodspec.ts` suffix. 
```javascript
file1_spec.ts //runs in int, demo, local
file1_prodspec.ts //runs in int, demo, local, prod
file1_otherspec.ts //runs in int, demo, local
```

## Recording Test Runs

Test runs can be reported to the [Cypress Dashboard](https://dashboard.cypress.io/projects/s1jczs), which will store logs, screenshots and other metrics related to the tests and execution environment. Recording test runs during development doesn't provide much historic value, so this feature should only be used during deployment or as a health check against live environments. Details for recording can be found in the [Docker readme](./docker/README.md)

## Details for the curious
- `cypress.json` is the default configuration file if the `--config-file` flag is not set. It is configured to run tests against int. There are many more options documented [here](https://docs.cypress.io/guides/references/configuration.html#Options).


## WIP & Enhancements
### JSON Validation & Design Decisions

I'm tentatively designing the JSON schema validations with contract testing in mind. There will be one schema to verify property types are correct, but will not assert that the properties exist, and another/others that verify specific properties exist. The idea around contract testing is to maintain a record of what's actually needed by every feature that uses an endpoint, which will allow maintainers of the API to modify endpoints safely.

I'm using AJV wrapped in Chai to validate the JSON schemas of endpoint responses. There are many versions of the schema definitions but the package installed supports up to Draft 07. Right now I'm using whatever Draft is default in the Chai package since that seems to be sufficient for our test needs, but will look in a specific Draft version once I understand the differences better.
Some resources:
- http://json-schema.org/specification.html
- https://json-schema.org/understanding-json-schema/index.html
- https://assertible.com/json-schema-validation

