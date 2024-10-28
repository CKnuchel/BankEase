using BankEase.Models;
using Microsoft.EntityFrameworkCore;

namespace BankEase.Data
{
	public class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options)
	{
		#region Properties
		private DbSet<Customer> Customer { get; set; }
		#endregion

		#region Protecteds
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<Customer>()
			            .HasIndex(e => e.CustomerNumber)
			            .IsUnique();
		}
		#endregion
	}
}