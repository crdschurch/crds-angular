USE MinistryPlatform
GO

-- =========================================
-- Author:		Jonathan Horner
-- Create Date:	7/19/2017
-- Description: Create the GO Trip Waiver
-- =========================================

IF NOT EXISTS(SELECT * FROM cr_waivers WHERE Waiver_Name = 'GO Trip Waiver')
BEGIN
	DECLARE @domainId INT = 1;

	INSERT INTO cr_Waivers
	(
		Waiver_Name
		, Waiver_Text
		, Domain_ID
	)
	VALUES
	(
		'GO Trip Waiver'
		, '<h2 style="text-align: center;">Waiver and Release Agreement</h2>
			<p style="text-align: center;">
			  For Crossroads Activities and Trips
			</p>

			<p>
			  The undersigned (&ldquo;Participant&rdquo;) hereby acknowledges the considerable risks associated with participation in volunteer activities of Crossroads Community Church, Inc. (&ldquo;Crossroads&rdquo;), especially as to participation in Crossroads trips of any kind.  These risks include, but are not limited to, risk of death, injury, disease, war, terrorist acts, extreme weather conditions, inadequate medical services and supplies, criminal activity, random acts of violence, unexpected scheduling changes and such things as may be the consequence of the negligence of others.  The undersigned hereby acknowledges that, with full awareness and acceptance of these risks, his/her participation in any and all Crossroads sponsored activities, including trips, is entirely voluntary.
			</p>

			<p>
			  Participant acknowledges that Crossroads has made no representations or assurances as to safety or health conditions, personal safety or property safety for participants in activities and trips.  Participant acknowledges having been given full opportunity to make his/her own assessment of whether or not to chose to participate voluntarily in Crossroads activities and trips.
			</p>

			<p>
			  Participant hereby waives any and all rights to claims that might, otherwise, arise out of Participant&rsquo;s participation in any and all trips and activities organized by Crossroads. Participant hereby releases Crossroads, it&rsquo;s staff, board, agents, representatives and other volunteers from any and all claims of liability for anything other than the consequences of gross negligence.
			</p>

			<p>
			  Participant is aware of no medical conditions of any type or nature that would prevent him/her from full participation in the Crossroads activities or which would increase Participant&rsquo;s risks of injury, loss or other adverse consequence.  Participant acknowledges having had every opportunity, at his/her sole cost, to be examined by a doctor of Participant&rsquo;s choosing, to assess and verify Participant&rsquo;s health conditions.  Participant further acknowledges having had the opportunity to have an attorney, of Participant&rsquo;s choosing and at Participant&rsquo;s expense, review this Waiver and Release Agreement and, by signing this Agreement, has affirms he/she has either done so to his/her satisfaction or voluntarily has elected not to do so.
			</p>

			<p>
			  Participant waives any defense to the enforcement of any provision of this Agreement that might arise from claim of lack of consideration and hereby affirms that this constitutes a legal, valid and binding waiver enforceable against Participant, Participant&rsquo;s heirs and estate.
			</p>

			<p>
			  Participant further acknowledges that he/she is not covered by any policy or policies of insurance obtained by Crossroads, that would provide coverage of any kind, except as may have been expressly provided to Participant in writing.
			</p>

			<p>
			  Furthermore, Participant affirms that there have been no oral commitments or statements on which Participant has relied, in determining to volunteer for this activity or trip, which would in any way undermine, compromise or contradict the terms of this Agreement.  Participant agrees that the laws of the State of Ohio will be determinative for this Agreement.
			</p>

			<p>
			  Finally, Participant states that he/she has read carefully the terms and conditions of this Agreement, understands these completely and, by voluntarily choosing to sign, whether electronically or otherwise, hereby agrees to be bound by its terms.
			</p>'
		, @domainId
	)
END