export const unsupportedMediaProperties = {
  title: "Unsupported Media response - property types",
  type: "object",
  properties: {
    Message:{
      type: "string"
    }
  }
};

export const unsupportedMediaContract = {
  title: "Unsupported Media response - testability contract",
  type: "object",
  required: ["Message"]
};