using System;

namespace Marketplace.Db.Models
{
    public interface IDataToProcess
    {
        Guid Id { get; set; }
        ProcessingDataStatus Status { get; set; }
        DateTime? LockTime { get; set; }
        string? ErrorMessage { get; set; }
    }
}