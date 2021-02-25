using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Db.Models
{
    public class NftIncomeTransaction
    {
        [Key]
        public Guid Id { get; set; }
        
        public uint CollectionId { get; set; }
        
        public uint TokenId { get; set; }
        
        /// <summary>
        /// True if matcher deposit was called.
        /// </summary>
        public bool Deposited { get; set; }
        
        public BigInteger Value { get; set; } 

        [Required]
        public string OwnerPublicKey { get; set; } = null!;

        [NotMapped]
        public byte[] OwnerPublicKeyBytes
        {
            get => Convert.FromBase64String(OwnerPublicKey);
            set => OwnerPublicKey = Convert.ToBase64String(value);
        }
        
        [ForeignKey(nameof(UniqueProcessedBlock))]
        public ulong UniqueProcessedBlockId { get; set; }

        [Required]
        public virtual UniqueProcessedBlock UniqueProcessedBlock { get; set; } = null!;
    }
}