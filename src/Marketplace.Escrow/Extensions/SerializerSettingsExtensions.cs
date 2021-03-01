using Marketplace.Escrow.MatcherContract.Calls;
using Polkadot.BinarySerializer;
using Polkadot.DataStructs;

namespace Marketplace.Escrow.Extensions
{
    public static class SerializerSettingsExtensions
    {
        public static SerializerSettings RegisterMatcherContract(this SerializerSettings settings,
            PublicKey matcherPublicKey)
        {
            return settings
                    .AddContractCallParameter<GetOwnerParameter>(matcherPublicKey.Bytes, new byte[] {0x58, 0xe3, 0x16, 0x2e})
                    .AddContractCallParameter<SetAdminParameter>(matcherPublicKey.Bytes, new byte[] {0x87, 0xdc, 0x2d, 0xd3})
                    .AddContractCallParameter<GetTotalParameter>(matcherPublicKey.Bytes, new byte[] {0x35, 0x96, 0x1d, 0x4e})
                    .AddContractCallParameter<ResetTotalParameter>(matcherPublicKey.Bytes, new byte[] {0xee, 0xc9, 0x1f, 0x63})
                    .AddContractCallParameter<RegisterDepositParameter>(matcherPublicKey.Bytes, new byte[] {0x5e, 0xb1, 0xcb, 0x1f})
                    .AddContractCallParameter<GetBalanceParameter>(matcherPublicKey.Bytes, new byte[] {0x03, 0xe6, 0x37, 0x92})
                    .AddContractCallParameter<WithdrawParameter>(matcherPublicKey.Bytes, new byte[] {0xb4, 0x12, 0x51, 0xe9})
                    .AddContractCallParameter<RegisterNftDepositParameter>(matcherPublicKey.Bytes, new byte[] {0xa0, 0x38, 0xf5, 0xee})
                    .AddContractCallParameter<GetNftDepositParameter>(matcherPublicKey.Bytes, new byte[] {0x96, 0x56, 0xc0, 0xb1})
                    .AddContractCallParameter<AskParameter>(matcherPublicKey.Bytes, new byte[] {0x7d, 0x02, 0xce, 0xb8})
                    .AddContractCallParameter<GetLastAskIdParameter>(matcherPublicKey.Bytes, new byte[] {0x95, 0x07, 0xea, 0xf8})
                    .AddContractCallParameter<GetAskById>(matcherPublicKey.Bytes, new byte[] {0x25, 0xcf, 0x7a, 0x9f})
                    .AddContractCallParameter<GetAskIdByTokenParameter>(matcherPublicKey.Bytes, new byte[] {0xb4, 0x33, 0xc6, 0xef})
                    .AddContractCallParameter<CancelParameter>(matcherPublicKey.Bytes, new byte[] {0x89, 0x8f, 0xa4, 0x1a})
                    .AddContractCallParameter<BuyParameter>(matcherPublicKey.Bytes, new byte[] {0x15, 0x1e, 0x67, 0xbe})
                ;
        }
    }
}