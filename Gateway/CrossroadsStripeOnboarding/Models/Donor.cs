namespace CrossroadsStripeOnboarding.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Donor
    {
        public Donor()
        {
            DonorAccounts = new HashSet<DonorAccount>();
            RecurringGifts = new HashSet<RecurringGift>();
        }

        [Key]
        public int Donor_ID { get; set; }

        public int Contact_ID { get; set; }

        public int Statement_Frequency_ID { get; set; }

        public int Statement_Type_ID { get; set; }

        public int Statement_Method_ID { get; set; }

        public DateTime Setup_Date { get; set; }

        public int? Envelope_No { get; set; }

        public bool Cancel_Envelopes { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }

        public bool? First_Contact_Made { get; set; }

        public int Domain_ID { get; set; }

        [Column("__ExternalPersonID")]
        public int? C__ExternalPersonID { get; set; }

        [Column("_First_Donation_Date")]
        public DateTime? C_First_Donation_Date { get; set; }

        [Column("_Last_Donation_Date")]
        public DateTime? C_Last_Donation_Date { get; set; }

        [StringLength(255)]
        public string Processor_ID { get; set; }

        public virtual ICollection<DonorAccount> DonorAccounts { get; set; }

        public virtual ICollection<RecurringGift> RecurringGifts { get; set; }
    }
}
