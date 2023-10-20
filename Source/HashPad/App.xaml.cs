using System.Windows;

using HashPad.Models;
using HashPad.Views;

namespace HashPad;

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