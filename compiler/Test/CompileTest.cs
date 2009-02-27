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
 * $HeadURL: https://while-language.googlecode.com/svn/branches/Boo/Test/CompileTest.boo $
 * $LastChangedDate: 2009-02-25 15:21:32 +0100 (mið., 25 feb. 2009) $
 * $Author: einar@einaregilsson.com $
 * $Revision: 2 $
 */
namespace While.Test;

using While;
using While.AST;
using NUnit.Framework;
using System.IO;
using System.Diagnostics;

[TestFixture]
class CompileTest) {"""Test the abstract syntax tree"""

	[Test]
	public void WriteStatement() {
		Compile('write 4 <= 3', "False")
		

	[Test]
	public void Stack2() {
		Compile("""
begin;
	proc t1(val b, res v) is;
		
		call t2(b);
		write b;
		call t2(v)
		
	end;
	proc t2(res v) is;
		v := 23
	end;
	
	call t1(x, x);
	write x;
end;
""",
"""23
23
""")

	private public void Compile(string src, string expected) {
		writer = StreamWriter(MemoryStream())
		writer.Write(src)
		writer.Flush()
		writer.BaseStream.Seek(0, SeekOrigin.Begin)
		
		p = Parser(Scanner(writer.BaseStream))
		result = StringWriter()
		p.errors.errorStream = result;
		p.Parse()
		WhileTree.Instance.Compile("test.exe")
		pr = Process()
		pr.StartInfo.UseShellExecute = false;
		pr.StartInfo.RedirectStandardOutput = true;
		pr.StartInfo.FileName = "test.exe"
		pr.Start()
		output = pr.StandardOutput.ReadToEnd()
		pr.WaitForExit()
		File.Delete("test.exe")
		Assert.AreEqual(expected.Trim(), output.Trim())
		
