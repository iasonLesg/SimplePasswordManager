using Password_Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Local_Password_Manager
{
    /// <summary>
    /// Interaction logic for EditButton.xaml
    /// </summary>
    public partial class EditButton : Window
    {
       
        public bool DeleteEntry=false;
        public string password;
        public string name;

        public EditButton()
        {
            InitializeComponent();
        }
        public EditButton(string PassName,string Password)
        {
            InitializeComponent();
            TxtEditPassword.Text = Password;
            TxtEditSiteName.Text = PassName;
            name = PassName;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            
            this.Close();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (TxtEditSiteName.Text == "")
            {
                CustomMessageBox message = new CustomMessageBox("Cannot Edit Data when Name is empty! Please fill in Name");
                message.ShowDialog();


            }
            else if (TxtEditPassword.Text == "")
            {
                CustomMessageBox message = new CustomMessageBox("Cannot Edit Data when Password is empty! Please fill in Name");
                message.ShowDialog();

            }
            else if (SaveLoad.Dict.ContainsKey(TxtEditSiteName.Text) && name!= TxtEditSiteName.Text)
            {
                CustomMessageBox message = new CustomMessageBox("ERROR: Data already exists with the same Name!");
                message.ShowDialog();

            }
            else
            {
                password = TxtEditPassword.Text;
                name = TxtEditSiteName.Text;
                this.DialogResult = true;
                this.Close();
            }
               
        }

        private void DeleteIcon_Click(object sender, RoutedEventArgs e)
        {
        
            string currentSite = TxtEditSiteName.Text;

            bool confirmDelete = DeleteMessageBox.Show(currentSite);

            if (confirmDelete)
            {

                CustomMessageBox.Show($"{currentSite} was deleted.", "🗑️");
                DeleteEntry = true;
                this.DialogResult = true;
                this.Close();
            }
        }
        private void GeneratePass_Click(object sender, RoutedEventArgs e)
        {
            // Generate a secure 16-character password
            string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*?";
            Random random = new Random();
            char[] chars = new char[16];

            for (int i = 0; i < 16; i++)
            {
                chars[i] = validChars[random.Next(0, validChars.Length)];
            }

            // Set the result in your edit textbox
            TxtEditPassword.Text = new string(chars);
        }

    }
}
