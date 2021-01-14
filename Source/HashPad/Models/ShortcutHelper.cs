using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using File = System.IO.File;

using IWshRuntimeLibrary;

namespace HashPad.Models
{
	internal class ShortcutHelper
	{
		public static (string name, string executablePath, string aliasPath) GetNamePaths()
		{
			string aliasPath = null;
			if (PlatformInfo.IsPackaged)
			{
				// Get path to app execution alias.
				var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
					@"Microsoft\WindowsApps",
					Path.GetFileName(ProductInfo.Location));
				if (File.Exists(path))
					aliasPath = path;
			}

			return ((ProductInfo.Title ?? ProductInfo.Product), ProductInfo.Location, aliasPath);
		}

		private static string GetShortcutPath(string name) =>
			Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.SendTo), $"{name}.lnk");

		public static bool Exists()
		{
			var (name, executablePath, aliasPath) = GetNamePaths();

			var shortcutPath = GetShortcutPath(name);
			if (!File.Exists(shortcutPath))
				return false;

			var shell = new WshShell();
			var shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
			return (shortcut.TargetPath == (aliasPath ?? executablePath));
		}

		public static void Create()
		{
			var (name, executablePath, aliasPath) = GetNamePaths();

			var shell = new WshShell();
			var shortcut = (IWshShortcut)shell.CreateShortcut(GetShortcutPath(name));
			shortcut.Description = name;
			shortcut.TargetPath = (aliasPath ?? executablePath);
			shortcut.IconLocation = executablePath;
			shortcut.Save();
		}

		public static void Remove()
		{
			var (name, _, _) = GetNamePaths();

			File.Delete(GetShortcutPath(name));
		}
	}
}