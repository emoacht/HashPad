using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHashChecker.Models
{
	public static class StringHelper
	{
		public static bool IsOverHalfLower(string source)
		{
			if (string.IsNullOrEmpty(source))
				return false;

			uint upperCount = 0, lowerCount = 0;

			foreach (var c in source)
			{
				if (char.IsUpper(c)) upperCount++;
				else if (char.IsLower(c)) lowerCount++;
			}

			return (0 < lowerCount) && (upperCount <= lowerCount);
		}
	}
}