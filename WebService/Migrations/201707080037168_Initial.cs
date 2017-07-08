namespace WebService.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Games",
                c => new
                    {
                        GameID = c.Int(nullable: false, identity: true),
                    })
                .PrimaryKey(t => t.GameID);
            
            CreateTable(
                "dbo.Items",
                c => new
                    {
                        ItemID = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        LabelsAsString = c.String(),
                        Points = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ItemID);
            
            CreateTable(
                "dbo.ImageSubmissions",
                c => new
                    {
                        ImageID = c.Int(nullable: false, identity: true),
                        GameID = c.Int(nullable: false),
                        Image = c.Binary(nullable: false),
                        VerificationResult = c.Boolean(nullable: false),
                        LabelsAsString = c.String(),
                        ItemID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ImageID)
                .ForeignKey("dbo.Items", t => t.ItemID, cascadeDelete: true)
                .Index(t => t.ItemID);
            
            CreateTable(
                "dbo.GameItems",
                c => new
                    {
                        Game_GameID = c.Int(nullable: false),
                        Item_ItemID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Game_GameID, t.Item_ItemID })
                .ForeignKey("dbo.Games", t => t.Game_GameID, cascadeDelete: true)
                .ForeignKey("dbo.Items", t => t.Item_ItemID, cascadeDelete: true)
                .Index(t => t.Game_GameID)
                .Index(t => t.Item_ItemID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ImageSubmissions", "ItemID", "dbo.Items");
            DropForeignKey("dbo.GameItems", "Item_ItemID", "dbo.Items");
            DropForeignKey("dbo.GameItems", "Game_GameID", "dbo.Games");
            DropIndex("dbo.GameItems", new[] { "Item_ItemID" });
            DropIndex("dbo.GameItems", new[] { "Game_GameID" });
            DropIndex("dbo.ImageSubmissions", new[] { "ItemID" });
            DropTable("dbo.GameItems");
            DropTable("dbo.ImageSubmissions");
            DropTable("dbo.Items");
            DropTable("dbo.Games");
        }
    }
}
