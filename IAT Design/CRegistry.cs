/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Security.Cryptography;
using System.IO;

namespace IATClient
{
    class CRegistry
    {
        private static RegistryKey GetBaseKey()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true);
            key = key.OpenSubKey("IATSoftware", true);
            return key.OpenSubKey("IATClient", true);
        }

        private static void AddEncryptedValue(String value, String subKeyName)
        {
            RegistryKey key = GetBaseKey();
            RegistryKey subKey = key.CreateSubKey(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(subKeyName)));
            byte[] DES = new byte[8];
            byte[] IV = new byte[8];
            Random rand = new Random();
            rand.NextBytes(DES);
            rand.NextBytes(IV);
            byte []valueKey = new byte[16];
            Array.Copy(DES, valueKey, 8);
            Array.Copy(IV, 0, valueKey, 8, 8);
            subKey.SetValue("Key", Convert.ToBase64String(valueKey), RegistryValueKind.String);
            DESCryptoServiceProvider desCrypt = new DESCryptoServiceProvider();
            MemoryStream memStream = new MemoryStream();
            ICryptoTransform desTrans = desCrypt.CreateEncryptor(DES, IV);
            CryptoStream cStream = new CryptoStream(memStream, desTrans, CryptoStreamMode.Write);
            cStream.Write(System.Text.Encoding.UTF8.GetBytes(value), 0, System.Text.Encoding.UTF8.GetByteCount(value));
            cStream.FlushFinalBlock();
            subKey.SetValue("Value", Convert.ToBase64String(memStream.ToArray()), RegistryValueKind.String);
        }

        private static String GetEncryptedValue(String subKeyName)
        {
            RegistryKey key = GetBaseKey();
            RegistryKey subKey = key.OpenSubKey(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(subKeyName)));
            if (subKey == null)
                return null;
            String keyStr = (String)subKey.GetValue("Key");
            byte[] keyAry = Convert.FromBase64String(keyStr);
            byte[] DES = new byte[8];
            byte[] IV = new byte[8];
            Array.Copy(keyAry, DES, 8);
            Array.Copy(keyAry, 8, IV, 0, 8);
            DESCryptoServiceProvider desCrypt = new DESCryptoServiceProvider();
            ICryptoTransform desTrans = desCrypt.CreateDecryptor(DES, IV);
            MemoryStream memStream = new MemoryStream();
            CryptoStream cStream = new CryptoStream(memStream, desTrans, CryptoStreamMode.Write);
            byte[] valBytes = Convert.FromBase64String((String)subKey.GetValue("Value"));
            cStream.Write(valBytes, 0, valBytes.Length);
            cStream.FlushFinalBlock();
            return System.Text.Encoding.UTF8.GetString(memStream.ToArray());
        }

        private static bool SubKeyExists(String subKeyName)
        {
            RegistryKey key = GetBaseKey();
            if (key.OpenSubKey(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(subKeyName))) == null)
                return false;
            return true;
        }

        public static String GetIATPassword(String IATName)
        {
            return GetEncryptedValue(IATName);
        }

        public static bool ContainsIAT(String IATName)
        {
            RegistryKey key = GetBaseKey();
            RegistryKey subKey = key.OpenSubKey(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(IATName)));
            if (subKey == null)
                return false;
            return true;
        }

        public static void DeleteIAT(String iatName)
        {
            RegistryKey key = GetBaseKey();
            try
            {
                key.DeleteSubKey(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(iatName)));
            }
            catch (Exception)
            { }
        }

        public static void AddIATPassword(String iatName, String password)
        {
            AddEncryptedValue(password, iatName);
        }
    }
}
*/