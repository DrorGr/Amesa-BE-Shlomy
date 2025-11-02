using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AMESA_be.common.Models;

   [Table("users")]
    public class User
    {
        [Key]
        [Column("Id")]
        public long Id { get; set; }

        [Required]
        [StringLength(15)]
        [Column("UserName")]
        public required string UserName { get; set; }

        [Required]
        [StringLength(15)]
        [Column("FirstName")]
        public required string FirstName { get; set; }

        [Required]
        [StringLength(20)]
        [Column("LastName")]
        public string LastName { get; set; }

        [Column("Gender")]
        public int? Gender { get; set; }

        [Required]
        [Column("DateOfBirth", TypeName = "date")]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [Column("Country")]
        public int Country { get; set; }

        /// <summary>
        /// JSON data representing the user's address. Mapped as a string.
        /// Your data access layer (like EF Core) can be configured to handle serialization.
        /// </summary>
        [Column("Address", TypeName = "json")]
        public string? Address { get; set; }

        [Column("Tenant")]
        public long? Tenant { get; set; }

        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        [StringLength(100)]
        [EmailAddress]
        [Column("Email")]
        public string? Email { get; set; }

        /// <summary>
        /// JSON data representing the user's phone numbers. Mapped as a string.
        /// </summary>
        [Column("Phones", TypeName = "json")]
        public string? Phones { get; set; }

        [Column("Password")]
        public string? Password { get; set; }

        [Column("IsSystem")]
        public int IsSystem { get; set; } = 0;

        [Required]
        [Column("UserId")]
        public long UserId { get; set; }

        [Column("LastUpdateDate")]
        public DateTime? LastUpdateDate { get; set; }

        [Column("createdDate")]
        public DateTime? CreatedDate { get; set; }

        [Column("ChangePassword")]
        public bool ChangePassword { get; set; } = false;
    }