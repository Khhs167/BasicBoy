using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BBACompiler
{
    class Program
    {

        static Dictionary<string, Macro> macros = new Dictionary<string, Macro>();

        static void Main(string[] args)
        {
            List<byte> output = new List<byte>();
            string file = Directory.GetCurrentDirectory() + "/test.bba";

            Console.WriteLine("Compiling script...");
            var includeDir = System.Environment.GetEnvironmentVariable("bbaIncludePath");
            if (includeDir == null)
            {
                includeDir = Path.GetDirectoryName(AppContext.BaseDirectory) + "/include";
                System.Environment.SetEnvironmentVariable("bbaIncludePath", includeDir);
            }

            Console.WriteLine("IncludeDir: " + includeDir);

            List<string> lines = File.ReadAllLines(file).ToList();

            for (int i = 0; i < lines.Count; i++)
            {
                lines[i] = lines[i].Split(";")[0];
            }


            while(string.Join("", lines).Contains(".include"))
            {
                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i].StartsWith(".include"))
                    {

                        var fileToInc = lines[i].Split(" ", StringSplitOptions.RemoveEmptyEntries)[1];
                        lines.RemoveAt(i);
                        i--;
                        if (i < 0)
                        {
                            i++;
                        }


                        lines.InsertRange(i, File.ReadAllLines(includeDir + "/" + fileToInc).ToList());

                    }
                }
            }

            for (int i = 0; i < lines.Count; i++)
            {

                if (lines[i].StartsWith(";"))
                    continue;

                if (lines[i].StartsWith("."))
                {
                    if (lines[i].StartsWith(".macro"))
                    {
                        List<string> code = new List<string>();
                        int ptr = 1;
                        while (lines[i + ptr] != ".END")
                        {
                            code.Add(lines[i + ptr].Trim());
                            ptr++;
                        }

                        string[] rargs = lines[i].Split(" ", StringSplitOptions.RemoveEmptyEntries);
                        List<string> fargs = new List<string>();

                        for (int a = 2; a < rargs.Length; a++)
                        {
                            fargs.Add(rargs[a]);
                        }
                        macros.Add(rargs[1], new Macro { code = code.ToArray(), args = fargs.ToArray() });

                        for (int l = 0; l < ptr; l++)
                        {
                            lines.RemoveAt(i);
                        }
                    }
                    else if (lines[i].StartsWith(".operation"))
                    {
                        string[] vals = lines[i].Split(" ", StringSplitOptions.RemoveEmptyEntries);
                        if (vals.Length < 2)
                            continue;

                        byte op = BitConverter.GetBytes(Convert.ToInt16(vals[1], 16))[0];
                        List<byte> paraBytes = new List<byte>();
                        for (int o = 2; o < vals.Length; o++)
                        {
                            if (vals[o].StartsWith("0x"))
                            {
                                paraBytes.Add(0x00);
                                paraBytes.AddRange(BitConverter.GetBytes(Convert.ToInt16(vals[o], 16)).Reverse());
                            }
                            else
                            {
                                paraBytes.Add(0x01);
                                paraBytes.AddRange(BitConverter.GetBytes(Convert.ToInt16(vals[o], 16)).Reverse());
                            }
                        }

                        output.Add(op);
                        output.AddRange(paraBytes);
                    }
                }
                else
                {
                    string[] vals = lines[i].Split(" ", StringSplitOptions.RemoveEmptyEntries);
                    if (vals.Length < 1)
                        continue;
                    if (!macros.ContainsKey(vals[0]))
                    {
                        Console.WriteLine("Invalid macro: " + vals[0]);
                    }
                    else
                    {
                        lines.RemoveAt(i);
                        foreach (var line in macros[vals[0]].code)
                        {
                            string addLine = line;
                            for (int a = 0; a < macros[vals[0]].args.Length; a++)
                            {
                                addLine = addLine.Replace(".operation", "\b\b").Replace(macros[vals[0]].args[a], vals[a + 1]).Replace("\b\b", ".operation");
                            }
                            Console.WriteLine(addLine);
                            lines.Insert(i, addLine);
                        }
                        i--;
                    }
                }

                
            }

            File.WriteAllBytes(Path.ChangeExtension(file, ".bbg"), output.ToArray());
            
            
        }
    }

    class Macro
    {
        public string[] code = new string[0];
        public string[] args = new string[0];
    }
}
