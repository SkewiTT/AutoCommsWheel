using Microsoft.VisualBasic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AutoCommsWheel
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// From XAML, runs on window load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);
            window.KeyDown += HandleKeyPress;
        }

        private void HandleKeyPress(object sender, KeyEventArgs e)
        {
            // if the key pressed matches the one set in the settings, open the config window
            if ((Key)settings.Default.ConfigOpenKeybind == e.Key)
            {
                // Open the config window if it doesn't exist, otherwise tab into it
                if (!Helpers.IsWindowOpen<ConfigWindow>())
                {
                    ConfigWindow configWindow = new ConfigWindow();
                    configWindow.Show();
                }
                else
                {
                    Application.Current.Windows.OfType<ConfigWindow>().First().Activate();
                }
            }
        }
    }
}