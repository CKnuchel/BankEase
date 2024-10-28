using Microsoft.EntityFrameworkCore;

namespace BankEase.Data
{
	public class DatabaseContext : DbContext
	{
		#region Constructors
		public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
		{
		}
		#endregion

		// DBSets
	}
}