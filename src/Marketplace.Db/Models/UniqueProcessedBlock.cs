using System;
using System.ComponentModel.DataAnnotations;

namespace Marketplace.Db.Models
{
    public class UniqueProcessedBlock : IProcessedBlock
    {
        [Key]
        public ulong BlockNumber { get; set; }
        public DateTime ProcessDate { get; set; }
    }
}