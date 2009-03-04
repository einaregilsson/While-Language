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
using System.Diagnostics.SymbolStore;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using While;
using While.AST.Sequences;
using While.AST.Statements;


namespace While.AST {

    /// <summary>
    ///	Root node in the abstract syntax tree. Has a static property, .Instance;
    ///	that always contains the last parsed tree. 
    /// </summary>
    public class WhileProgram : Node {

        public WhileProgram(ProcedureSequence procs, StatementSequence stmts) {
            AddChild(procs);
            AddChild(stmts);
        }

        public ProcedureSequence Procedures {
            get { return (ProcedureSequence)this[0]; }
            set { this[0] = value; }
        }

        public StatementSequence Statements {
            get { return (StatementSequence)this[1]; }
            set { this[1] = value; }
        }

        private static WhileProgram _instance;
        public static WhileProgram Instance {
            get { return _instance; }
            set { _instance = value; }
        }

        public override string ToString() {
            return Procedures + "\n\n" + Statements.ToString();
        }

        public override void Compile(ILGenerator il) {
            //do nothing
        }

        public void Compile(string filename) {
            AssemblyName name = new AssemblyName(filename);
            AssemblyBuilder assembly = Thread.GetDomain().DefineDynamicAssembly(name, AssemblyBuilderAccess.RunAndSave);
            Node.Module = assembly.DefineDynamicModule(filename, Options.Debug);
            MethodBuilder mainMethod = Module.DefineGlobalMethod("Main", MethodAttributes.HideBySig | MethodAttributes.Static | MethodAttributes.Public, typeof(void), new Type[] { });

            if (Options.Debug) {
                Node.DebugWriter = Module.DefineDocument(Options.InputFilename, Guid.Empty, Guid.Empty, SymDocumentType.Text);
            }

            Procedures.Compile(null);

            ILGenerator il = mainMethod.GetILGenerator();
            if (Options.BookVersion) {
                SymbolTable.PushScope();
            }

            if (SequencePoints.Count > 0) { //Make possible to start on "begin" statement
                EmitDebugInfo(il, 0, true);
            }

            Statements.Compile(il);
            if (SequencePoints.Count > 0) { //Make possible to end on "end" statement
                EmitDebugInfo(il, 1, true);
            }
            il.Emit(OpCodes.Ret);


            Module.CreateGlobalFunctions();
            assembly.SetEntryPoint(mainMethod, PEFileKinds.ConsoleApplication);
            if (Options.Debug) {
                Module.SetUserEntryPoint(mainMethod);
            }
            assembly.Save(filename);
        }
    }
}
