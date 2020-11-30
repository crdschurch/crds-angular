FROM cypress/included:5.6.0
WORKDIR /api_tests

# Copy configs
COPY *.json ./ 
COPY cypress cypress

ENV CI=1
RUN npm ci --production && npx cypress verify

# Same default as $npm run test
ENTRYPOINT [ "npx", "cypress", "run" ]
CMD [ "--config-file", "/cypress/config/api/int.json" ]