using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Marketplace.Backend.Base58
{
	public class ArrayHelpers
	{
		public static T[] ConcatArrays<T>(params T[][] arrays)
		{
            if (arrays != null) {
                var result = new T[arrays.Sum(arr => arr.Length)];
                int offset = 0;
                for (int i = 0; i < arrays.Length; i++)
                {
                    var arr = arrays[i];
                    Buffer.BlockCopy(arr, 0, result, offset, arr.Length);
                    offset += arr.Length;
                }
                return result;
            }
            else throw new ArgumentNullException();
		}

		public static T[] ConcatArrays<T>(T[] arr1, T[] arr2)
		{
			if ((arr1 != null) && (arr2 != null)) 
            {
                var result = new T[arr1.Length + arr2.Length];
                Buffer.BlockCopy(arr1, 0, result, 0, arr1.Length);
                Buffer.BlockCopy(arr2, 0, result, arr1.Length, arr2.Length);
                return result;
            }
            else throw new FormatException();
		}

		public static T[] SubArray<T>(T[] arr, int start, int length)
		{
            if ((arr != null) &&
                (start >= 0) &&
                (length >= 0) &&
                (start + length <= arr.Length)
            ) {
                var result = new T[length];
                Buffer.BlockCopy(arr, start, result, 0, length);
                return result;
            }
            else throw new FormatException();
		}

		public static T[] SubArray<T>(T[] arr, int start)
		{
            if ((arr != null) &&
                (start >= 0) &&
                (start <= arr.Length))
    			return SubArray(arr, start, arr.Length - start);
            else throw new FormatException();
		}
	}
}