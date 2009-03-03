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
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using While.AST.Expressions;
using While.AST.Statements;
using While.AST.Sequences;

namespace While.AST {

    /// <summary>
    /// Procedure class. A procedure can take 0-1 value arguments and;
    /// 0-1 result argument that must be the last one.
    /// </summary>
    public class Procedure : Node {

        public VariableSequence ValueArguments {
            get { return (VariableSequence)this[0]; }
        }

        public Variable ResultArgument {
            get { return (Variable)this[1]; }
            set { this[1] = value; }
        }

        public StatementSequence Statements {
            get { return (StatementSequence)this[2]; }
            set { this[2] = value; }
        }

        string _name;
        public string Name {
            get { return _name; }
            set { _name = value; }
        }

        public int ArgumentCount {
            get {
                if (ResultArgument != null) {
                    return ValueArguments.ChildNodes.Count + 1;
                } else {
                    return ValueArguments.ChildNodes.Count;
                }
            }
        }

        public bool HasResultArgument {
            get { return ResultArgument != null; }
        }

        public Procedure(string name, VariableSequence valArgs, Variable resultArg, StatementSequence statements) {
            AddChild(valArgs);
            AddChild(resultArg);
            AddChild(statements);
            _name = name;
        }



        public override void Compile(ILGenerator il) {
            foreach (Variable arg in ValueArguments) {
                SymbolTable.DefineArgument(arg.Name);
            }
            if (HasResultArgument) {
                SymbolTable.DefineResultArgument(ResultArgument.Name);
            }
            EmitDebugInfo(il, 0, true);
            Statements.Compile(il);
            EmitDebugInfo(il, 1, true);
            il.Emit(OpCodes.Ret);
            SymbolTable.Clear();
        }

        /// <summary>
        /// Compiles the signature for the procedure but not the body.
        /// This needs to be done first so that other methods can 
        /// call this method, this way we don't have problems with;
        /// dependencies between methods.
        /// </summary>
        public MethodBuilder CompileSignature(ModuleBuilder module) {
            int argCount = ValueArguments.ChildNodes.Count;
            if (HasResultArgument) {
                argCount++;
            }
            Type[] argTypes = new Type[argCount];

            for (int i = 0; i < argTypes.Length; i++) {
                argTypes[i] = typeof(int);
            }
            if (HasResultArgument) {
                argTypes[argTypes.Length - 1] = typeof(int).MakeByRefType();
            }

            MethodBuilder method = module.DefineGlobalMethod(_name, MethodAttributes.HideBySig | MethodAttributes.Static | MethodAttributes.Public, typeof(void), argTypes);
            int pos = 1;
            foreach (Variable arg in ValueArguments) {
                SymbolTable.DefineArgument(arg.Name);
                method.DefineParameter(pos, ParameterAttributes.In, arg.Name);
                pos++;
            }

            if (HasResultArgument) {
                SymbolTable.DefineArgument(ResultArgument.Name);
                method.DefineParameter(pos, ParameterAttributes.Out, ResultArgument.Name);
            }
            SymbolTable.Clear();
            return method;
        }

        public override string ToString() {
            string s = "procedure " + _name + "(";
            if (ValueArguments.ChildNodes.Count > 0) {
                s += "val ";
                s += ValueArguments[0];
                for (int i = 1; i < ValueArguments.ChildNodes.Count; i++) {
                    s += ", " + ValueArguments[i];
                }
                if (HasResultArgument) {
                    s += ", res " + ResultArgument;
                }
            } else if (HasResultArgument) {
                s += "res " + ResultArgument;
            }
            s += ")\n";
            s += "\t" + Statements.ToString().Replace("\n", "\n\t");
            s += "\nend;";
            return s;
        }
    }
}