using System;
using System.Linq;
using System.Security.Cryptography;

namespace HashPad.Models;

public enum HashType
{
	None = 0,
	Md5,
	Sha1,
	Sha2_256,
	Sha2_384,
	Sha2_512,
	Sha3_256,
	Sha3_384,
	Sha3_512
}

internal static class HashTypeHelper
{
	public static HashAlgorithm GetAlgorithm(this HashType type)
	{
		return type switch
		{
			HashType.Md5 => MD5.Create(),
			HashType.Sha1 => SHA1.Create(),
			HashType.Sha2_256 => SHA256.Create(),
			HashType.Sha2_384 => SHA384.Create(),
			HashType.Sha2_512 => SHA512.Create(),
			HashType.Sha3_256 => SHA3_256.Create(),
			HashType.Sha3_384 => SHA3_384.Create(),
			HashType.Sha3_512 => SHA3_512.Create(),
			_ => throw new NotSupportedException() // HashType.None
		};
	}

	public static int GetBitArrayLength(this HashType type)
	{
		// MD5:      128b -> 16B -> 32
		// Sha1:     160b -> 20B -> 40
		// Sha2_256: 256b -> 32B -> 64
		// Sha2_384: 384b -> 48B -> 96
		// Sha2_512: 512b -> 64B -> 128
		// Sha3_256: 64
		// Sha3_384: 96
		// Sha3_512: 128

		return type switch
		{
			HashType.Md5 => 128,
			HashType.Sha1 => 160,
			HashType.Sha2_256 => 256,
			HashType.Sha2_384 => 384,
			HashType.Sha2_512 => 512,
			HashType.Sha3_256 => 256,
			HashType.Sha3_384 => 384,
			HashType.Sha3_512 => 512,
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