using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Threading;

namespace SocketClient
{

    
    internal class Program
    {
        

        static async Task Main(string[] args)
        {

            BigInteger E, n;
            List<BigInteger> x = new List<BigInteger>();
            List<BigInteger> s = new List<BigInteger>();
            List<BigInteger> data = new List<BigInteger>();
            string body = "";

            IPHostEntry ipHostInfo = await Dns.GetHostEntryAsync(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];

            IPEndPoint ipEndPoint = new(ipAddress, 11_001);

            using Socket client = new(
                ipEndPoint.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            
            while (true) {

                try
                {

                    await client.ConnectAsync(ipEndPoint);
                    break;
                }
                catch (Exception)
                {

                    Console.WriteLine("Waiting for heartbeat");
                    Thread.Sleep(8000);
                }
            }
            
            
            
            while (true)
            {
                // Send message.
                var message = "Third app opening connection<|EOM|>";
                var messageBytes = Encoding.UTF8.GetBytes(message);
                _ = await client.SendAsync(messageBytes, SocketFlags.None);
                Console.WriteLine($"Socket client sent message: \"{message}\"");

                // Receive ack.
                var buffer = new byte[1_024];
                var received = await client.ReceiveAsync(buffer, SocketFlags.None);
                var response = Encoding.UTF8.GetString(buffer, 0, received);
                var eom = "<|EOM|>";
                if (response.IndexOf(eom) > -1)
                {
                    body = response.Replace(eom, "");

                    Console.WriteLine(
                        $"Socket client received message: \"{response}\"");
                    break;
                }
                // Sample output:
                //     Socket client sent message: "Hi friends 👋!<|EOM|>"
                //     Socket client received acknowledgment: "<|ACK|>"
            }

            client.Shutdown(SocketShutdown.Both);

            data = TextFormat(body);
            E = data[0];
            n = data[1];
            data.RemoveAt(0);
            data.RemoveAt(0);

            for (int i = 0; i < data.Count(); i++)
            {

                if (i % 2 == 0) x.Add(data[i]);
                else s.Add(data[i]);
            }

            if (DSVerification(x, s, E, n)) Console.WriteLine("Parasas patvirtintas");
            else Console.WriteLine("Nepatvirtintas");
            Console.ReadKey();
        }

        private static bool DSVerification(List<BigInteger> x, List<BigInteger> s, BigInteger E, BigInteger n) {

            for (int i = 0; i < x.Count; i++) {

                if (BigInteger.ModPow(s[i], E, n) != x[i]) return false;
            }

            return true;
        }

        private static List<BigInteger> TextFormat(string body)
        {

            var result = new List<BigInteger>();

            bool parEnounctered = false;
            StringBuilder sb = new StringBuilder();
            // Split the string by lines
            string[] lines = body.Split('\n');

            for (int i = 0; i < lines[0].Length; i++)
            {
                if (!parEnounctered) if (lines[0].ElementAt(i) == '(')
                    {
                        parEnounctered = true;
                        continue;
                    }

                if (parEnounctered)
                {

                    if (lines[0].ElementAt(i) != ',' && lines[0].ElementAt(i) != ')')
                    {
                        sb.Append(lines[0].ElementAt(i));
                    }
                    else if (lines[0].ElementAt(i) == ',' || lines[0].ElementAt(i) == ')')
                    {
                        result.Add(BigInteger.Parse(sb.ToString()));
                        sb.Clear();
                    }
                }
            }


            // Skip the first line (public key information - optional processing)
            for (int i = 2; i < lines.Length; i++)
            {

                string line = lines[i].Trim(); // Trim any leading/trailing whitespace

                if (line.Count() == 0) continue;

                // Split the line by comma (",")
                string[] parts = line.Split(',');

                // Remove parentheses from each part (optional)
                for (int j = 0; j < parts.Length; j++)
                {
                    parts[j] = parts[j].Trim().TrimStart('(').TrimEnd(';'); // Trim whitespace, parentheses
                }

                // Convert strings to BigIntegers and add to the list
                BigInteger firstValue = BigInteger.Parse(parts[0]);
                BigInteger secondValue = BigInteger.Parse(parts[1].TrimEnd(')')); // Trim trailing semicolon
                result.Add(firstValue);
                result.Add(secondValue);
            }

            return result;
        }
    }
    
}
