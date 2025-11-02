using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace AMESA_be.common.Contracts.Login
{
    [Table("login_audit")]
    public class LoginAudit
    {
        [Column("ID")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Column("CREATED_AT")]
        public DateTime? CreatedAt { get; set; }
        [Column("CLIENT_PC")]
        [Required]
        [StringLength(100)]
        public string ClientPc { get; set; }
        [Required]
        [StringLength(100)]
        [Column("SERVICE_SERVER")]
        public string ServiceServer { get; set; }
        [Required]
        [StringLength(100)]
        [Column("USER_NAME")]
        public string UserName { get; set; }
        [Required]
        [StringLength(30)]
        [Column("ACTION")]
        public string Action { get; set; }
        [Required]
        [StringLength(30)]
        [Column("STATUS")]
        public string Status { get; set; }
        [Required]
        [Column("TOKEN")]
        public string Token { get; set; }
        [Required]
        [StringLength(1024)]
        [Column("REASON")]
        public string Reason { get; set; }
        [Required]
        [StringLength(100)]
        [Column("sessionId")]
        public string SessionId { get; set; }
    }
}
