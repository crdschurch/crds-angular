USE [MinistryPlatform]
GO

/****** Object:  StoredProcedure [dbo].[report_CRDS_Event_Setup_List]    Script Date: 5/25/2017 12:47:51 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



/* Modifications: 
	KD 5/25: Added expected participants
*/

-- =============================================

ALTER PROCEDURE [dbo].[report_CRDS_Event_Setup_List]
-- Add the parameters for the stored procedure here
      @DomainID             VARCHAR(40) = '0FDE7F32-37E3-4E0B-B020-622E0EBD6BF0'  -- = Domain 1
,@UserID              VARCHAR(40)
,@PageID              INT
,@BeginDate           DATETIME 
,@EndDate             DATETIME 
,@EventTitle          VARCHAR(75)  
,@EventTypeID         INT = NULL
,@EventStatus         VARCHAR(40) = 'Approved'
,@LocationID          INT = NULL
,@BuildingID          INT = NULL
--,@ReservationType     VARCHAR(12)  --"Room" or "Equipment"
,@RoomID              INT = NULL
,@EquipID             INT = NULL
,@RoomStatus          VARCHAR(40) = 'Approved'
,@EquipStatus         VARCHAR(40) = 'Approved'
,@CongregationID      INT = Null
AS
     BEGIN
         SET nocount ON
      SET fmtonly OFF

      DECLARE @ContactID INT
	   
      SELECT @ContactID = contact_id
        FROM dp_users
       WHERE user_guid = @UserID

	   SELECT @LocationID = location_id
	   FROM Congregations 
	   WHERE congregation_id = @CongregationID

	   -- try this to make sure building field is populated for something besides Oakley
	   SELECT @BuildingID = building_id
	   FROM Buildings
	   WHERE location_id = @BuildingID

      SELECT E.event_id
        INTO #e
        FROM events E	
       WHERE  (@BeginDate IS NULL OR E.event_start_date >= @BeginDate)
         AND (@EndDate IS NULL OR E.event_start_date < @EndDate + 1)
         AND (@CongregationID IS NULL OR EXISTS (SELECT 1
                                                   FROM congregations c
                                                  WHERE c.congregation_id = e.congregation_id
                                                    AND c.congregation_id = @CongregationID))
         AND (@EventTitle IS NULL OR e.event_title LIKE '%' + @EventTitle + '%')
         AND (@EventTypeID IS NULL OR EXISTS (SELECT 1
		                                        FROM event_types et
											   WHERE et.event_type_id = e.event_type_id
											     AND et.event_type_id = @EventTypeID))
		 AND (@EventStatus IS NULL OR 'Y' = (CASE WHEN @EventStatus = '*All' THEN 'Y'
		                                        WHEN @EventStatus = 'Approved' AND e._approved = 1 THEN 'Y'
										        WHEN @EventStatus = 'Rejected' AND e._approved = 0 THEN 'Y'
									            WHEN @EventStatus = 'Pending' AND e._approved IS NULL THEN 'Y'
										        ELSE 'N'
										   END)) 
         /*AND (ISNULL(@LocationID,0) = 0 OR EXISTS (SELECT 1
		                                             FROM event_rooms er
													 JOIN rooms r ON r.room_id = er.room_id
													 JOIN buildings b ON b.building_id = r.building_id
													 JOIN locations l ON l.location_id = b.location_id
													WHERE er.event_id = e.event_id
													  AND l.location_id = @LocationID)
									    OR EXISTS (SELECT 1
										             FROM event_equipment ee
													 JOIN equipment eq ON eq.equipment_id = ee.equipment_id
													 JOIN rooms r ON r.room_id = ee.room_id
													 JOIN buildings b ON b.building_id = r.building_id
													 JOIN locations l ON l.location_id = b.location_id
													WHERE ee.event_id = e.event_id
													  AND l.location_id = @LocationID)
										OR (NOT EXISTS (SELECT 1
		                                                 FROM event_rooms er
			                                    	    WHERE er.event_id = e.event_id) AND 
			                                NOT EXISTS (SELECT 1
		                                                 FROM event_equipment ee
						                                WHERE ee.event_id = e.event_id)))*/
         AND (ISNULL(@BuildingID,0) = 0 OR EXISTS (SELECT 1
                                                     FROM event_rooms ER
                                               INNER JOIN rooms R ON R.room_id = ER.room_id
                                                    WHERE ER.event_id = E.event_id
                                                      AND R.building_id = @BuildingID))
/*		 AND (@ReservationType IS NULL OR EXISTS (SELECT 1
		                                            FROM event_rooms er
												   WHERE er.event_id = e.event_id
												     AND (@ReservationType = 'All' OR @ReservationType = 'Room'))
								       OR EXISTS (SELECT 1
									                FROM event_equipment ee
												   WHERE ee.event_id = e.event_id
												     AND (@ReservationType = 'All' OR @ReservationType = 'Equipment')))*/
         AND (ISNULL(@RoomID,0) = 0 OR EXISTS (SELECT 1
                                                 FROM event_rooms er
                                                WHERE er.event_id = e.event_id
                                                  AND er.room_id = @RoomID))
         AND (ISNULL(@EquipID,0) = 0 OR EXISTS (SELECT 1
		                                          FROM event_equipment ee
								                 WHERE ee.event_id = e.event_id
									               AND ee.equipment_id = @EquipID))



      CREATE INDEX ix_e_eventid
        ON #e(event_id)

/*      INSERT INTO ministryplatform.dbo.journeys (journey_name,description,domain_id)
	  VALUES ((SELECT COUNT(*) FROM #e),'event record count',1)
*/

      SELECT /*c.congregation_id
	         ,c.congregation_name
			 ,*/@LocationID as location_id
			 ,l.location_name
			 ,CONVERT(DATE, e.event_start_date) AS event_start_date
             ,SUBSTRING(CONVERT(VARCHAR, e.event_start_date, 120), 12,5) AS event_start
             ,SUBSTRING(CONVERT(VARCHAR, e.event_end_date,120), 12,5) AS event_end
			 ,e.event_id
             ,e.event_title
			 ,e.event_type_id
             ,et.event_type
             ,e.participants_expected AS event_expected_count
			 ,(CASE WHEN e._approved = 1 THEN 'Approved'
			        WHEN e._approved = 0 THEN ' Rejected'
					ELSE 'Pending' END) AS event_status
             ,Cast(e.meeting_instructions AS VARCHAR(2000)) AS event_notes
             ,LEFT(CONVERT(VARCHAR, Dateadd(n,-(1 * e.minutes_for_setup),e.event_start_date), 120),16) AS event_setup_start
             ,LEFT(CONVERT(VARCHAR, Dateadd(n, e.minutes_for_cleanup,e.event_end_date),120),16) AS event_teardown_end
			 ,cn.display_name as event_contact_name
             ,'Room' AS room_label
	    	 ,r.room_id
			 ,r.room_name
			 ,rl.layout_name AS room_layout
			 ,(CASE WHEN er._approved = 1 THEN 'Approved'
			        WHEN er._approved = 0 THEN ' Rejected'
					ELSE 'Pending' END) AS room_status
			 ,er.notes AS room_notes
             ,LEFT(CONVERT(VARCHAR, Dateadd(n,-(1 * ISNULL(r.setup_time,e.minutes_for_setup)),e.event_start_date), 120),16) AS room_reservation_start
             ,LEFT(CONVERT(VARCHAR, Dateadd(n, ISNULL(r.teardown_time,e.minutes_for_cleanup),e.event_end_date),120),16) AS room_reservation_end 
			 ,NULL AS equip_label
			 ,NULL AS equip_id
			 ,NULL AS equip_name
			 ,NULL AS equip_count
			 ,NULL AS equip_placement
			 ,NULL AS equip_status
			 ,NULL AS equip_notes
			 ,NULL AS equip_reservation_start
			 ,NULL AS equip_reservation_end
			 ,e.Participants_Expected
        FROM events e
		JOIN #e on #e.event_id = e.event_id
--  LEFT OUTER JOIN congregations c ON c.congregation_id = e.congregation_id
        JOIN event_types et ON et.event_type_id = e.event_type_id
  LEFT OUTER JOIN contacts cn ON cn.contact_id = e.primary_contact
        JOIN event_rooms er ON er.event_id = e.event_id
        JOIN rooms r ON r.room_id = er.room_id
  LEFT OUTER JOIN room_layouts rl ON er.room_layout_id = rl.room_layout_id
        JOIN buildings b ON b.building_id = r.building_id
		JOIN locations l ON l.location_id = b.location_id 
       WHERE e.cancelled != 1
	     AND er.event_room_id IS NOT NULL
         AND er.cancelled != 1 
         AND NOT EXISTS (select 1 from event_equipment ee where ee.event_id = er.event_id and ee.room_id = er.room_id)
		 AND (ISNULL(@LocationID,0) = 0 OR @locationID = l.location_id)
/*		 AND (@ReservationType = 'All' OR @ReservationType = 'Room')*/
		 AND (@RoomStatus IS NULL OR 1 = (CASE WHEN @RoomStatus = '*All' THEN 1
		                                       WHEN @RoomStatus = 'Approved' AND er._approved = 1 THEN 1
											   WHEN @RoomStatus = 'Rejected' AND er._approved = 0 THEN 1
										       WHEN @RoomStatus = 'Pending' AND er._approved IS NULL THEN 1
										       ELSE 0
										  END)) 
		 AND ISNULL(@EquipStatus,'*All') = '*All'
       UNION
      SELECT /*c.congregation_id
	         ,c.congregation_name
			 ,*/@LocationID as location_id
			 ,l.location_name
			 ,CONVERT(DATE, e.event_start_date) AS event_start_date
             ,SUBSTRING(CONVERT(VARCHAR, e.event_start_date, 120), 12,5) AS event_start
             ,SUBSTRING(CONVERT(VARCHAR, e.event_end_date,120), 12,5) AS event_end
			 ,e.event_id
             ,e.event_title
			 ,e.event_type_id
             ,et.event_type
             ,e.participants_expected AS event_expected_count
			 ,(CASE WHEN e._approved = 1 THEN 'Approved'
			        WHEN e._approved = 0 THEN ' Rejected'
					ELSE 'Pending' END) AS event_status
             ,Cast(e.meeting_instructions AS VARCHAR(2000)) AS event_notes
             ,LEFT(CONVERT(VARCHAR, Dateadd(n,-(1 * e.minutes_for_setup),e.event_start_date), 120),16) AS event_setup_start
             ,LEFT(CONVERT(VARCHAR, Dateadd(n, e.minutes_for_cleanup,e.event_end_date),120),16) AS event_teardown_end
			 ,cn.display_name as event_contact_name
             ,'Room' AS room_label
	    	 ,r.room_id
			 ,r.room_name
			 ,rl.layout_name AS room_layout
			 ,(CASE WHEN er.event_room_id IS NULL THEN 'Unreserved'
			        WHEN er._approved = 1 THEN 'Approved'
			        WHEN er._approved = 0 THEN ' Rejected'
					ELSE 'Pending' END) AS room_status
			 ,er.notes AS room_notes
             ,LEFT(CONVERT(VARCHAR, Dateadd(n,-(1 * ISNULL(r.setup_time,e.minutes_for_setup)),e.event_start_date), 120),16) AS room_reservation_start
             ,LEFT(CONVERT(VARCHAR, Dateadd(n, ISNULL(r.teardown_time,e.minutes_for_cleanup),e.event_end_date),120),16) AS room_reservation_end 
			 ,'Equipment' AS equip_label
			 ,eq.equipment_id AS equip_id
			 ,eq.equipment_name AS equip_name
			 ,ee.quantity_requested AS equip_count
			 ,ee.desired_placement_or_location AS equip_placement
			 ,(CASE WHEN ee._approved = 1 THEN 'Approved'
			        WHEN ee._approved = 0 THEN ' Rejected'
					ELSE 'Pending' END) AS equip_status
			 ,ee.notes AS equip_notes
             ,LEFT(CONVERT(VARCHAR, Dateadd(n,-(1 * e.minutes_for_setup),e.event_start_date), 120),16) AS equip_reservation_start
             ,LEFT(CONVERT(VARCHAR, Dateadd(n, e.minutes_for_cleanup,e.event_end_date),120),16) AS equip_reservation_end 
			 ,e.Participants_Expected
        FROM events e
		JOIN #e on #e.event_id = e.event_id
--  LEFT OUTER JOIN congregations c ON c.congregation_id = e.congregation_id
        JOIN event_types et ON et.event_type_id = e.event_type_id
  LEFT OUTER JOIN contacts cn ON cn.contact_id = e.primary_contact
  LEFT OUTER JOIN event_equipment ee ON ee.event_id = e.event_id
  LEFT OUTER JOIN equipment eq ON eq.equipment_id = ee.equipment_id 
  LEFT OUTER JOIN rooms r ON r.room_id = ee.room_id
  LEFT OUTER JOIN event_rooms er ON er.event_id = ee.event_id AND er.room_id = ee.room_id
  LEFT OUTER JOIN room_layouts rl ON rl.room_layout_id = er.room_layout_id
        JOIN buildings b ON b.building_id = r.building_id
		JOIN locations l ON l.location_id = b.location_id 
       WHERE e.cancelled != 1
         AND ee.cancelled != 1 
	     AND ee.event_equipment_id IS NOT NULL
		 AND (ISNULL(@LocationID,0) = 0 OR @locationID = l.location_id)
		 AND (ISNULL(@EquipID,0) = 0 OR @EquipID = eq.equipment_id)
/*		 AND (@ReservationType = 'All' OR @ReservationType = 'Equipment')*/
		 AND (@RoomStatus IS NULL OR 1 = (CASE WHEN @RoomStatus = '*All' THEN 1
		                                       WHEN @RoomStatus = 'Approved' AND er._approved = 1 THEN 1
											   WHEN @RoomStatus = 'Rejected' AND er._approved = 0 THEN 1
										       WHEN @RoomStatus = 'Pending' AND er._approved IS NULL THEN 1
										       ELSE 0
										  END)) 
		 AND (@EquipStatus IS NULL OR 1 = (CASE WHEN @EquipStatus = '*All' THEN 1
		                                        WHEN @EquipStatus = 'Approved' AND ee._approved = 1 THEN 1
											    WHEN @EquipStatus = 'Rejected' AND ee._approved = 0 THEN 1
										        WHEN @EquipStatus = 'Pending' AND ee._approved IS NULL THEN 1
										        ELSE 0
											END)) 
       UNION
	  SELECT @LocationID as location_id
			 ,(CASE WHEN e.location_id IS NOT NULL THEN el.location_name
			        WHEN c.location_id IS NOT NULL THEN cl.location_name
					ELSE NULL 
			   END) AS location_name
			 ,CONVERT(DATE, e.event_start_date) AS event_start_date
             ,SUBSTRING(CONVERT(VARCHAR, e.event_start_date, 120), 12,5) AS event_start
             ,SUBSTRING(CONVERT(VARCHAR, e.event_end_date,120), 12,5) AS event_end
			 ,e.event_id
             ,e.event_title
			 ,e.event_type_id
             ,et.event_type
             ,e.participants_expected AS event_expected_count
			 ,(CASE WHEN e._approved = 1 THEN 'Approved'
			        WHEN e._approved = 0 THEN ' Rejected'
					ELSE 'Pending' END) AS event_status
             ,Cast(e.meeting_instructions AS VARCHAR(2000)) AS event_notes
             ,LEFT(CONVERT(VARCHAR, Dateadd(n,-(1 * e.minutes_for_setup),e.event_start_date), 120),16) AS event_setup_start
             ,LEFT(CONVERT(VARCHAR, Dateadd(n, e.minutes_for_cleanup,e.event_end_date),120),16) AS event_teardown_end
			 ,cn.display_name as event_contact_name
             ,NULL AS room_label
	    	 ,NULL AS room_id
			 ,NULL AS room_name
			 ,NULL AS  room_layout
			 ,NULL AS  room_status
			 ,NULL AS  room_notes
             ,NULL AS  room_reservation_start
             ,NULL AS  room_reservation_end 
			 ,NULL AS  equip_label
			 ,NULL AS  equip_id
			 ,NULL AS  equip_name
			 ,NULL AS equip_count
			 ,NULL AS equip_placement
			 ,NULL AS  equip_status
			 ,NULL AS  equip_notes
             ,NULL AS equip_reservation_start
             ,NULL AS equip_reservation_end
			 ,e.Participants_Expected
        FROM events e
		JOIN #e on #e.event_id = e.event_id      
        JOIN event_types et ON et.event_type_id = e.event_type_id
  LEFT OUTER JOIN contacts cn ON cn.contact_id = e.primary_contact
  LEFT OUTER JOIN locations el ON el.location_id = e.location_id
        JOIN congregations c ON c.congregation_id = e.congregation_id
  LEFT OUTER JOIN locations cl ON cl.location_id = c.location_id
       WHERE e.cancelled != 1
		 AND (ISNULL(@LocationID,0) = 0 OR @LocationID = el.location_id OR @LocationID = cl.location_id)
		 AND (NOT EXISTS (SELECT 1
		                    FROM event_rooms er
						   WHERE er.event_id = e.event_id) AND 
			  NOT EXISTS (SELECT 1
		                    FROM event_equipment ee
						   WHERE ee.event_id = e.event_id))
		 AND ISNULL(@RoomStatus,'*All') = '*All'
		 AND ISNULL(@EquipStatus,'*All') = '*All'
     END;




GO


