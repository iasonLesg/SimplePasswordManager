using System.Windows;

namespace Local_Password_Manager
{
    public partial class DeleteMessageBox : Window
    {
        public DeleteMessageBox(string siteNameToDelete)
        {
            InitializeComponent();

            // Format the text dynamically using the input
            MessageBlock.Text = $"Are you sure you want to delete \"{siteNameToDelete}\"?\n\nThis action cannot be undone.";
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            // Close the window and return false
            this.DialogResult = false;
            this.Close();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            // Close the window and return true
            this.DialogResult = true;
            this.Close();
        }

        /// <summary>
        /// Shows the delete confirmation box and returns true if the user clicks Delete.
        /// </summary>
        public static bool Show(string siteName)
        {
            DeleteMessageBox msgBox = new DeleteMessageBox(siteName);

            // ShowDialog() automatically matches the this.DialogResult we set in the click events!
            bool? result = msgBox.ShowDialog();

            return result == true;
        }
    }
}