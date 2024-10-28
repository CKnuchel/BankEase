using BankEase.Models;
using Microsoft.EntityFrameworkCore;

namespace BankEase.Data
{
	public class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options)
	{
		#region Properties
		public DbSet<Customer> Customer { get; set; }
		public DbSet<Account> Account { get; set; }
		public DbSet<TransactionRecord> TransactionRecord { get; set; }
		#endregion

		#region Protecteds
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// Die CustomerNumber auf Unique stelen
			modelBuilder.Entity<Customer>()
			            .HasIndex(e => e.CustomerNumber)
			            .IsUnique();

			// Die IBAN auf Unique stellen
			modelBuilder.Entity<Account>()
			            .HasIndex(e => e.IBAN)
			            .IsUnique();

			// Defaulwerte für die TransactionRecord
			modelBuilder.Entity<TransactionRecord>()
			            .Property(e => e.TransactionTime)
			            .HasDefaultValueSql("GETDATE()");
		}
		#endregion
	}
}