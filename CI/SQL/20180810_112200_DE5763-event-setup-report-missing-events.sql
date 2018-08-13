USE [MinistryPlatform]
GO

/* Modifications: 
	KD 5/25: Added expected participants
*/

-- =============================================

CREATE OR ALTER PROCEDURE [dbo].[report_CRDS_Event_Setup_List]
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



    CREATE INDEX ix_e_eventid ON #e(event_id)

    DECLARE @details TABLE (
        Event_ID INT NOT NULL,
        Event_Room_ID INT,
        Event_Equipment_ID INT,
        Location_ID INT
    );

    INSERT INTO @details
        (Event_ID, Event_Room_ID, Event_Equipment_ID, Location_ID)
    SELECT
        e.Event_ID,
        er.Event_Room_ID,
        ee.Event_Equipment_ID,
        l.Location_ID
    FROM
        Events e
        LEFT JOIN Event_Rooms er ON er.Event_ID = e.Event_ID AND COALESCE(er.Cancelled, 0) = 0
        LEFT JOIN Rooms r ON r.Room_ID = er.Room_ID
        LEFT JOIN Event_Equipment ee ON ee.Event_ID = e.Event_ID AND ee.Room_ID = er.Room_ID AND ee.Cancelled = 0
        LEFT JOIN Congregations cg ON cg.Congregation_ID = e.Congregation_ID
        LEFT JOIN Buildings b ON b.Building_ID = r.Building_ID
        LEFT JOIN Locations l ON l.Location_ID = COALESCE(b.Location_ID, e.Location_ID, cg.Location_ID)
    WHERE
        e.Event_ID IN (SELECT Event_ID FROM #e)
        AND e.Cancelled <> 1
        AND (ISNULL(@LocationID, 0) = 0 OR @LocationID = l.Location_ID)
        AND (ISNULL(@RoomID, 0) = 0 OR (r.Room_ID IS NULL OR @RoomID = r.Room_ID))
        AND (ISNULL(@EquipID, 0) = 0 OR (ee.Equipment_ID IS NULL OR @EquipID = ee.Equipment_ID))
        AND (@RoomStatus IS NULL OR 1 = (CASE
            WHEN @RoomStatus = '*All' THEN 1
            WHEN @RoomStatus = 'Approved' AND er._approved = 1 THEN 1
            WHEN @RoomStatus = 'Rejected' AND er._approved = 0 THEN 1
            WHEN @RoomStatus = 'Pending' AND er._approved IS NULL AND er.Event_Room_ID IS NOT NULL THEN 1
            ELSE 0
        END)) 
        AND (@EquipStatus IS NULL OR 1 = (CASE
            WHEN @EquipStatus = '*All' THEN 1
            WHEN @EquipStatus = 'Approved' AND ee._approved = 1 THEN 1
            WHEN @EquipStatus = 'Rejected' AND ee._approved = 0 THEN 1
            WHEN @EquipStatus = 'Pending' AND ee._approved IS NULL AND ee.Event_Equipment_ID IS NOT NULL THEN 1
            ELSE 0
        END))
    UNION
    -- Find active equipment linked to the event, but not associated with a room (or associated
    -- with a cancelled room).  These are errors (i.e., active equipment should always be
    -- associated with an active room linked to the event) 
    SELECT
        e.Event_ID,
        er.Event_Room_ID,
        ee.Event_Equipment_ID,
        l.Location_ID
    FROM
        Events e
        INNER JOIN Event_Equipment ee ON ee.Event_ID = e.Event_ID AND ee.Cancelled = 0
        LEFT JOIN Event_Rooms er ON er.Event_ID = e.Event_ID AND er.Room_ID = ee.Room_ID AND COALESCE(er.Cancelled, 0) = 0
        LEFT JOIN Congregations cg ON cg.Congregation_ID = e.Congregation_ID
        LEFT JOIN Locations l ON l.Location_ID = COALESCE(e.Location_ID, cg.Location_ID)
    WHERE
        e.Event_ID IN (SELECT Event_ID FROM #e)
        AND e.Cancelled <> 1
        AND er.Event_Room_ID IS NULL
        AND (ISNULL(@LocationID, 0) = 0 OR @LocationID = l.Location_ID)
        AND (ISNULL(@EquipID, 0) = 0 OR (ee.Equipment_ID IS NULL OR @EquipID = ee.Equipment_ID))
        AND (@EquipStatus IS NULL OR 1 = (CASE
            WHEN @EquipStatus = '*All' THEN 1
            WHEN @EquipStatus = 'Approved' AND ee._approved = 1 THEN 1
            WHEN @EquipStatus = 'Rejected' AND ee._approved = 0 THEN 1
            WHEN @EquipStatus = 'Pending' AND ee._approved IS NULL AND ee.Event_Equipment_ID IS NOT NULL THEN 1
            ELSE 0
        END))
    ;

    SELECT
        @LocationID as location_id
        ,l.location_name
        ,CONVERT(DATE, e.event_start_date) AS event_start_date
        ,SUBSTRING(CONVERT(VARCHAR, e.event_start_date, 120), 12,5) AS event_start
        ,SUBSTRING(CONVERT(VARCHAR, e.event_end_date,120), 12,5) AS event_end
        ,e.event_id
        ,e.event_title
        ,e.event_type_id
        ,et.event_type
        ,e.participants_expected AS event_expected_count
        ,event_status =
            CASE
                WHEN e._approved = 1 THEN 'Approved'
                WHEN e._approved = 0 THEN ' Rejected'
                ELSE 'Pending'
            END
        ,Cast(e.meeting_instructions AS VARCHAR(2000)) AS event_notes
        ,LEFT(CONVERT(VARCHAR, Dateadd(n,-(1 * e.minutes_for_setup),e.event_start_date), 120),16) AS event_setup_start
        ,LEFT(CONVERT(VARCHAR, Dateadd(n, e.minutes_for_cleanup,e.event_end_date),120),16) AS event_teardown_end
        ,c.display_name as event_contact_name
        ,room_label = CASE WHEN r.Room_ID IS NOT NULL THEN 'Room' ELSE NULL END
        ,r.room_id
        ,r.room_name
        ,rl.layout_name AS room_layout
        ,room_status = 
            CASE
                WHEN er._approved = 1 THEN 'Approved'
                WHEN er._approved = 0 THEN ' Rejected'
                WHEN er._approved IS NULL AND er.Event_Room_ID IS NOT NULL THEN 'Pending'
                ELSE NULL
            END
        ,er.notes AS room_notes
        ,room_reservation_start =
            CASE
                WHEN er.Event_Room_ID IS NULL THEN NULL
                ELSE LEFT(CONVERT(VARCHAR, Dateadd(n,-(1 * ISNULL(r.setup_time,e.minutes_for_setup)),e.event_start_date), 120),16)
            END
        ,room_reservation_end =
            CASE
                WHEN er.Event_Room_ID IS NULL THEN NULL
                ELSE LEFT(CONVERT(VARCHAR, Dateadd(n, ISNULL(r.teardown_time,e.minutes_for_cleanup),e.event_end_date),120),16)
            END
        ,equip_label = CASE WHEN eq.Equipment_ID IS NOT NULL THEN 'Equipment' ELSE NULL END
        ,eq.equipment_id AS equip_id
        ,eq.equipment_name AS equip_name
        ,ee.quantity_requested AS equip_count
        ,ee.desired_placement_or_location AS equip_placement
        ,equip_status =
            CASE
                WHEN ee.Event_Equipment_ID IS NULL THEN NULL
                WHEN ee._approved = 1 THEN 'Approved'
                WHEN ee._approved = 0 THEN ' Rejected'
                ELSE 'Pending'
            END
        ,ee.notes AS equip_notes
        ,equip_reservation_start =
            CASE
                WHEN ee.Event_Equipment_ID IS NULL THEN NULL
                ELSE LEFT(CONVERT(VARCHAR, Dateadd(n,-(1 * e.minutes_for_setup),e.event_start_date), 120),16)
            END
        ,equip_reservation_end =
            CASE
                WHEN ee.Event_Equipment_ID IS NULL THEN NULL
                ELSE LEFT(CONVERT(VARCHAR, Dateadd(n, e.minutes_for_cleanup,e.event_end_date),120),16)
            END
        ,e.Participants_Expected
    FROM
        @details d
        INNER JOIN Events e ON e.Event_ID = d.Event_ID
        INNER JOIN Event_Types et ON et.Event_Type_ID = e.Event_Type_ID
        LEFT JOIN Contacts c ON c.Contact_ID = e.Primary_Contact
        LEFT JOIN Event_Rooms er ON er.Event_Room_ID = d.Event_Room_ID
        LEFT JOIN Rooms r ON r.Room_ID = er.Room_ID
        LEFT JOIN Room_Layouts rl ON rl.Room_Layout_ID = er.Room_Layout_ID
        LEFT JOIN Event_Equipment ee ON ee.Event_Equipment_ID = d.Event_Equipment_ID
        LEFT JOIN Equipment eq ON eq.Equipment_ID = ee.Equipment_ID
        LEFT JOIN Locations l ON l.Location_ID = d.Location_ID
    ;
END
GO
