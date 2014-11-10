using CmdLine;

namespace Its.Configuration.Console
{
    [CommandLineArguments(
        Program = "Its.Configuration.Console",
        Title = "Its.Configuration utility console")]
    public class ConsoleParameters
    {
        [CommandLineParameter(
            Name = "command",
            ParameterIndex = 1,
            Required = true,
            Description = "The command to execute.")]
        public string Command { get; set; }

        [CommandLineParameter(
            Command = "f",
            Name = "filespec",
            ParameterIndex = 2,
            Required = false,
            Description = "Specifies the file to execute the command against.")]
        public string FileSpec { get; set; }

        [CommandLineParameter(
            Command = "t",
            Name = "text",
            ParameterIndex = 3,
            Required = false,
            Description = "Specifies the text to execute the command against.")]
        public string Text { get; set; }

        [CommandLineParameter(
            Command = "c",
            Name = "certificate",
            ParameterIndex = 3,
            Required = true,
            Description = "Specifies the file path of a certificate to use for encryption or decryption.")]
        public string Certificate { get; set; }

        [CommandLineParameter(
            Command = "p",
            Name = "password",
            ParameterIndex = 4,
            Required = false,
            Description = "Specifies the password for the certificate.")]
        public string Password { get; set; }

        [CommandLineParameter(
            Command = "?", 
            Name = "Help",
            Default = false,
            Description = "Show Help",
            IsHelp = true)]
        public bool Help { get; set; }
    }
}