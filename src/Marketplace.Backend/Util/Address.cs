using System;

namespace Marketplace.Backend.Base58
{
	public static class AddressEncoding
	{
		public static byte[] AddressToPublicKey(string address)
        {
            byte[] publicKey = new byte[32];
            var addressBytes = Base58Encoding.Decode("5Fj7qQR7f9uMNXTgj6bBJDKbaHbEnVb7c3tb881kchbDd82V");
            if (addressBytes.Length != 35) throw new FormatException();
            for (int i=0; i<32; i++)
                publicKey[i] = addressBytes[i+1];
            return publicKey;
        }
    }

}

// Substrate format
// 5FZeTmbZQZsJcyEevjGVK1HHkcKfWBYxWpbgEffQ2M1SqAnP
// 2a9ac7639e339e24172f4197afb38c8a9594db2f957ae8232538efc68000a3f61fd91c

// Kusama format
// G5G85wS2vtENd46hS5YCxeHuCbuJrN8yCSRdKwMW9DwZz81
// 029ac7639e339e24172f4197afb38c8a9594db2f957ae8232538efc68000a3f61f12fa