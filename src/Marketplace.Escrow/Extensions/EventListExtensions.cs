using System.Linq;
using OneOf;
using Polkadot.BinaryContracts.Events;
using Polkadot.BinaryContracts.Events.PhaseEnum;
using Polkadot.BinaryContracts.Events.System;

namespace Marketplace.Escrow.Extensions
{
    public static class EventListExtensions
    {
        public static bool ExtrinsicSuccess(this EventList eventList, uint extrinsicIndex)
        {
            return eventList.Events
                .Where(e => e.Event is ExtrinsicSuccess)
                .Any(e => IsApplyExtrinsic(e, extrinsicIndex));
        }

        private static bool IsApplyExtrinsic(EventRecord eventRecord, uint extrinsicIndex)
        {
            return eventRecord.Phase.Value.Match(
                apply => apply.Value == extrinsicIndex,
                _ => false,
                _ => false);
        }
    }
}