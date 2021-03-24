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
                publicKey[i] = addressBytes[i+2];
            return publicKey;
        }
    }

}