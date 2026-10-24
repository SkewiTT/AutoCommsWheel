using Microsoft.VisualBasic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
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

            if (!string.IsNullOrEmpty(settings.Default.SelectedWheelName))
            {
                WheelManager.Load(settings.Default.SelectedWheelName);
            }

            LoadWheelPaths();
        }

        private void HandleKeyPress(object sender, KeyEventArgs e)
        {
            // if the key pressed matches the one set in the settings, open the config window
            if ((Key)settings.Default.ConfigOpenKeybind == e.Key)
            {
                // Open the config window if it doesn't exist, otherwise tab into it
                if (!Helpers.IsWindowOpen<ConfigWindow>())
                {
                    ConfigWindow configWindow = new ConfigWindow(Window.GetWindow(this));
                    configWindow.Show();
                }
                else
                {
                    Application.Current.Windows.OfType<ConfigWindow>().First().Activate();
                }
            }
        }

        public void LoadWheelPaths()
        {
            // Get rid of existing wheel before redrawing
            WheelCanvas.Children.Clear();

            // Don't start drawing if there's nothing to draw
            if (WheelManager.CurrentWheel == null)
            {
                return;
            }

            // This window
            Window window = Window.GetWindow(this);

            // number of slices
            int nbsOfSlices = 9; //WheelManager.CurrentWheel.Main.Count();

            // Background color
            SolidColorBrush bgColor = new SolidColorBrush(Color.FromArgb(180, 30, 30, 30));

            // Center of screen x
            double cx = window.ActualWidth / 2;

            // Center of screen y
            double cy = window.ActualHeight / 2;

            // Outer slice radius
            double outerRadius = cy/2;

            // Inner slice radius
            double innerRadius = cy/8;

            // radius of cancel circle
            double ellipseRadius = innerRadius * 0.8;

            // Margin in degrees
            double margin = 5;

            // Amount of degrees between each slice
            double sweep = 360 / nbsOfSlices;

            // Starting position in the wheel for y, -90 degrees is the top of the wheel
            double startingAngle = -90;

            // Path holding the pathGeometry
            System.Windows.Shapes.Path path = new System.Windows.Shapes.Path();

            PathGeometry wheel = new PathGeometry(new List<PathFigure>());

            for (int i = 0; i < nbsOfSlices; i++)
            {
                PathFigure slice = new PathFigure(new Point(cx + outerRadius * Math.Cos(Helpers.DegreesToRadian(startingAngle + sweep * i + margin/ Math.PI)), cy + outerRadius * Math.Sin(Helpers.DegreesToRadian(startingAngle + sweep * i + margin/ Math.PI))), new List<PathSegment>()
                {
                    new ArcSegment(new Point(cx + outerRadius * Math.Cos(Helpers.DegreesToRadian(startingAngle + sweep * (i + 1) - margin/Math.PI)), cy + outerRadius * Math.Sin(Helpers.DegreesToRadian(startingAngle + sweep * (i + 1) - margin/Math.PI))), new Size(outerRadius,outerRadius), 0, false, SweepDirection.Clockwise, true),
                    new LineSegment(new Point(cx + innerRadius * Math.Cos(Helpers.DegreesToRadian(startingAngle + sweep * (i + 1) - margin)), cy + innerRadius * Math.Sin(Helpers.DegreesToRadian(startingAngle + sweep * (i + 1) - margin))), true),
                    new ArcSegment(new Point(cx + innerRadius * Math.Cos(Helpers.DegreesToRadian(startingAngle + sweep * i + margin)), cy + innerRadius * Math.Sin(Helpers.DegreesToRadian(startingAngle + sweep * i + margin))), new Size(innerRadius, innerRadius), 0, false, SweepDirection.Counterclockwise, true)
                }, true);
                
                wheel.Figures.Add(slice);
            }

            // Center cancel circle
            Ellipse cancelCircle = new Ellipse
            {
                Width = ellipseRadius * 2,
                Height = ellipseRadius * 2,
                Fill = bgColor,
                Stroke = Brushes.White,
                StrokeThickness = 1
            };

            // Ellipse is positioned by its top left corner, so shift by the radius
            Canvas.SetLeft(cancelCircle, cx - ellipseRadius);
            Canvas.SetTop(cancelCircle, cy - ellipseRadius);

            // wheel settings
            path.Data = wheel;
            path.Fill = bgColor;
            path.StrokeThickness = 1;
            path.Stroke = Brushes.White;

            // Adding both the cancel circle and the wheel to the canvas
            WheelCanvas.Children.Add(cancelCircle);
            WheelCanvas.Children.Add(path);
        }

        public void UnloadWheelPaths()
        {
            // Clear the paths
            WheelCanvas.Children.Clear();
        }
    }
}