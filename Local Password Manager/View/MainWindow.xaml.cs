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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Local_Password_Manager
{
    public partial class MainWindow : Window
    {

        // --- GLOBAL HOOK VARIABLES ---
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int VK_LCONTROL = 0xA2;
        private const int VK_RCONTROL = 0xA3;
        private bool togleview = true;
        private LowLevelKeyboardProc _proc;
        private IntPtr _hookID = IntPtr.Zero;

        // --- DOUBLE TAP VARIABLES ---
        private DateTime _lastCtrlPressTime = DateTime.MinValue;
        private readonly TimeSpan _doubleTapThreshold = TimeSpan.FromMilliseconds(400);
        public MainWindow()
        {
            InitializeComponent();
            GetPass();
            _proc = HookCallback;
            _hookID = SetHook(_proc);
        }
        protected override void OnClosed(EventArgs e)
        {
            UnhookWindowsHookEx(_hookID);
            base.OnClosed(e);
        }

        // --- HOOK SETUP ---
        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
      
        // --- KEYSTROKE LISTENER ---
        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            // If a key is pressed down
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                // Read which key was pressed
                int vkCode = Marshal.ReadInt32(lParam);

                // Check if it was Left Ctrl or Right Ctrl
                if (vkCode == VK_LCONTROL || vkCode == VK_RCONTROL)
                {
                    DateTime now = DateTime.Now;

                    if (now - _lastCtrlPressTime < _doubleTapThreshold)
                    {
                        // DOUBLE TAP DETECTED!
                        _lastCtrlPressTime = DateTime.MinValue;

                        // Because this event comes from the OS background thread, 
                        // we MUST use the Dispatcher to safely update the UI thread.
                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            TogleVault();
                        }));
                    }
                    else
                    {
                        // FIRST TAP DETECTED
                        _lastCtrlPressTime = now;
                    }
                }
            }

            // Pass the key press to the next application (don't block the keyboard!)
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        // ... Keep your TogleVault() method exactly as it is here ...

        // --- WIN32 API IMPORTS ---
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    
        private void ToggleVault_Click(object sender, RoutedEventArgs e)
        {
            TogleVault();
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // If the user is holding the key down, ignore it
            if (e.IsRepeat) return;

            // Check if the pressed key is the Control key (Left or Right)
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                DateTime now = DateTime.Now;

                // Check if the time between now and the last press is within our 400ms threshold
                if (now - _lastCtrlPressTime < _doubleTapThreshold)
                {
                    // Double-tap detected! Toggle the vault.
                    TogleVault();

                    // Reset the timer so pressing it a 3rd time doesn't instantly trigger it again
                    _lastCtrlPressTime = DateTime.MinValue;
                }
                else
                {
                    // First tap detected. Record the exact time it happened.
                    _lastCtrlPressTime = now;
                }
            }
        }
        private void TogleVault()
        {
            // If the app is currently visible, hide it and change icon to Locked
            if (MainAppContainer.Visibility == Visibility.Visible)
            {
                MainAppContainer.Visibility = Visibility.Hidden;
                ToggleVaultBtn.Content = "🔒";
            }
            // If the app is hidden, show it and change icon to Unlocked
            else
            {
                // 1. Make the window visible
                MainAppContainer.Visibility = Visibility.Visible;
                ToggleVaultBtn.Content = "🔓";

                // 2. FORCE THE WINDOW TO THE FRONT
                if (this.WindowState == WindowState.Minimized)
                {
                    this.WindowState = WindowState.Normal;
                }

                this.Activate(); // Brings window to the foreground
                this.Focus();    // Requests keyboard focus
                this.Topmost = true; // Briefly bring to top
                this.Topmost = false; // Reset so it doesn't stay permanently on top

                // Ensure that when we open the vault, we are looking at the passwords list
                Back(null, null);
            }
        }

        private void TogleVaultold()
        {
            // If the app is currently visible, hide it and change icon to Locked
            if (MainAppContainer.Visibility == Visibility.Visible)
            {
                MainAppContainer.Visibility = Visibility.Hidden;
                ToggleVaultBtn.Content = "🔒";
            }
            // If the app is hidden, show it and change icon to Unlocked
            else
            {
                MainAppContainer.Visibility = Visibility.Visible;
                ToggleVaultBtn.Content = "🔓";

                // Ensure that when we open the vault, we are looking at the passwords list,
                // just in case we closed the vault while halfway through adding a new password.
                Back(null, null);
            }
        }

        private void GetPass()
        {
            SaveLoad.UpdatePass();
            // Pass the dictionary into the populate method
            //PopulatePasswords(SaveLoad.Dict);
            TogleView();
        }
     
         private void ToggleView_Click(object sender, RoutedEventArgs e)
        {
            TogleView(true);
        }
        private void TogleView(bool Change=false) {
            if (Change)
            {
                if (togleview)
                {
                    togleview = false;
                    PopulatePasswords(SaveLoad.Dict);


                }
                else
                {
                    PopulatePasswordsList(SaveLoad.Dict);
                    togleview = true;
                }

            }
            else {


                if (togleview)
                {
                    PopulatePasswordsList(SaveLoad.Dict);

                }
                else
                {

                    PopulatePasswords(SaveLoad.Dict);
                }

            }
           



        }

        private void AddPass(object sender, RoutedEventArgs e)
        {
            // Hide the passwords list and show the Add Password form
            PasswordsPanel.Visibility = Visibility.Hidden;
            SearchBox.Visibility = Visibility.Hidden;
            AddPassGrid.Visibility = Visibility.Visible;
        }

        private void Back(object sender, RoutedEventArgs e)
        {
            // Hide the Add Password form and go back to the passwords list
            AddPassGrid.Visibility = Visibility.Hidden;
            PasswordsPanel.Visibility = Visibility.Visible;
            SearchBox.Visibility = Visibility.Visible;

            // Clear the text boxes so they are empty next time
            TxtSiteName.Text = "";
            TxtPassword.Text = "";
        }

        // ==========================================
        // ACTION EVENT HANDLERS
        // ==========================================

        private void Save(object sender, RoutedEventArgs e)
        {
            string siteName = TxtSiteName.Text;
            string password = TxtPassword.Text;

            // Make sure the boxes aren't empty
            if (string.IsNullOrWhiteSpace(siteName) || string.IsNullOrWhiteSpace(password))
            {
                if (string.IsNullOrWhiteSpace(siteName))
                {
                    CustomMessageBox message = new CustomMessageBox("Cannot Edit Data when Name is empty! Please fill in Name");
                    message.ShowDialog();


                }
                else if (string.IsNullOrWhiteSpace(password))
                {
                    CustomMessageBox message = new CustomMessageBox("Cannot Edit Data when Password is empty! Please fill in Name");
                    message.ShowDialog();

                }
                return;
            }
        
            else if (SaveLoad.Dict.ContainsKey(siteName))
            {
                CustomMessageBox message = new CustomMessageBox("ERROR: Data already exists with the same Name!");
                message.ShowDialog();
                return;
            }
            // Add or update the dictionary
            SaveLoad.Dict[siteName] = password;
            SaveLoad.SavePasswordsToJson();
            // Refresh the UI to show the new button
           // PopulatePasswords(SaveLoad.Dict);
            TogleView();
            // Go back to the main screen
            Back(null, null);
        }

        private void GeneratePass(object sender, RoutedEventArgs e)
        {
            // Simple random password generator
            string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*?";
            Random random = new Random();
            char[] chars = new char[16]; // 16 character password

            for (int i = 0; i < 16; i++)
            {
                chars[i] = validChars[random.Next(0, validChars.Length)];
            }

            // Put the generated password directly into the textbox
            TxtPassword.Text = new string(chars);
        }
        private void Settings_Click(object sender, RoutedEventArgs e) { 
        
        Settings settings = new Settings();
            settings.ShowDialog();
            TogleView();
        }

        
        private void PopulatePasswords(Dictionary<string, string> passwordDict)
        {
            BorderContainer.Children.Clear();

            // 1. Sort alphabetically and group by the first letter
            var groupedPasswords = passwordDict
                .OrderBy(kvp => kvp.Key, StringComparer.OrdinalIgnoreCase)
                .GroupBy(kvp => string.IsNullOrWhiteSpace(kvp.Key) ? "#" : kvp.Key.Substring(0, 1).ToUpper());

            // 2. Loop through each Letter Category (e.g., "A", "B")
            foreach (var group in groupedPasswords)
            {
                // --- Create the Letter Header ---
                TextBlock letterHeader = new TextBlock
                {
                    Text = group.Key,
                    Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#4FD1C5"),
                    FontSize = 30,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(10, 20, 0, 10)
                };
                BorderContainer.Children.Add(letterHeader);

                // --- Create a WrapPanel specifically for this letter ---
                WrapPanel wrapPanel = new WrapPanel
                {
                    Orientation = Orientation.Horizontal
                };

                // 3. Loop through the passwords under this letter
                foreach (var entry in group)
                {
                    string siteName = entry.Key;
                    string password = entry.Value;

                    // --- Create the Card Background ---
                    Border card = new Border
                    {
                        Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#1E2A38"),
                        CornerRadius = new CornerRadius(8),
                        Width = 230,
                        Height = 130,
                        Margin = new Thickness(10)
                    };

                    // --- Create the Layout Grid inside the card ---
                    Grid cardGrid = new Grid();
                    cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                    // --- Main Button (Copies Password) ---
                    Button copyBtn = new Button
                    {
                        Content = siteName,
                        Background = Brushes.Transparent,
                        Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#4FD1C5"),
                        FontSize = 20,
                        FontWeight = FontWeights.Bold,
                        BorderThickness = new Thickness(0),
                        Cursor = Cursors.Hand,
                        Tag = password // Hide password here
                    };
                    copyBtn.Click += GeneratedPasswordButton_Click;
                    Grid.SetColumn(copyBtn, 0);

                    // --- Edit Button (Opens EditWindow) ---
                    Button editBtn = new Button
                    {
                        Content = "✏️",
                        Background = Brushes.Transparent,
                        Foreground = Brushes.White,
                        FontSize = 18,
                        BorderThickness = new Thickness(0),
                        Cursor = Cursors.Hand,
                        VerticalAlignment = VerticalAlignment.Top,
                        Margin = new Thickness(0, 5, 5, 0),
                        Tag = siteName // Keep track of WHICH site this edit button belongs to
                    };
                    editBtn.Click += OpenEditWindow_Click;
                    Grid.SetColumn(editBtn, 1);

                    // Piece it all together
                    cardGrid.Children.Add(copyBtn);
                    cardGrid.Children.Add(editBtn);
                    card.Child = cardGrid;

                    wrapPanel.Children.Add(card);
                }

                // Add the filled WrapPanel to the main screen
                BorderContainer.Children.Add(wrapPanel);
            }
        }
        private void PopulatePasswordsList(Dictionary<string, string> passwordDict)
        {
            BorderContainer.Children.Clear();

            // 1. Sort and group
            var groupedPasswords = passwordDict
                .OrderBy(kvp => kvp.Key, StringComparer.OrdinalIgnoreCase)
                .GroupBy(kvp => string.IsNullOrWhiteSpace(kvp.Key) ? "#" : kvp.Key.Substring(0, 1).ToUpper());

            foreach (var group in groupedPasswords)
            {
                // Letter Header
                TextBlock letterHeader = new TextBlock
                {
                    Text = group.Key,
                    Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#4FD1C5"),
                    FontSize = 24,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(10, 20, 0, 10)
                };
                BorderContainer.Children.Add(letterHeader);

                // --- Changed to StackPanel for vertical list behavior ---
                StackPanel listContainer = new StackPanel
                {
                    Orientation = Orientation.Vertical
                };

                foreach (var entry in group)
                {
                    string siteName = entry.Key;
                    string password = entry.Value;

                    // --- Updated Border for List Style ---
                    Border listItem = new Border
                    {
                        Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#1E2A38"),
                        CornerRadius = new CornerRadius(4),
                        Height = 50, // Smaller height for a list row
                        Margin = new Thickness(10, 2, 10, 2), // Vertical gap between rows
                        Padding = new Thickness(10, 0, 10, 0)
                    };

                    Grid cardGrid = new Grid();
                    cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                    // Main Button (Site Name)
                    Button copyBtn = new Button
                    {
                        Content = siteName,
                        Background = Brushes.Transparent,
                        Foreground = Brushes.White,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        FontSize = 16,
                        BorderThickness = new Thickness(0),
                        Cursor = Cursors.Hand,
                        Tag = password
                    };
                    copyBtn.Click += GeneratedPasswordButton_Click;
                    Grid.SetColumn(copyBtn, 0);

                    // Edit Button
                    Button editBtn = new Button
                    {
                        Content = "✏️",
                        Background = Brushes.Transparent,
                        Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#4FD1C5"),
                        FontSize = 16,
                        BorderThickness = new Thickness(0),
                        Cursor = Cursors.Hand,
                        Tag = siteName
                    };
                    editBtn.Click += OpenEditWindow_Click;
                    Grid.SetColumn(editBtn, 1);

                    cardGrid.Children.Add(copyBtn);
                    cardGrid.Children.Add(editBtn);
                    listItem.Child = cardGrid;

                    listContainer.Children.Add(listItem);
                }

                BorderContainer.Children.Add(listContainer);
            }
        }
        private void SearchTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchTxt.Text.ToLower();

            // Loop through the StackPanel (which now contains TextBlocks and WrapPanels)
            for (int i = 0; i < BorderContainer.Children.Count; i++)
            {
                if (BorderContainer.Children[i] is WrapPanel wrapPanel)
                {
                    bool hasVisibleCards = false;

                    // Loop through the generated Cards inside the WrapPanel
                    foreach (UIElement element in wrapPanel.Children)
                    {
                        if (element is Border card)
                        {
                            // Dig into the card to find the main Copy button text
                            Grid cardGrid = card.Child as Grid;
                            Button copyBtn = cardGrid.Children[0] as Button;
                            string buttonText = copyBtn.Content.ToString().ToLower();

                            if (buttonText.Contains(searchText))
                            {
                                card.Visibility = Visibility.Visible;
                                hasVisibleCards = true;
                            }
                            else
                            {
                                card.Visibility = Visibility.Collapsed;
                            }
                        }
                    }

                    // Hide the entire WrapPanel if all its cards are hidden by the search
                    wrapPanel.Visibility = hasVisibleCards ? Visibility.Visible : Visibility.Collapsed;

                    // Also hide the Letter Header above it
                    if (i > 0 && BorderContainer.Children[i - 1] is TextBlock header)
                    {
                        header.Visibility = wrapPanel.Visibility;
                    }
                }
            }
        }



        private async void GeneratedPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button clickedButton)
            {
                // 1. Get the password and copy it
                string hiddenPassword = clickedButton.Tag.ToString();
                Clipboard.SetText(hiddenPassword);

                // 2. Show the floating toast notification
                SaveBorder.Visibility = Visibility.Visible;

                // 3. Hide the main vault UI
                TogleVault();

        
                await Task.Delay(2000);

                // 5. Hide the toast notification
                SaveBorder.Visibility = Visibility.Hidden;
            }
        }

        private void OpenEditWindow_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button clickedButton)
            {
                string siteName = clickedButton.Tag.ToString();

                // Create and show your Edit Button window
                // Ensure you have a Window created in your project named "EditButton"
                EditButton editWindow = new EditButton(siteName, SaveLoad.Dict[siteName]);
                editWindow.ShowDialog();
                if (editWindow.DialogResult==true) {

                    SaveLoad.UpdateDictionary(siteName, editWindow.name, editWindow.password, editWindow.DeleteEntry);
                    //PopulatePasswords(SaveLoad.Dict);
                    TogleView();
                }
              
            }
        }
        private void DeletePass(object sender, RoutedEventArgs e)
        {
            // Here you can toggle your "Delete Mode" logic
            CustomMessageBox.Show("Delete mode activated! (You can implement the visual changes here)");
        }

    }
}