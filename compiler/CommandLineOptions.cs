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
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace While {

    /// <summary>
    /// Class for parsing command line options.
    /// </summary>
    public class CommandLineOptions {

        public CommandLineOptions(string[] args) {
            if (args.Length == 0) {
                _empty = true;
                return;
            }
            for (int i = 0; i < args.Length; i++) {
                string arg = args[i];
                string commandArg = null;
                bool wasArg = false;
                if (IsParam(arg, "help", "h", false, out commandArg) || IsParam(arg, "help", "\\?", false, out commandArg)) {
                    _help = true;
                    wasArg = true;
                } else if (IsParam(arg, "debug", "d", false, out commandArg)) {
                    _debug = true;
                    wasArg = true;
                } else if (IsParam(arg, "coursesyntax", "c", false, out commandArg)) {
                    _bookVersion = false;
                    wasArg = true;
                } else if (IsParam(arg, "out", "o", true, out commandArg)) {
                    _outputFile = commandArg;
                    wasArg = true;
                } else if (IsParam(arg, "plugins", "p", true, out commandArg)) {
                    _plugins = new List<string>(commandArg.Split(','));
                    wasArg = true;
                } else {
                    if (i != args.Length - 1) {
                        Console.Error.WriteLine("ERROR: Unknown command line option " + arg);
                        While.Environment.Exit(1);
                    }
                }
                if (i == args.Length - 1 && wasArg && !_help) {
                    Console.Error.WriteLine("ERROR: Missing input file name.");
                    While.Environment.Exit(1);
                }
            }

            _inputFile = args[args.Length - 1];
            
            if (_inputFile.ToLower() == "stdin") {
                _readStdIn = true;
                if (_outputFile == null) {
                    Console.Error.WriteLine("ERROR: /out:<filename> must be specified when reading source from the standard input stream (STDIN).");
                    While.Environment.Exit(4);
                }
            }
            if (_outputFile == null) {
                _outputFile = Regex.Replace(Path.GetFileName(_inputFile), @"\.w(hile)?$", "");
            }

            if (!_outputFile.ToLower().EndsWith(".exe")) {
                _outputFile += ".exe";
            }
        }

        private bool IsParam(string arg, string longForm, string shortForm, bool requireArgument, out string argument) {
            Match m = Regex.Match(arg, @"^((--|/)" + longForm + "|(-|/)" + shortForm + ")", RegexOptions.IgnoreCase);
            argument = null;
            if (!m.Success) {
                return false;
            }
            if (!requireArgument && arg.Length != m.Value.Length) {
                return false;
            }
            if (requireArgument) {
                if (m.Value.Length == arg.Length) {
                    Console.Error.WriteLine("ERROR: Missing argument for option " + arg);
                    While.Environment.Exit(1);
                }
                if (arg[m.Value.Length] != ':' && arg[m.Value.Length] != '=') {
                    return false;
                }
                if (m.Value.Length+1 == arg.Length) {
                    Console.Error.WriteLine("ERROR: Missing argument for option " + arg);
                    While.Environment.Exit(1);
                }
                argument = arg.Substring(m.Value.Length + 1);
            }
            return true;
        }

        private bool _debug = false;
        public bool Debug { get { return _debug; } }

        private string _outputFile = null;
        public string OutputFilename { get { return _outputFile; } }

        private List<string> _plugins = new List<string>();
        public List<string> Plugins { get { return _plugins; } }

        private string _inputFile = "";
        public string InputFilename { get { return _inputFile; } }

        private bool _readStdIn = false;
        public bool ReadStdIn { get { return _readStdIn; } }

        private bool _empty = false;
        public bool Empty { get { return _empty; } }

        private bool _help = false;
        public bool Help { get { return _help; } }

        private bool _bookVersion = true;
        public bool BookVersion {
            get { return _bookVersion; }
            set { _bookVersion = value; }
        }


        public static void Print() {
            Console.Error.WriteLine(@"
            Compiler Options:
            	
/? or /help            Print this help message

/out:<filename>        Specify the name of the compiled executable. If
/o:<filename>          this is not specified the name of the input file
                       plus .exe will be used.

/debug                 Include debug information in compiled file 
/d      

/plugins:<p1>[,p2...]  Load the given plugins and let them process the
/p:<p1>[,p2]           abstract syntax tree before compilation. Plugins
                       implement the ICompilerPlugin interface and are
                       stored in a plugins folder in the folder where
                       wc.exe resides. 

/coursesyntax          Use the modified syntax as I learned it in the
/c                     Program Analysis course taught at DTU. This means
                       that () are not used for if and while blocks, 
                       instead if and while blocks end with fi and od
                       respectively. This switch also means that all 
                       variables must be declared before use inside of
                       begin-end blocks. 
                       
                       Example:
                       
                       begin
                           var x;
                           x := 3;
                           if x < 4 then
                               write 1
                           else
                               write 2
                           fi;
                           begin
                               var y;
                               y := x;
                               while x < 20 do
                                   x := x +1;
                                   skip
                               od
                           end;
                           skip
                       end
                           
");
        }
    }
}