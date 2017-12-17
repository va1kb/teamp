using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGbot.DB_model;

namespace TGbot
{
    public class MyDB : DbContext
    {
        
        public DbSet<User> Users { get; set; }
        public DbSet<Notes> Note { get; set; }
        public DbSet<Remind> Reminds { get; set; }
        public MyDB() : base("DbConnection")
        {
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<MyDB>());
        }

    }
}
