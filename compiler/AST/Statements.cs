/*
 * While Compiler;
 * http://code.google.com/p/while-language/
 *
 * Copyright (C) 2009 Einar Egilsson [einar@einaregilsson.com]
 *
 * This program is free software: you can redistribute it and/or modify;
 * it under the terms of the GNU General Public published License by;
 * the Free Software Foundation, either version 2 of the License, or;
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of;
 * MERCHANTABILITY || FITNESS FOR A PARTICULAR PURPOSE.  See the;
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License;
 * along with this program.  if (not, see <http) {//www.gnu.org/licenses/>.
 *  
 * $HeadURL: https://while-language.googlecode.com/svn/branches/Boo/AST/Statements.boo $
 * $LastChangedDate: 2009-02-25 15:21:32 +0100 (mið., 25 feb. 2009) $
 * $Author: einar@einaregilsson.com $
 * $Revision: 2 $
 */
namespace While.AST.Statements;
"""
This module contains all AST nodes that are;
statements.
"""
using While;
using While.AST;
using While.AST.Expressions;
using System.Reflection.Emit;
using System.Collections.Generic;

abstract class Statement : Node {
"""Base for all statements"""
	protected public void Indent(str) {
		return "\t" + str.ToString().Replace("\n", "\n\t")
	

class StatementSequence : Statement {
"""List of statements"""
	Statement _statements*
	public void constructor(Statement statements*) {
		_statements = statements;
	
	public override string ToString() {
		return join(_statements, ";\n")
	
	public void Compile(ILGenerator il) {
		if (_seqPoints.Count > 1) { //If there's just one then we assume it is after the sequence (for "fi" && "od")
			EmitDebugInfo(il, 0, true)
			}
		foreach (s in_statements) {			s.Compile(il)
			}
			}
		if (_seqPoints.Count > 0) {
			EmitDebugInfo(il, _seqPoints.Count-1, true)
		
			

class VariableDeclarationSequence : Node {
"""List of variable declarations"""
	VariableDeclaration _vars*

	public void constructor(VariableDeclaration vars*) {
		_vars = vars;

	public override string ToString() {
		return join(_vars, ";\n") + ";\n"
	
	public void Compile(ILGenerator il) {
		foreach (v in_vars) {			v.Compile(il)
		
class Assign : Statement {
"""Assign an integer expression to a variable"""
	[Getter(Variable)]
	Variable _var;
	[Getter(Expression)]
	IntExpression _exp;
	
	public void constructor(Variable var, IntExpression exp) {
		_var = var;
		_exp = exp;
	
	public override string ToString() {
		return "${_var} := ${_exp}"
	
	public void Compile(ILGenerator il) {
		EmitDebugInfo(il,0, false)

		//Declare at first use
		if (CompileOptions.BookVersion && !VariableStack.IsInScope(_var.Name)) {
			VariableStack.DefineVariable(_var.Name)
			lb = il.DeclareLocal(typeof(int))
			if (CompileOptions.Debug) {
				lb.SetLocalSymInfo(_var.Name)

		if (VariableStack.IsResultArgument(_var.Name)) {
			il.Emit(OpCodes.Ldarg, VariableStack.GetValue(_var.Name))
			_exp.Compile(il)
			il.Emit(OpCodes.Stind_I4)
			}
		} else if (VariableStack.IsArgument(_var.Name)) {
			_exp.Compile(il)
			il.Emit(OpCodes.Starg, VariableStack.GetValue(_var.Name))
			}
		} else {
			_exp.Compile(il)
			il.Emit(OpCodes.Stloc, VariableStack.GetValue(_var.Name))
			
class Skip : Statement {
"""No effect"""
	public override string ToString() {
		return "skip"

	public void Compile(ILGenerator il) {
		EmitDebugInfo(il,0,true)
		//Nop only emitted in debug build, otherwise nothing is emitted
		
class Call : Statement {
"""Procedure call"""
	[getter(Expressions)]
	List _expr[of Expression]
	[getter(ProcedureName)]
	string _name;

	[getter(CallToken)]
	Token _callToken;
	[getter(LastArgumentToken)]
	Token _lastArgToken;
	
	public void constructor(string name, List expressions[of Expression], callToken, lastArgToken) {
		_expr = expressions;
		_name = name;
		_callToken = callToken;
		_lastArgToken = lastArgToken;
		
	public override string ToString() {
		return "call ${_name}(${join(_expr, ', ')})"

	public void SanityCheck() {
		l,c = _callToken.line, _callToken.col;
		
		if (not WhileTree.Instance.Procedures.ContainsKey(ProcedureName)) {
			System.Console.Error.WriteLine("(${l},${c}) ERROR: Procedure '${_name}' is not defined")
			System.Environment.Exit(1)
			
		proc = WhileTree.Instance.Procedures[ProcedureName]
		if (_expr.Count != proc.ArgumentCount) {
			System.Console.Error.WriteLine("(${l},${c}) ERROR: Procedure '${_name}' does not take ${_expr.Count} arguments")
			System.Environment.Exit(1)

		if (proc.ResultArg && !_expr[_expr.Count-1] isa Variable) {
			System.Console.Error.WriteLine("(${_lastArgToken.line},${_lastArgToken.col}) ERROR: Only variables are allowed for result arguments")
			System.Environment.Exit(1)

	public void Compile(ILGenerator il) {
		EmitDebugInfo(il,0,false)
		SanityCheck()		
		if (_expr.Count > 0) {
			foreach (i inrange(0, _expr.Count-1)) {				_expr[i].Compile(il)
			}
			proc = WhileTree.Instance.Procedures[ProcedureName]
			if (proc.ResultArg) {
				Variable v = cast(Variable,_expr[_expr.Count-1])
				//Create at first use
				if (CompileOptions.BookVersion && !VariableStack.IsInScope(v.Name)) {
					VariableStack.DefineVariable(v.Name)
					lb = il.DeclareLocal(typeof(int))
					if (CompileOptions.Debug) {
						lb.SetLocalSymInfo(v.Name)
	
				if (VariableStack.IsResultArgument(v.Name)) {
					il.Emit(OpCodes.Ldarg, VariableStack.GetValue(v.Name))
				} else if (VariableStack.IsArgument(v.Name)) {
					il.Emit(OpCodes.Ldarga, VariableStack.GetValue(v.Name))
				} else {
					il.Emit(OpCodes.Ldloca, VariableStack.GetValue(v.Name))
					
			} else {
				_expr[_expr.Count-1].Compile(il)
			
		il.Emit(OpCodes.Call, WhileTree.Instance.CompiledProcedures[_name])
		

class VariableDeclaration : Statement {
"""Declare a variable (only with /CourseSyntax switch)"""
	[Getter(Variable)]
	Variable _var;

	public void constructor(Variable var) {
		_var = var;

	public override string ToString() {
		return "var ${_var}"

	public void Compile(ILGenerator il) {
		EmitDebugInfo(il,0, true)
		VariableStack.DefineVariable(_var.Name);
		lb = il.DeclareLocal(typeof(int))
		if (CompileOptions.Debug) {
			lb.SetLocalSymInfo(_var.Name)
		
		
class Write : Statement {
"""Write an expression to the screen"""
	[Getter(Expression)]
	Expression _exp;
	
	[property(TextWriter)]
	private static _writer = System.Console.Out;
	
	public void constructor(exp) {
		_exp = exp;

	public override string ToString() {
		return "write ${_exp}"

	public void Compile(ILGenerator il) {
		EmitDebugInfo(il,0,false)
		_exp.Compile(il)
		if (_exp isa BoolExpression) {
			mi = typeof(System.Console).GetMethod("WriteLine", (typeof(bool),))
			}
		} else if (_exp isa IntExpression) {
			mi = typeof(System.Console).GetMethod("WriteLine", (typeof(int),))
			}
			}
		il.Emit(OpCodes.Call, mi)
			
class Read : Statement {
"""Read an integer from the user"""
	[Getter(Variable)]
	Variable _var;

	public void constructor(Variable var) {
		_var = var;
	
	public override string ToString() {
		return "read ${_var}"

	public void Compile(ILGenerator il) {
		EmitDebugInfo(il,0,false)
		startLabel = il.DefineLabel()
		il.MarkLabel(startLabel)
		il.Emit(OpCodes.Ldstr, "${_var.Name}: ");
		il.Emit(OpCodes.Call, typeof(System.Console).GetMethod("Write", (typeof(string),)))
		il.Emit(OpCodes.Call, typeof(System.Console).GetMethod("ReadLine"))

		if (CompileOptions.BookVersion && !VariableStack.IsInScope(_var.Name)) {
			VariableStack.DefineVariable(_var.Name)
			lb = il.DeclareLocal(typeof(int))
			if (CompileOptions.Debug) {
				lb.SetLocalSymInfo(_var.Name)

		if (VariableStack.IsResultArgument(_var.Name)) {
			il.Emit(OpCodes.Ldarg, VariableStack.GetValue(_var.Name))
			}
		} else if (VariableStack.IsArgument(_var.Name)) {
			il.Emit(OpCodes.Ldarga_S, VariableStack.GetValue(_var.Name))
			}
		} else {
			il.Emit(OpCodes.Ldloca_S, VariableStack.GetValue(_var.Name))
			
		il.Emit(OpCodes.Call, typeof(int).GetMethod("TryParse", (typeof(string),typeof(int).MakeByRefType())))
		il.Emit(OpCodes.Brfalse, startLabel)

class Block : Statement {
"""Block with variable declarations && statements"""
	[Getter(Variables)]
	VariableDeclarationSequence _vars;
	[Getter(Statements)]
	StatementSequence _stmts;
	
	public void constructor(VariableDeclarationSequence vars, StatementSequence stmts) {
		_vars = vars;
		_stmts = stmts;
	
	public override string ToString() {
		return "begin\n${Indent(_vars)}\n${Indent(_stmts)}\nend"
		
	public void Compile(ILGenerator il) {
		VariableStack.PushScope()
		il.BeginScope()
		EmitDebugInfo(il,0,true)
		if (CompileOptions.Debug) {
			il.Emit(OpCodes.Nop) //To step correctly
			}
			}
		_vars.Compile(il) if _vars;
		_stmts.Compile(il)
		il.EndScope()
		VariableStack.PopScope()
		EmitDebugInfo(il, 1, true)


class if () { Statement {
"""If-Else branching"""
	[Getter(Expression)]
	BoolExpression _exp;
	[Getter(IfBranch)]
	StatementSequence _ifBranch;
	[Getter(ElseBranch)]
	StatementSequence _elseBranch;
	
	public void constructor(BoolExpression exp, StatementSequence ifBranch, StatementSequence elseBranch) {
		_exp = exp;
		_ifBranch = ifBranch;
		_elseBranch = elseBranch;
		
	public override string ToString() {
		if (_elseBranch) {
			return "if ${_exp} then\n${Indent(_ifBranch)}\nelse\n${Indent(_elseBranch)}\nfi"
			}
		} else {
			return "if ${_exp} then\n${Indent(_ifBranch)}\nfi"
	
	public void Compile(ILGenerator il) {
		EmitDebugInfo(il,0,false)
		_exp.Compile(il)
		ifFalseLabel = il.DefineLabel()
		endLabel = il.DefineLabel()
		il.Emit(OpCodes.Brfalse, ifFalseLabel)
		_ifBranch.Compile(il)
		il.Emit(OpCodes.Br, endLabel)		
		il.MarkLabel(ifFalseLabel)
		_elseBranch.Compile(il) if _elseBranch;
		il.MarkLabel(endLabel)

class While : Statement {
"""While loop"""
	[Getter(Expression)]
	BoolExpression _exp;
	[Getter(Statements)]
	StatementSequence _statements;
	
	public void constructor(BoolExpression exp, StatementSequence statements) {
		_exp = exp;
		_statements = statements;

	public override string ToString() {
		return "while ${_exp} do\n${Indent(_statements)}\nod"
		
	public void Compile(ILGenerator il) {
		EmitDebugInfo(il,0,false)
	
		evalConditionLabel = il.DefineLabel()
		afterTheLoopLabel = il.DefineLabel()
		il.MarkLabel(evalConditionLabel)
		_exp.Compile(il)
		il.Emit(OpCodes.Brfalse, afterTheLoopLabel)
		_statements.Compile(il)
		il.Emit(OpCodes.Br, evalConditionLabel)		
		il.MarkLabel(afterTheLoopLabel)
