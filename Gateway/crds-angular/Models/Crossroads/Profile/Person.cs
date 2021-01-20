using System;
using System.Collections.Generic;
using crds_angular.Models.Crossroads.Attribute;
using MinistryPlatform.Translation.Models;
using Newtonsoft.Json;
using Address = crds_angular.Models.MP.Address;

namespace crds_angular.Models.Crossroads.Profile
{
  public class Person
  {
    [JsonProperty(PropertyName = "addressId")]
    public int? AddressId { get; set; }

    [JsonProperty(PropertyName = "addressLine1")]
    public string AddressLine1 { get; set; }

    [JsonProperty(PropertyName = "addressLine2")]
    public string AddressLine2 { get; set; }

    [JsonProperty(PropertyName = "age")]
    public int Age { get; set; }

    [JsonProperty(PropertyName = "passportNumber")]
    public string PassportNumber { get; set; }

    [JsonProperty(PropertyName = "passportFirstname")]
    public string PassportFirstname { get; set; }

    [JsonProperty(PropertyName = "passportLastname")]
    public string PassportLastname { get; set; }

    [JsonProperty(PropertyName = "passportMiddlename")]
    public string PassportMiddlename { get; set; }

    [JsonProperty(PropertyName = "passportExpiration")]
    public string PassportExpiration { get; set; }

    [JsonProperty(PropertyName = "passportCountry")]
    public string PassportCountry { get; set; }

    [JsonProperty(PropertyName = "anniversaryDate")]
    public string AnniversaryDate { get; set; }

    [JsonProperty(PropertyName = "city")]
    public string City { get; set; }

    [JsonProperty(PropertyName = "congregationId")]
    public int? CongregationId { get; set; }

    [JsonProperty(PropertyName = "contactId")]
    public int ContactId { get; set; }

    [JsonProperty(PropertyName = "dateOfBirth")]
    public string DateOfBirth { get; set; }

    [JsonProperty(PropertyName = "emailAddress")]
    public string EmailAddress { get; set; }

    [JsonProperty(PropertyName = "employerName")]
    public string EmployerName { get; set; }

    [JsonProperty(PropertyName = "firstName")]
    public string FirstName { get; set; }

    [JsonProperty(PropertyName = "foreignCountry")]
    public string ForeignCountry { get; set; }

    [JsonProperty(PropertyName = "genderId")]
    public int? GenderId { get; set; }

    [JsonProperty(PropertyName = "homePhone")]
    public string HomePhone { get; set; }

    [JsonProperty(PropertyName = "householdId")]
    public int HouseholdId { get; set; }

    [JsonProperty(PropertyName = "householdName")]
    public string HouseholdName { get; set; }

    [JsonProperty(PropertyName = "lastName")]
    public string LastName { get; set; }

    [JsonProperty(PropertyName = "maidenName")]
    public string MaidenName { get; set; }

    [JsonProperty(PropertyName = "maritalStatusId")]
    public int? MaritalStatusId { get; set; }

    [JsonProperty(PropertyName = "middleName")]
    public string MiddleName { get; set; }

    [JsonProperty(PropertyName = "mobileCarrierId")]
    public int? MobileCarrierId { get; set; }

    [JsonProperty(PropertyName = "mobilePhone")]
    public string MobilePhone { get; set; }

    [JsonProperty(PropertyName = "nickName")]
    public string NickName { get; set; }

    // this is only sent from the client when resetting the password as
    // part of a profile save
    [JsonProperty(PropertyName = "newPassword")]
    public string NewPassword { get; set; }

    [JsonProperty(PropertyName = "oldEmail")]
    public string OldEmail { get; set; }

    [JsonProperty(PropertyName = "oldPassword")]
    public string OldPassword { get; set; }

    [JsonProperty(PropertyName = "postalCode")]
    public string PostalCode { get; set; }

    [JsonProperty(PropertyName = "county")]
    public string County { get; set; }

    [JsonProperty(PropertyName = "state")]
    public string State { get; set; }

    [JsonProperty(PropertyName = "householdMembers")]
    public List<MpHouseholdMember> HouseholdMembers { get; set; }

    [JsonProperty(PropertyName = "attributeTypes")]
    public Dictionary<int, ObjectAttributeTypeDTO> AttributeTypes { get; set; }

    [JsonProperty(PropertyName = "singleAttributes")]
    public Dictionary<int, ObjectSingleAttributeDTO> SingleAttributes { get; set; }

    [JsonProperty(PropertyName = "participantStartDate")]
    public DateTime? ParticipantStartDate { get; set; }

    [JsonProperty(PropertyName = "attendanceStartDate")]
    public DateTime? AttendanceStartDate { get; set; }

    public MpMyContact GetContact()
    {
      return new MpMyContact
      {
        Contact_ID = ContactId,
        Email_Address = EmailAddress,
        First_Name = FirstName,
        Middle_Name = MiddleName,
        Last_Name = LastName,
        Display_Name = $"{this.LastName}, {this.NickName}",
        Maiden_Name = MaidenName,
        Nickname = NickName,
        Mobile_Phone = MobilePhone,
        Mobile_Carrier = MobileCarrierId,
        Date_Of_Birth = DateOfBirth,
        Marital_Status_ID = MaritalStatusId,
        Gender_ID = GenderId,
        Employer_Name = EmployerName,
        Address_ID = AddressId,
        Address_Line_1 = AddressLine1,
        Address_Line_2 = AddressLine2,
        City = City,
        State = State,
        Postal_Code = PostalCode,
        County = County,
        Congregation_ID = CongregationId,
        Household_ID = HouseholdId,
        Passport_Country = PassportCountry,
        Passport_Expiration = ParseExpiration(PassportExpiration),
        Passport_Firstname = PassportFirstname,
        Passport_Lastname = PassportLastname,
        Passport_Middlename = PassportMiddlename,
        Passport_Number = PassportNumber
      };
    }

    public MpHousehold GetHousehold()
    {
      return new MpHousehold
      {
        Address_ID = AddressId,
        Household_ID = HouseholdId,
        Home_Phone = HomePhone,
        Congregation_ID = CongregationId
      };
    }

    public Address GetAddress()
    {
      return new Address
      {
        Address_ID = AddressId,
        Address_Line_1 = AddressLine1,
        Address_Line_2 = AddressLine2,
        City = City,
        State = State,
        Postal_Code = PostalCode,
        County = County,
        Foreign_Country = ForeignCountry
      };
    }

    //Dictionary<string, object> userUpdateValues = new Dictionary<string, object>();
    public Dictionary<string, object> GetUserUpdateValues()
    {
      var userUpdateValues = new Dictionary<string, object>();

      if (!string.IsNullOrEmpty(NewPassword))
      {
        userUpdateValues["Password"] = NewPassword;
      }

      userUpdateValues["User_Name"] = EmailAddress;
      userUpdateValues["User_Email"] = EmailAddress;
      userUpdateValues["Display_Name"] = $"{this.LastName}, {this.NickName}";

      return userUpdateValues;
    }

    private DateTime? ParseExpiration(string exp)
    {
      if (exp == null)
        return null;

      return DateTime.Parse(PassportExpiration);
    }
  }
}
