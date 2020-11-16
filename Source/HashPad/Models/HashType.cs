using System;
using System.Linq;
using System.Security.Cryptography;

namespace HashPad.Models
{
	public enum HashType
	{
		None = 0,
		Md5,
		Sha1,
		Sha256,
		Sha384,
		Sha512
	}

	internal static class HashTypeHelper
	{
		public static HashAlgorithm GetAlgorithm(this HashType type)
		{
			return type switch
			{
				HashType.Md5 => new MD5CryptoServiceProvider(),
				HashType.Sha1 => new SHA1CryptoServiceProvider(),
				HashType.Sha256 => new SHA256CryptoServiceProvider(),
				HashType.Sha384 => new SHA384CryptoServiceProvider(),
				HashType.Sha512 => new SHA512CryptoServiceProvider(),
				_ => throw new NotSupportedException() // HashType.None
			};
		}

		public static int GetBitArrayLength(this HashType type)
		{
			// MD5:    128b -> 16B -> 32
			// SHA1:   160b -> 20B -> 40
			// SHA256: 256b -> 32B -> 64
			// SHA384: 384b -> 48B -> 96
			// SHA512: 512b -> 64B -> 128

			return type switch
			{
				HashType.Md5 => 128,
				HashType.Sha1 => 160,
				HashType.Sha256 => 256,
				HashType.Sha384 => 384,
				HashType.Sha512 => 512,
				_ => 0
			};
		}

		public static int GetHexStringLength(this HashType type)
		{
			// 8bits -> 1byte -> 2chars in hex string
			return GetBitArrayLength(type) / 4;
		}

		public static bool TryGetHashType(int hexStringLength, out HashType type)
		{
			type = Enum.GetValues(typeof(HashType)).Cast<HashType>()
				.FirstOrDefault(x => x.GetHexStringLength() == hexStringLength);

			return type != default;
		}
	}
}