/**
 * $Id: Resolver.cs 168 2009-05-28 10:24:29Z eboeg $ 
 * 
 * This file is part of the Process Language Runtime (PLR) 
 * and is licensed under the GPL v3.0.
 * 
 * Author: Einar Egilsson (einar@einaregilsson.com) 
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Babel;
using System.Reflection;

namespace CCS.LanguageService
{
	public class Resolver 	{
        private List<string> channels = new List<string>();
        private List<string> processes = new List<string>();
        private List<string> classes = new List<string>();
        private List<string> importedClasses = new List<string>();
        private string source;
        private string[] lines;
        public Resolver(string source) {
            this.source = source;
            lines = source.Split('\n');
            ParseItems();
        }

        private void ParseItems() {
            //Scanner scanner = new Scanner();
            //scanner.SetSource(source, 0);
            //int state=0,start,end;
            //int tokenType = -1;
            //while (tokenType != ((int)Tokens.EOF)) {
            //    tokenType = scanner.GetNext(ref state, out start, out end);
            //    if (tokenType == (int)Tokens.LCASEIDENT || tokenType == (int)Tokens.OUTACTION) {
            //        string channel = scanner.yytext.Replace("_", "");
            //        if (!channels.Contains(channel)) {
            //            channels.Add(channel);
            //        }
            //    } else if (tokenType == (int)Tokens.PROC) {
            //        if (!processes.Contains(scanner.yytext)) {
            //            processes.Add(scanner.yytext);
            //        }
            //    } else if (tokenType == (int)Tokens.FULLCLASS) {
            //        if (!importedClasses.Contains(scanner.yytext)) {
            //            importedClasses.Add(scanner.yytext);
            //        }
            //    }
            //}
        }

		#region IASTResolver Members

        private List<string> FindMethods() {
            List<string> methods = new List<string>();
            if (this.importedClasses.Contains("PLR.Runtime.BuiltIns")) {
                this.importedClasses.Remove("PLR.Runtime.BuiltIns");
            }

            foreach (string clazz in this.importedClasses) {
                Type type = Type.GetType(clazz);
                if (type != null) {
                    methods.AddRange(GetMethodsFromType(type));
                }
            }
            return methods;
        }
        private List<string> GetMethodsFromType(Type type) {
            List<string> methods = new List<string>();
            MethodInfo[] staticMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
            foreach (MethodInfo mi in staticMethods) {
                if (!mi.Name.StartsWith("get_") && !mi.Name.StartsWith("set_")) {
                    string name = mi.Name + "(";
                    bool canUse = true;
                    foreach (ParameterInfo pi in mi.GetParameters()) {
                        if (name[name.Length - 1] != '(') {
                            name += ", ";
                        }
                        if (!pi.ParameterType.Equals(typeof(string)) && !pi.ParameterType.Equals(typeof(int))) {
                            canUse = false;
                            break;
                        }
                        if (pi.IsOut || pi.IsRetval) {
                            canUse = false;
                        }
                        if (pi.ParameterType.Equals(typeof(string))) {
                            name += "string " + pi.Name;
                        }
                        if (pi.ParameterType.Equals(typeof(int))) {
                            name += "int " + pi.Name;
                        }
                    }
                    if (canUse) {
                        methods.Add(name + ")" + "class=" + type.FullName);
                    }
                }
            }
            return methods;
        }


		public IList<Babel.Declaration> FindCompletions(object result, int line, int col)
		{
            string currLine = this.lines[line].Substring(0, col);
            Match m = Regex.Match(currLine, @"[a-zA-Z_]\w*$");
            string tipWord = "";
            if (m.Success) {
                tipWord = m.Groups[0].Value;
            }

            var completions = new List<Declaration>();
            foreach (string channel in channels) {
                if (channel != tipWord.Replace("_", "")) {
                    string outChannel = "_" + channel + "_";
                    completions.Add(new Declaration(" " + channel + "\n\n Receive on the channel " + channel + " ", channel, 36, channel));
                    completions.Add(new Declaration(" " + outChannel + "\n\n Send on the channel " + channel + " ", outChannel, 37, outChannel));
                }
            }

            foreach (string proc in processes) {
                if (proc != tipWord) {
                    completions.Add(new Declaration(" " + proc + "\n\n Invoke or define the process " + proc + " ",proc, 146, proc));
                }
            }

            List<string> availableMethods = FindMethods();
            foreach (string method in availableMethods) {
                string methodStripped = method.Substring(0, method.IndexOf('('));
                string[] parts = Regex.Split(method, "class=");
                string className = parts[1];
                string goodMethod = parts[0];
                if (methodStripped != tipWord) {
                    completions.Add(new Declaration(" :" + goodMethod + "\n\n Call the method " + methodStripped + " from the class " + className + " ", ":" + methodStripped, 72, ":" + methodStripped));
                }
            }

            //Keywords
            completions.Add(new Declaration(" use\n\n A keyword to be followed by a fully qualified class name ", "use", 206, "use"));
            completions.Add(new Declaration(" true\n\n The boolean constant 'true' ", "true", 206, "true"));
            completions.Add(new Declaration(" false\n\n The boolean constant 'false' ", "false", 206, "false"));
            completions.Add(new Declaration(" and\n\n The logical operation 'and' ", "and", 206, "and"));
            completions.Add(new Declaration(" or\n\n The logical operation 'or' ", "or", 206, "or"));
            completions.Add(new Declaration(" xor\n\n The logical operation 'xor' ", "xor", 206, "xor"));
            completions.Add(new Declaration(" if\n\n The start token of a conditional process, e.g. 'if <cond> then <proc1> else <proc2>' ", "if", 206, "if"));
            completions.Add(new Declaration(" then\n\n The then token in an if process, e.g. 'if <cond> then <proc1> else <proc2>' ", "then", 206, "then"));
            completions.Add(new Declaration(" else\n\n The else token in an if process, e.g. 'if <cond> then <proc1> else <proc2>' ", "else", 206, "else"));
            completions.Add(new Declaration(" 0\n\n Terminate a process by turning into the nil process, 0 ", "0", 206, "0"));

            completions.Sort(delegate(Declaration d1, Declaration d2) {
                if (d1.Name == "_" + d2.Name + "_") {
                    return -1;
                } else if (d2.Name == "_" + d1.Name + "_") {
                    return 1;
                }

                return d1.Name.Replace("_", "").Replace(":", "").CompareTo(d2.Name.Replace("_", "").Replace(":", ""));
            });
            return completions;
		}

		public IList<Babel.Declaration> FindMembers(object result, int line, int col)
		{
			// ManagedMyC.Parser.AAST aast = result as ManagedMyC.Parser.AAST;
			List<Babel.Declaration> members = new List<Babel.Declaration>();

            //foreach (string state in aast.startStates.Keys)
            //    members.Add(new Declaration(state, state, 0, state));
			return members;
		}

		public string FindQuickInfo(object result, int line, int col)
		{
			return "unknown";
		}


		#endregion
	}
}
