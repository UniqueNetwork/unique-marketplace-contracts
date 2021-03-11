using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marketplace.Db.Models
{
    [Table("UniqueProcessedBlock")]
    public class UniqueProcessedBlock : IProcessedBlock
    {
        [Key]
        public ulong BlockNumber { get; set; }
        public DateTime ProcessDate { get; set; }
    }
}