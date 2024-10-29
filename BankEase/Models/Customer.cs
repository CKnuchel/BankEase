using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankEase.Models
{
    [Table("CUSTOMER")]
    public class Customer
    {
        #region Properties
        [Key]
        [Column(name: "ID")]
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        [Column(name: "CUSTOMER_NUMBER")]
        public required string CustomerNumber { get; set; }

        [Required]
        [StringLength(10)]
        [Column(name: "TITLE")]
        public required string Title { get; set; }

        [Required]
        [StringLength(30)]
        [Column(name: "FIRST_NAME")]
        public required string FirstName { get; set; }

        [Required]
        [StringLength(30)]
        [Column(name: "LAST_NAME")]
        public required string LastName { get; set; }

        [Required]
        [StringLength(30)]
        [Column(name: "STREET")]
        public required string Street { get; set; }

        [Required]
        [StringLength(30)]
        [Column(name: "CITY")]
        public required string City { get; set; }

        [Required]
        [Column(name: "ZIPCODE", TypeName = "smallint")]
        public short ZipCode { get; set; }
        #endregion
    }
}