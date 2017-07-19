namespace WebService.Migrations
{
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<WebService.Models.WebServiceContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(WebService.Models.WebServiceContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
            context.Items.AddOrUpdate(
                x => x.Name,
                //new Item() { Name = "Shoe", Points = 50, Labels = new List<string>() { "shoe", "footwear" } },
                //new Item() { Name = "Key", Points = 50, Labels = new List<string>() { "key", "keys", "keychain", "metal", "tool" } },
                new Item() { Name = "Mouse", Points = 200, Labels = new List<string>() { "mouse", "input device", "electronics", "electronic device", "technology", "computer component" } },
                new Item() { Name = "Watch", Points = 100, Labels = new List<string>() { "watch", "time", "jewelry", "clock" } },
                new Item() { Name = "Glasses", Points = 150, Labels = new List<string>() { "glasses", "shades", "jewelry", "eyewear", "goggles" } },
                new Item() { Name = "Scissors", Points = 100, Labels = new List<string>() { "scissor", "scissors", "tool", "hardware", "knife", "blade" } }
            );
        }
    }
}
