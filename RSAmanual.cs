using System;
using System.Collections;
using System.Numerics;
using System.Security.Cryptography;
using System.Threading;

namespace e_media0_3
{
    class RSAmanual
    {
        public byte[] EncryptMe(byte[] myFile, int begin, string pubKey, string privKey)
        {
            BigInteger n = BigInteger.Parse(pubKey);
            int e = int.Parse(privKey);
            try
            {
                byte[] encryptedFile = FileEncryption(myFile, e, n, begin);

                return encryptedFile;
            }
            catch (Exception en)
            {
                System.Windows.MessageBox.Show(en.Message);
            }

            return null;
        }

        public byte[] DecryptMe(byte[] myFile, int begin, string pubKey, string privKey)
        {
            BigInteger n = BigInteger.Parse(pubKey);
            BigInteger d = BigInteger.Parse(privKey);
            byte[] decryptedData = new byte[begin];
            Array.Copy(myFile, 0, decryptedData, 0, begin);
            int len = 384;

            BigInteger inv = new BigInteger(new byte[len]);

            if (((myFile.Length - begin) % len) != 0)
                myFile = AddDataToArray(myFile, new byte[len - ((myFile.Length - begin) % len)]);

            int times = (myFile.Length - begin) / len;
            for (int i = 0; i < times; i++)
            {
                byte[] fragment = new byte[len];
                Array.Copy(myFile, begin + (i * len), fragment, 0, len);
                
                BigInteger tmp = new BigInteger(fragment, true, false);

                BigInteger inv2 = tmp;

                if (tmp > n)
                    throw new Exception("message bigger than n, big no no \nat: " + i.ToString() + " from " + times.ToString());
                if (tmp < 0)
                    throw new Exception("message is smaller than 0");

                tmp = BigInteger.ModPow(tmp, d, n);

                tmp = tmp ^ inv;
                inv = inv2;

                byte[] decryptedChunk = tmp.ToByteArray(true, false);

                int desLength = 360;

                if (decryptedChunk.Length != desLength && decryptedChunk.Length < desLength)
                    decryptedChunk = AddDataToArray(decryptedChunk, new byte[desLength - decryptedChunk.Length]);//tu

                decryptedData = AddDataToArray(decryptedData, decryptedChunk);
            }

            return decryptedData;
        }

        private byte[] FileEncryption(byte[] myFile, int e, BigInteger n, int begin)
        {
            byte[] encryptedData = new byte[begin];
            Array.Copy(myFile, 0, encryptedData, 0, begin);

            int len = 360;
            BigInteger inv = new BigInteger(new byte[len]);

            if (((myFile.Length - begin) % len) != 0)
                myFile = AddDataToArray(myFile, new byte[len - ((myFile.Length - begin) % len)]);

            int times = (myFile.Length - begin) / len;
            for (int i = 0; i < times; i++)
            {
                byte[] fragment = new byte[len];
                Array.Copy(myFile, begin + (i * len), fragment, 0, len);
                
                BigInteger tmp = new BigInteger(fragment, true, false);

                tmp = inv ^ tmp;

                if (tmp > n || tmp < 0)
                    throw new Exception("message bigger than n, big no no");

                tmp = BigInteger.ModPow(tmp, e, n);

                inv = tmp;

                byte[] encryptedChunk = tmp.ToByteArray(true, false);

                int desLength = 384;
                if (encryptedChunk.Length != desLength && encryptedChunk.Length < desLength)
                    encryptedChunk = AddDataToArray(encryptedChunk, new byte[desLength - encryptedChunk.Length]);
                
                encryptedData = AddDataToArray(encryptedData, encryptedChunk);
            }

            return encryptedData;
        }

        private static byte[] AddDataToArray(byte[] array, byte[] addedArray)
        {
            byte[] sum = new byte[array.Length + addedArray.Length];
            Array.Copy(array, 0, sum, 0, array.Length);
            Array.Copy(addedArray, 0, sum, array.Length, addedArray.Length);

            return sum;
        }

        BigInteger q = 0;

        private void ThreadProc()
        {
            q = GetProbablyPrimeNumber();
        }

        public void GenerateKeys()
        {
            uint e = 0;
            BigInteger n = 0;
            BigInteger d = 0;
            /*var RSA = new RSACryptoServiceProvider(384);
            RSAParameters s = new RSAParameters();*/

            while (true)
            {
                Thread t = new Thread(new ThreadStart(ThreadProc));
                t.Start();
                Thread.Sleep(0);
                BigInteger p = GetProbablyPrimeNumber();
                t.Join();

                n = BigInteger.Multiply(p, q);
                BigInteger phi = BigInteger.Multiply(p - 1, q - 1);

                e = ChooseE(phi, n);
                d = GetD(phi, e);
                if (d > 0)
                {
                    {
                        /*s.D = d.ToByteArray(true,false);
                        s.DP = (d % (p - 1)).ToByteArray(true, false);
                        s.DQ = (d % (q - 1)).ToByteArray(true, false);
                        s.Exponent = BitConverter.GetBytes(e);
                        s.InverseQ = BigInteger.ModPow(q, p - 2, p).ToByteArray(true, false);
                        s.Modulus = n.ToByteArray(true, false);
                        s.P = p.ToByteArray(true, false);
                        s.Q = q.ToByteArray(true, false);
                        RSA.ImportParameters(s);*/
                    }
                    break;
                }
            }
            {/* var pubKey = RSA.ExportParameters(false);
             string pubKeyString;
             {
                 //we need some buffer
                 var sw = new System.IO.StringWriter();
                 //we need a serializer
                 var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
                 //serialize the key into the stream
                 xs.Serialize(sw, pubKey);
                 //get the string from the stream
                 pubKeyString = sw.ToString();
             }

             var privKey = RSA.ExportParameters(true);
             string privKeyString;
             {
                 //we need some buffer
                 var sw = new System.IO.StringWriter();
                 //we need a serializer
                 var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
                 //serialize the key into the stream
                 xs.Serialize(sw, privKey);
                 //get the string from the stream
                 privKeyString = sw.ToString();
             }*/
            }

            string[] lines = { "n: " + n.ToString() + "\nd: " + d.ToString(), "e: " + e.ToString() +
                               "\n\nn: " + Convert.ToBase64String(n.ToByteArray(true,false)) +
                               "\nd: " + Convert.ToBase64String(d.ToByteArray(true,false)) +
                               "\ne: " + Convert.ToBase64String(BitConverter.GetBytes(e))/* +
                               "\nauto RSA pubKey: " + pubKeyString +
                               "\nauto RSA privKey: " + privKeyString*/};
            System.IO.File.WriteAllLines(@"C:\Users\Kuba\source\repos\e_media0_3\key.txt", lines);
        }

        private BigInteger GetD(BigInteger phi, uint e)//euclid
        {
            BigInteger d = 1;
            BigInteger phi1 = phi;
            BigInteger phi2 = phi;
            BigInteger e_tmp = e;

            while (true)
            {
                BigInteger tmp = BigInteger.Divide(phi1, e_tmp);

                BigInteger e2_tmp = BigInteger.Multiply(tmp, e_tmp);
                BigInteger d_tmp = BigInteger.Multiply(tmp, d);

                e2_tmp = phi1 - e2_tmp;
                d_tmp = phi2 - d_tmp;

                phi1 = e_tmp;
                phi2 = d;

                e_tmp = e2_tmp;
                if (d_tmp < 0)
                    d = d_tmp + phi;
                else
                    d = d_tmp;
                if (e_tmp == 1)
                    return d;
            }

            throw new NotImplementedException();
        }

        private uint ChooseE(BigInteger phi, BigInteger n)//wybranie e wzglednie pierwszego z phi
        {
            for (uint e = 65537; e < phi; e++)
            {
                if (e % 2 != 0)
                {
                    if (phi % e != 0)
                        if (n % e != 0)
                            return e;
                }
            }
            throw new Exception();
        }

        public BigInteger GetProbablyPrimeNumber()
        {
            RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();

            byte[] randomNumber = new byte[192];
            rngCsp.GetNonZeroBytes(randomNumber);
            BigInteger n = BigInteger.Abs(new BigInteger(randomNumber));
            while (!IsItProbablyPrime(n))
            {
                rngCsp.GetNonZeroBytes(randomNumber);
                n = BigInteger.Abs(new BigInteger(randomNumber));
            }
            rngCsp.Dispose();
            return n;
        }

        private bool IsItProbablyPrime(BigInteger num)//miller-rabin
        {
            if (num % 2 == 0)
                return false;
            if (num % 3 == 0)
                return false;
            if (num % 5 == 0)
                return false;
            if (num % 7 == 0)
                return false;

            BigInteger n = num - 1;
            int k = 0;

            while (n % 2 == 0)
            {
                n /= 2;
                k += 1;
            }
            BigInteger m = (num - 1) / BigInteger.Pow(2, k);

            BigInteger b = BigInteger.ModPow(2, m, num);

            if (b.IsOne || b == BigInteger.MinusOne)
                return true;
            else
            {
                for (int i = 1; i < k; i++)
                {
                    b = BigInteger.ModPow(b, 2, num);
                    if (b.IsOne)
                        return false;
                    if (b == num - 1)
                        break;
                }
                if (b != num - 1)
                    return false;
            }
            return true;
        }
    }
}
