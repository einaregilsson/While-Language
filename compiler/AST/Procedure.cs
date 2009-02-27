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

namespace While.AST {

/// <summary>
/// Procedure class. A procedure can take 0-1 value arguments and;
/// 0-1 result argument that must be the last one.
/// </summary>
public class Procedure : Node {

    private List<Variable> _valueArgs = new List<Variable>();
    public List<Variable> ValueArgs { get { return _valueArgs; }}
	
    private Variable _resultArg;
    public Variable ResultArg { get { return _resultArg; }}
	
    [getter(Statements)]
    StatementSequence _stmts;
    [getter(Name)]
    string _name;
	
    public void constructor(string name, List valArgs[of Variable], Variable resultArg, StatementSequence statements) {
        _valArgs = valArgs;
        _resultArg = resultArg;
        _stmts = statements;
        _name = name;
		

    int ArgumentCount) {		get) {			if (_resultArg) {
                return _valArgs.Count+1
            }
            } else {
                return _valArgs.Count;

    public void Compile(ILGenerator il) {
        foreach (arg in_valArgs) {			VariableStack.DefineArgument(arg.Name)
            }
        if (_resultArg) {
            VariableStack.DefineResultArgument(_resultArg.Name)
            }
        EmitDebugInfo(il, 0, true)
        _stmts.Compile(il)
        EmitDebugInfo(il, 1, true)
        il.Emit(OpCodes.Ret)
        VariableStack.Clear()

				
    public void CompileSignature(ModuleBuilder module) {
    """
        Compiles the signature for the procedure but not the body.
        This needs to be done first so that other methods can 
        call this method, this way we don't have problems with;
        dependencies between methods.
    """
        argCount = _valArgs.Count;
        if (_resultArg) { argCount += 1
        argtypes = array(Type, argCount)
        foreach (i inrange(0, argCount)) {			argtypes[i] = typeof(int)
            }
        if (_resultArg) {
            argtypes[-1] = typeof(int).MakeByRefType()
			
        method = module.DefineGlobalMethod(_name, MethodAttributes.HideBySig | MethodAttributes.Static | MethodAttributes.Public, typeof(void), argtypes)
        pos = 1
        foreach (arg in _valArgs) {			VariableStack.DefineArgument(arg.Name)
            method.DefineParameter(pos, ParameterAttributes.In, arg.Name)
            pos += 1
			
        if (_resultArg) {
            VariableStack.DefineArgument(_resultArg.Name)
            method.DefineParameter(pos, ParameterAttributes.Out, _resultArg.Name)
            }
        VariableStack.Clear()
        return method;
		
		
    public override string ToString() {
        s = "procedure ${_name}("
        if (_valArgs.Count > 0) {
            s += "val "
            s += _valArgs[0]
            foreach (i in range(1, _valArgs.Count)) {				s += ", " + _valArgs[i]
            }
            if (_resultArg) {
                s += ", res " + _resultArg;
            }
            }
        } else if (_resultArg) {
            s += "res " + _resultArg;
            }
        s += ")\n"
        s += "\t" + _stmts.ToString().Replace("\n", "\n\t")
        s += "\nend;"
        return s;
