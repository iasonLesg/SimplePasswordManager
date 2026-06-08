using Microsoft.Win32; // Required for OpenFileDialog and SaveFileDialog
using Password_Manager;
using System;
using System.Windows;

namespace Local_Password_Manager
{
    public partial class Settings : Window
    {
      
        public Settings()
        {
            InitializeComponent();
        }

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
         
            
           PasswordPromptWindow prompt = new PasswordPromptWindow("Input Current Password");
            prompt.ShowDialog();
           
            if (prompt.DialogResult==true)
            {
                if (SaveLoad.TryDecript(prompt.Password))
                {
                    PasswordPromptWindow prompt2 = new PasswordPromptWindow("Input New Password");
                    prompt2.ShowDialog();
                    if (prompt2.DialogResult == true)
                    {
                        SaveLoad.resetpass(prompt2.Password);
                    }
                }
                else
                {

                 
                    if (SaveLoad.wrongtries > 4)
                    {
                        CustomMessageBox message = new CustomMessageBox("Too Many tries! Application will shut down!");
                        message.ShowDialog();
                        Application.Current.Shutdown();
                    }
                    else {
                        CustomMessageBox message = new CustomMessageBox("Wrong password! Please try again.");
                        message.ShowDialog();
                        SaveLoad.wrongtries++;
                    }


                }

            }


        }

        private void ExportData_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new Microsoft.Win32.OpenFolderDialog
            {
                Title = "Select a folder to save your exported vault"
            };

            if (folderDialog.ShowDialog() == true)
            {
                string selectedFolder = folderDialog.FolderName; 

           
                SaveLoad.ExportPasswordsToJson(selectedFolder);
                CustomMessageBox message = new CustomMessageBox("Success saving the file", " ✓ ");
                message.ShowDialog();
              
            }
        

           
        }

        private void ImportData_Click(object sender, RoutedEventArgs e)
        {
            TriggerImportFlow(isMerge: false);
        }

        private void ImportDataMerge_Click(object sender, RoutedEventArgs e)
        {
            TriggerImportFlow(isMerge: true);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // Closes the settings window
        }

        // Shared function to handle the import process
        private void TriggerImportFlow(bool isMerge)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = isMerge ? "Select Data to Merge" : "Select Data to Import",
                Filter = "Encrypted Vault Files (*.json)|*.json|All Files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;

                PasswordPromptWindow prompt = new PasswordPromptWindow();
                prompt.ShowDialog();


                SaveLoad.ImportPasswordsFromJson(prompt.Password, filePath, isMerge);

            }
        }
    }
}