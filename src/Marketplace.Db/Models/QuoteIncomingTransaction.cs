using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Db.Models
{
    [Index(nameof(AccountPublicKey))]
    [Table("QuoteIncomingTransaction")]
    public class QuoteIncomingTransaction : IDataToProcess
    {
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Positive when marketplace receives kusama, negative when marketplace uses it to pay for tokens or withdrawals. 
        /// </summary>
        public BigInteger Amount { get; set; }
        
        public ulong QuoteId { get; set; }

        public string Description { get; set; } = null!;

        public string AccountPublicKey { get; set; } = null!;
        
        [NotMapped]
        public byte[] AccountPublicKeyBytes
        {
            get => Convert.FromBase64String(AccountPublicKey);
            set => AccountPublicKey = Convert.ToBase64String(value);
        }

        public ulong? BlockId { get; set; }

        public ProcessingDataStatus Status { get; set; }
        
        [ConcurrencyCheck]
        public DateTime? LockTime { get; set; }

        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Someone sent kusama to marketplace.
        /// </summary>
        /// <returns></returns>
        public static QuoteIncomingTransaction Incoming(BigInteger amount, byte[] accountPublicKey, ulong blockId)
        {
            if (amount <= 0)
            {
                throw new Exception("Negative income.");
            }

            return new QuoteIncomingTransaction()
            {
                Amount = amount,
                Id = Guid.NewGuid(),
                Description = $"Transfered to marketplace",
                AccountPublicKeyBytes = accountPublicKey,
                BlockId = blockId,
            };
        }
    }
}