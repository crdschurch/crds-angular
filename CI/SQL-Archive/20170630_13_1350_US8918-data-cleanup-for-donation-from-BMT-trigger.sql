USE [MinistryPlatform]
GO

WITH BMT_Corrupted_Donation_Data (
  Donation_Date,
  Donation_ID,
  Donation_Distribution_ID,
  Amount,
  First_Name,
  Last_Name,
  Donation_Distributions_Congregation_ID,
  Donation_Distributions_Congregation,
  Donor_Current_Congregation_ID,
  Donor_Current_Congregation,
  Batch_ID,
  Batch_Entry_Type,
  Setup_Date
)
AS (
    SELECT
      Donations.Donation_Date,
      Donations.Donation_ID,
      Donation_Distributions.Donation_Distribution_ID,
      Donation_Distributions.Amount AS Distribution_Amount,
      Contacts.First_Name,
      Contacts.Last_Name,
      Donation_Distributions.Congregation_ID AS Donation_Distributions_Congregation_ID,
      Con_1.Congregation_Name       AS Donation_Distributions_Congregation,
      Households.Congregation_ID AS Donor_Current_Congregation_ID,
      Con_2.Congregation_Name       AS Donor_Current_Congregation,
      Donations.Batch_ID,
      Batch_Entry_Types.Batch_Entry_Type,
      Batches.Setup_Date
    FROM Donations
      INNER JOIN Donation_Distributions
        ON Donations.Donation_ID = Donation_Distributions.Donation_ID
      INNER JOIN Congregations AS Con_1
        ON Donation_Distributions.Congregation_ID = Con_1.Congregation_ID
      INNER JOIN Donors
        ON Donations.Donor_ID = Donors.Donor_ID
      INNER JOIN Contacts
        ON Donors.Contact_ID = Contacts.Contact_ID
      INNER JOIN Households
        ON Contacts.Household_ID = Households.Household_ID
      INNER JOIN Congregations AS Con_2
        ON Households.Congregation_ID = Con_2.Congregation_ID
      INNER JOIN Batches
        ON Donations.Batch_ID = Batches.Batch_ID
      INNER JOIN Batch_Entry_Types
        ON Batches.Batch_Entry_Type_ID = Batch_Entry_Types.Batch_Entry_Type_ID
    WHERE
      Batch_Entry_Types.Batch_Entry_Type_ID = 12   -- Batch Manager Tool
      AND
      Donations.Donation_Status_Date >= '2017-5-23'
      AND
      Con_1.Congregation_Name = 'Not site specific'
      AND
      Con_1.Congregation_ID <> Con_2.Congregation_ID
)
UPDATE Donation_Distributions
SET Donation_Distributions.Congregation_ID = BMT_Corrupted_Donation_Data.Donor_Current_Congregation_ID
FROM Donation_Distributions
  INNER JOIN BMT_Corrupted_Donation_Data
    ON Donation_Distributions.Donation_Distribution_ID = BMT_Corrupted_Donation_Data.Donation_Distribution_ID
GO