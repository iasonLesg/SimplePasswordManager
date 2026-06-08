using Password_Manager;
using System.Windows;

namespace Local_Password_Manager
{
    public partial class Login : Window
    {
        private int _failedAttempts = 0;
        private const int MaxAttempts = 3;

        public Login()
        {
            InitializeComponent();
            CheckInitialState();
        }

        private void CheckInitialState()
        {
            // Assuming SaveLoad.Initialise() returns TRUE if files exist
            bool isAlreadySetup = SaveLoad.InitialiseDir();

            if (isAlreadySetup) // Existing User
            {
                LoginPanel.Visibility = Visibility.Visible;
                BtnAction.Content = "Login";
                TxtLoginMaster.Focus();
            }
            else // New User
            {
                SetupPanel.Visibility = Visibility.Visible;
                BtnAction.Content = "Create Account";
                TxtCreateMaster.Focus();
            }
        }

        private void Action_Click(object sender, RoutedEventArgs e)
        {
            // Determine which password box is being used
            string password = (SetupPanel.Visibility == Visibility.Visible)
                              ? TxtCreateMaster.Password
                              : TxtLoginMaster.Password;

            if (SetupPanel.Visibility == Visibility.Visible)
            {
                SaveLoad.resetpass(password);
                ProceedToApp();
            }
            else
            {
                // LOGIN LOGIC
                if (SaveLoad.TryDecript(password))
                {
                    ProceedToApp();
                }
                else
                {
                    _failedAttempts++;
                    ErrorMsg.Visibility = Visibility.Visible; // Show the error text

                    if (_failedAttempts >= MaxAttempts)
                    {
                        Application.Current.Shutdown(); // Close after 3 tries
                    }
                }
            }
        }

        private void ProceedToApp()
        {
            MainWindow main = new MainWindow();
            main.Show();
            this.Close();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}