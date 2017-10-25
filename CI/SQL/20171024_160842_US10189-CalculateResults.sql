USE [MinistryPlatform]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[api_crds_Calculate_Current_SRFP_Results]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[api_crds_Calculate_Current_SRFP_Results] AS'
END
GO
-- =============================================
-- Author:      Phil Lachmann and Joe Kerstanoff
-- Create date: 2017-10-23
-- Description:    Gets the most recent SRFP result
-- =============================================
ALTER PROCEDURE [dbo].api_crds_Calculate_Current_SRFP_Results
    @ContactID INT,
	@FormID INT
AS
BEGIN

    DECLARE @CategoryWeights TABLE (
        categoryID varchar(10),
		CategoryName varchar(20),
		CategoryMultiplier numeric(18, 8)
    );

	DECLARE @srfpAnswers TABLE(
		answer_value  INT,
		question_weight INT,
		category varchar(20)
		);

	insert into @CategoryWeights values ('F', 'Financial', 1.705);
	insert into @CategoryWeights values ('I', 'Intellectual', 1);
	insert into @CategoryWeights values ('P', 'Physical', 1);
	insert into @CategoryWeights values ('R', 'Relational', 1);
	insert into @CategoryWeights values ('S', 'Spiritual', 0.667);

    insert into @srfpAnswers
	select
crs.Submission_Data as answer_value,
wmd.Metadata_Value as question_weight,
cmd.Metadata_Value as category
from form_responses fr
join cr_Form_Response_Submissions frs on fr.Form_Response_ID = frs.Form_Response_ID
join cr_submissions crs on crs.Submission_ID = frs.Data_record_id
join cr_form_metadata wmd on wmd.Form_Field_Name = frs.Form_Field_Name
and wmd.Metadata_Label = 'weight'
and fr.Form_ID = wmd.form_id
and wmd.start_date <= fr.Response_Date
and (wmd.end_date is null or wmd.end_date > fr.Response_Date)
join cr_form_metadata cmd on cmd.Form_Field_Name = frs.Form_Field_Name
and cmd.Metadata_Label = 'category'
and fr.Form_id = cmd.form_id
and cmd.start_date <= fr.Response_Date
and (cmd.end_date is null or cmd.end_date > fr.Response_Date)
where fr.Form_Response_ID = (select top 1 form_response_id from FORM_RESPONSES where Contact_ID = @ContactID and form_id = @formID order by Response_Date);

select cw.CategoryName, Floor(sum(CAST(answer_value as numeric(18, 8)) * CAST(question_weight as numeric(18,8)) * cw.CategoryMultiplier)) as Score from @srfpAnswers sa join @CategoryWeights cw on cw.categoryID = sa.category  group by cw.CategoryName;
END