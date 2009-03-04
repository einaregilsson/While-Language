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
using NUnit.Framework;
using While.AST;

namespace While.Test {

    /// <summary>
    /// Test the abstract syntax tree
    /// </summary>
    [TestFixture]
    public class ASTTest : WhileTest {

        [Test]
        public void AssignStatement() {
            Parse("x := 3", "x := 3", true);
        }

        [Test]
        public void SkipStatement() {
            Parse("skip", "skip", true);
        }

        [Test]
        public void ReadStatement() {
            Parse("read x", "read x", true);
        }

        [Test]
        public void WriteStatement() {
            Parse("write x", "write x", true);
        }

        [Test]
        public void WriteStatementWithSimpleExpression() {
            Parse("write x + 2", "write (x + 2)", true);
        }

        [Test]
        public void WriteStatementWithExpression() {
            Parse("write 25+(32-1*3)%2", "write (25 + ((32 - (1 * 3)) % 2))", true);
        }

        [Test]
        public void WhileStatement() {
            Parse("while true do skip od", @"
while true do
	skip
od
		", false);
        }

        [Test]
        public void WhileStatementWithExpression() {
            Parse("while 1+2 < 4+2 do skip od", @"
while ((1 + 2) < (4 + 2)) do
	skip
od
		", false);
        }

        [Test]
        public void IfStatement() {
            Parse("if true then skip fi", @"
if true then
	skip
fi
		", false);
        }

        [Test]
        public void IfStatementWithExpression() {
            Parse("if 1+3*4%2 <= 3 then skip fi", @"
if ((1 + ((3 * 4) % 2)) <= 3) then
	skip
fi
		", false);
        }

        [Test]
        public void IfElseStatement() {
            Parse("if true then skip else skip fi", @"
if true then
	skip
else
	skip
fi
		", false);
        }

        [Test]
        public void BlockStatement() {
            Parse("begin var x; var y; skip; skip end", @"
begin
	var x;
	var y;
	
	skip;
	skip
end
		", false);
        }

        [Test]
        public void StatementSequence() {
            Parse("skip; skip; read x; begin var y; skip end; while true do skip od", @"
skip;
skip;
read x;
begin
	var y;
	
	skip
end;
while true do
	skip
od
		", false);
        }

        private void Parse(string src, string expAst, bool bookVersion) {
            string[] cmdline = new string[] { };
            if (!bookVersion) {
                cmdline = new string[] { "/coursesyntax" };
            }
            string result = Parse(src, new CommandLineOptions(cmdline));
            Assert.AreEqual("", result);
            Assert.AreEqual(expAst.Trim().Replace("\r", ""), WhileProgram.Instance.ToString().Trim());
        }
    }
}
