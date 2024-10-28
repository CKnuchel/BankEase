using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankEase.Models
{
	[Table("TRANSACTION_RECORD")]
	public class TransactionRecord
	{
		#region Properties
		[Key]
		[Column(name: "ID")]
		public int Id { get; set; }

		[Required]
		[Column(name: "TYPE", TypeName = "char(1)")]
		public char Type { get; set; }

		[Required]
		[Column(name: "TEXT")]
		[StringLength(30)]
		public required string Text { get; set; }

		[Required]
		[Column(name: "AMOUNT", TypeName = "decimal(9,2)3")]
		public decimal Amount { get; set; }

		[Required]
		[Column(name: "TRANSACTION_TIME", TypeName = "datetime")]
		public DateTime TransactionTime { get; set; }

		[Required]
		[Column(name: "ACCOUNT_ID")]
		public int AccountId { get; set; }

		[ForeignKey("AccountId")]
		public required Account Account { get; set; }
		#endregion
	}
}