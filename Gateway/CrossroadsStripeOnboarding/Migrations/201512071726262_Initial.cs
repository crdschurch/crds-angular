namespace CrossroadsStripeOnboarding.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.StripeAccounts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Type = c.Int(nullable: false),
                        OldCardId = c.String(),
                        NewCardId = c.String(),
                        Fingerprint = c.String(),
                        Last4 = c.String(),
                        ExpMonth = c.Int(nullable: false),
                        ExpYear = c.Int(nullable: false),
                        Institution = c.String(),
                        StripeCustomerId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.StripeCustomers", t => t.StripeCustomerId, cascadeDelete: true)
                .Index(t => t.StripeCustomerId);
            
            CreateTable(
                "dbo.StripeCustomers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CustomerId = c.String(),
                        ExternalPersonId = c.String(),
                        Imported = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.StripeAccounts", "StripeCustomerId", "dbo.StripeCustomers");
            DropIndex("dbo.StripeAccounts", new[] { "StripeCustomerId" });
            DropTable("dbo.StripeCustomers");
            DropTable("dbo.StripeAccounts");
        }
    }
}
