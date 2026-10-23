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
using System.Windows.Shapes;

namespace AutoCommsWheel
{
    /// <summary>
    /// Interaction logic for WheelPreviewWindow.xaml
    /// </summary>
    public partial class WheelPreviewWindow : Window
    {
        public WheelPreviewWindow(WheelDefinition currentWheel)
        {
            InitializeComponent();
            WheelPreviewTreeView.ItemsSource = currentWheel.Main;
        }
    }
}
