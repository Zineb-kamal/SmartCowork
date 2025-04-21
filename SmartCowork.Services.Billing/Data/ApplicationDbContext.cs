
using Microsoft.EntityFrameworkCore;
using SmartCowork.Services.Billing.Models;

namespace SmartCowork.Services.Billing.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuration des relations
            modelBuilder.Entity<InvoiceItem>()
                .HasOne(i => i.Invoice)
                .WithMany(inv => inv.Items)
                .HasForeignKey(i => i.InvoiceId);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Invoice)
                .WithMany(inv => inv.Transactions)
                .HasForeignKey(t => t.InvoiceId);
        }
    }
}