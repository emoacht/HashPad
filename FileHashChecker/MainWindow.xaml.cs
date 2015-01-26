using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FileHashChecker
{
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel mainWindowViewModel;

        public MainWindow()
        {
            InitializeComponent();

            mainWindowViewModel = this.DataContext as MainWindowViewModel;
            if (mainWindowViewModel != null)
            {
                this.Loaded += OnLoaded;
                this.Drop += OnDrop;

                this.SetBinding(
                    UIElement.AllowDropProperty,
                    new Binding("IsReady")
                    {
                        Source = mainWindowViewModel,
                        Mode = BindingMode.OneWay
                    });
            }
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            var args = Environment.GetCommandLineArgs().Skip(1); // The first element is this executable file path.

            await mainWindowViewModel.CheckFileAsync(args);
        }

        private async void OnDrop(object sender, DragEventArgs e)
        {
            var paths = ((DataObject)e.Data).GetFileDropList().Cast<string>();

            await mainWindowViewModel.CheckFileAsync(paths);
        }
    }
}