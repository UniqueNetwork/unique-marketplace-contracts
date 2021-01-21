using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Db.Models
{
    [Index(nameof(CollectionId))]
    [Index(nameof(CreationDate))]
    public class Offer
    {
        [Key]
        public Guid Id { get; set; }

        public DateTime CreationDate { get; set; }
        
        public ulong CollectionId { get; set; }

        public ulong TokenId { get; set; }
        
        public BigInteger Price { get; set; }
        
        [Required]
        public string Seller { get; set; } = null!;
        
        [Required]
        public string Metadata { get; set; } = null!;
        
        public OfferStatus OfferStatus { get; set; }

        public virtual ICollection<Trade> Trades { get; set; } = null!;
    }
}