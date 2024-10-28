using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankEase.Models
{
	[Table("ACCOUNT")]
	public class Account
	{
		#region Properties
		[Key]
		[Column(name: "ID")]
		public int Id { get; set; }

		[Required]
		[StringLength(30)]
		[Column(name: "IBAN")]
		public required string IBAN { get; set; }

		[Required]
		[Column(name: "BALANCE", TypeName = "decimal(9,2)")]
		public decimal Balance { get; set; }

		[Required]
		[Column(name: "OVERDRAFT", TypeName = "decimal(7,2)")]
		public decimal Overdraft { get; set; }

		[Required]
		[Column(name: "CUSTOMER_ID")]
		public int CustomerId { get; set; }

		[ForeignKey("CustomerId")]
		public required Customer Customer { get; set; }
		#endregion
	}
}