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

        public ConfigWindow()
        {
            InitializeComponent();
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
            if (!wheelsCollection.Any(wc => wc.FileName == settings.Default.SelectedWheelName)) SaveAndWriteSelectedWheel();
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
                if (Helpers.IsJsonValid(openFileDialog1.FileName) == false)
                {
                    MessageBox.Show("The selected json file isn't valid", "Confirm", MessageBoxButton.OK, MessageBoxImage.Error);
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

                if (!alreadyExists)
                {
                    // Only Add the wheel to the collection if this newly imported wheel doesn't exist yet
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
            }
            // Write value to the textbox
            SelectedWheelTextBox.Text = wheelName;
        }
    }
}
