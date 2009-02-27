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
 * $HeadURL: https://while-language.googlecode.com/svn/branches/Boo/Test/ASTTest.boo $
 * $LastChangedDate: 2009-02-25 15:21:32 +0100 (mið., 25 feb. 2009) $
 * $Author: einar@einaregilsson.com $
 * $Revision: 2 $
 */
namespace While.Test;

using While;
using While.AST;
using NUnit.Framework;
using System.IO;

[TestFixture]
class ASTTest) {"""Test the abstract syntax tree"""

	[Test]
	public void AssignStatement() {
		Parse('x := 3', 'x := 3', true)
		
	[Test]
	public void SkipStatement() {
		Parse('skip', 'skip', true)

	[Test]
	public void ReadStatement() {
		Parse('read x', 'read x', true)
		
	[Test]
	public void WriteStatement() {
		Parse('write x', 'write x', true)

	[Test]
	public void WriteStatementWithExpression() {
		Parse('write 25+(32-1*3)%2', 'write (25 + ((32 - (1 * 3)) % 2))', true)

	[Test]
	public void WhileStatement() {
		Parse('while true do skip od', """
while true do;
	skip;
od;
		""", false)
		
	[Test]
	public void WhileStatementWithExpression() {
		Parse('while 1+2 < 4+2 do skip od', """
while ((1 + 2) < (4 + 2)) do;
	skip;
od;
		""",false)

	[Test]
	public void IfStatement() {
		Parse('if true then skip fi', """
if true then;
	skip;
fi;
		""",false)

	[Test]
	public void IfStatementWithExpression() {
		Parse('if 1+3*4%2 <= 3 then skip fi', """
if ((1 + ((3 * 4) % 2)) <= 3) then;
	skip;
fi;
		""",false)

	[Test]
	public void IfElseStatement() {
		Parse('if true then skip else skip fi', """
if true then;
	skip;
else;
	skip;
fi;
		""",false)

	[Test]
	public void BlockStatement() {
		Parse('begin var x; var y; skip; skip end', """
begin;
	var x;
	var y;
	
	skip;
	skip;
end;
		""",false)

	[Test]
	public void StatementSequence() {
		Parse('skip; skip; read x; begin var y; skip end; while true do skip od', """
skip;
skip;
read x;
begin;
	var y;
	
	skip;
end;
while true do;
	skip;
od;
		""",false)
		
	private public void Parse(string src, string expAst, bool bookVersion) {
		writer = StreamWriter(MemoryStream())
		writer.Write(src)
		writer.Flush()
		writer.BaseStream.Seek(0, SeekOrigin.Begin)
		CompileOptions.BookVersion = bookVersion;
		p = Parser(Scanner(writer.BaseStream))
		result = StringWriter()
		p.errors.errorStream = result;
		p.Parse()
		Assert.AreEqual("", result.ToString())
		Assert.AreEqual(expAst.Trim().Replace("\r",""), WhileTree.Instance.ToString().Trim())
