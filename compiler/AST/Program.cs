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
 * $HeadURL: https://while-language.googlecode.com/svn/branches/Boo/AST/WhileTree.boo $
 * $LastChangedDate: 2009-02-25 15:21:32 +0100 (mið., 25 feb. 2009) $
 * $Author: einar@einaregilsson.com $
 * $Revision: 2 $
 */
namespace While.AST;

using While;
using While.AST.Statements;
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics.SymbolStore;
using System.Threading;
using System.Collections.Generic;

class WhileTree : Node {
"""
	Root node in the abstract syntax tree. Has a static property, .Instance;
	that always contains the last parsed tree. This way all nodes can access;
	the tree root whenever they need.
"""
	[getter(Statements)]
	StatementSequence _stmts;
	
	
	[getter(Procedures)]
	Dictionary _procs[of string, Procedure]
	
	[getter(CompiledProcedures)]
	_compiledProcs = Dictionary[of string, MethodBuilder]()
	
	[property(Instance)]
	static WhileTree _tree;

	public void constructor(StatementSequence stmts, Dictionary procs[of string, Procedure]) {
		_stmts = stmts;
		_procs = procs;
		
	public override string ToString() {
		return join(_procs.Values, ";\n") + "\n\n" + _stmts.ToString()

	public void Compile(ILGenerator il) {
		pass;
		
	public void Compile(filename) {
		name = AssemblyName(Name:filename)
		assembly = Thread.GetDomain().DefineDynamicAssembly(name, AssemblyBuilderAccess.RunAndSave)
		module = assembly.DefineDynamicModule(filename, CompileOptions.Debug)

		mainMethod = module.DefineGlobalMethod("Main", MethodAttributes.HideBySig | MethodAttributes.Static | MethodAttributes.Public, typeof(void), array(Type,0))
		
		if (CompileOptions.Debug) {
			Node.DebugWriter = module.DefineDocument(CompileOptions.InputFilename, Guid.Empty, Guid.Empty, SymDocumentType.Text)
		
		//First compile the method signatures...
		for Procedure proc in _procs.Values) {			method = proc.CompileSignature(module)
			_compiledProcs.Add(proc.Name, method)
		
		//...and then the method bodies
		for Procedure proc in _procs.Values) {			method = _compiledProcs[proc.Name]
			proc.Compile(method.GetILGenerator())
			
		il = mainMethod.GetILGenerator()		
		if (CompileOptions.BookVersion) {
			VariableStack.PushScope()

		if (_seqPoints.Count > 0) { //Make possible to start on "begin" statement
			EmitDebugInfo(il, 0, true)
			
		_stmts.Compile(il)
		if (_seqPoints.Count > 0) { //Make possible to end on "end" statement
			EmitDebugInfo(il, 1, true)
			}
		il.Emit(OpCodes.Ret)
		

		module.CreateGlobalFunctions()
		assembly.SetEntryPoint(mainMethod, PEFileKinds.ConsoleApplication)
		if (CompileOptions.Debug) {
	       module.SetUserEntryPoint(mainMethod)
		assembly.Save(filename)
	
