namespace CallCreditApiDelegation.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class callcreditinfoObj2 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CallCreditInfo",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Password = c.String(),
                        passwordLastChangeDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.DailyMonitorReports",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        DailyRequestsCounter = c.Int(nullable: false),
                        RequestDate = c.DateTime(nullable: false),
                        IP = c.String(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.DailyMonitorReports");
            DropTable("dbo.CallCreditInfo");
        }
    }
}
