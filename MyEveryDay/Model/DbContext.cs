using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyEveryDay.Model
{
    public class MyEveryDayDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        private bool hasDb = false;

        internal static MyEveryDayDbContext GetNew()
        {
            return new MyEveryDayDbContext();
        }

        private MyEveryDayDbContext()
        {
            if (!hasDb)
            {
                Database.EnsureCreated();
                hasDb = true;
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=db.sqlite");
        }

        public DbSet<Record> Records { get; set; }
        public DbSet<Template> Templates { get; set; }
    }
}