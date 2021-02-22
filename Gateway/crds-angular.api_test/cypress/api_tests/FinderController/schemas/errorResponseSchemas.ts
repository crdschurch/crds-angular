export const errorResponseProperties = {
  title: "Error response - property types",
  type: "object",
  properties: {
    Message:{
      type: "string"
    },
    Errors:{
      type: "array",
      minItems: 0,
      items: {
        type: "string"
      }
    }
  }
};

export const errorResponseContract = {
  title: "Error response - contract",
  type: "object",
  required: ["Message"]
};