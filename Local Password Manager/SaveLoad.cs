using Local_Password_Manager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;


namespace Password_Manager
{
    internal class SaveLoad
    {
        public static Dictionary<string, string> Dict = new Dictionary<string, string>();

        public static int wrongtries=0;
        private static readonly byte[] salt = Encoding.UTF8.GetBytes("ChangeSaltValue!");
        private static  string Pass = "Testo";

        public static void resetpass(string newpass) {
            Pass = newpass;
            SavePasswordsToJson();

        }

        public class PasswordEntry
        {
            public string SiteName { get; set; }
            public string Password { get; set; }
        }



        public static void UpdateDictionary(string oldName, string newName, string newPassword, bool deleteEntry)
        {

            if (deleteEntry == true)
            {
                Dict.Remove(oldName);
                SavePasswordsToJson();
            }
            else
            {
                if (newName == null || newName == "")
                {

                    CustomMessageBox message = new CustomMessageBox("Something went wrong! Make sure the Name is valid and not empty!");
                    message.ShowDialog();


                }
                else if (newPassword == null || newPassword == "")
                {

                    CustomMessageBox message = new CustomMessageBox("Something went wrong! Make sure the Password is valid and not empty!");
                    message.ShowDialog();

                }
                else
                {

                    if (Dict.ContainsKey(oldName))
                    {
                        if (oldName != newName)
                        {
                            Dict.Remove(oldName);
                        }
                        Dict[newName] = newPassword;
                        SavePasswordsToJson();
                    }
                    else
                    {
                        Dict[newName] = newPassword;
                    }


                }


            }


        }
        private static string GetDirectory()
        {
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "PasswordManager"
            );
            if (!Directory.Exists(appDataPath))
                Directory.CreateDirectory(appDataPath);

            return appDataPath + Path.DirectorySeparatorChar;
        }

        private static string GetFilePath()
        {
            return Path.Combine(GetDirectory(), "Sites.json");
            // return Path.Combine("C:/Users/Bur/Desktop/savefile", "Sites.json"); //debug

        }

        public static bool InitialiseDir()
        {

            string filePath = GetFilePath();
            return File.Exists(filePath);
        }

        public static bool TryDecript(string password)
        {
            Pass = password;
            try
            {

                List<PasswordEntry> a =LoadPasswordsFromJson();
                if (a == null) {
                    return false;
                }
                
                return true;
            }
            catch { return false; }

        }
        public static void SavePasswordsToJson()
        {
            Dictionary<string, string> dict = Dict;
            string password = Pass;
            string filePath = GetFilePath();

            List<PasswordEntry> passwords = new List<PasswordEntry>();

            // Use a foreach loop to iterate through the Dictionary easily
            foreach (var entry in dict)
            {
                PasswordEntry tempEntry = new PasswordEntry
                {
                    SiteName = entry.Key,
                    Password = entry.Value
                };

                passwords.Add(tempEntry);
            }

            // Serialize the list
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(passwords, options);

            // Encrypt and save
            DeriveKeyAndIV(password, out byte[] key, out byte[] iv);
            byte[] encryptedData = EncryptStringToBytes_Aes(jsonString, key, iv);

            File.WriteAllBytes(filePath, encryptedData);
        }


        public static void ExportPasswordsToJson(string directory)
        {
            Dictionary<string, string> dict = Dict;
            string password = Pass;
            string filePath = Path.Combine(directory, "PasswordExport.json");
         
            List<PasswordEntry> passwords = new List<PasswordEntry>();

            // Use a foreach loop to iterate through the Dictionary easily
            foreach (var entry in dict)
            {
                PasswordEntry tempEntry = new PasswordEntry
                {
                    SiteName = entry.Key,
                    Password = entry.Value
                };

                passwords.Add(tempEntry);
            }

            // Serialize the list
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(passwords, options);

            // Encrypt and save
            DeriveKeyAndIV(password, out byte[] key, out byte[] iv);
            byte[] encryptedData = EncryptStringToBytes_Aes(jsonString, key, iv);

            File.WriteAllBytes(filePath, encryptedData);
        }

        public static void UpdatePass()
        {

            Dict.Clear();

            // Make sure SaveLoad exists in your project context
            var entries = SaveLoad.LoadPasswordsFromJson();
            if (entries == null || entries.Count == 0)
                return; // Nothing to load, just continue

            foreach (var entry in entries)
            {
                Dict[entry.SiteName] = entry.Password;
            }



        }

        public static List<PasswordEntry> LoadPasswordsFromJson()
        {
            string filePath = GetFilePath();

            if (!File.Exists(filePath))
            {
                // First launch, nothing to decrypt
                return new List<PasswordEntry>();
            }
            DeriveKeyAndIV(Pass, out byte[] key, out byte[] iv);


            //DeriveKeyAndIV(password, out byte[] key, out byte[] iv);

            try
            {
                byte[] encryptedData = File.ReadAllBytes(filePath);
                string jsonString = DecryptStringFromBytes_Aes(encryptedData, key, iv);
                return JsonSerializer.Deserialize<List<PasswordEntry>>(jsonString);
            }
            catch (Exception)
            {
               
                return null; // Not reached
            }
        }


        public static void ImportPasswordsFromJson(string password, string Directory, bool merge)
        {

            if (merge == false) { Dict.Clear(); }
            string filePath = Directory;

            if (!File.Exists(filePath))
            {
                CustomMessageBox message = new CustomMessageBox("Something went wrong ! Path does not exist.");
                message.ShowDialog();
                goto END;
            }
            DeriveKeyAndIV(password, out byte[] key, out byte[] iv);


            List<PasswordEntry> entries=new List<PasswordEntry>();

            try
            {
                byte[] encryptedData = File.ReadAllBytes(filePath);
                string jsonString = DecryptStringFromBytes_Aes(encryptedData, key, iv);
                entries = JsonSerializer.Deserialize<List<PasswordEntry>>(jsonString);
            }
            catch (Exception)
            {

                CustomMessageBox message = new CustomMessageBox("The password used might be wrong! Please try again.");
                message.ShowDialog();
                goto END;
            }

            int similar = 0;




            // Make sure SaveLoad exists in your project context
            if (entries == null || entries.Count == 0) {
                CustomMessageBox message = new CustomMessageBox("Warning the password table is empty");
                message.ShowDialog();
                goto END;
            }
            foreach (var entry in entries)
            {
                if (Dict.ContainsKey(entry.SiteName))
                {
                    similar++;

                }
                else {
                    Dict[entry.SiteName] = entry.Password;
                }
                  
            }

    

            if (similar > 0) {
                CustomMessageBox message = new CustomMessageBox("There were " +similar+ " Instances of the same entity names, if you want to add them please change the entity names.");
                message.ShowDialog();

            }

        END:
            int iskip = 0;
            SavePasswordsToJson();


        }
        private static void DeriveKeyAndIV(string password, out byte[] key, out byte[] iv)
        {
            using var deriveBytes = new Rfc2898DeriveBytes(password, salt, 10000);
            key = deriveBytes.GetBytes(32); // AES-256
            iv = deriveBytes.GetBytes(16);  // AES block size
        }

        private static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
        {
            using Aes aesAlg = Aes.Create();
            aesAlg.Key = Key;
            aesAlg.IV = IV;

            using var msEncrypt = new MemoryStream();
            using (var csEncrypt = new CryptoStream(msEncrypt, aesAlg.CreateEncryptor(), CryptoStreamMode.Write))
            using (var swEncrypt = new StreamWriter(csEncrypt))
            {
                swEncrypt.Write(plainText);
            }
            return msEncrypt.ToArray();
        }

        private static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            using Aes aesAlg = Aes.Create();
            aesAlg.Key = Key;
            aesAlg.IV = IV;

            using var msDecrypt = new MemoryStream(cipherText);
            using var csDecrypt = new CryptoStream(msDecrypt, aesAlg.CreateDecryptor(), CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);
            return srDecrypt.ReadToEnd();
        }



    }
}
