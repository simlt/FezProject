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
                new Item() { Name = "Shoe", Labels = new List<string>() { "shoe" } },
                new Item() { Name = "PC", Labels = new List<string>() { "pc" } },
                new Item() { Name = "Key", Labels = new List<string>() { "key" } }
            );
        }
    }
}
