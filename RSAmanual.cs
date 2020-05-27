using System;
using System.Collections;
using System.Numerics;
using System.Security.Cryptography;
using System.Threading;

namespace e_media0_3
{
    class RSAmanual
    {
        BigInteger q = 0;

        public void GenerateKeys()
        {
            int e = 0;
            BigInteger n = 0;
            BigInteger d = 0;
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
                    break;
            }
            
            string[] lines = { "n: " + n.ToString(), "d: " + d.ToString(), "e: " + e.ToString() };
            System.IO.File.WriteAllLines(@"C:\Users\Kuba\source\repos\e_media0_3\key.txt", lines);
        }

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
            /*System.Windows.MessageBox.Show("File size is (if successful): " + myFile.Length.ToString() +
                "and actually is: " + decryptedData.Length.ToString());*/
            /*if (myFile.Length > decryptedData.Length)
                decryptedData = AddDataToArray(decryptedData, new byte[myFile.Length - decryptedData.Length]);*/

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
            /*System.Windows.MessageBox.Show("File size should be: " + myFile.Length.ToString() +
                "and actually is: " + encryptedData.Length.ToString());*/
            /*if (myFile.Length > encryptedData.Length)
                encryptedData = AddDataToArray(encryptedData, new byte[myFile.Length - encryptedData.Length]);*/

            return encryptedData;
        }

        private static byte[] AddDataToArray(byte[] array, byte[] addedArray)
        {
            byte[] sum = new byte[array.Length + addedArray.Length];
            Array.Copy(array, 0, sum, 0, array.Length);
            Array.Copy(addedArray, 0, sum, array.Length, addedArray.Length);

            return sum;
        }

        private BigInteger GetD(BigInteger phi, int e)//euclid
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

        private int ChooseE(BigInteger phi, BigInteger n)
        {
            for (int e = 65537; e < phi; e++)
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

        private void ThreadProc()
        {
            q = GetProbablyPrimeNumber();
            //System.Windows.MessageBox.Show("Call to join from thread");
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

        private bool IsItProbablyPrime(BigInteger num)
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
                    /*if (b == BigInteger.MinusOne)
                        return true;*/
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
