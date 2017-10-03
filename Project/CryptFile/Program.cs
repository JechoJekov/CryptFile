using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Security.Cryptography.IO;
using System.IO;
using System.Reflection;

namespace CryptFile
{
    class Program
    {
        static int Main(string[] args)
        {
            PrintHeader();

            if (args.Length < 3 || args.Length > 4)
            {
                Help();
                return 1;
            }

            if (IsHelpArgument(args[0]))
            {
                // Display the help screen 
                Help();
                return 0;
            }

            var command = args[0];
            var inputPath = args[1];
            var outputPath = args[2];

            var encrypt = command.Equals("-e", StringComparison.OrdinalIgnoreCase);
            var decrypt = command.Equals("-d", StringComparison.OrdinalIgnoreCase);

            if (encrypt || decrypt)
            {
                // Valid command
            }
            else
            {
                PrintError("Invalid command: '{0}'.", command);
                return 1;
            }

            if (false == File.Exists(inputPath))
            {
                PrintError("File not found: '{0}'.", inputPath);
                return 1;
            }

            string password;
            if (args.Length > 3)
            {
                password = args[3];
            }
            else
            {
                password = ReadPassword("Password: ");
            }

            if (password.Length == 0)
            {
                PrintError("Password cannot be empty.");
                return 1;
            }

            try
            {
                using (var inputStream = File.OpenRead(inputPath))
                {
                    using (var outputStream = File.Create(outputPath))
                    {
                        try
                        {
                            if (encrypt)
                            {
                                StreamCryptoUtils.EncryptStream(inputStream, outputStream, password);

                                Console.WriteLine("File encrypted.");
                            }
                            else
                            {
                                StreamCryptoUtils.DecryptStream(inputStream, outputStream, password);

                                Console.WriteLine("File decrypted.");
                            }
                        }
                        catch (StreamCryptoException exc)
                        {
                            string message;
                            switch (exc.Error)
                            {
                                case StreamCryptoError.DecryptionError:
                                    message = "Wrong password or file has been tampered with. Decryption error: " + exc.Message;
                                    break;
                                case StreamCryptoError.IntegrityCheckFailed:
                                    message = "Wrong password or file has been tampered with. " + exc.Message;
                                    break;
                                case StreamCryptoError.NotEncrypted:
                                    message = "The file is not encrypted.";
                                    break;
                                default:
                                    message = exc.Message;
                                    break;
                            }

                            PrintError(message);
                            return 1;
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                PrintError(exc.Message);
                return 1;
            }

            return 0;
        }

        #region Helper methods

        /// <summary>
        /// Reads a password from the command line.
        /// </summary>
        /// <param name="prompt"></param>
        /// <returns></returns>
        static string ReadPassword(string prompt)
        {
            if (false == string.IsNullOrEmpty(prompt))
            {
                Console.Write(prompt);
            }

            var password = "";

            ConsoleKeyInfo key;

            while ((key = System.Console.ReadKey(true)).Key != ConsoleKey.Enter)
            {
                if (key.Key == ConsoleKey.Backspace)
                {
                    if (password.Length > 0)
                    {
                        password = password.Substring(0, password.Length - 1);
                    }
                }
                else
                {
                    password += key.KeyChar;
                }
            }

            Console.WriteLine();

            return password;
        }

        static void PrintError(string format, params object[] args)
        {
            PrintError(string.Format(format, args));
        }

        static void PrintError(string message)
        {
            Console.WriteLine(@"ERROR: {0}", message);
        }

        static readonly string[] HelpArgumentList = { "?", "-?", "/?", "-h", "/h", "--h", "-help", "/help" };

        /// <summary>
        /// Returns a value indicating if the specified argument is about requesting help.
        /// </summary>
        /// <param name="argument"></param>
        /// <returns></returns>
        static bool IsHelpArgument(string argument)
        {
            if (argument == null)
            {
                throw new ArgumentNullException("argument");
            }

            return HelpArgumentList.Contains(argument, StringComparer.OrdinalIgnoreCase);
        }

        #endregion

        #region Help

        static void PrintHeader()
        {
            Console.WriteLine(@"
File encryption tool
");
        }

        static void Help()
        {
            var fileName = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location);

            Console.WriteLine($@"
Encryption: {fileName} -e <input_file> <output_file> [password]

Arguments:
    
    input_file          File to encrypt.
    output_file         Output (encrypted) file.
    password            Password to use.


Decryption: {fileName} -d <input_file> <output_file> [password]

Arguments:
    
    input_file          File to decrypt.
    output_file         Output (plaintext) file.
    password            Password to use.
"
                );
        }

        #endregion
    }
}
