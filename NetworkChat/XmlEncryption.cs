using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Xml;
using System.Security.Cryptography.Xml;

namespace NetworkChat
{
    class XmlEncryption
    {
        private static string sec = "ty6i_09...JM_)(*&^tyUij";

        public static string encryptXml(string Xml)
        {
            System.Text.UTF8Encoding UTF8 = new System.Text.UTF8Encoding();

            MD5CryptoServiceProvider HashProvider = new MD5CryptoServiceProvider();

            byte[] TDESKey = HashProvider.ComputeHash(UTF8.GetBytes(sec));

            TripleDESCryptoServiceProvider encKey = new TripleDESCryptoServiceProvider();
            encKey.Key = TDESKey;
            encKey.Mode = CipherMode.ECB;
            encKey.Padding = PaddingMode.PKCS7;

            XmlDocument xd = new XmlDocument();
            xd.LoadXml(Xml);

            EncryptedXml encXml = new EncryptedXml(xd);
            byte[] encryptedOrder = encXml.EncryptData(xd.DocumentElement, encKey, false);

            EncryptedData encryptedData = new EncryptedData();
            encryptedData.Type = EncryptedXml.XmlEncElementUrl;
            encryptedData.EncryptionMethod = new
            EncryptionMethod(EncryptedXml.XmlEncTripleDESUrl);
            encryptedData.CipherData = new CipherData();
            encryptedData.CipherData.CipherValue = encryptedOrder;

            EncryptedXml.ReplaceElement(xd.DocumentElement, encryptedData, false);

            return xd.InnerXml;
        }


        public static string decryptXml(string Xml)
        {
            XmlDocument xd = new XmlDocument();
            xd.LoadXml(Xml);

            System.Text.UTF8Encoding UTF8 = new System.Text.UTF8Encoding();
            MD5CryptoServiceProvider HashProvider = new MD5CryptoServiceProvider();
            
            byte[] TDESKey = HashProvider.ComputeHash(UTF8.GetBytes(sec));
            TripleDESCryptoServiceProvider TDESAlgorithm = new TripleDESCryptoServiceProvider();
            TDESAlgorithm.Key = TDESKey;
            TDESAlgorithm.Mode = CipherMode.ECB;
            TDESAlgorithm.Padding = PaddingMode.PKCS7;


            XmlElement encOrderElem = xd.DocumentElement;
            EncryptedData encData = new EncryptedData();
            encData.LoadXml(encOrderElem);


            EncryptedXml encryptedXml = new EncryptedXml();
            byte[] decryptedOrder = encryptedXml.DecryptData(encData, TDESAlgorithm);


            encryptedXml.ReplaceData(encOrderElem, decryptedOrder);

            return xd.InnerXml;

        }
    }
}
