using System;
using Crossroads.Web.Common.MinistryPlatform;

namespace MinistryPlatform.Translation.Models
{
    [MpRestApiTable(Name = "Contacts")]
    public class MpMyContact
    {
        public int? Address_ID { get; set; }
        public string Address_Line_1 { get; set; }
        public string Address_Line_2 { get; set; }
        public int Age { get; set; }
        public string City { get; set; }
        public int? Congregation_ID { get; set; }
        public int Contact_ID { get; set; }
        public string County { get; set; }
        public string Date_Of_Birth { get; set; }
        public string Display_Name { get; set; }
        public string Email_Address { get; set; }
        public string Employer_Name { get; set; }
        public string First_Name { get; set; }
        public string Current_School { get; set; }
        public string Foreign_Country { get; set; }
        public int? Gender_ID { get; set; }
        public string Home_Phone { get; set; }
        public int Household_ID { get; set; }
        public string Household_Name { get; set; }
        public string Last_Name { get; set; }
        public string Maiden_Name { get; set; }
        public int? Marital_Status_ID { get; set; }
        public string Middle_Name { get; set; }
        public int? Mobile_Carrier { get; set; }
        public string Mobile_Phone { get; set; }
        public string Nickname { get; set; }
        public string Postal_Code { get; set; }
        public string State { get; set; }
        public string ID_Number { get; set; }
        public string Passport_Firstname { get; set; }
        public string Passport_Lastname { get; set; }
        public string Passport_Middlename { get; set; }
        public string Passport_Country { get; set; }
        public string Passport_Number { get; set; }
        public DateTime? Passport_Expiration { get; set; }
        public DateTime? Participant_Start_Date { get; set; }
        public DateTime? Attendance_Start_Date { get; set; }
    }
}