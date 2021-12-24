using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shell;

using HashPad.Properties;

namespace HashPad.Models
{
	internal class ShortcutManager
	{
		public const string AddOption = "/add";
		public const string RemoveOption = "/remove";

		public static bool Check(string[] args)
		{
			if (args is { Length: > 0 })
			{
				if (args.Contains(AddOption))
				{
					Add();
					return true;
				}
				if (args.Contains(RemoveOption))
				{
					Remove();
					return true;
				}
			}

			if (ShortcutHelper.Exists())
				SetRemoveJumpList();
			else
				SetAddJumpList();

			return false;
		}

		public static void Add()
		{
			ShortcutHelper.Create();
			SetRemoveJumpList();
		}

		public static void Remove()
		{
			ShortcutHelper.Remove();
			SetAddJumpList();
		}

		private static void SetAddJumpList() => SetJumpList(Resources.JumpAdd, Resources.JumpAddDescription, AddOption);
		private static void SetRemoveJumpList() => SetJumpList(Resources.JumpRemove, Resources.JumpRemoveDescription, RemoveOption);

		private static void SetJumpList(string title, string description, string arguments)
		{
			var (_, executablePath, aliasPath) = ShortcutHelper.GetNamePaths();

			JumpTask GetJumpTask() => new()
			{
				Title = title,
				Description = description,
				Arguments = arguments,
				ApplicationPath = (aliasPath ?? executablePath),
				IconResourcePath = executablePath,
				IconResourceIndex = 0
			};

			var jumpList = new JumpList();
			jumpList.JumpItems.Add(GetJumpTask());
			jumpList.Apply();
		}
	}
}