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
using While;
using While.AST;
using While.AST.Expressions;
using While.AST.Sequences;
using While.Parsing;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System;

namespace While.AST.Statements {

    public abstract class Statement : Node {
        protected string Indent(string str) {
            return "\t" + str.ToString().Replace("\n", "\n\t");
        }
    }


    public class VariableDeclaration : Statement {

        public Variable Variable {
            get { return (Variable)this[0]; }
            set { this[0] = value; }
        }

        public VariableDeclaration(Variable var) {
            AddChild(var);
        }

        public override string ToString() {
            return "var " + Variable.ToString();
        }

        public override void Compile(ILGenerator il) {
            EmitDebugInfo(il, 0, true);
            SymbolTable.DefineVariable(Variable.Name);
            LocalBuilder lb = il.DeclareLocal(typeof(int));
            if (Options.Debug) {
                lb.SetLocalSymInfo(Variable.Name);
            }
        }
    }


    /// <summary>
    /// Assign an integer expression to a variable
    /// </summary>
    public class Assign : Statement {

        public Variable Variable {
            get { return (Variable)this[0]; }
            set { this[0] = value; }
        }

        public TypedExpression<int> Expression {
            get { return (TypedExpression<int>)this[1]; }
            set { this[1] = value; }
        }

        public Assign(Variable var, TypedExpression<int> exp) {
            AddChild(var);
            AddChild(exp);
        }

        public override string ToString() {
            return Variable.ToString() + " := " + Expression.ToString();
        }

        public override void Compile(ILGenerator il) {
            EmitDebugInfo(il, 0, false);

            //Declare at first use
            if (Options.BookVersion && !SymbolTable.IsInScope(Variable.Name)) {
                SymbolTable.DefineVariable(Variable.Name);
                LocalBuilder lb = il.DeclareLocal(typeof(int));
                if (Options.Debug) {
                    lb.SetLocalSymInfo(Variable.Name);
                }
            }

            if (SymbolTable.IsResultArgument(Variable.Name)) {
                il.Emit(OpCodes.Ldarg, SymbolTable.GetValue(Variable.Name));
                Expression.Compile(il);
                il.Emit(OpCodes.Stind_I4);
            } else if (SymbolTable.IsArgument(Variable.Name)) {
                Expression.Compile(il);
                il.Emit(OpCodes.Starg, SymbolTable.GetValue(Variable.Name));
            } else {
                Expression.Compile(il);
                il.Emit(OpCodes.Stloc, SymbolTable.GetValue(Variable.Name));
            }
        }
    }

    public class Skip : Statement {

        public override string ToString() {
            return "skip";
        }

        public override void Compile(ILGenerator il) {
            EmitDebugInfo(il, 0, true);
            //Nop only emitted in debug build, otherwise nothing is emitted
        }
    }

    public class Call : Statement {

        private string _name;
        public string ProcedureName {
            get { return _name; }
            set { _name = value; }
        }

        private Token _callToken;
        private Token _lastArgToken;

        public List<Node> Expressions {
            get { return this.ChildNodes; }
        }

        public Call(string name, List<Expression> expressions, Token callToken, Token lastArgToken) {
            foreach (Expression ex in expressions) {
                AddChild(ex);
            }
            _name = name;
            _callToken = callToken;
            _lastArgToken = lastArgToken;
        }

        public override string ToString() {
            return string.Format("call {0}({1})", ProcedureName, Join(this, ", "));
        }

        public void SanityCheck() {
            int l = _callToken.line;
            int c = _callToken.col;

            if (!WhileProgram.Instance.Procedures.ContainsProcedure(ProcedureName)) {
                System.Console.Error.WriteLine(string.Format("({0},{1}) ERROR: Procedure '{2}' is not defined", l, c, ProcedureName));
                global::While.Environment.Exit(1);
            }

            Procedure proc = WhileProgram.Instance.Procedures.GetByName(ProcedureName);
            if (this.Expressions.Count != proc.ArgumentCount) {
                System.Console.Error.WriteLine(string.Format("({0},{1}) ERROR: Procedure '{2}' does not take {3} arguments", l, c, ProcedureName, Expressions.Count));
                global::While.Environment.Exit(1);
            }

            if (proc.HasResultArgument && !(Expressions[Expressions.Count - 1] is Variable)) {
                System.Console.Error.WriteLine(string.Format("({0},{1}) ERROR: Only variables are allowed for result arguments", _lastArgToken.line, _lastArgToken.col));
                global::While.Environment.Exit(1);
            }
        }

        public override void Compile(ILGenerator il) {
            EmitDebugInfo(il, 0, false);
            SanityCheck();
            if (this.ChildNodes.Count > 0) {
                for (int i = 0; i < ChildNodes.Count - 1; i++) {
                    this[i].Compile(il);
                }
                Procedure proc = WhileProgram.Instance.Procedures.GetByName(ProcedureName);
                if (proc.HasResultArgument) {
                    Variable v = (Variable)this[ChildNodes.Count - 1];
                    //Create at first use
                    if (Options.BookVersion && !SymbolTable.IsInScope(v.Name)) {
                        SymbolTable.DefineVariable(v.Name);
                        LocalBuilder lb = il.DeclareLocal(typeof(int));
                        if (Options.Debug) {
                            lb.SetLocalSymInfo(v.Name);
                        }
                    }

                    if (SymbolTable.IsResultArgument(v.Name)) {
                        il.Emit(OpCodes.Ldarg, SymbolTable.GetValue(v.Name));
                    } else if (SymbolTable.IsArgument(v.Name)) {
                        il.Emit(OpCodes.Ldarga, SymbolTable.GetValue(v.Name));
                    } else {
                        il.Emit(OpCodes.Ldloca, SymbolTable.GetValue(v.Name));
                    }

                } else {
                    this[ChildNodes.Count - 1].Compile(il);
                }
            }
            il.Emit(OpCodes.Call, WhileProgram.Instance.Procedures.Compiled[ProcedureName]);
        }
    }

    /// <summary>
    /// Write an expression to the screen
    /// </summary>
    public class Write : Statement {

        public Expressions.Expression Expression {
            get { return (Expressions.Expression)this[0]; }
            set { this[0] = value; }
        }

        public Write(Expression exp) {
            AddChild(exp);
        }

        public override string ToString() {
            return "write " + Expression.ToString();
        }

        public override void Compile(ILGenerator il) {
            EmitDebugInfo(il, 0, false);
            Expression.Compile(il);
            MethodInfo mi;
            Type[] argTypes = new Type[1];
            if (Expression is TypedExpression<bool>) {
                argTypes[0] = typeof(bool);
            } else if (Expression is TypedExpression<int>) {
                argTypes[0] = typeof(int);
            }
            mi = typeof(System.Console).GetMethod("WriteLine", argTypes);
            il.Emit(OpCodes.Call, mi);
        }
    }

    /// <summary>
    /// Read an integer from the user
    /// </summary>
    public class Read : Statement {

        public Variable Variable {
            get { return (Variable)this[0]; }
            set { this[0] = value; }
        }

        public Read(Variable var) {
            AddChild(var);
        }

        public override string ToString() {
            return "read " + Variable;
        }

        public override void Compile(ILGenerator il) {
            EmitDebugInfo(il, 0, false);
            Label startLabel = il.DefineLabel();
            il.MarkLabel(startLabel);
            il.Emit(OpCodes.Ldstr, Variable.Name + ": ");
            il.Emit(OpCodes.Call, typeof(System.Console).GetMethod("Write", new Type[] { typeof(string) }));
            il.Emit(OpCodes.Call, typeof(System.Console).GetMethod("ReadLine"));

            if (Options.BookVersion && !SymbolTable.IsInScope(Variable.Name)) {
                SymbolTable.DefineVariable(Variable.Name);
                LocalBuilder lb = il.DeclareLocal(typeof(int));
                if (Options.Debug) {
                    lb.SetLocalSymInfo(Variable.Name);
                }
            }

            if (SymbolTable.IsResultArgument(Variable.Name)) {
                il.Emit(OpCodes.Ldarg, SymbolTable.GetValue(Variable.Name));
            } else if (SymbolTable.IsArgument(Variable.Name)) {
                il.Emit(OpCodes.Ldarga_S, SymbolTable.GetValue(Variable.Name));
            } else {
                il.Emit(OpCodes.Ldloca_S, SymbolTable.GetValue(Variable.Name));
            }

            il.Emit(OpCodes.Call, typeof(int).GetMethod("TryParse", new Type[] { typeof(string), typeof(int).MakeByRefType() }));
            il.Emit(OpCodes.Brfalse, startLabel);
        }
    }
    
    
    /// <summary>
    /// Block with variable declarations && statements
    /// </summary>
    public class Block : Statement {

        public VariableDeclarationSequence Variables {
            get { return (VariableDeclarationSequence) this[0];}
            set { this[0] = value;}
        }

        public StatementSequence Statements {
            get { return (StatementSequence) this[1];}
            set { this[1] = value;}
        }

        public Block(VariableDeclarationSequence vars, StatementSequence stmts) {
            AddChild(vars);
            AddChild(stmts);
        }

        public override string ToString() {
            return string.Format("begin\n{0}\n{1}\nend", Indent(Variables.ToString()), Indent(Statements.ToString()));
        }

        public override void Compile(ILGenerator il) {
            SymbolTable.PushScope();
            il.BeginScope();
            EmitDebugInfo(il, 0, true);
            if (Options.Debug) {
                il.Emit(OpCodes.Nop); //To step correctly
            }
            if (Variables != null) {
                Variables.Compile(il);
            }
            Statements.Compile(il);
            il.EndScope();
            SymbolTable.PopScope();
            EmitDebugInfo(il, 1, true);
        }
    }

    /// <summary>
    /// If-Else branching
    /// </summary>
    public class If : Statement {

        public TypedExpression<bool> Expression {
            get { return (TypedExpression<bool>)this[0]; }
            set { this[0] = value; }
        }

        public StatementSequence IfBranch {
            get { return (StatementSequence)this[1]; }
            set { this[1] = value; }
        }

        public StatementSequence ElseBranch {
            get { return (StatementSequence)this[2]; }
            set { this[2] = value; }
        }

        public If(TypedExpression<bool> exp, StatementSequence ifBranch, StatementSequence elseBranch) {
            AddChild(exp);
            AddChild(ifBranch);
            AddChild(elseBranch);
        }

        public override string ToString() {
            string s = string.Format("if {0} then\n{1}", Expression, Indent(IfBranch.ToString()));
            if (ElseBranch != null) {
                s += string.Format("\nelse\n{0}", Indent(ElseBranch.ToString()));
            }
            s += "\nfi";
            return s;
        }

        public override void Compile(ILGenerator il) {
            EmitDebugInfo(il, 0, false);
            Expression.Compile(il);
            Label ifFalseLabel = il.DefineLabel();
            Label endLabel = il.DefineLabel();
            il.Emit(OpCodes.Brfalse, ifFalseLabel);
            IfBranch.Compile(il);
            il.Emit(OpCodes.Br, endLabel);
            il.MarkLabel(ifFalseLabel);
            if (ElseBranch != null) {
                ElseBranch.Compile(il);
            }
            il.MarkLabel(endLabel);
        }
    }

    /// <summary>
    /// While loop
    /// </summary>
    public class While : Statement {

        public While(TypedExpression<bool> exp, StatementSequence statements) {
            AddChild(exp);
            AddChild(statements);
        }

        public TypedExpression<bool> Expression {
            get { return (TypedExpression<bool>)this[0]; }
            set { this[0] = value; }
        }

        public StatementSequence Statements {
            get { return (StatementSequence)this[1]; }
            set { this[1] = value; }
        }

        public override string ToString() {
            return string.Format("while {0} do\n{1}\nod", Expression, Indent(Statements.ToString()));
        }

        public override void Compile(ILGenerator il) {
            EmitDebugInfo(il, 0, false);
            Label evalConditionLabel = il.DefineLabel();
            Label afterTheLoopLabel = il.DefineLabel();
            il.MarkLabel(evalConditionLabel);
            Expression.Compile(il);
            il.Emit(OpCodes.Brfalse, afterTheLoopLabel);
            Statements.Compile(il);
            il.Emit(OpCodes.Br, evalConditionLabel);
            il.MarkLabel(afterTheLoopLabel);
        }
    }
}
