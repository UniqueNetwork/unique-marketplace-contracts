using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;

namespace Marketplace.Db.Models
{
    public class NftOutgoingTransaction : IDataToProcess
    {
        [Key]
        public Guid Id { get; set; }
        
        public ulong CollectionId { get; set; }
        
        public ulong TokenId { get; set; }
        
        public BigInteger Value { get; set; } 

        [Required]
        public string RecipientPublicKey { get; set; } = null!;

        [NotMapped]
        public byte[] RecipientPublicKeyBytes
        {
            get => Convert.FromBase64String(RecipientPublicKey);
            set => RecipientPublicKey = Convert.ToBase64String(value);
        }

        public ProcessingDataStatus Status { get; set; }

        [ConcurrencyCheck]
        public DateTime? LockTime { get; set; }

        public string? ErrorMessage { get; set; }
    }
}