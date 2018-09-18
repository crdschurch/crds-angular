USE MinistryPlatform

DECLARE @BODY NVARCHAR(MAX) = '<font face="arial, sans, sans-serif">' +
                                '<span style="font-size: 13px; white-space: pre-wrap;">' +
                                  'Hi [Pin_First_Name],' +
                                  '<br />' +
                                  '<p>' +
                                    '[Community_Member_Name] ([Community_Member_Email]) from [Community_Member_City], [Community_Member_State] says hi! That means they''re interested in connecting.' +
                                    'Be a hero and respond to this email right now. Your new Crossroads best friend is waiting. Don''t know what to say? Don''t sweat it.   ' +
                                    'Reply with an answer to these questions and you''re guaranteed to break the ice.    ' +
                                  '</p>' +
                                  '<ol>' +
                                    '<li>What''s your favorite band? </li>' +
                                    '<li>How did you you get connected to Crossroads? </li>' +
                                    '<li>Would you rather punch a shark or slap a bull?  </li>' +
                                  '</ol>' +
                                  'For bonus points, include your favorite, non-email way to connect with people (text, Facebook Messenger, Facetime, etc.). ''Cause let''s face it, no one likes email.  ' +
                                '</span>' +
                              '</font>' +
                              '<div>' +
                                '<font face="arial, sans, sans-serif">' +
                                  '<span style="font-size: 13px; white-space: pre-wrap;">' +
                                    '<br />' +
                                  '</span>' +
                                '</font>' +
                              '</div>' +
                              '<div>' +
                                '<font face="arial, sans, sans-serif">' +
                                  '<span style="font-size: 13px; white-space: pre-wrap;">' +
                                    'Here''s what ' +
                                  '</span>' +
                                '</font>' +
                                '<span style="font-family: arial, sans, sans-serif; font-size: 13px; white-space: pre-wrap;">' +
                                  '[Community_Member_Name] has to say: [User_Message]' +
                                '</span>' +
                                '</div>' +
                                '<div>' +
                                '<font face="arial, sans, sans-serif">' +
                                  '<span style="font-size: 13px; white-space: pre-wrap;">' +
                                    '<br />' +
                                  '</span>' +
                                '</font>' +
                              '</div>' +
                              '<div>' +
                                '<font face="arial, sans, sans-serif">' +
                                  '<span style="font-size: 13px; white-space: pre-wrap;">' +
                                    'Get to know your new Crossroads pal!' +
                                    '<br />' +
                                    '<p>' +
                                    '</p>' +
                                  '</span>' +
                                '</font>' +
                              '</div>';

IF EXISTS (SELECT *
FROM dp_Communications
WHERE communication_id = 2013) 
BEGIN
  UPDATE dp_Communications SET body = @BODY WHERE communication_id = 2013
END

GO
