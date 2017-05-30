namespace WebService.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ImageSubmissions",
                c => new
                    {
                        ImageID = c.Int(nullable: false, identity: true),
                        Image = c.Binary(nullable: false),
                        VerificationResult = c.Boolean(nullable: false),
                        LabelsAsString = c.String(),
                        ItemID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ImageID)
                .ForeignKey("dbo.Items", t => t.ItemID, cascadeDelete: true)
                .Index(t => t.ItemID);
            
            CreateTable(
                "dbo.Items",
                c => new
                    {
                        ItemID = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        LabelsAsString = c.String(),
                    })
                .PrimaryKey(t => t.ItemID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ImageSubmissions", "ItemID", "dbo.Items");
            DropIndex("dbo.ImageSubmissions", new[] { "ItemID" });
            DropTable("dbo.Items");
            DropTable("dbo.ImageSubmissions");
        }
    }
}
