using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace WebService.Models
{
    public class WebServiceContext : DbContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, please use data migrations.
        // For more information refer to the documentation:
        // http://msdn.microsoft.com/en-us/data/jj591621.aspx
    
        public WebServiceContext() : base("name=WebServiceContext")
        {
        }

        public System.Data.Entity.DbSet<WebService.Models.Item> Items { get; set; }

        public System.Data.Entity.DbSet<WebService.Models.ImageSubmission> ImageSubmissions { get; set; }

        public System.Data.Entity.DbSet<WebService.Models.Game> Games { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Game>().HasMany<Item>(g => g.Items).WithMany();
        }
    }
}
