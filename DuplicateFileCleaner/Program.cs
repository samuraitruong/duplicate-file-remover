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


        [Option('i', "input", Required = true, HelpText = "The input path")]
        public string Input { get; set; }
        [Option('o', "output", Required = false, HelpText = "The output path")]
        public string Output { get; set; }

        [Option('t', "testonly", Required = false, HelpText = "Print only")]
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
            var replacedInput = input.ToLower();
            replacedInput = replacedInput.Replace(".bluray", string.Empty);
            replacedInput = replacedInput.Replace(".fullhd", string.Empty);
            replacedInput = replacedInput.Replace(".full.hd", string.Empty);
            replacedInput = replacedInput.Replace(".repack", string.Empty);
            replacedInput = replacedInput.Replace(".unrated", string.Empty);
            replacedInput = replacedInput.Replace(".extended", string.Empty);
            replacedInput = replacedInput.Replace(".imax", string.Empty);
            replacedInput = replacedInput.Replace(".limited", string.Empty);
            replacedInput = replacedInput.Replace("1080p", "|");
            replacedInput = replacedInput.Replace("720p", "|");
            // Regex regex = new Regex("(.*)\\.(1080p|720p).*");
            // var match = regex.Match(replacedInput);
            // if (match != null)
            // {
            //     return match.Groups[1].Value.ToLower();
            // }
            return replacedInput.Split('|')[0];
        }
        private static string PrintMB(double number)
        {
            return $"{number / 1024 / 1024:0.00MB}";
        }
        private static string PrintMB(long number)
        {
            return $"{number / 1024 / 1024:0.00MB}";
        }
        private static void RunWithOptions(Options options)
        {
            Console.WriteLine("Input Path: " + options.Input);

            if (!string.IsNullOrEmpty(options.Output))
            {
                Directory.CreateDirectory(options.Output);
            }
            var files = Directory.GetFiles(options.Input, options.Extensions);

            var fi = files.Select(x => new FileInfo(x));

            var grouped = fi.GroupBy(x => GetFileName(x.Name));
            double totalDelete = 0;
            int deleteCount = 0;
            foreach (var item in grouped)
            {
                try
                {
                    var ordered = item.OrderByDescending(x => x.Length).ToList();
                    Console.WriteLine($"---------------------------------- {item.Key} ----------------------------------");
                    Console.WriteLine($"KEEP: {ordered[0].Name} - {PrintMB(ordered[0].Length)}");
                    foreach (var toDelete in ordered.Skip(1))
                    {
                        totalDelete += toDelete.Length;
                        deleteCount++;
                        if (options.TestOnly)
                        {
                            Console.WriteLine($"MARKED: {ordered[0].Name} - {PrintMB(toDelete.Length)}");
                            continue;
                        }
                        if (string.IsNullOrEmpty(options.Output))
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
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            Console.WriteLine($"{deleteCount} files removed. Total size {PrintMB(totalDelete)}");
        }
    }
}
