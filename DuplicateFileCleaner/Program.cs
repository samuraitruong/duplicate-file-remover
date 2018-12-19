using CommandLine;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace DuplicateFileCleaner
{
    public class Options

    {
        [Option('e', "extension", Required = true, HelpText = "File extensions")]
        public string Extensions { get; set; }


        [Option('i', "input", Required =true, HelpText ="The input path")]
        public string Input { get; set; }
        [Option('o', "output", Required = false, HelpText = "The output path")]
        public string Output { get; set; }

        [Option('t', "testonly", Required = false, HelpText = "The output path")]
        public bool TestOnly { get; set; }

    }
    
    class Program
    {
        static void Main(string[] args)
        {

            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(options =>
                {
                    RunWithOptions(options);
                });
        }
                       
        private static string GetFileName(string input)
        {
            Regex regex = new Regex("(.*)\\.(1080p|720p).*");
            var match = regex.Match(input);
            if (match != null)
            {
                return match.Groups[1].Value;
            }
            return input;
        }
        private static string PrintMB(long number)
        {
            return $"{number / 1024 / 1024:0.00MB}";
        }
        private static void RunWithOptions(Options options)
        {
            Console.WriteLine("Input Path: " + options.Input);
                        
            if(!string.IsNullOrEmpty(options.Output))
            {
                Directory.CreateDirectory(options.Output);
            }
            var files = Directory.GetFiles(options.Input, options.Extensions);

            var fi = files.Select(x => new FileInfo(x));

            var grouped = fi.GroupBy(x => GetFileName(x.Name));

            foreach (var item in grouped)
            {
                var ordered = item.OrderByDescending(x => x.Length).ToList();

                Console.WriteLine($"KEEP: {ordered[0].Name} - {PrintMB(ordered[0].Length)}");
                foreach (var toDelete in ordered.Skip(1))
                {
                    if(options.TestOnly)
                    {
                        Console.WriteLine($"MARKED: {ordered[0].Name} - {PrintMB(toDelete.Length)}");
                    }
                    if(string.IsNullOrEmpty(options.Output))
                    {

                        Console.WriteLine($"DEL: {ordered[0].Name} - {PrintMB(toDelete.Length)}");
                        toDelete.Delete();
                    }
                    else
                    {
                        Console.WriteLine($"MOVE: {ordered[0].Name} - {PrintMB(toDelete.Length)}");
                        var outputFIle = Path.Join(options.Output, toDelete.Name);
                        if (File.Exists(outputFIle)) File.Delete(outputFIle);
                        toDelete.MoveTo(outputFIle);
                    }
                }
            }
        }
    }
}
