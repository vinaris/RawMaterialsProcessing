using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RawMaterialsProcessing.Models;

namespace RawMaterialsProcessing.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Nomenclature> Nomenclature { get; set; }
        public DbSet<MachineTool> MachineTools { get; set; }
        public DbSet<Party> Parties { get; set; }
        public DbSet<Operation> Operations { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
            : base (options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Operation>().HasKey(k => new {k.MachineToolId, k.NomenclatureId});
        }
    }
}
