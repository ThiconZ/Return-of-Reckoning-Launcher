using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace RoRLauncher
{
    internal class Hash
    {
        public static string GetMd5HashFromFile(string fileName)
        {
            System.IO.FileStream fileStream = new System.IO.FileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            System.Security.Cryptography.MD5 mD = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] array = mD.ComputeHash(fileStream);
            fileStream.Close();
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
            for (int i = 0; i < array.Length; i++)
            {
                stringBuilder.Append(array[i].ToString("x2"));
            }
            return stringBuilder.ToString();
        }
    }
}
