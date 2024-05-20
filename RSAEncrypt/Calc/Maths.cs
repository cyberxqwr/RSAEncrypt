using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RSAEncrypt.Calc
{
    public class Maths : IDisposable
    {

        // Disposable

        bool is_disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            this.is_disposed = true;
        }

        // Disposable

        public Maths() { }

        // Values aquisition below

        public BigInteger E(BigInteger phi) { return GetE(phi); }

        private BigInteger GetE(BigInteger phi)
        {

            BigInteger e = 2;
            while (GCD(e, phi) != 1)
            {
                e++;
            }
            return e;
        }

        private BigInteger GCD(BigInteger a, BigInteger b)
        {

            if (a == 0)
            {
                return b;
            }
            else
            {
                return GCD(b % a, a);
            }
        }

        public BigInteger D (BigInteger e, BigInteger phi) { return GetD(e, phi); }

        private BigInteger GetD(BigInteger e, BigInteger phi)
        {

            BigInteger modulus;
            BigInteger d = 0;

            do {

                d++;
                modulus = (d * e) % phi;
            } while (modulus != 1);

            return d;
        }

        // Conversions from text to ASCII

        public List<BigInteger> ASCII (string text) { return AsciiConverter(text); }

        private List<BigInteger> AsciiConverter(string text)
        {

            List<BigInteger> result = new List<BigInteger>();

            foreach (char c in text)
            {
                result.Add(c);
            }

            return result;
        }

        // Regular Encryption

        public List<BigInteger> ENCRYPT(BigInteger e, List<BigInteger> AsciiVals, BigInteger n) { return EncryptRSA(e, AsciiVals, n); }

        private List<BigInteger> EncryptRSA(BigInteger e, List<BigInteger> AsciiVals, BigInteger n)
        {

            List<BigInteger> tempVals = new List<BigInteger>();
            foreach (BigInteger val in AsciiVals)
            {

                tempVals.Add(BigInteger.ModPow(val, e, n));
            }

            return tempVals;
        }

        // Digital Signature encryption

        public List<BigInteger> ENCRYPTDS(BigInteger d, List<BigInteger> AsciiVals, BigInteger n) { return EncryptDS(d, AsciiVals, n); }

        private List<BigInteger> EncryptDS(BigInteger d, List<BigInteger> AsciiVals, BigInteger n)
        {

            List<BigInteger> tempVals = new List<BigInteger>();
            foreach (BigInteger val in AsciiVals)
            {

                tempVals.Add(BigInteger.ModPow(val, d, n));
            }

            return tempVals;
        }

        // Decryption methods below

        public List<BigInteger> PQ(BigInteger n) { return FindPQ(n); }

        private List<BigInteger> FindPQ(BigInteger n)
        {

            List<BigInteger> primeVals = new List<BigInteger> ();

            while (n % 2 == 0)
            {
                primeVals.Add(2);
                n /= 2;
            }

            for (BigInteger i = 3; i * i <= n; i += 2)
            {
                while (n % i == 0)
                {
                    primeVals.Add(i);
                    n /= i;
                }
            }


            if (n > 2)
                primeVals.Add(n);

            return primeVals;
        }

        public List<BigInteger> DECRYPT(string cipherText, BigInteger D, BigInteger n) { return DecryptRSA(cipherText, D, n); }

        private List<BigInteger> DecryptRSA(string cipherText, BigInteger D, BigInteger n)
        {

            List<BigInteger> asciiVals = new List<BigInteger>();
            StringBuilder sb = new StringBuilder();

            foreach (char c in cipherText)
            {

                if (c != ' ') sb.Append(c);
                else
                {

                    asciiVals.Add(BigInteger.Parse(sb.ToString()));
                    sb.Clear();
                }
            }

            List<BigInteger> decodedValues = new List<BigInteger>();
            for (int i = 0; i < asciiVals.Count; i++) decodedValues.Add(BigInteger.ModPow(asciiVals[i], D, n));

            return decodedValues;
        }

    }
}
