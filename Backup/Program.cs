using System;
using System.Collections.Generic;
using System.Text;
using BackupUtility;
using System.IO;

namespace Backup
{
    class Program
    {
        static void Main(string[] args)
        {
            int idx = 0;
            try
            {
                if (args[0] == "/?" || args[0] == "-h")
                {
                    Console.WriteLine("Backup Utility");
                    Console.WriteLine("This program backs ups directories by copying newer files");
                    Console.WriteLine("to a backup directory.");
                    Console.WriteLine("\nArguments:");
                    Console.WriteLine("Backup.exe [-h or /?] source destination [| source2 destination2]");
                    Console.WriteLine("/? or -h:    Displays this help message");
                    Console.WriteLine("source:      The source directory to be copied");
                    Console.WriteLine("destination: The destination directory to copy to");
                    Console.ReadKey();
                    idx++;
                }
                //AnalysisResults ar = Utilities.Analyze(@"C:\documents and settings\burkemd1\my documents\test",
                //    @"U:\Test");

                doAnalysisAndBackup(args, idx);

                
                Console.ReadKey();
            }
            catch (System.IndexOutOfRangeException)
            {
                Console.WriteLine("An invalid argument, or an invalid number of arguments were given.");
                Console.Read();
                return;
            }
            catch (System.IO.IOException ex)
            {
                Console.WriteLine("An error occurred while backing up files:");
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }
        
        }

        private static void doAnalysisAndBackup(string[] args, int idx)
        {
            string source = args[idx];
            string destination = args[++idx];

            if (source.EndsWith(@"\"))
                source.Remove(source.Length - 1);
            if (destination.EndsWith(@"\"))
                destination.Remove(destination.Length - 1);

            Console.WriteLine(source + " -> " + destination);

            AnalysisResults ar = Utilities.Analyze(source, destination);

            if (ar.Count == 0)
                Console.WriteLine("No files need to be backed up!");
            else
            {
                ar.Print();
                Utilities.Backup(ar);
            }

            Console.WriteLine("\n\n");

            if (idx + 1 < args.Length)
            {
                if (args[++idx] == "|")
                    doAnalysisAndBackup(args, ++idx);
            }
        }
    }
}
