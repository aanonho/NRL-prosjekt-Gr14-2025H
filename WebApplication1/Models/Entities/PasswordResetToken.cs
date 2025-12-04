// Databasetabell for passordreset-token som kobler hash til bruker og utløpstidspunkt.
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Entities
{
    [Table("PasswordResetTokens")]
    public class PasswordResetToken
    {
        // Primærnøkkel generert av databasen.
        [Key]
        public int Id { get; set; }

        // Kobling til brukeren tokenet gjelder for.
        [Required]
        [Column("UserID")]
        public int UserId { get; set; }

        // Hash-verdi lagres i databasen slik at rå token ikke ligger i klartekst.
        [Required]
        [StringLength(128)]
        public string TokenHash { get; set; } = string.Empty;

        // Når tokenet ble opprettet og når det utløper.
        public DateTime CreatedAtUtc { get; set; }

        public DateTime ExpiresAtUtc { get; set; }

        // Settes når tokenet er brukt, ellers null.
        public DateTime? RedeemedAtUtc { get; set; }

        // Navigasjonsreferanse for Entity Framework.
        [ForeignKey(nameof(UserId))]
        public UserEntity? User { get; set; }
    }
}
