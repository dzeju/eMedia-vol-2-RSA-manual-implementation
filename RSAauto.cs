using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace e_media0_3
{
    class RSAauto
    {
        public byte[] EncryptMe(byte[] myFile, int begin, string pubKey, string privKey)
        {
            try
            {

                string public_key = "<RSAKeyValue><Modulus>" + pubKey + "</Modulus><Exponent>" + privKey +"</Exponent></RSAKeyValue>";

                //Create byte arrays to hold original, encrypted, and decrypted data.
                byte[] dataToEncrypt = new byte[myFile.Length - begin];
                Array.Copy(myFile, begin, dataToEncrypt, 0, myFile.Length - begin);
                byte[] encryptedData = new byte[begin];
                Array.Copy(myFile, 0, encryptedData, 0, begin);


                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(384))
                {

                    int len = 360;

                    if (((myFile.Length - begin) % len) != 0)
                        myFile = AddDataToArray(myFile, new byte[len - ((myFile.Length - begin) % len)]);

                    RSA.FromXmlString(public_key);

                    RSA.ExportParameters(false);

                    string privKeyString;
                    {
                        //we need some buffer
                        var sw = new System.IO.StringWriter();
                        //we need a serializer
                        var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
                        //serialize the key into the stream
                        xs.Serialize(sw, pubKey);
                        //get the string from the stream
                        privKeyString = sw.ToString();
                    }

                    System.IO.File.WriteAllText(@"C:\Users\Kuba\source\repos\e_media0_3\key_auto.txt", privKeyString);

                    int times = (myFile.Length - begin) / len;
                    for (int i = 0; i < times; i++)
                    {
                        byte[] fragment = new byte[len];
                        Array.Copy(myFile, begin + (i * len), fragment, 0, len);

                        byte[] encryptedChunk = RSA.Encrypt(fragment, false);

                        int desLength = 384;
                        if (encryptedChunk.Length != desLength && encryptedChunk.Length < desLength)
                            encryptedChunk = AddDataToArray(encryptedChunk, new byte[desLength - encryptedChunk.Length]);

                        encryptedData = AddDataToArray(encryptedData, encryptedChunk);
                    }
                    
                    return encryptedData;
                }
            }
            catch (ArgumentNullException) { return null; }
        }

        private static byte[] AddDataToArray(byte[] array, byte[] addedArray)
        {
            byte[] sum = new byte[array.Length + addedArray.Length];
            Array.Copy(array, 0, sum, 0, array.Length);
            Array.Copy(addedArray, 0, sum, array.Length, addedArray.Length);

            return sum;
        }

        /*public static byte[] RSAEncrypt(byte[] DataToEncrypt, RSAParameters RSAKeyInfo, bool DoOAEPPadding)
        {
            try
            {
                byte[] encryptedData;
                //Create a new instance of RSACryptoServiceProvider.
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(384))
                {

                    //Import the RSA Key information. This only needs
                    //toinclude the public key information.
                    RSA.ImportParameters(RSAKeyInfo);

                    //Encrypt the passed byte array and specify OAEP padding.  
                    //OAEP padding is only available on Microsoft Windows XP or
                    //later.  
                    encryptedData = RSA.Encrypt(DataToEncrypt, DoOAEPPadding);
                }
                return encryptedData;
            }
            //Catch and display a CryptographicException  
            //to the console.
            catch (CryptographicException e)
            {
                Console.WriteLine(e.Message);

                return null;
            }
        }*/

        public byte[] DecryptMe(byte[] myFile, int begin, string pubKey, string privKey)
        {
            try
            {

                string public_key = "<RSAKeyValue><Modulus>" + pubKey + "</Modulus><Exponent>" + privKey + "</Exponent></RSAKeyValue>";

                //Create byte arrays to hold original, encrypted, and decrypted data.
                byte[] dataToDecrypt = new byte[myFile.Length - begin];
                Array.Copy(myFile, begin, dataToDecrypt, 0, myFile.Length - begin);
                byte[] decryptedData = new byte[begin];
                Array.Copy(myFile, 0, decryptedData, 0, begin);


                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(384))
                {

                    int len = 384;

                    if (((myFile.Length - begin) % len) != 0)
                        myFile = AddDataToArray(myFile, new byte[len - ((myFile.Length - begin) % len)]);

                    RSA.FromXmlString(public_key);

                    int times = (myFile.Length - begin) / len;
                    for (int i = 0; i < times; i++)
                    {
                        byte[] fragment = new byte[len];
                        Array.Copy(myFile, begin + (i * len), fragment, 0, len);

                        byte[] decryptedChunk = RSA.Encrypt(fragment, false);

                        int desLength = 360;
                        if (decryptedChunk.Length != desLength && decryptedChunk.Length < desLength)
                            decryptedChunk = AddDataToArray(decryptedChunk, new byte[desLength - decryptedChunk.Length]);

                        decryptedData = AddDataToArray(decryptedData, decryptedChunk);
                    }

                    return decryptedData;
                }
            }
            catch (ArgumentNullException) { return null; }
        }


        public static byte[] RSADecrypt(byte[] DataToDecrypt, RSAParameters RSAKeyInfo, bool DoOAEPPadding)
        {
            try
            {
                byte[] decryptedData;
                //Create a new instance of RSACryptoServiceProvider.
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(384))
                {
                    //Import the RSA Key information. This needs
                    //to include the private key information.
                    RSA.ImportParameters(RSAKeyInfo);

                    //Decrypt the passed byte array and specify OAEP padding.  
                    //OAEP padding is only available on Microsoft Windows XP or
                    //later.  
                    decryptedData = RSA.Decrypt(DataToDecrypt, DoOAEPPadding);
                }
                return decryptedData;
            }
            //Catch and display a CryptographicException  
            //to the console.
            catch (CryptographicException e)
            {
                Console.WriteLine(e.ToString());

                return null;
            }
        }
    }
}
