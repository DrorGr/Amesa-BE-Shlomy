using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AMESA_be.LotteryDevice.DTOs
{
    [Table("users")]
    public class User
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [StringLength(15)]
        public string UserName { get; set; }

        [Required]
        [StringLength(15)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(20)]
        public string LastName { get; set; }

        public int Gender { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        public int Country { get; set; }

        [Column(TypeName = "json")]
        public string Address { get; set; }

        public long Tenant { get; set; }

        public bool IsActive { get; set; } = true;

        [StringLength(100)]
        public string Email { get; set; }

        [Column(TypeName = "json")]
        public string Phones { get; set; }

        public string Password { get; set; }

        [Required]
        public long UserId { get; set; }

        public DateTime LastUpdateDate { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}