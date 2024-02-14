using System;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Encoder
{
    public partial class Form1 : Form
    {
        private Point pos;
        private bool dragging;
        string line = "";
        byte[] bytes;
        bool flagCaesar;
        bool flagVegener;
        const string alphabetRus = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ";
        const string alphabetEng = "ABCDEFGHIJKLMNOPQRSTYVWXYZ";
        string password = "";
        Bitmap picture;
        int i = 0;

        public Form1()
        {
            InitializeComponent();

            background.MouseDown += MouseClickDown;
            background.MouseUp += MouseClickUp;
            background.MouseMove += MouseClickMove;

            desk.Visible = false;

            Turtle.Visible = false;

            Task.Run(() =>
            {
                while (true)
                {
                    if (i > 7)
                    {
                        i /= 8;
                    }
                    switch (i)
                    {
                        case 0:
                            picture = Properties.Resources.Tur1;
                            break;
                        case 1:
                            picture = Properties.Resources.Tur2;
                            break;
                        case 2:
                            picture = Properties.Resources.Tur3;
                            break;
                        case 3:
                            picture = Properties.Resources.Tur4;
                            break;
                        case 4:
                            picture = Properties.Resources.Tur5;
                            break;
                        case 5:
                            picture = Properties.Resources.Tur6;
                            break;
                        case 6:
                            picture = Properties.Resources.Tur7;
                            break;
                        case 7:
                            picture = Properties.Resources.Tur8;
                            break;
                    }
                    i++;

                    Turtle.Image = picture;
                    System.Threading.Thread.Sleep(80);
                }
            });
        }

        private void encodingButton_Click(object sender, EventArgs e)
        {
            desk.Visible = true;
            caesarVar.Visible = false;
            caesarKey.Visible = false;
            caesarEncBut.Visible = false;
            labelKey.Visible = false;
            rusButton.Visible = false;
            engButton.Visible = false;
            back.Visible = false;
            vegenerBack.Visible = false;
            vegenerRus.Visible = false;
            vegenerEncBut.Visible = false;
            vegenerEng.Visible = false;
            vegenerKey.Visible = false;
            vegenerText.Visible = false;
            vegenerVar.Visible = false;
            xorEncBut.Visible = false;
            xorPassword.Visible = false;
            xorText.Visible = false;
            xorVar.Visible = false;
            scytaleText.Visible = false;
            scytaleEncBut.Visible = false;
            scytaleKey.Visible = false;
            scytaleVar.Visible = false;
            caesarKey.Text = null;
            vegenerKey.Text = null;
            xorText.Text = null;
            scytaleText.Text = null;
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            desk.Visible = false;
        }

        private void MouseClickDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            pos.X = e.X;
            pos.Y = e.Y;
        }
        private void MouseClickUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }
        private void MouseClickMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point currentPoint = PointToScreen(new Point(e.X, e.Y));
                this.Location = new Point(currentPoint.X - pos.X, currentPoint.Y - pos.Y + background.Top);
            }
        }

        private void ASCII_Encoding(string text)
        {
            Encoding ascii = Encoding.ASCII;

            bytes = ascii.GetBytes(text);
            foreach (byte byt in bytes)
            {
                encodedText.Text += "" + byt + " ";
            }
        }

        private void BinaryCode(string text)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach(byte byt in Encoding.Unicode.GetBytes(text))
            {
                stringBuilder.Append(Convert.ToString(byt, 2).PadLeft(2, ' '));
            } 
            line = "0" + stringBuilder.ToString();
            for(int i = 0; i < line.Length - 2; i++)
            {
                encodedText.Text += line[i];
            }
        }

        private void HexadecimalCode(string text)
        {
            string str = BitConverter.ToString(Encoding.Unicode.GetBytes(text)).Replace("00", "").Replace("-", "");
            encodedText.Text = str.ToLower();
        }

        private void Cipher_Caesar(string text)
        {
            var fullAlphabet = "";
            if (flagCaesar)
            {
                fullAlphabet = alphabetRus.ToLower();
            }
            else
            {
                fullAlphabet = alphabetEng.ToLower();
            }

            try
            {
                int key = Convert.ToInt32(caesarKey.Text);       
                var sizeAlphabet = fullAlphabet.Length;
                var retVal = "";
                text = text.ToLower();
                for(int i = 0; i < text.Length; i++)
                {
                    var c = text[i];
                    var index = fullAlphabet.IndexOf(c);
                    if(index < 0)
                    {
                        retVal += c.ToString();
                    }
                    else
                    {
                        var codeIndex = (sizeAlphabet + index + key) % sizeAlphabet;
                        retVal += fullAlphabet[codeIndex];
                    }
                }
                encodedText.Text = retVal.ToLower();
            }
            catch (Exception ex)
            {
                encodedText.Text = ex.Message;
            }
        }

        private void Cipher_XOR(string text, string secretKey)
        {
            var currentKey = GetRepeatKey(secretKey, text.Length);
            var res = string.Empty;
            for(int i = 0; i < text.Length; i++)
            {
                res += ((char)(text[i] ^ currentKey[i])).ToString();
            }

            encodedText.Text = res;
        }

        private void Cipher_Scytale(string text, int diament)
        {
            var key = text.Length % diament;
            if (key > 0)
            {
                text += new string(' ', diament - key);
            }

            var column = text.Length / diament;
            var result = "";

            for (int i = 0; i < column; i++)
            {
                for (int j = 0; j < diament; j++)
                {
                    result += text[i + column * j].ToString();
                }
            }

            encodedText.Text = result;
        }

        private void Cipher_AES(string text)
        {
            using (Aes myAes = Aes.Create())
            {
                try
                {
                    byte[] encrypted = EncryptStringToBytes_Aes(text, myAes.Key, myAes.IV);
                    //string decrypted = DecryptStringFromBytes_Aes(encrypted, myAes.Key, myAes.IV);

                    encodedText.Text = BitConverter.ToString(encrypted);
                    //encodedText.Text += decrypted;
                }
                catch
                {
                    encodedText.Text = "Входная строка имела неверный формат.";
                }
            }
        }

        static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
        {
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException(nameof(plainText));
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException(nameof(Key));
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException(nameof(IV));

            byte[] encrypted;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            return encrypted;
        }

        static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException(nameof(cipherText));
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException(nameof(Key));
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException(nameof(IV));

            string plaintext = null;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }

        private void Cipher_Atbash(string text)
        {
            string alphabet = alphabetEng.ToLower();
            string symbols = Reverse(alphabet);
            var outputText = string.Empty;
            for(int i = 0; i < text.Length; i++)
            {
                var index = alphabet.IndexOf(text[i]);
                if(index >= 0)
                {
                    outputText += symbols[index].ToString();
                }
            }

            encodedText.Text = outputText;
        }

        private string Reverse(string alphabet)
        {
            var reversedText = string.Empty;
            for(int i =  alphabet.Length - 1; i >= 0; i--)
            {
                reversedText += alphabet[i];
            }

            return reversedText;
        }

        private void Cipher_Vigenere(string text, string password)
        {
            var gamma = GetRepeatKey(password, text.Length);
            var retValue = "";
            int letterIndex;
            int codeIndex;
            int q;
            for (int i = 0; i < text.Length; i++)
            {
                if (flagVegener)
                {
                    letterIndex = alphabetRus.IndexOf(text[i]);
                    codeIndex = alphabetRus.IndexOf(gamma[i]);
                    q = alphabetRus.Length;
                }
                else
                {
                    letterIndex = alphabetEng.IndexOf(text[i]);
                    codeIndex = alphabetEng.IndexOf(gamma[i]);
                    q = alphabetEng.Length;
                }

                if (letterIndex < 0)
                {
                    retValue += text[i].ToString();
                }
                else if(flagVegener)
                {
                    retValue += alphabetRus[(q + letterIndex + codeIndex) % q].ToString();
                }
                else
                {
                    retValue += alphabetEng[(q + letterIndex + codeIndex) % q].ToString();
                }
            }

            encodedText.Text = retValue.ToLower();
        }

        private string GetRepeatKey(string s, int n)
        {
            var p = s;
            while(p.Length < n)
            {
                p += p;
            }

            return p.Substring(0, n);
        }

        private void ascButton_Click(object sender, EventArgs e)
        {
            encodedText.Text = "";
            line = text.Text;
            ASCII_Encoding(line);
        }

        private void binButton_Click(object sender, EventArgs e)
        {
            encodedText.Text = "";
            line = text.Text;
            BinaryCode(line);
        }

        private void hexaButton_Click(object sender, EventArgs e)
        {
            encodedText.Text = "";
            line = text.Text;
            HexadecimalCode(line);
        }

        private void caesarButton_Click(object sender, EventArgs e)
        {
            rusButton.Visible = true;
            engButton.Visible = true;
            caesarVar.Visible = true;
        }
        private void Cipher_Caesar_But(object sender, EventArgs e)
        {
            encodedText.Text = "";
            line = text.Text;
            Cipher_Caesar(line);
        }

        private void XORButton_Click(object sender, EventArgs e)
        {
            xorEncBut.Visible = true;
            xorPassword.Visible = true;
            xorText.Visible = true;
            xorVar.Visible = true;
        }

        private void ScytaleButton_Click(object sender, EventArgs e)
        {
            scytaleText.Visible = true;
            scytaleEncBut.Visible = true;
            scytaleKey.Visible = true;
            scytaleVar.Visible = true;
        }

        private void aesButton_Click(object sender, EventArgs e)
        {
            encodedText.Text = "";
            line = text.Text;
            Cipher_AES(line);
        }

        private void atbashButton_Click(object sender, EventArgs e)
        {
            encodedText.Text = "";
            line = text.Text.ToLower();
            Cipher_Atbash(line);
        }

        private void vigenereButton_Click(object sender, EventArgs e)
        {
            vegenerVar.Visible = true;
            vegenerEng.Visible = true;
            vegenerRus.Visible = true;
        }

        private void escapeButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void rusButton_Click(object sender, EventArgs e)
        {
            flagCaesar = true;
            caesarKey.Visible = true;
            caesarEncBut.Visible = true;
            labelKey.Visible = true;
            engButton.Visible = false;
            rusButton.Visible = false;
            back.Visible = true;
        }

        private void engButton_Click(object sender, EventArgs e)
        {
            flagCaesar = false;
            caesarKey.Visible = true;
            caesarEncBut.Visible = true;
            labelKey.Visible = true;
            engButton.Visible = false;
            rusButton.Visible = false;
            back.Visible = true;
        }

        private void back_Click(object sender, EventArgs e)
        {
            engButton.Visible = true;
            rusButton.Visible = true;
            caesarKey.Visible = false;
            caesarEncBut.Visible = false;
            labelKey.Visible = false;
            back.Visible = false;
        }

        private void vegenerRus_Click(object sender, EventArgs e)
        {
            vegenerText.Visible = true;
            vegenerKey.Visible = true;
            vegenerEncBut.Visible = true;
            vegenerBack.Visible = true;
            vegenerEng.Visible = false;
            vegenerRus.Visible = false;
            flagVegener = true;
        }

        private void vegenerEng_Click(object sender, EventArgs e)
        {
            vegenerText.Visible = true;
            vegenerKey.Visible = true;
            vegenerEncBut.Visible = true;
            vegenerBack.Visible = true;
            vegenerEng.Visible = false;
            vegenerRus.Visible = false;
            flagVegener = false;
        }

        private void vegenerBack_Click(object sender, EventArgs e)
        {
            vegenerVar.Visible = true;
            vegenerEng.Visible = true;
            vegenerRus.Visible = true;
            vegenerKey.Visible = false;
            vegenerText.Visible = false;
            vegenerBack.Visible = false;
            vegenerEncBut.Visible = false;
        }

        private void vegenerEncBut_Click(object sender, EventArgs e)
        {
            encodedText.Text = "";
            line = text.Text.ToUpper();
            password = vegenerKey.Text.ToUpper();
            Cipher_Vigenere(line, password);
        }

        private void xorEncBut_Click(object sender, EventArgs e)
        {
            encodedText.Text = "";
            line = text.Text;
            password = xorText.Text;
            Cipher_XOR(line, password);
        }

        private void scytaleEncBut_Click(object sender, EventArgs e)
        {
            try
            {
                encodedText.Text = "";
                line = text.Text;
                Cipher_Scytale(line, Convert.ToInt32(scytaleText.Text));
            }
            catch(Exception ex)
            {
                encodedText.Text = "Входная строка имела неверный формат.";
            }
        }

        private void turtleButton_Click(object sender, EventArgs e)
        {
            if (i % 2 == 0)
            {
                pictureBox1.Visible = false;
                Turtle.Visible = true;
                
            }
            else
            {
                pictureBox1.Visible = true;
                Turtle.Visible = false;
            }
            i++;
        }
    }
}
