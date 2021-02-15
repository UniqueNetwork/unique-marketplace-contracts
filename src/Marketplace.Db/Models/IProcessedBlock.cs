using System;

namespace Marketplace.Db.Models
{
    /// <summary>
    /// Table with scanned and processed blockchain blocks.
    /// </summary>
    public interface IProcessedBlock
    {
        public ulong BlockNumber { get; set; }
        
        public DateTime ProcessDate { get; set; }
    }
}