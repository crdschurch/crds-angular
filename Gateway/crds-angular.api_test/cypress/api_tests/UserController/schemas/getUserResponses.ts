// This schema verify a property has the correct type if it exists, it does not assert the property must exist
export const getUserSchemaProperties = {
  title: "MP authenticated response - property types",
  type: "object",
  properties: {
    userToken: {
      type: "string"
    },
    userTokenExp: {
      type: [ "string", "null"]
    },
    refreshToken: {
      type: [ "string", "null"]
    },
    userId: {
      type: "number"
    },
    username: {
      type: "string"
    },
    userEmail: {
      type: "string"
    },
    roles: {
      type: "array",
      items: {
        title: "MP Role",
        type: "object",
        properties: {
          Id: { type: "number" },
          Name: { type: "string" }
        }
      },
      minItems: 0
    },
    canImpersonate: {
      type: "boolean"
    },
    age: {
      type: "number"
    },
    userPhone: {
      type: ["string", "null"]
    }
  }
};

// This schema asserts that a property exists, it does not assert it is of the correct type
export const getUserAuthContract = {
  title: "MP user response - user info contract",
  type: "object",
  required: ["userToken", "userId", "userEmail", "username", "roles", "age", "userPhone", "canImpersonate"],
};