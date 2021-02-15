declare namespace MPModels {
  type NullableNumber = number | null; //Database return type

  interface User {
    User_Name: string,
    User_ID: number,
    Password: string,
    PasswordResetToken: string,
    Contact_ID: number
  }

  interface Contact {
    Contact_ID: number,
    Email_Address: string,
    Donor_Record: NullableNumber,
    Household_ID: number
  }
}