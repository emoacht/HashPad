using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using HashPad.Models;
using HashPad.Views;

namespace HashPad
{
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			if (ShortcutManager.Check(e.Args))
				this.Shutdown(0);

			this.MainWindow = new MainWindow();
			this.MainWindow.Show();
		}
	}
}