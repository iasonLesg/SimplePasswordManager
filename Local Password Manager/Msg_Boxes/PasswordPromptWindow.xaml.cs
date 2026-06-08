using System.Windows;

namespace Local_Password_Manager
{
    public partial class PasswordPromptWindow : Window
    {
       
        public string Password="";
       
        public PasswordPromptWindow(string StringShow= "Unlock Data File")
        {


            InitializeComponent();
            LBL_PASS.Text = StringShow;
            this.Loaded += (s, e) => FilePasswordBox.Focus();
        }

        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            Password = FilePasswordBox.Password;

            if (string.IsNullOrWhiteSpace(Password))
            {
              
                CustomMessageBox message = new CustomMessageBox("Please enter a password.");
                message.ShowDialog();
                return;
            }

            // Call your actual logic here using the path and the password
            // ProcessImport(_filePath, password, _isMerge);

            // MessageBox.Show($"File ready for processing!\nPath: {_filePath}\nMerge Mode: {_isMerge}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            this.DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}