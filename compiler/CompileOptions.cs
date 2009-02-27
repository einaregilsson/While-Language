/*
 * While Compiler
 * http://code.google.com/p/while-language/
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
using System.Text.RegularExpressions;

namespace While {

    /// <summary>
    /// Class for parsing command line options.
    /// The options are static available properties;
    /// so essentially they are global variables that;
    /// anything in the assembly can access.
    /// </summary>
    public static class CompileOptions {

        private static bool _debug = false;
        public static bool Debug { get { return _debug; } }

        private static string _outputFile = "";
        public static string OutputFilename { get { return _outputFile; } }

        private static string _inputFile = "";
        public static string InputFilename { get { return _inputFile; } }

        private static bool _readStdIn = false;
        public static bool ReadStdIn { get { return _readStdIn; } }

        private static bool _empty = false;
        public static bool Empty { get { return _empty; } }

        private static bool _help = false;
        public static bool Help { get { return _help; } }

        private static bool _bookVersion = true;
        public static bool BookVersion { get { return _bookVersion; } }

        public static void Init(string[] args) {
            if (args.Length == 0) {
                _empty = true;
                return;
            }
            bool gotOut = false;
            _inputFile = args[args.Length - 1];
            _outputFile = Path.GetFileName(Regex.Replace(@"\.w(hile)?$", _inputFile, ""));

            foreach (string arg in args) {
                string larg = arg.ToLower();
                _help |= larg == "/?" || larg == "/help";
                _debug |= larg == "/debug";

                if (larg == "/coursesyntax") {
                    _bookVersion = false;
                }
                if (larg.StartsWith("/out:")) {
                    gotOut = true;
                    _outputFile = larg.Substring(5);
                }
            }

            if (_inputFile.ToLower() == "stdin") {
                _readStdIn = true;
                if (!gotOut) {
                    Console.Error.WriteLine("ERROR: /out:<filename> must be specified when reading source from the standard input stream (STDIN).");
                    Environment.Exit(4);
                }
            }

            if (!_outputFile.ToLower().EndsWith(".exe")) {
                _outputFile += ".exe";
            }
        }

        public static void Print() {
            Console.Error.WriteLine(@"
            Compiler Options:
            	
/? or /help            Print this help message

/out:<filename>        Specify the name of the compiled executable

/debug                 Include debug information in compiled file

/coursesyntax          Use the modified syntax as I learned it in the
                       Program Analysis course taught at DTU. This means
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