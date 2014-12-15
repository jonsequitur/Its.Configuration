// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using CmdLine;
using Its.Recipes;

namespace Its.Configuration.Console
{
    public class Program
    {
        private static void Main()
        {
            ConsoleParameters parameters = null;

            // parse the command line
            try
            {
                parameters = CommandLine.Parse<ConsoleParameters>();
            }
            catch (CommandLineHelpException helpException)
            {
                // User asked for help 
                CommandLine.WriteLineColor(ConsoleColor.Magenta, helpException.ArgumentHelp.GetHelpText(System.Console.BufferWidth));
                Environment.Exit(1);
            }
            catch (CommandLineException exception)
            {
                // Some other kind of command line error 
                CommandLine.WriteLineColor(ConsoleColor.Red, exception.ArgumentHelp.Message);
                CommandLine.WriteLineColor(ConsoleColor.Cyan, exception.ArgumentHelp.GetHelpText(System.Console.BufferWidth));
                Environment.Exit(1);
            }

            try
            {
                RunCommand(parameters);
                Environment.Exit(0);
            }
            catch (Exception exception)
            {
                CommandLine.WriteLineColor(ConsoleColor.Red, exception.ToString());
                Environment.Exit(1);
            }
        }

        public static void RunCommand(ConsoleParameters parameters)
        {
            Validate(parameters);

            switch (parameters.Command.ToLowerInvariant())
            {
                case "encrypt":
                    CommandLine.WriteLineColor(ConsoleColor.Green,
                                               Encrypt(parameters));
                    break;
                case "decrypt":
                    var plaintext = Decrypt(parameters);

                    using (TextColor(ConsoleColor.Green))
                    {
                        System.Console.WriteLine(plaintext);
                    }

                    break;
                default:
                    CommandLine.WriteLineColor(ConsoleColor.Red,
                                               string.Format("Command {0} not supported.", parameters.Command));
                    Environment.Exit(1);
                    break;
            }
        }

        public static IDisposable TextColor(ConsoleColor color)
        {
            var previousColor = System.Console.ForegroundColor;
            System.Console.ForegroundColor = color;
            return new AnonymousDisposable(() => { System.Console.ForegroundColor = previousColor; });
        }

        private static void Validate(ConsoleParameters parameters)
        {
            if (!string.IsNullOrWhiteSpace(parameters.FileSpec) && !string.IsNullOrWhiteSpace(parameters.Text))
            {
                CommandLine.WriteLineColor(ConsoleColor.Red, "You cannot specify both the /f and /t switches.");
                Environment.Exit(1);
            }

            if (string.IsNullOrWhiteSpace(parameters.FileSpec) && string.IsNullOrWhiteSpace(parameters.Text))
            {
                CommandLine.WriteLineColor(ConsoleColor.Red, "You must specify either the /f or /t switch.");
                Environment.Exit(1);
            }
        }

        public static string Encrypt(ConsoleParameters parameters)
        {
            var plaintext = GetText(parameters);
            return plaintext.Encrypt(new X509Certificate2(parameters.Certificate, parameters.Password));
        }

        public static string Decrypt(ConsoleParameters parameters)
        {
            var cipherText = GetText(parameters);
            return cipherText.Decrypt(new X509Certificate2(parameters.Certificate, parameters.Password));
        }

        private static string GetText(ConsoleParameters parameters)
        {
            if (!string.IsNullOrWhiteSpace(parameters.FileSpec))
            {
                return File.ReadAllText(parameters.FileSpec);
            }

            if (!string.IsNullOrWhiteSpace(parameters.Text))
            {
                return parameters.Text;
            }

            return string.Empty;
        }
    }
}