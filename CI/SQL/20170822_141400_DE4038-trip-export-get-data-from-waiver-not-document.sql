USE [MinistryPlatform]
GO

/****** Object:  StoredProcedure [dbo].[report_CRDS_Trip_Export]    Script Date: 8/22/2017 2:12:44 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


ALTER PROCEDURE [dbo].[report_CRDS_Trip_Export] 
  @eventid INT 
AS 
BEGIN
SET nocount ON;

DECLARE @missiontripeventtype INT = 6;

DECLARE @pledgediscontinued INT = 3;

-- MAPPING VALUES
DECLARE @validpassport INT = 1464;

DECLARE @tshirtsize INT = 21;

DECLARE @scrubsizetop INT = 22;

DECLARE @scrubsizebottom INT = 23;

DECLARE @dietaryrestrictions INT = 65;

DECLARE @allergies INT = 67;

DECLARE @sprituallife INT = 60;

DECLARE @previoustripexperience INT = 62;

DECLARE @profession INT = 61;

DECLARE @frequentflyerdelta INT = 3958;

DECLARE @frequentflyersouthafrica INT = 3959;

DECLARE @frequentflyerunited INT = 3960;

DECLARE @frequentflyerusair INT = 3980;

DECLARE @frequentflyeramerican INT = 9049;

DECLARE @frequentflyersouthwest INT = 4623;

DECLARE @internationaltravelexperience INT = 66;

DECLARE @experienceservingabroad INT = 68;

DECLARE @abusevictim INT = 69;

DECLARE @why INT = 1434;

DECLARE @emergencycontactfirstname INT = 1439;

DECLARE @emergencycontactlastname INT = 1440;

DECLARE @emergencycontactprimaryphone INT = 1442;

DECLARE @emergencycontactsecondaryphone INT = 1443;

DECLARE @emergencycontactemailaddress INT = 1441;

DECLARE @emergencycontactrelationship INT = 1272; 

DECLARE @lotterypreference INT = 1444;

DECLARE @commonname INT = 1445;

DECLARE @requestedroommate1 INT = 1446;

DECLARE @requestedroommate2 INT = 1447;

DECLARE @supportpersonemail INT = 1448;

DECLARE @gogroupleaderinterest INT = 1449;

DECLARE @tripguardianfirstname INT = 1426;

DECLARE @tripguardianlastname INT = 1427;

DECLARE @howdidyouhearabouttrip INT = 1433;

DECLARE @medicalconditions INT = 1432;

DECLARE @medicationrestrictions INT = 100; 

DECLARE @medicationstaking INT = 101; 

DECLARE @workteampreference1 INT = 1423;

DECLARE @workteampreference2 INT = 1425;

DECLARE @workteamexperience INT = 1424;

DECLARE @stewardsofchildren INT = 9;

DECLARE @visa INT = 11;

DECLARE @diploma INT = 1;

DECLARE @medicallicense INT = 2;

DECLARE @letterverification INT = 3;

DECLARE @goodfaithletter INT = 4;

DECLARE @goodstandingletter INT = 5;

DECLARE @form10 INT = 6;

DECLARE @medicalapplication INT = 7;

DECLARE @orchardafricawaiver INT = 8;

DECLARE @copyofpassport INT = 10;

DECLARE @ipromise INT = 12;

DECLARE @proofofguardianship INT = 16;

DECLARE @countrydocumentation INT = 17;


SELECT 	c.first_name,
       c.middle_name,
       c.last_name,
       c.maiden_name,
       c.nickname,
	   c.display_name,
       c.email_address,
       c.date_of_birth,
       ms.marital_status,
       g.gender,
       c.employer_name,
       p.attendance_start_date AS first_attendance_ever,
       c.mobile_phone,
       a.address_line_1,
       a.address_line_2,
       a.city,
       a.[state/region],
       a.postal_code,
       a.county,
       a.foreign_country,
       h.home_phone,
       cn.congregation_name,
	case
       when p.Group_Leader_Status_ID = 4 then 'Yes'
       when p.Group_Leader_Status_ID <> 4 then 'No'
       else 'Unknown'
    end as Approved_Small_Group_Leader,

  ISNULL((SELECT top 1 response
	   FROM [ministryplatform].[dbo].[form_response_answers] r
	   join (select fra.Form_Response_Answer_ID as max_id
				from dbo.Form_Response_Answers fra )m
				on m.max_id = r.Form_Response_Answer_ID
   WHERE r.form_field_id = @validpassport
     AND r.form_response_id = fr.form_response_id
	 order by r.Form_Response_Answer_ID desc),'MISSING') AS validpassport,

       c.passport_firstname,
       c.passport_middlename,
       c.passport_lastname,
       c.passport_number,
       c.passport_country,
       c.passport_expiration,

  (SELECT top 1 response
	   FROM [ministryplatform].[dbo].[form_response_answers] r
	   join (select fra.Form_Response_Answer_ID as max_id
				from dbo.Form_Response_Answers fra )m
				on m.max_id = r.Form_Response_Answer_ID
   WHERE r.form_field_id = @tripguardianfirstname
     AND r.form_response_id = fr.form_response_id
	 order by r.Form_Response_Answer_ID desc) AS tripguardianfirstname,

  (SELECT top 1 response
	   FROM [ministryplatform].[dbo].[form_response_answers] r
	   join (select fra.Form_Response_Answer_ID as max_id 
				from dbo.Form_Response_Answers fra )m
				on m.max_id = r.Form_Response_Answer_ID
   WHERE r.form_field_id = @tripguardianlastname
     AND r.form_response_id = fr.form_response_id
	 order by r.Form_Response_Answer_ID desc) AS tripguardianlastname,

  (SELECT top 1 attribute_name
   FROM vw_crds_Contact_Attributes
   WHERE contact_id = c.contact_id
     AND attribute_type_id = @tshirtsize 
	 order by Start_Date desc) AS tshirtsize,

  (SELECT top 1 attribute_name
   FROM vw_crds_Contact_Attributes
   WHERE contact_id = c.contact_id
     AND attribute_type_id = @scrubsizetop 
	 order by Start_Date desc) AS scrubsizetop,

  (SELECT top 1 attribute_name
   FROM vw_crds_Contact_Attributes
   WHERE contact_id = c.contact_id
     AND attribute_type_id = @scrubsizebottom 
	 order by Start_Date desc) AS scrubsizebottom,

  (SELECT STUFF(
                  (SELECT '|' + attribute_name
                   FROM dbo.vw_crds_contact_attributes AS ca
                   WHERE ca.contact_id = c.contact_id
                     AND ca.attribute_type_id = @dietaryrestrictions
                     FOR xml path(''), TYPE).value('.', 'nvarchar(max)'), 1, 1, '')) AS dietary_restrictions,

   (SELECT top 1 notes
	   FROM vw_crds_contact_attributes
	   WHERE contact_id = c.contact_id
		 AND attribute_type_id = @allergies 
	 order by Start_Date desc) AS allergies,

   (SELECT TOP 1 notes
	   FROM vw_crds_contact_attributes
	   WHERE contact_id = c.contact_id
		 AND attribute_type_id = @medicationrestrictions 
		ORDER BY Start_Date DESC) AS medicationrestrictions,

   (SELECT TOP 1 notes
	   FROM vw_crds_contact_attributes
	   WHERE contact_id = c.contact_id
		 AND attribute_type_id = @medicationstaking 
		ORDER BY Start_Date DESC) AS medicationstaking,

	(SELECT TOP 1 notes
	   FROM vw_crds_contact_attributes
	   WHERE contact_id = c.contact_id
		 AND attribute_type_id = @medicalconditions 
		ORDER BY Start_Date DESC) AS medicalconditions,

  (SELECT STUFF(
                  (SELECT '|' + attribute_name
                   FROM dbo.vw_crds_contact_attributes AS ca
                   WHERE ca.contact_id = c.contact_id
                     AND ca.attribute_type_id = @sprituallife
                     FOR xml path(''), TYPE).value('.', 'nvarchar(max)'), 1, 1, '')) AS spritual_life,

  ISNULL((SELECT top 1 response
	   FROM [ministryplatform].[dbo].[form_response_answers] r
	   join (select fra.Form_Response_Answer_ID as max_id
				from dbo.Form_Response_Answers fra )m
				on m.max_id = r.Form_Response_Answer_ID
   WHERE r.form_field_id = @why
     AND r.form_response_id = fr.form_response_id
	 order by r.Form_Response_Answer_ID desc),'MISSING') AS whygoontrip,

  ISNULL((SELECT top 1 response
	   FROM [ministryplatform].[dbo].[form_response_answers] r
	   join (select fra.Form_Response_Answer_ID as max_id 
				from dbo.Form_Response_Answers fra )m
				on m.max_id = r.Form_Response_Answer_ID
   WHERE r.form_field_id = @emergencycontactfirstname
     AND r.form_response_id = fr.form_response_id
	 order by r.Form_Response_Answer_ID desc),'MISSING') AS emergencycontactfirstname,

  ISNULL((SELECT top 1 response
	   FROM [ministryplatform].[dbo].[form_response_answers] r
	   join (select fra.Form_Response_Answer_ID as max_id 
				from dbo.Form_Response_Answers fra )m
				on m.max_id = r.Form_Response_Answer_ID
   WHERE r.form_field_id = @emergencycontactlastname
     AND r.form_response_id = fr.form_response_id
	 order by r.Form_Response_Answer_ID desc),'MISSING') AS emergencycontactlastname,


	ISNULL((SELECT top 1 response
	   FROM [ministryplatform].[dbo].[form_response_answers] r
	   join (select fra.Form_Response_Answer_ID as max_id 
				from dbo.Form_Response_Answers fra )m
				on m.max_id = r.Form_Response_Answer_ID
	   WHERE r.form_field_id = @emergencycontactprimaryphone
		 AND r.form_response_id = fr.Form_Response_ID
		 order by r.Form_Response_Answer_ID desc),'MISSING') as emergencycontactprimaryphone,

  (SELECT top 1  response
   FROM [ministryplatform].[dbo].[form_response_answers] r
   join (select fra.Form_Response_Answer_ID as max_id 
				from dbo.Form_Response_Answers fra )m
				on m.max_id = r.Form_Response_Answer_ID
   WHERE r.form_field_id = @emergencycontactsecondaryphone
     AND r.form_response_id = fr.form_response_id
	 order by r.Form_Response_Answer_ID desc) AS emergencycontactsecondaryphone,

 (SELECT top 1  response
   FROM [ministryplatform].[dbo].[form_response_answers] r
   join (select fra.Form_Response_Answer_ID as max_id 
				from dbo.Form_Response_Answers fra )m
				on m.max_id = r.Form_Response_Answer_ID
   WHERE r.form_field_id = @emergencycontactemailaddress
     AND r.form_response_id = fr.form_response_id
	 order by r.Form_Response_Answer_ID desc) AS emergencycontactemailaddress,

  (SELECT TOP 1 response
	FROM [ministryplatform].[dbo].[form_response_answers] r
	JOIN (SELECT fra.Form_Response_Answer_ID as max_id 
			FROM dbo.Form_Response_Answers fra )m
			ON m.max_id = r.Form_Response_Answer_ID
	WHERE r.form_field_id = @emergencycontactrelationship
		AND r.form_response_id = fr.Form_Response_ID
		ORDER BY r.Form_Response_Answer_ID DESC) AS emergencycontactrelationship,

  (SELECT top 1  response
   FROM [ministryplatform].[dbo].[form_response_answers] r
   join (select fra.Form_Response_Answer_ID as max_id 
				from dbo.Form_Response_Answers fra )m
				on m.max_id = r.Form_Response_Answer_ID
   WHERE r.form_field_id = @lotterypreference
     AND r.form_response_id = fr.form_response_id
	 order by r.Form_Response_Answer_ID desc) AS lotterypreference,

  (SELECT top 1  response
   FROM [ministryplatform].[dbo].[form_response_answers] r
   join (select fra.Form_Response_Answer_ID as max_id 
				from dbo.Form_Response_Answers fra )m
				on m.max_id = r.Form_Response_Answer_ID
   WHERE r.form_field_id = @commonname
     AND r.form_response_id = fr.form_response_id
	 order by r.Form_Response_Answer_ID desc) AS commonname,

  (SELECT top 1  response
   FROM [ministryplatform].[dbo].[form_response_answers] r
   join (select fra.Form_Response_Answer_ID as max_id 
				from dbo.Form_Response_Answers fra )m
				on m.max_id = r.Form_Response_Answer_ID
   WHERE r.form_field_id = @requestedroommate1
     AND r.form_response_id = fr.form_response_id
	 order by r.Form_Response_Answer_ID desc) AS requestedroommate1,

  (SELECT top 1  response
   FROM [ministryplatform].[dbo].[form_response_answers] r
   join (select fra.Form_Response_Answer_ID as max_id 
				from dbo.Form_Response_Answers fra )m
				on m.max_id = r.Form_Response_Answer_ID
   WHERE r.form_field_id = @requestedroommate2
     AND r.form_response_id = fr.form_response_id
	 order by r.Form_Response_Answer_ID desc) AS requestedroommate2,

  (SELECT top 1  response
   FROM [ministryplatform].[dbo].[form_response_answers] r
   join (select fra.Form_Response_Answer_ID as max_id 
				from dbo.Form_Response_Answers fra )m
				on m.max_id = r.Form_Response_Answer_ID
   WHERE r.form_field_id = @supportpersonemail
     AND r.form_response_id = fr.form_response_id
	 order by r.Form_Response_Answer_ID desc) AS supportpersonemail,

  (SELECT top 1  response
   FROM [ministryplatform].[dbo].[form_response_answers] r
   join (select fra.Form_Response_Answer_ID as max_id 
				from dbo.Form_Response_Answers fra )m
				on m.max_id = r.Form_Response_Answer_ID
   WHERE r.form_field_id = @gogroupleaderinterest
     AND r.form_response_id = fr.form_response_id
	 order by r.Form_Response_Answer_ID desc) AS gogroupleaderinterest,

       sponsoredchild.sponsoredchild,
       sponsoredchild.sponsoredchildfirstname,
       sponsoredchild.sponsoredchildlastname,
       sponsoredchild.sponsoredchildidnumber,

  (SELECT top 1 notes
   FROM vw_crds_contact_attributes
   WHERE contact_id = c.contact_id
     AND attribute_type_id = @previoustripexperience 
	 order by Start_Date desc) AS previous_trip_experience,

  (SELECT STUFF(
                  (SELECT '|' + attribute_name
                   FROM dbo.vw_crds_contact_attributes AS ca
                   WHERE ca.contact_id = c.contact_id
                     AND ca.attribute_type_id = @profession
                     FOR xml path(''), TYPE).value('.', 'nvarchar(max)'), 1, 1, '')) AS profession,

  (SELECT top 1 notes
   FROM vw_crds_contact_attributes
   WHERE contact_id = c.contact_id
     AND attribute_id = @frequentflyerdelta 
	 order by Start_Date desc) AS ff_delta,

  (SELECT top 1 notes
   FROM vw_crds_contact_attributes
   WHERE contact_id = c.contact_id
     AND attribute_id = @frequentflyersouthafrica 
	 order by Start_Date desc) AS ff_south_african_airlines,

  (SELECT top 1 notes
   FROM vw_crds_contact_attributes
   WHERE contact_id = c.contact_id
     AND attribute_id = @frequentflyerunited 
	 order by Start_Date desc) AS ff_united,

  (SELECT top 1 notes
   FROM vw_crds_contact_attributes
   WHERE contact_id = c.contact_id
     AND attribute_id = @frequentflyerusair 
	 order by Start_Date desc) AS ff_usair,

  (SELECT top 1 notes
   FROM vw_crds_contact_attributes
   WHERE contact_id = c.contact_id
     AND attribute_id = @frequentflyeramerican 
	 order by Start_Date desc) AS ff_americanair,

  (SELECT top 1 notes
   FROM vw_crds_contact_attributes
   WHERE contact_id = c.contact_id
     AND attribute_id = @frequentflyersouthwest
	 order by Start_Date desc) as ff_southwest,

  (SELECT top 1 attribute_name
   FROM vw_crds_Contact_Attributes
   WHERE contact_id = c.contact_id
     AND attribute_type_id = @internationaltravelexperience 
	 order by Start_Date desc) AS international_travel_experience,

  (SELECT top 1 notes
   FROM vw_crds_contact_attributes
   WHERE contact_id = c.contact_id
     AND attribute_type_id = @experienceservingabroad 
	 order by Start_Date desc) AS experience_serving_abroad,
       (convert(varchar(10), fr.response_date, 101)) signupdate,
       spouse.spouseontrip,
       spouse.spousename,
       dep.deposit,
       dep.payment_type,
       dep.donation_date,
       dep.pledge_status,
       dep.amount depositamount,

  (SELECT top 1 attribute_name
   FROM vw_crds_Contact_Attributes
   WHERE contact_id = c.contact_id
     AND attribute_type_id = @abusevictim 
	 order by Start_Date desc) AS abuse_victim,

  (SELECT top 1 (CASE d.received WHEN 0 THEN 'No' WHEN 1 THEN 'Yes' END) AS docreceived
   FROM dbo.cr_eventparticipant_documents d
   WHERE d.event_participant_id = ep.event_participant_id
     AND d.document_id = @stewardsofchildren) AS stewardsofchildren,

  (SELECT top 1 (CASE d.received WHEN 0 THEN 'No' WHEN 1 THEN 'Yes' END) AS docreceived
   FROM dbo.cr_eventparticipant_documents d
   WHERE d.event_participant_id = ep.event_participant_id
     AND d.document_id = @visa) AS visa,

  (SELECT top 1 (CASE d.received WHEN 0 THEN 'No' WHEN 1 THEN 'Yes' END) AS docreceived
   FROM dbo.cr_eventparticipant_documents d
   WHERE d.event_participant_id = ep.event_participant_id
     AND d.document_id = @diploma) AS diploma,

  (SELECT top 1 (CASE d.received WHEN 0 THEN 'No' WHEN 1 THEN 'Yes' END) AS docreceived
   FROM dbo.cr_eventparticipant_documents d
   WHERE d.event_participant_id = ep.event_participant_id
     AND d.document_id = @medicallicense) AS medicallicense,

  (SELECT top 1 (CASE d.received WHEN 0 THEN 'No' WHEN 1 THEN 'Yes' END) AS docreceived
   FROM dbo.cr_eventparticipant_documents d
   WHERE d.event_participant_id = ep.event_participant_id
     AND d.document_id = @letterverification) AS letterverification,

  (SELECT top 1 (CASE d.received WHEN 0 THEN 'No' WHEN 1 THEN 'Yes' END) AS docreceived
   FROM dbo.cr_eventparticipant_documents d
   WHERE d.event_participant_id = ep.event_participant_id
     AND d.document_id = @goodfaithletter) AS goodfaithletter,

  (SELECT top 1 (CASE d.received WHEN 0 THEN 'No' WHEN 1 THEN 'Yes' END) AS docreceived
   FROM dbo.cr_eventparticipant_documents d
   WHERE d.event_participant_id = ep.event_participant_id
     AND d.document_id = @goodstandingletter) AS goodstandingletter,

  (SELECT top 1 (CASE d.received WHEN 0 THEN 'No' WHEN 1 THEN 'Yes' END) AS docreceived
   FROM dbo.cr_eventparticipant_documents d
   WHERE d.event_participant_id = ep.event_participant_id
     AND d.document_id = @form10) AS form10,

  (SELECT top 1 (CASE d.received WHEN 0 THEN 'No' WHEN 1 THEN 'Yes' END) AS docreceived
   FROM dbo.cr_eventparticipant_documents d
   WHERE d.event_participant_id = ep.event_participant_id
     AND d.document_id = @medicalapplication) AS medicalapplication,

  (SELECT top 1 (CASE d.received WHEN 0 THEN 'No' WHEN 1 THEN 'Yes' END) AS docreceived
   FROM dbo.cr_eventparticipant_documents d
   WHERE d.event_participant_id = ep.event_participant_id
     AND d.document_id = @orchardafricawaiver) AS orchardafricawaiver,

  (SELECT top 1 (CASE d.received WHEN 0 THEN 'No' WHEN 1 THEN 'Yes' END) AS docreceived
   FROM dbo.cr_eventparticipant_documents d
   WHERE d.event_participant_id = ep.event_participant_id
     AND d.document_id = @copyofpassport) AS copyofpassport,

  (SELECT top 1 (CASE d.received WHEN 0 THEN 'No' WHEN 1 THEN 'Yes' END) AS docreceived
   FROM dbo.cr_eventparticipant_documents d
   WHERE d.event_participant_id = ep.event_participant_id
     AND d.document_id = @ipromise) AS ipromise,

  (SELECT TOP 1 (CASE w.Accepted WHEN 1 THEN 'Yes' ELSE 'No' END) AS docreceived
   FROM dbo.cr_event_participant_waivers w
   WHERE w.Event_Participant_ID = ep.Event_Participant_ID		
   UNION ALL
   SELECT 'No' AS docreceived
   WHERE NOT EXISTS
   (
       SELECT 1
	   FROM dbo.cr_event_participant_waivers w
	   WHERE w.Event_Participant_ID = ep.Event_Participant_ID		   
   )) AS waiver,

  (SELECT top 1 (CASE d.received WHEN 0 THEN 'No' WHEN 1 THEN 'Yes' END) AS docreceived
   FROM dbo.cr_eventparticipant_documents d
   WHERE d.event_participant_id = ep.event_participant_id
     AND d.document_id = @proofofguardianship) AS proofofguardianship,

  (SELECT top 1 (CASE d.received WHEN 0 THEN 'No' WHEN 1 THEN 'Yes' END) AS docreceived
   FROM dbo.cr_eventparticipant_documents d
   WHERE d.event_participant_id = ep.event_participant_id
     AND d.document_id = @countrydocumentation) AS countrydocumentation,

  (SELECT stuff(
                  (SELECT '|' + e.event_title
                   FROM ministryplatform.dbo.event_participants trip
                   INNER JOIN dbo.events e ON trip.event_id = e.event_id
                   AND e.event_type_id = @missiontripeventtype
                   AND e.event_id != @eventid
                   WHERE trip.participant_id = ep.participant_id
                     FOR xml path(''), TYPE).value('.', 'nvarchar(max)'), 1, 1, '')) AS previoustrips,

  (SELECT top 1  response
   FROM [ministryplatform].[dbo].[form_response_answers] r
   join (select fra.Form_Response_Answer_ID as max_id 
				from dbo.Form_Response_Answers fra )m
				on m.max_id = r.Form_Response_Answer_ID
   WHERE r.form_field_id = @workteampreference1
     AND r.form_response_id = fr.form_response_id
	 order by r.Form_Response_Answer_ID desc) AS workteampreference1,

  (SELECT top 1  response
   FROM [ministryplatform].[dbo].[form_response_answers] r
   join (select fra.Form_Response_Answer_ID as max_id
				from dbo.Form_Response_Answers fra )m
				on m.max_id = r.Form_Response_Answer_ID
   WHERE r.form_field_id = @workteampreference2
     AND r.form_response_id = fr.form_response_id
	 order by r.Form_Response_Answer_ID desc) AS workteampreference2,

  (SELECT top 1  response
   FROM [ministryplatform].[dbo].[form_response_answers] r
   join (select fra.Form_Response_Answer_ID as max_id 
				from dbo.Form_Response_Answers fra )m
				on m.max_id = r.Form_Response_Answer_ID
   WHERE r.form_field_id = @workteamexperience
     AND r.form_response_id = fr.form_response_id
	 order by r.Form_Response_Answer_ID desc) AS workteamexperience
     
FROM [ministryplatform].[dbo].[event_participants] ep
INNER JOIN dbo.participants p ON ep.participant_id = p.participant_id
INNER JOIN dbo.pledge_campaigns pc ON ep.event_id = pc.event_id
INNER JOIN dbo.contacts c ON p.contact_id = c.contact_id
INNER JOIN dbo.pledges pg ON pc.pledge_campaign_id = pg.pledge_campaign_id
                         AND c.donor_record = pg.donor_id
                         AND pg.pledge_status_id != @pledgediscontinued
LEFT JOIN households h ON c.household_id = h.household_id
LEFT JOIN addresses a ON h.address_id = a.address_id
LEFT JOIN congregations cn ON h.congregation_id = cn.congregation_id
LEFT JOIN genders g ON c.gender_id = g.gender_id
LEFT JOIN marital_statuses ms ON c.marital_status_id = ms.marital_status_id 
LEFT JOIN dbo.form_responses fr ON p.contact_id = fr.contact_id
                                 AND pc.pledge_campaign_id = fr.pledge_campaign_id
CROSS APPLY dbo.crds_tripdeposit(c.contact_id, ep.event_id, pc.pledge_campaign_id) dep 
CROSS APPLY dbo.crds_spouseontrip(c.contact_id, ep.event_id) spouse 
CROSS APPLY dbo.crds_sponsoredchild(c.contact_id) sponsoredchild
WHERE ep.event_id = @eventid
order by Last_Name;

END;






GO


