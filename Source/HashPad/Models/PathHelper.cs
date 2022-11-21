using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HashPad.Models
{
	internal static class PathHelper
	{
		public static string[] Restore(string[] fragments)
		{
			if (fragments is not { Length: > 0 })
				return null;

			var segments = string.Join("*", fragments).Split(Path.DirectorySeparatorChar);
			if (segments.Length < 2) // root and (file path or folder path) 
				return null;

			var root = segments[0];
			if (!new Regex(@"^[a-zA-Z]:$").IsMatch(root))
				return null;

			try
			{
				return Search(root + Path.DirectorySeparatorChar, 1).ToArray();
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
				return null;
			}

			IEnumerable<string> Search(string currentPath, int currentIndex)
			{
				if (currentIndex < segments.Length - 1)
				{
					foreach (var folderPath in Directory.EnumerateDirectories(currentPath, segments[currentIndex]))
					{
						foreach (var filePath in Search(folderPath, currentIndex + 1))
							yield return filePath;
					}
				}
				else
				{
					foreach (var filePath in Directory.EnumerateFiles(currentPath, segments[currentIndex]))
						yield return filePath;
				}
			}
		}
	}
}