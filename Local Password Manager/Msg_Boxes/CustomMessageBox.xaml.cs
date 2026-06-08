using System.Windows;

namespace Local_Password_Manager
{
    public partial class CustomMessageBox : Window
    {
        public CustomMessageBox(string message, string icon= "⚠️")
        {
            InitializeComponent();

            // Set the UI text to the parameters we passed in
            MessageBlock.Text = message;
            IconBlock.Text = icon;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        /// <summary>
        /// Call this method to display the modern message box anywhere in your app.
        /// </summary>
        /// <param name="message">The text you want to display.</param>
        /// <param name="icon">The emoji/icon you want to display (Defaults to ⚠️).</param>
        public static void Show(string message, string icon = "⚠️")
        {
            // Create and show the window dynamically
            CustomMessageBox msgBox = new CustomMessageBox(message, icon);
            msgBox.ShowDialog();
        }
    }
}