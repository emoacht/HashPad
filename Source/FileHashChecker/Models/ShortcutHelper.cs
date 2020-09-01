using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using File = System.IO.File;

using IWshRuntimeLibrary;

namespace FileHashChecker.Models
{
	internal class ShortcutHelper
	{
		private static (string name, string executablePath) GetNameExecutablePath()
		{
			var assembly = Assembly.GetExecutingAssembly();

			var title = ((AssemblyTitleAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyTitleAttribute))).Title;
			if (string.IsNullOrEmpty(title))
				title = assembly.GetName().Name;

			return (title, assembly.Location);
		}

		private static string GetShortcutFilePath(string name) =>
			Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.SendTo), $"{name}.lnk");

		public static bool Exists()
		{
			var (name, executablePath) = GetNameExecutablePath();

			var shortcutFilePath = GetShortcutFilePath(name);
			if (!File.Exists(shortcutFilePath))
				return false;

			var shell = new WshShell();
			var shortcut = (IWshShortcut)shell.CreateShortcut(shortcutFilePath);
			return (shortcut.TargetPath == executablePath);
		}

		public static void Create()
		{
			var (name, executablePath) = GetNameExecutablePath();

			var shell = new WshShell();
			var shortcut = (IWshShortcut)shell.CreateShortcut(GetShortcutFilePath(name));
			shortcut.Description = name;
			shortcut.TargetPath = executablePath;
			shortcut.IconLocation = executablePath;
			shortcut.Save();
		}

		public static void Remove()
		{
			var (name, _) = GetNameExecutablePath();

			File.Delete(GetShortcutFilePath(name));
		}
	}
}