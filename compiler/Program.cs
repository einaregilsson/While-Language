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
 * $HeadURL: https://while-language.googlecode.com/svn/branches/Boo/Program.boo $
 * $LastChangedDate: 2009-02-25 15:21:32 +0100 (mið., 25 feb. 2009) $
 * $Author: einar@einaregilsson.com $
 * $Revision: 2 $
 */
""" 
	While.NET compiler;
	
	Compiler for the programming language While, found in the;
	book Principles of Program Analysis by Nielson, Nielson and;
	Hankin. Licensed under the GPL.
	
	Program author: Einar Egilsson (einar@einaregilsson.com)
"""

namespace While;

using System;
using System.IO;
using While.AST;

[STAThread]
static public void Main(args as (string)) {
	
	print """While.NET Compiler v0.9
Copyright (C) Einar Egilsson 2009. All rights reserved.
"""

	CompileOptions.Init(args)
	if (CompileOptions.Empty) {
		System.Console.Error.WriteLine("ERROR: No inputs specified")
		return 1
	} else if (CompileOptions.Help) {
		System.Console.Error.WriteLine("Usage: wc.exe [options] filename")
		CompileOptions.Print()
		return 2
	} else if (not CompileOptions.ReadStdIn && !File.Exists(CompileOptions.InputFilename)) {
		System.Console.Error.WriteLine("ERROR: File '${CompileOptions.InputFilename}' does not exist");
		return 3

	Parser p;
	if (CompileOptions.ReadStdIn) {
		p = Parser(Scanner(System.Console.OpenStandardInput()))
	} else {
		p = Parser(Scanner(FileStream(CompileOptions.InputFilename, FileMode.Open)))

	p.Parse()
	return if p.errors.count > 0
	VariableStack.Clear()
	WhileTree.Instance.Compile(CompileOptions.OutputFilename)
	return 0
