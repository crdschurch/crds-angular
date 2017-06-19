update dp_communications
set Body = 'It appears there was an issue processing your recurring gift. Please login to your account on crossroads.net and <a href="https://www.crossroads.net/profile/giving" target="_blank">update your recurring gift</a> account information. If your recurring gift fails more than twice we will automatically remove your recurring gift from our system and stop attempting to charge the account.<div><br /></div><div><b>You're Giving To:</b> [Program_Name]</div><div><b>Amount:</b> $[Donation_Amount]</div><div><b>Frequency:</b> [Frequency]</div><div><b>Date Attempted:</b> [Donation_Date]</div><div><b>Payment Method:</b> [Payment_Method]</div><div><b>Decline Reason:</b> [Decline_Reason]<br /></div><div><br /></div><div>Some potential causes include:</div><div><ul><li><b>Using a savings account.</b> Some banks do not allow online ACH transactions with savings accounts. Our system does not officially support the use of savings accounts, please try again with a checking account.<br /></li><li><b>Expired Credit Card.</b> If you've had your recurring gift setup for a while, your credit card may be expired. Simply login to your account and update the payment information.<br /></li><li><b>Non sufficient funds.</b> You may not have the amount of funds in your account for the gift you're trying to give. Timing could be the trick here. Check your account balance and try again. Note, transactions take 5-10 business days to completely transfer from the day you initiate the gift.<br /></li><li><b>Entered incorrect payment information.</b> You may have entered your information incorrectly when completing the form. Double check your Routing/Account Number(s), Account Holder Name and Account Holder Type. Don't worry it happens to all of us (#fatfinger). Simply retry your gift on crossroads.net.</li></ul></div><div>Please contact the Finance Team at <a href="mailto:finance@crossroads.net" target="_blank">giving@crossroads.net</a> if you have further questions.</div><div><br /></div><div>Thanks again for being a generous part of the team!</div><div><br /></div><div>Crossroads</div>'
where communication_id =  13002


