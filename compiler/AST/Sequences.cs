using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection.Emit;
using While.AST.Statements;
using While.AST.Expressions;

namespace While.AST.Sequences {

    public class ProcedureSequence : Node{
        public override string ToString() {
            return Join(this, ";\n");
        }

        public void AddProcedure(Procedure p) {
            AddChild(p);
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

    public class StatementSequence : Node{

        public override string ToString() {
            return Join(this, ";\n");
        }

        public void AddStatement(Statement s) {
            AddChild(s);
        }

        public override void Compile(ILGenerator il) {
            if (_sequencePoints.Count > 1) { //If there's just one then we assume it is after the sequence (for "fi" && "od")
                EmitDebugInfo(il, 0, true);
            }
            foreach (Statement s in ChildNodes) {
                s.Compile(il);
            }
            if (_sequencePoints.Count > 0) {
                EmitDebugInfo(il, _sequencePoints.Count - 1, true);
            }
        }
    }

    public class VariableSequence : Node {
        public VariableSequence(List<Variable> variables) {
            foreach (Variable var in variables) {
                AddChild(var);
            }
        }
        public override void Compile(ILGenerator il) {
            return; //Do nothing
        }
    }

    public class VariableDeclarationSequence : Node {

        public override string ToString() {
            return Join(this, ";\n") + ";\n";
        }

        public void AddVariableDeclaration(VariableDeclaration vd) {
            AddChild(vd);
        }

        public override void Compile(ILGenerator il) {
            foreach (VariableDeclaration v in this) {
                v.Compile(il);
            }
        }
    }
}
