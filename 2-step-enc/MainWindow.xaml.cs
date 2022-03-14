using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Controls;
using System.Security.Cryptography;

namespace _2_step_enc
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // target file to encrypt or decrypt
        static string file_target;
        // file to be created by encrypting or decrypting
        static string file_destination;
        static string file_destination_name;
        // our string which will be used to encrypt
        // our target file
        static string mypassword;
        // custom files-to-be-encrypted extention
        // if changed dont forget to change the
        // file_selector.Filter line
        const string myExtention = ".synx";
        public MainWindow()
        {
            InitializeComponent();
        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
        private void Button_Make_Key(object sender, RoutedEventArgs e)
        {
            byte[] bytesToBeEncrypted = Encoding.UTF8.GetBytes(UserInput.Text);
            // using AES encryption to encrypt 'UserInput.Text'
            // where the secret key will be in the below line
            // change "MyPassword" with your custom secret key
            byte[] passwordBytes = Encoding.UTF8.GetBytes("MyPassword");
            // change "MyPassword" with your custom secret key
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);
            byte[] bytesEncrypted = AES_Encrypt(bytesToBeEncrypted, passwordBytes);
            Output.Text = Convert.ToBase64String(bytesEncrypted);
            Output_Copy.Text = Convert.ToBase64String(bytesEncrypted);
            if(Output_Copy.Text != Output.Text)
            {
                // additianal useless line
                mypassword = "MyPassword";
            }
            else
            {
                // this is our encypted string
                // which will be used to encrypt
                // our target file
                mypassword = Output.Text;
            }
            //Decryption Process of above key maker
            //byte[] bytesToBeDecrypted = Convert.FromBase64String(encryptedResult);
            //byte[] passwordBytesdecrypt = Encoding.UTF8.GetBytes("Password");
            //passwordBytesdecrypt = SHA256.Create().ComputeHash(passwordBytesdecrypt);
            //byte[] bytesDecrypted = AES_Decrypt(bytesToBeDecrypted, passwordBytes);
            //string decryptedResult = Encoding.UTF8.GetString(bytesDecrypted);
        }
        private void Button_Copy(object sender, RoutedEventArgs e)
        {
            // copy file-encryption string to clipboard
            System.Windows.Clipboard.SetText(Output.Text);
        }
        private void Button_Clear(object sender, RoutedEventArgs e)
        {
            // clear everything
            UserInput.Text = "Your string here";
            Output.Text = "";
            Output_Copy.Text = "";
            UserInput_File.Text = "No selected file yet";
            file_path.Text = "No selected file yet";
            string4.Text = "";
            New_File_Name.Text = "";
        }
        public static byte[] AES_Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
        {
            byte[] encryptedBytes = null;
            // salt bytes can also be modified to your custom byte array
            byte[] saltBytes = new byte[] { 6, 9, 4, 2, 0, 7, 1, 1 };
            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;
                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);
                    AES.Mode = CipherMode.CBC;
                    using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                        cs.Close();
                    }
                    encryptedBytes = ms.ToArray();
                }
            }
            return encryptedBytes;
        }
        public static byte[] AES_Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes)
        {
            byte[] decryptedBytes = null;
            // salt bytes can also be modified to your custom byte array
            byte[] saltBytes = new byte[] { 6, 9, 4, 2, 0, 7, 1, 1 };
            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;
                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);
                    AES.Mode = CipherMode.CBC;
                    using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                        cs.Close();
                    }
                    decryptedBytes = ms.ToArray();
                }
            }
            return decryptedBytes;
        }
        // You can also pass your custom secret key to EncryptFile() and custom number of iterations
        public void EncryptFile(string sourceFilename, string destinationFilename, string password = "MyPassword", int iterations = 1042)
        {
            // salt bytes can also be modified to your custom byte array
            byte[] salt = new byte[] { 6, 9, 4, 2, 0, 7, 1, 1 };
            AesManaged aes = new AesManaged();
            aes.BlockSize = aes.LegalBlockSizes[0].MaxSize;
            aes.KeySize = aes.LegalKeySizes[0].MaxSize;
            // NB: Rfc2898DeriveBytes initialization and subsequent calls to GetBytes must be eactly the same, including order, on both the encryption and decryption sides.
            Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(password, salt, iterations);
            aes.Key = key.GetBytes(aes.KeySize / 8);
            aes.IV = key.GetBytes(aes.BlockSize / 8);
            aes.Mode = CipherMode.CBC;
            ICryptoTransform transform = aes.CreateEncryptor(aes.Key, aes.IV);

            using (FileStream destination = new FileStream(destinationFilename, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                using (CryptoStream cryptoStream = new CryptoStream(destination, transform, CryptoStreamMode.Write))
                {
                    using (FileStream source = new FileStream(sourceFilename, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        source.CopyTo(cryptoStream);
                    }
                }
            }
        }
        // You can also pass your custom secret key to DecryptFile() and custom number of iterations
        public void DecryptFile(string sourceFilename, string destinationFilename, string password = "MyPassword", int iterations = 1042)
        {
            byte[] salt = new byte[] { 6, 9, 4, 2, 0, 7, 1, 1 };
            AesManaged aes = new AesManaged();
            aes.BlockSize = aes.LegalBlockSizes[0].MaxSize;
            aes.KeySize = aes.LegalKeySizes[0].MaxSize;
            // NB: Rfc2898DeriveBytes initialization and subsequent calls to   GetBytes   must be eactly the same, including order, on both the encryption and decryption sides.
            Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(password, salt, iterations);
            aes.Key = key.GetBytes(aes.KeySize / 8);
            aes.IV = key.GetBytes(aes.BlockSize / 8);
            aes.Mode = CipherMode.CBC;
            ICryptoTransform transform = aes.CreateDecryptor(aes.Key, aes.IV);

            using (FileStream destination = new FileStream(destinationFilename, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                using (CryptoStream cryptoStream = new CryptoStream(destination, transform, CryptoStreamMode.Write))
                {
                    try
                    {
                        using (FileStream source = new FileStream(sourceFilename, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            source.CopyTo(cryptoStream);
                        }
                    }
                    catch (CryptographicException exception)
                    {
                        if (exception.Message == "Padding is invalid and cannot be removed.")
                            throw new ApplicationException("Universal Microsoft Cryptographic Exception (Not to be believed!)", exception);
                        else
                            throw;
                    }
                }
            }
        }
        private void Button_Pick_File(object sender, RoutedEventArgs e)
        {
            OpenFileDialog file_selector = new OpenFileDialog();
            file_selector.Title = "Select your file";
            file_selector.Filter = "All Files (*.*)|*.*|Synx File(*.synx)|*.synx";
            file_selector.FilterIndex = 1;
            file_selector.ShowDialog();
            if(file_selector.FileName != "")
            {
                string[] file = file_selector.FileName.Split('\\');
                file_target = file_selector.FileName;
                string file_name = file[file.Length - 1];
                UserInput_File.Text = "File : " + file_name;
                file_path.Text = file_selector.FileName;
                if (file_name.Contains(myExtention))
                {
                    string temp_file = file_name;
                    int fileExtPos = temp_file.LastIndexOf(".");
                    if (fileExtPos >= 0)
                        temp_file = temp_file.Substring(0, fileExtPos);
                    New_File_Name.Text = temp_file;
                    file[file.Length - 1] = temp_file;
                    file_destination_name = temp_file;
                    file_destination = string.Join("\\", file);
                }
                else
                {
                    file_destination = file_target + myExtention;
                    New_File_Name.Text = file_name + myExtention;
                }
                
            } else {
                UserInput_File.Text = "No selected file yet";
                file_path.Text = "No selected file yet";
            }
        }
        private void Button_Decrypt_File(object sender, RoutedEventArgs e)
        {
            try
            {
                DecryptFile(file_target, file_destination, mypassword);
                New_File_Name.Text = "Decrypted !";
            }
            catch(Exception excep)
            {
                New_File_Name.Text = "Error : " + excep;
            }
        }
        private void Button_Encrypt_File(object sender, RoutedEventArgs e)
        {
            try
            {
                EncryptFile(file_target, file_destination, mypassword);
                New_File_Name.Text += "   |   done!";
            }
            catch(Exception excep)
            {
                New_File_Name.Text = "Error : " + excep;
            }
        }
        private void UserInput_Copy_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
        private void close_button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}
