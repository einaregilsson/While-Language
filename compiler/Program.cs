/*
 * While Compiler
 * http://while-language.googlecode.com
 *
 * Copyright (C) 2009 Einar Egilsson [einar@einaregilsson.com]
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *  
 * $HeadURL$
 * $LastChangedDate$
 * $Author$
 * $Revision$
 */
using System;
using System.IO;
using System.Reflection;
using While.AST;
using While.Parsing;

namespace While {
    /// <summary>
    /// While.NET compiler;
    ///
    /// Compiler for the programming language While, found in the;
    ///	book Principles of Program Analysis by Nielson, Nielson and;
    ///	Hankin. Licensed under the GPL.
    ///	
    ///	Program author: Einar Egilsson (einar@einaregilsson.com)
    /// </summary>
    public class Program {
        
        public static int Main(string[] args) {
            try {
                Version v = Assembly.GetExecutingAssembly().GetName().Version;
                string version = v.Major + "." + v.Minor + "." + v.Build;
                Console.WriteLine("While.NET Compiler v" + version);
                Console.WriteLine("Copyright (C) Einar Egilsson 2009. All rights reserved.");
                Console.WriteLine();

                CommandLineOptions options = new CommandLineOptions(args);
                if (options.Empty) {
                    System.Console.Error.WriteLine("ERROR: No inputs specified");
                    return 1;
                } else if (options.Help) {
                    System.Console.Error.WriteLine("Usage: wc.exe [options] filename");
                    CommandLineOptions.Print();
                    return 2;
                } else if (!options.ReadStdIn && !File.Exists(options.InputFilename)) {
                    System.Console.Error.WriteLine("ERROR: File '{0}' does not exist", options.InputFilename);
                    return 3;
                }

                Parser parser;
                if (options.ReadStdIn) {
                    //Passing the StdIn stream directly to the scanner doesn't work well
                    //so we do it this way instead
                    string source = Console.In.ReadToEnd();
                    StreamWriter writer = new StreamWriter(new MemoryStream());
                    writer.Write(source);
                    writer.Flush();
                    writer.BaseStream.Seek(0, SeekOrigin.Begin);
                    parser = new Parser(new Scanner(writer.BaseStream), options);
                } else {
                    parser = new Parser(new Scanner(new FileStream(options.InputFilename, FileMode.Open)), options);
                }
                parser.Parse();
                if (parser.errors.count > 0) {
                    return 1;
                }
                WhileProgram.SymbolTable.Clear();
                WhileProgram.Instance.Compile(options.OutputFilename);
                return 0;
            } catch (Exception ex) {
                Console.Error.WriteLine("ERROR: " + ex.Message);
                Console.Error.WriteLine(ex.StackTrace);
                return 1;
            }
        }
    }
}