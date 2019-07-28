using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using Sdl.Sdlx.ManagedFramework;
using System.Text.RegularExpressions;




namespace ConsoleApp2
{
    class Program
    {
        static void Main(string[] args)
        {
            string correctedFilesPath = ConfigurationManager.AppSettings["correctedFiles"];
            string[] correctedFiles = Directory.GetFiles(correctedFilesPath, "*.itd", SearchOption.AllDirectories);
            string wrongFilesPath = ConfigurationManager.AppSettings["wrongFiles"];
            string[] wrongFiles = Directory.GetFiles(wrongFilesPath, "*.zip", SearchOption.AllDirectories);

            string sourceLang;
            string targetLang;
            string fileOriginalPath;
            string jobID;
            string patternJobID = "\\\\[0-9]+\\\\";
            Regex rgxJobID = new Regex(patternJobID);
            string xliffName;

            using (IItd itd = SdlxFactory.Default.GetItd())
            {
                foreach (var correctfile in correctedFiles)
                {
                    itd.Open(correctfile);
                    fileOriginalPath = itd.DocumentInfo.OriginalSourceFile.ToString();
                    MatchCollection matches = rgxJobID.Matches(fileOriginalPath);
                    jobID = matches[0].ToString().Replace("\\", "");
                    Console.WriteLine(jobID);
                    sourceLang = itd.DocumentInfo.SourceLanguage.ToString();
                    Console.WriteLine(sourceLang);
                    targetLang = itd.DocumentInfo.TargetLanguage.ToString();
                    Console.WriteLine(targetLang);
                    xliffName = correctfile.Substring(correctfile.LastIndexOf("\\") + 1);
                    Console.WriteLine(xliffName);

                    string archivePatternJobID = "[0-9]+_";
                    string archiveJobID;

                    foreach (string wrongFile in wrongFiles)
                    {
                        Regex rgxArchiveJobID = new Regex(archivePatternJobID);
                        MatchCollection archiveMatches = rgxArchiveJobID.Matches(wrongFile);
                        archiveJobID = archiveMatches[0].ToString().Replace("_", "");
                        if (wrongFile.Contains(jobID))
                        {
                            using (FileStream zipToOpen = new FileStream(wrongFile, FileMode.Open))
                            {
                                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                                {
                                    foreach (var entry in archive.Entries)
                                    {
                                        if (entry.FullName.Contains($"TGT/{sourceLang}_{targetLang}/{xliffName}"))
                                        {
                                            Console.WriteLine("Uwaga, działam!");
                                            entry.Delete();
                                            archive.CreateEntryFromFile(correctfile, $"TGT/{sourceLang}_{targetLang}/{xliffName}");
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }

                }
            }


            Console.ReadLine();
        }
    }
}
