using RSAEncrypt.Calc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RSAEncrypt
{
    internal class Program
    {

        static void Main(string[] args)
        {

            
            int option = 0;
            bool runs = true;

            while (runs)
            {
                Console.WriteLine("Pasirinkti metoda.");
                Console.WriteLine("1) Sifruoti");
                Console.WriteLine("2) Desifruoti");
                Console.WriteLine("3) Nuskaityti teksta is failo");
                Console.WriteLine("4) Sifruoti DS");
                Console.WriteLine("9) Baigti darba");

                int.TryParse(Console.ReadLine(), out option);

                switch (option)
                {
                    case 1:
                        Case1();
                        Console.Clear();
                        break;
                    case 2:
                        Case2();
                        Console.Clear();
                        break;
                    case 3:
                        Console.WriteLine(Case3());
                        Console.ReadKey();
                        Console.Clear();
                        break;
                    case 4:
                        Case4();
                        break;
                    case 9:
                        return;
                    default: Console.WriteLine("Nera tokio pasirinkimo.");
                        break;
                }
            }

        }

        private static void Case1()
        {
            Console.WriteLine("Iveskite pirmini skaiciu p");
            string pString = Console.ReadLine();
            BigInteger p = BigInteger.Parse(pString);

            Console.WriteLine("Iveskite pirmini skaiciu q");
            string qString = Console.ReadLine();
            BigInteger q = BigInteger.Parse(qString);

            BigInteger n = p * q;
            BigInteger phi = (p - 1) * (q - 1);

            Console.WriteLine("Iveskite teksta:");
            string plainText = Console.ReadLine();

            using (Maths CALC = new Maths())
            {

                List<BigInteger> ASCII = CALC.ASCII(plainText);
                List<BigInteger> EncryptedASCII = new List<BigInteger>();

                BigInteger E = CALC.E(phi);
                BigInteger D = CALC.D(E, phi);

                Console.WriteLine("n = " + n);
                Console.WriteLine("e = " + E);
                Console.WriteLine("d = " + D);

                Console.WriteLine("Public key: ({0}, {1})", E, n);
                Console.WriteLine("Private key: ({0}, {1})", D, n);

                string fileData = E.ToString() + "-" + n.ToString() + "-";

                Console.WriteLine("Default ASCII: ");
                foreach (BigInteger asciiCode in ASCII) { Console.Write(asciiCode + " "); }

                EncryptedASCII = CALC.ENCRYPT(E, ASCII, n);
                Console.WriteLine("\nEncrypted ASCII: ");
                foreach (BigInteger asciiCode in EncryptedASCII) {
                    
                    Console.Write(asciiCode + " ");
                    fileData += asciiCode + " ";
                }

                FileReader.WriteEncryptedMessageToFile("RSAData.txt", fileData);
                Console.WriteLine("\nDuomenys isaugoti temp/RSAData.txt");

                Console.ReadKey();
            }
        }

        private static void Case2()
        {

            BigInteger p = 1;
            Console.WriteLine("Iveskite skaiciu n:");
            BigInteger n = BigInteger.Parse(Console.ReadLine());
            using (Maths CALC = new Maths())
            {

                List<BigInteger> pqVals = CALC.PQ(n);

                if (pqVals.Count > 1) {

                    for (int i = 0; i < pqVals.Count - 1; i++)
                    {

                            p *= pqVals[i];
                    }

                }   else p = pqVals[0];
                
                BigInteger phi = (p - 1) * (pqVals.Last() - 1);
                BigInteger E = CALC.E(phi);
                BigInteger D = CALC.D(E, phi);

                Console.WriteLine("Private key: ({0}, {1})", D, n);

                Console.WriteLine("Iveskite uzkoduotus duomenis:");
                string cipherText = Console.ReadLine();
                
                List<BigInteger> DecryptedASCII = CALC.DECRYPT(cipherText, D, n);

                Console.WriteLine("Decoded ASCII:");
                foreach (BigInteger asciiCode in DecryptedASCII) {  Console.Write(asciiCode + " "); }
            }

            Console.ReadKey();
        }

        private static string Case3()
        {

            if (FileReader.FileExists("RSAData.txt"))
            {
                string fileData = FileReader.ReadFileToString("RSAData.txt");
                StringBuilder sb = new StringBuilder();
                sb.Append("Zinute: ");
                int dashCount = 0;

                for (int i = 0; i < fileData.Count(); i++)
                {
                    if (dashCount == 2)
                    {

                        sb.Append(fileData[i]);
                    }

                    if (fileData[i] == '-') { dashCount++; }

                }

                return sb.ToString();
            }

            return "Failas RSAData.txt neegzistuoja";
            
        }

        private static async void Case4()
        {
            Console.WriteLine("Iveskite pirmini skaiciu p");
            string pString = Console.ReadLine();
            BigInteger p = BigInteger.Parse(pString);

            Console.WriteLine("Iveskite pirmini skaiciu q");
            string qString = Console.ReadLine();
            BigInteger q = BigInteger.Parse(qString);

            BigInteger n = p * q;
            BigInteger phi = (p - 1) * (q - 1);

            Console.WriteLine("Iveskite teksta:");
            string plainText = Console.ReadLine();

            using (Maths CALC = new Maths())
            {

                //List<BigInteger> ASCII = CALC.ASCII(plainText);
                List<BigInteger> ASCII = [4];
                List<BigInteger> EncryptedASCII = new List<BigInteger>();

                BigInteger E = CALC.E(phi);
                BigInteger D = CALC.D(E, phi);

                Console.Clear();
                Console.WriteLine("n = " + n);
                Console.WriteLine("e = " + E);
                Console.WriteLine("d = " + D);

                Console.WriteLine("Public key: ({0}, {1})", E, n);
                Console.WriteLine("Private key: ({0}, {1})", D, n);

                EncryptedASCII = CALC.ENCRYPTDS(D, ASCII, n);

                Program program = new Program();

                await program.ClientConnection(ASCII, EncryptedASCII, E, n);
                

                Console.ReadKey();
            }
        }

        private string MessageBuilder(List<BigInteger> x, List<BigInteger> s, BigInteger E, BigInteger n)
        {

            string body = $"Public Key: ({E},{n})\nParaso reiksmes: \n";
            for (int i = 0; i < x.Count; i++) {

                body += $"({x[i]}, {s[i]});\n";
            }

            return body + "<|EOM|>";
        }


        private async Task ClientConnection(List<BigInteger> x, List<BigInteger> s, BigInteger E, BigInteger n)
        {

            IPHostEntry ipHostInfo = await Dns.GetHostEntryAsync(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];

            IPEndPoint ipEndPoint = new(ipAddress, 11_000);

            using Socket client = new(
                ipEndPoint.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            await client.ConnectAsync(ipEndPoint);
            while (true)
            {
                // Send message.
                var message = MessageBuilder(x, s, E, n);
                var messageBytes = Encoding.UTF8.GetBytes(message);
                _ = await client.SendAsync(messageBytes, SocketFlags.None);
                Console.WriteLine($"Socket client sent message: \"{message}\"");

                // Receive ack.
                var buffer = new byte[1_024];
                var received = await client.ReceiveAsync(buffer, SocketFlags.None);
                var response = Encoding.UTF8.GetString(buffer, 0, received);
                if (response == "Pranesimas gautas")
                {
                    Console.WriteLine(
                        $"Socket client received acknowledgment: \"{response}\"");
                    break;
                }
                // Sample output:
                //     Socket client sent message: "Hi friends 👋!<|EOM|>"
                //     Socket client received acknowledgment: "<|ACK|>"
            }

            client.Shutdown(SocketShutdown.Both);
        }

    }
}
