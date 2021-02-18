using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Db.Models
{
    [Index(nameof(AccountPublicKey))]
    public class KusamaTransaction
    {
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Positive when marketplace receives kusama, negative when marketplace uses it to pay for tokens or withdrawals. 
        /// </summary>
        public BigInteger Amount { get; set; }

        public string Description { get; set; } = null!;

        public string AccountPublicKey { get; set; } = null!;
        
        [ForeignKey(nameof(Block))]
        public ulong? BlockId { get; set; }

        public virtual KusamaProcessedBlock Block { get; set; } = null!;

        /// <summary>
        /// Someone sent kusama to marketplace.
        /// </summary>
        /// <returns></returns>
        public static KusamaTransaction Income(BigInteger amount, string accountPublicKey, ulong blockId)
        {
            if (amount <= 0)
            {
                throw new Exception("Negative income.");
            }

            return new KusamaTransaction()
            {
                Amount = amount,
                Id = Guid.NewGuid(),
                Description = $"Transfered to marketplace",
                AccountPublicKey = accountPublicKey,
                BlockId = blockId,
            };
        }
    }
}