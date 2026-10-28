using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace AutoCommsWheel
{
    /// <summary>
    /// Interaction logic for ConfigWindow.xaml
    /// </summary>
    public partial class ConfigWindow : Window
    {
        // Observable Collection holding the wheels
        ObservableCollection<WheelFile> wheelsCollection = new ObservableCollection<WheelFile>();

        // Record for the ObservableCollection<WheelFile>
        public record WheelFile(string FileName, string FullPath);

        private InputHook _inputHook;

        public ConfigWindow(Window mainWindow, InputHook inputHook)
        {
            InitializeComponent();
            _inputHook = inputHook;

            InitializeWheelsListBox();
            CheckSavedWheel();
        }

        public void InitializeWheelsListBox()
        {
            // Gives the listbox the Observable Collection
            WheelsListBox.ItemsSource = wheelsCollection;

            // Creates the localAppDir and the wheels folder if they don't exist
            Directory.CreateDirectory(Helpers.Wheels);

            // Adds the files to the observable Collection
            foreach (string f in Directory.GetFiles(Helpers.Wheels, "*.json"))
            {
                wheelsCollection.Add(new WheelFile(Path.GetFileName(f), f));
            }
        }

        private void CheckSavedWheel()
        {
            string? saved = settings.Default.SelectedWheelName;

            // Checks that saved isn't null and if a wheel collection exists for the saved name
            bool exists = !string.IsNullOrEmpty(saved) &&
                          wheelsCollection.Any(wc => string.Equals(wc.FileName, saved, StringComparison.OrdinalIgnoreCase));

            if (!exists || !WheelManager.Load(saved!)) SaveAndWriteSelectedWheel();
        }

        private void ConfigKeyBind_Loaded(object sender, RoutedEventArgs e)
        {
            ConfigKeyBind.Text = (string)new ModifierKeysConverter().ConvertTo((ModifierKeys)settings.Default.ConfigOpenModifiers, "".GetType()) + " " + (string)new KeyConverter().ConvertTo((Key)settings.Default.ConfigOpenKeybind, "".GetType());
        }

        private void ConfigKeyBind_KeyEntered(object sender, KeyEventArgs e)
        {
            // Current modifier keys pressed
            ModifierKeys pressedModifKeys = _inputHook.GetGlobalModifierKeys();

            // Resolve the alt issue. if it's an alt key we set the key to use to SystemKey as alt is in systemKey for some reason. otherwise we leave at base.
            Key key = e.Key == Key.System ? e.SystemKey : e.Key;

            // Means the operation has been handled, we take care of it now.
            e.Handled = true;

            // If any of the keys detected (we mean pressed, not held) are modifiers we don't want to save them yet.
            // We want ot wait for a "real" key and then check the modifier that was held at the same time
            // Checks for (in order) left ctrl, right ctrl, left alt, left alt, right alt, left shift, right shift, left Win, right Win, System key (alt), none, imeProcessed keys. 
            if (key is Key.LeftCtrl or Key.RightCtrl or Key.LeftAlt or Key.RightAlt or Key.LeftShift or Key.RightShift or Key.LWin or Key.RWin or Key.System or Key.None or Key.ImeProcessed)
            {
                return;
            }

            // here we check if any modifiers are already being held, not pressed just now.
            if (pressedModifKeys == ModifierKeys.None)
            {
                MessageBox.Show("Please hold a modifier key with the main key (ctrl, alt, shift)", "Config binding failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            settings.Default.ConfigOpenKeybind = (int)key;
            settings.Default.ConfigOpenModifiers = (int)pressedModifKeys;
            settings.Default.Save();

            ConfigKeyBind.Text = pressedModifKeys.ToString() + " " + key.ToString();

            // Warn the user if only shift is held and that it's a common combo when typing
            if (pressedModifKeys.HasFlag(ModifierKeys.Shift) && (!pressedModifKeys.HasFlag(ModifierKeys.Alt) && !pressedModifKeys.HasFlag(ModifierKeys.Control)))
            {
                MessageBox.Show("This is a common combo when typing, envision adding other control keys (ctrl, alt)", "Config binding Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnImportCommunicationWheel_Click(object sender, RoutedEventArgs e)
        {
            // Displays an OpenFileDialog so the user can select a file.  
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Files|*.json";
            openFileDialog1.Title = "Select a File";

            // Show the Dialog.  
            // If the user clicked OK in the dialog and  
            // a file was selected, open it.  
            if (openFileDialog1.ShowDialog() == true)
            {
                // Tells the user and returns if the json file isn't valid
                if (Helpers.LoadWheelFile(openFileDialog1.FileName) == null)
                {
                    MessageBox.Show("The selected json file isn't a valid wheel", "Import failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                //fileContent = new StreamReader(openFileDialog1.FileName).ReadToEnd();
                string fileName = Path.GetFileName(openFileDialog1.FileName);
                string newFilePath = Path.Combine(Helpers.Wheels, fileName);

                if (string.Equals(openFileDialog1.FileName, newFilePath, StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("The selected wheel already is the imported one", "Import failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                bool alreadyExists = wheelsCollection.Any(wc => string.Equals(wc.FileName, fileName, StringComparison.OrdinalIgnoreCase));

                // Add the new file to the observable collection, if it already exists ask the user whether to replace
                if (alreadyExists)
                {
                    MessageBoxResult result = MessageBox.Show("This wheel already exists, replace it?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.No)
                        return;
                }

                try
                {
                    // Copy the new Wheel to localAppData
                    File.Copy(openFileDialog1.FileName, newFilePath, true);
                }
                catch (Exception)
                {
                    MessageBox.Show("Import failed, file isn't accessible", "Import failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (alreadyExists)
                {
                    // Reload only if the replaced wheel is the active one
                    if (string.Equals(settings.Default.SelectedWheelName, fileName, StringComparison.OrdinalIgnoreCase))
                        WheelManager.Load(fileName);
                }
                else
                {
                    wheelsCollection.Add(new WheelFile(fileName, newFilePath));
                }
            }
        }

        private void BtnDeleteCommunicationWheel_Click(object sender, RoutedEventArgs e)
        {
            if (WheelsListBox.SelectedItem == null) return;

            MessageBoxResult result = MessageBox.Show("Delete this wheel?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                WheelFile wheelFileToDelete = (WheelFile)WheelsListBox.SelectedItem;

                try
                {
                    // delete the file
                    File.Delete(wheelFileToDelete.FullPath);
                }
                catch (Exception)
                {
                    MessageBox.Show("Can't delete the file!", "Delete failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // delete the record from the collection
                wheelsCollection.Remove(wheelFileToDelete);

                if (settings.Default.SelectedWheelName == wheelFileToDelete.FileName)
                {
                    SaveAndWriteSelectedWheel();
                }
            }
        }

        private void BtnSelectCommunicationWheel_Click(object sender, RoutedEventArgs e)
        {
            if (WheelsListBox.SelectedItem == null) return;

            // Get the selected wheel
            WheelFile selectedWheel = (WheelFile)WheelsListBox.SelectedItem;

            // Save the name of the selected wheel to settings
            SaveAndWriteSelectedWheel(selectedWheel.FileName);
        }

        private void BtnDeselectCommunicationWheel_Click(object sender, RoutedEventArgs e)
        {
            SaveAndWriteSelectedWheel();
        }

        private void SelectedWheelTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateSelectedDisplay(settings.Default.SelectedWheelName);
        }

        private void SaveAndWriteSelectedWheel(string? wheelName = null)
        {
            if (wheelName != null)
            {
                if (!WheelManager.Load(wheelName))
                {
                    MessageBox.Show("This wheel couldn't be loaded", "Load failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else
            {
                WheelManager.ClearLoadedWheel();
            }

            // Save value to settings
            settings.Default.SelectedWheelName = wheelName;
            settings.Default.Save();

            UpdateSelectedDisplay(wheelName);
        }

        private void UpdateSelectedDisplay(string? wheelName)
        {
            if (wheelName == null)
            {
                wheelName = "No wheel selected";
                BtnPreviewCommunicationWheel.IsEnabled = false;
            }
            else
            {
                BtnPreviewCommunicationWheel.IsEnabled = true;
            }
                // Write value to the textbox
                SelectedWheelTextBox.Text = wheelName;
        }

        private void BtnPreviewCommunicationWheel_Click(object sender, RoutedEventArgs e)
        {
            // Open the Wheel Preview window if it doesn't exist, otherwise tab into it
            if (!Helpers.IsWindowOpen<WheelPreviewWindow>())
            {
                WheelPreviewWindow configWindow = new WheelPreviewWindow(WheelManager.LoadedWheel);
                configWindow.Show();
            }
            else
            {
                Application.Current.Windows.OfType<WheelPreviewWindow>().First().Activate();
            }
        }
    }
}
