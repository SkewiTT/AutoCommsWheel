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
        // The hook for getting mouse button events.
        private InputHook _inputHook;

        public MainWindow()
        {
            InitializeComponent();
            ExtendedWindowStyles.ToTransparentWindow(this);
            _inputHook = new InputHook();
            _inputHook.MiddleDown += HandleMiddleMousePress;
            _inputHook.MiddleUp += HandleMiddleMouseRelease;
            _inputHook.KeyPressed += HandleGlobalKeyPress;
        }

        /// <summary>
        /// From XAML, runs on window load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _inputHook.Install();

            if (!string.IsNullOrEmpty(settings.Default.SelectedWheelName))
            {
                WheelManager.Load(settings.Default.SelectedWheelName);
            }

            // Set up the values before trying to draw the wheel;
            WheelManager.ResetWheelState();
        }

        // Removes the mouse wheel hook before closing the app as it will cause issues otherwise
        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            _inputHook.Uninstall();
        }

        // TODO : Also check for modifier. and on render of config show the modifiers in the keybind field.
        private void HandleGlobalKeyPress(int vkCode)
        {
            ModifierKeys pressedModifierKeys = (ModifierKeys)settings.Default.ConfigOpenModifiers;

            // Convert Windows' key code to the WPF Key you store in settings
            if ((Key)settings.Default.ConfigOpenKeybind == KeyInterop.KeyFromVirtualKey(vkCode) && (Keyboard.Modifiers & (ModifierKeys)settings.Default.ConfigOpenModifiers) == (ModifierKeys)settings.Default.ConfigOpenModifiers)
            {
                Console.WriteLine((ModifierKeys)settings.Default.ConfigOpenModifiers);
                Console.WriteLine(((ModifierKeys)settings.Default.ConfigOpenModifiers).ToString().Split(',').Length);

                // Only queue work when the key matches, so ordinary typing costs nothing. Use invokeAsync to not be in the hook's callback
                Dispatcher.InvokeAsync(OpenConfigWindow);
            }
        }

        private void OpenConfigWindow()
        {
            // Open the config window if it doesn't exist, otherwise tab into it
            if (!Helpers.IsWindowOpen<ConfigWindow>())
            {
                ConfigWindow configWindow = new ConfigWindow(this, _inputHook);
                configWindow.Show();
            }
            else
            {
                Application.Current.Windows.OfType<ConfigWindow>().First().Activate();
            }
        }

        // The app works by opening the wheel with a keybind and while holding that keybind hovering over the wheels as they switch until you reach the end of a branch.
        // So we handle the middle mouse button benig pressed and released for cancelling

        /// <summary>
        /// Handles what to do on middle mouse wheel down, so reset and drawWheel, we use InvokeAsync to run it outside the callback because we dont want to run the wheel code in windows' mouse callback.
        /// </summary>
        private void HandleMiddleMousePress()
        {
            Dispatcher.InvokeAsync(() =>
            {
                // Resets the saved path and spot in the graphic wheel tree to the start. This should already have been done before but we fire this in case
                WheelManager.ResetWheelState();

                // Draws and so opens the wheel
                DrawWheel();
            });
        }

        /// <summary>
        /// Handles what to do on middle mouse wheel up, so stop drawing and reset wheel, we use InvokeAsync to run it outside the callback because we dont want to run the wheel code in windows' mouse callback.
        /// </summary>
        private void HandleMiddleMouseRelease()
        {
            Dispatcher.InvokeAsync(() =>
            {
                // Stop drawing the wheel
                ClearWheel();

                // Reset the saved path and spot in the graphic wheel tree to the start.
                WheelManager.ResetWheelState();
            });
        }

        // TODO: fix hover here, it only runs if the app is focused which it is not especially with the system wide mouse wheel check now.
        // implement using the actual mouse's position and lighting up the right slice instead of simple mouseEnter or mouseLeave.
        public void DrawWheel()
        {
            // Get rid of existing wheel before redrawing
            WheelCanvas.Children.Clear();

            // Don't start drawing if there's nothing to draw
            if (WheelManager.LoadedWheel == null)
            {
                return;
            }

            // This window
            Window window = Window.GetWindow(this);

            // number of slices
            int nbsOfSlices = 9; //WheelManager.CurrentNodeList.Count(); 

            // Background color
            SolidColorBrush bgColor = new SolidColorBrush(Color.FromArgb(180, 30, 30, 30));

            // Hover Color
            SolidColorBrush hoverColor = new SolidColorBrush(Color.FromArgb(180, 20, 20, 20));

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
            double sweep = 360.0 / nbsOfSlices;

            // Starting position in the wheel for y, -90 degrees is the top of the wheel
            double startingAngle = -90;

            // List of slices
            List<System.Windows.Shapes.Path> paths = new List<System.Windows.Shapes.Path>();

            for (int i = 0; i < nbsOfSlices; i++)
            {
                // Path holding the pathGeometry
                System.Windows.Shapes.Path path = new System.Windows.Shapes.Path();

                // Path geometry holding the pathfigures
                PathGeometry wheel = new PathGeometry(new List<PathFigure>());

                PathFigure slice = new PathFigure(new Point(cx + outerRadius * Math.Cos(Helpers.DegreesToRadian(startingAngle + sweep * i + margin/ Math.PI)), cy + outerRadius * Math.Sin(Helpers.DegreesToRadian(startingAngle + sweep * i + margin/ Math.PI))), new List<PathSegment>()
                {
                    new ArcSegment(new Point(cx + outerRadius * Math.Cos(Helpers.DegreesToRadian(startingAngle + sweep * (i + 1) - margin/Math.PI)), cy + outerRadius * Math.Sin(Helpers.DegreesToRadian(startingAngle + sweep * (i + 1) - margin/Math.PI))), new Size(outerRadius,outerRadius), 0, false, SweepDirection.Clockwise, true),
                    new LineSegment(new Point(cx + innerRadius * Math.Cos(Helpers.DegreesToRadian(startingAngle + sweep * (i + 1) - margin)), cy + innerRadius * Math.Sin(Helpers.DegreesToRadian(startingAngle + sweep * (i + 1) - margin))), true),
                    new ArcSegment(new Point(cx + innerRadius * Math.Cos(Helpers.DegreesToRadian(startingAngle + sweep * i + margin)), cy + innerRadius * Math.Sin(Helpers.DegreesToRadian(startingAngle + sweep * i + margin))), new Size(innerRadius, innerRadius), 0, false, SweepDirection.Counterclockwise, true)
                }, true);
                
                // Add the figure to the wheel
                wheel.Figures.Add(slice);

                // wheel settings
                path.Data = wheel;
                path.Fill = bgColor;
                path.StrokeThickness = 1;
                path.Stroke = Brushes.White;
                path.MouseEnter += (s, e) => path.Fill = hoverColor;
                path.MouseLeave += (s, e) => path.Fill = bgColor;

                // Add the slice to the list of slices so the wheel
                paths.Add(path);
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

            // Adding both the cancel circle and the wheel slices to the canvas
            WheelCanvas.Children.Add(cancelCircle);
            paths.ForEach(path => WheelCanvas.Children.Add(path));

            // Because we drew the wheel, it is now open.
            WheelManager.IsWheelOpen = true;
        }

        public void ClearWheel()
        {
            // Clear the paths
            WheelCanvas.Children.Clear();
        }
    }
}