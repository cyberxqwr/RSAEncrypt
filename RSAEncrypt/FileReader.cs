using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RSAEncrypt
{
    public class FileReader
    {

        public static string ReadFileToString(string fileName)
        {
            string filePath = Path.Combine(GetTempFolderPath(), fileName);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Failas nerastas: {filePath}");
            }

            string content;
            using (StreamReader reader = new StreamReader(filePath))
            {
                content = reader.ReadToEnd();
            }

            return content;
        }

        public static void WriteEncryptedMessageToFile(string fileName, string cryptedMessage)
        {
            string tempFolderPath = Path.GetTempPath();
            string filePath = Path.Combine(tempFolderPath, fileName);

            try
            {
                File.WriteAllText(filePath, cryptedMessage);
            }
            catch (Exception ex)
            {
                
                Console.WriteLine("Klaida irasant faila: " + ex.Message);
            }
        }

        public static string GetTempFolderPath()
        {
            return Path.GetTempPath();
        }

        public static bool FileExists(string fileName)
        {

            string filePath = Path.Combine(GetTempFolderPath(), fileName);
            return File.Exists(filePath);
        }
    }
}
