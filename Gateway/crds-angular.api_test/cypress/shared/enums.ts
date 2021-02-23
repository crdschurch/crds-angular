export enum Placeholders {
  assignedInSetup = "This value should be assigned in the setup step. If it isn't, update the setup."
}

// MP lookup table
export enum HouseholdSource {
  communityGroup = 38
}

// MP group
// Available in Int, Demo, Prod unless specified
export enum Group {
  groupWithParticipants = 198355, 
  emptyGroup = 195326, 
  endedGroup = 177381 
}