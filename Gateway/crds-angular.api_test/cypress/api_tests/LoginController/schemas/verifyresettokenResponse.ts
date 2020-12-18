// This schema verify a property has the correct type if it exists, it does not assert the property must exist
export const mpVerifyResetTokenSchemaProperties = {
  title: "MP verify reset token response - property types",
  type: "object",
  properties: {
    TokenValid: {
      type: "boolean"
    }
  }
};

// This schema asserts that a property exists, it does not assert it is of the correct type
export const mpVerifyResetTokenContract = {
  title: "MP verify reset token response - status contract",
  type: "object",
  required: ["TokenValid"],
};