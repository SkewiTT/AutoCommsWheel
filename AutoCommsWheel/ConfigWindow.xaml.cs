using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
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

namespace AutoCommsWheel
{
    /// <summary>
    /// Interaction logic for ConfigWindow.xaml
    /// </summary>
    public partial class ConfigWindow : Window
    {
        private const string AppFolderName = "AutoCommsWheel";
        public static string Root => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppFolderName);
        public static string Wheels => Path.Combine(Root, "Wheels");

        // Observable Collection holding the wheels
        ObservableCollection<WheelFile> wheelsCollection = new ObservableCollection<WheelFile>();

        // Record for the ObservableCollection<WheelFile>
        public record WheelFile(string FileName, string FullPath);

        public ConfigWindow()
        {
            InitializeComponent();
            InitializeWheelsListBox();
        }

        public void InitializeWheelsListBox()
        {
            // Gives the listbox the Observable Collection
            WheelsListBox.ItemsSource = wheelsCollection;

            // Creates the localAppDir and the wheels folder if they don't exist
            Directory.CreateDirectory(Wheels);

            // Adds the files to the observable Collection
            foreach (string f in Directory.GetFiles(Wheels, "*.json"))
            {
                wheelsCollection.Add(new WheelFile(Path.GetFileName(f), f));
            }
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

                // Add the new file to the observable collection, if it already exists ask the user whether to replace
                if (wheelsCollection.Any(wc => wc.FileName == fileName))
                {
                    MessageBoxResult result = MessageBox.Show("This wheel already exists, replace it?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        // replace it by removing the old one first
                        wheelsCollection.Remove(wheelsCollection.First(wc => wc.FileName == fileName));
                    }
                    else
                    {
                        return;
                    }
                }

                // Copy the new Wheel to localAppData
                string newFilePath = Path.Combine(Wheels, fileName);
                File.Copy(openFileDialog1.FileName, newFilePath, true);

                // Add the wheel to the collection
                wheelsCollection.Add(new WheelFile(fileName, newFilePath));
            }
        }

        private void BtnDeleteCommunicationWheel_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Delete this wheel?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                if (WheelsListBox.SelectedItems == null) return;

                WheelFile wheelFileToDelete = wheelsCollection.First(wc => wc == (WheelFile)WheelsListBox.SelectedItem);

                // delete the record from the collection
                wheelsCollection.Remove(wheelFileToDelete);

                // delete the file
                File.Delete(wheelFileToDelete.FullPath);
            }
        }
    }
}
