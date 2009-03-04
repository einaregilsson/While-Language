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
using System.Reflection.Emit;
using System.Text;
using While.AST.Expressions;
using While.AST.Statements;

// This module contains all Sequence nodes. These nodes are simple
// collections of one type of childnode, for example Statements
// or VariableDeclaration's.
namespace While.AST.Sequences {

    public class ProcedureSequence : Node{

        public void AddProcedure(Procedure p) {
            AddChild(p);
        }

        public override string ToString() {
            return Join(this, ";\n");
        }

        private Dictionary<string, MethodBuilder> _compiledProcs = new Dictionary<string, MethodBuilder>();
        public Dictionary<string, MethodBuilder> Compiled {
            get { return _compiledProcs; }
        }

        public Procedure GetByName(string name) {
            foreach (Procedure p in this) {
                if (p.Name == name) {
                    return p;
                }
            }
            return null;
        }
        
        public bool ContainsProcedure(string name) {
            return GetByName(name) != null;
        }

        public override void Compile(ILGenerator il) {
            //First compile the method signatures...
            foreach (Procedure proc in this) {
                MethodBuilder method = proc.CompileSignature(Module);
                _compiledProcs.Add(proc.Name, method);
            }

            //...and then the method bodies
            foreach (Procedure proc in this) {
                MethodBuilder method = _compiledProcs[proc.Name];
                proc.Compile(method.GetILGenerator());
            }
        }
    }

    public class StatementSequence : Statement {

        public void AddStatement(Statement s) {
            AddChild(s);
        }

        public override string ToString() {
            return Join(this, ";\n");
        }

        public override void Compile(ILGenerator il) {
            if (SequencePoints.Count > 1) { //If there's just one then we assume it is after the sequence (for "fi" && "od")
                EmitDebugInfo(il, 0, true);
            }
            foreach (Statement s in ChildNodes) {
                s.Compile(il);
            }
            if (SequencePoints.Count > 0) {
                EmitDebugInfo(il, SequencePoints.Count - 1, true);
            }
        }
    }

    public class VariableSequence : Node {

        public void AddVariable(Variable v) {
            AddChild(v);
        }

        public override void Compile(ILGenerator il) {
            return; //Do nothing
        }
    }

    public class VariableDeclarationSequence : Node {

        public void AddVariableDeclaration(VariableDeclaration vd) {
            AddChild(vd);
        }

        public override string ToString() {
            return Join(this, ";\n") + ";\n";
        }

        public override void Compile(ILGenerator il) {
            foreach (VariableDeclaration v in this) {
                v.Compile(il);
            }
        }
    }
}
