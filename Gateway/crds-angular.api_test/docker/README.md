# Running on Docker
## Quick Start
0. Install Docker (and docker-compose) and start the service
1. Navigate to the current folder `./crds-angular.api_test/docker`
2. Set environment variables described in the [main readme](../README.md) and additional ones [below](#Environment-Variables)
3. Run tests with `docker-compose up`
- If changes are made to the tests or packages the Docker image must be updated. Rebuild and run with `docker-compose up --build`. 
- Running with a different config file does not require a rebuild, just change the environment variable and re-run with `docker-compose up` 


## Environment Variables
The following environment variable need to be available in the terminal you're running commands in.
```bash
CYPRESS_CONFIG_FILE
```

If you want to report results to the Cypress Dashboard, the following variables must also be set and the `docker-compose.yml` file updated. Results should *only* be reported to the Dashboard when running in the CI pipeline so we don't clog our metrics with experimental results.
```bash
CYPRESS_RECORD_KEY
CRDS_ENV
```
If you really need to record something locally, please set the CRDS_ENV variable to something like "test" or "experimental". Please do *not* set it to `int`, `demo` or `prod` as these are reserved for tests run in the CI pipeline.