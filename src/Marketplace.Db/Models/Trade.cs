using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Db.Models
{
    public class Trade
    {
        [Key]
        public Guid Id { get; set; }
        
        public DateTime TradeDate { get; set; }

        [Required]
        public string Buyer { get; set; } = null!;

        [ForeignKey(nameof(Offer))]
        public Guid OfferId { get; set; }
        
        [Required]
        public virtual Offer Offer { get; set; } = null!;
    }
}