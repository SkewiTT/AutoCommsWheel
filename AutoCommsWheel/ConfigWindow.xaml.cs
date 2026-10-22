using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AutoCommsWheel
{
    /// <summary>
    /// Interaction logic for ConfigWindow.xaml
    /// </summary>
    public partial class ConfigWindow : Window
    {
        public ConfigWindow()
        {
            InitializeComponent();
        }

        private void ConfigKeyBind_Loaded(object sender, RoutedEventArgs e)
        {
            ConfigKeyBind.Text = (string)new KeyConverter().ConvertTo((Key)settings.Default.ConfigOpenKeybind, "".GetType());
        }

        private void ConfigKeyBind_KeyEntered(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            settings.Default.ConfigOpenKeybind = (int)e.Key;
            settings.Default.Save();
            ConfigKeyBind.Text = e.Key.ToString();
        }
    }
}
