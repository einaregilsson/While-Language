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
using While.Parsing;
using NUnit.Framework;
using System.IO;

namespace While.Test {


    [TestFixture]
    public class ParseTest : WhileTest {

        [Test]
        public void AssignStatement() {
            Parse("x := 3", "");
        }

        [Test]
        public void SkipStatement() {
            Parse("skip", "");
        }

        [Test]
        public void ReadStatement() {
            Parse("read x", "");
        }

        [Test]
        public void WriteStatement() {
            Parse("write x", "");
        }

        [Test]
        public void WriteStatementWithExpression() {
            Parse("write 25+(32-1*3)%2", "");
        }

        [Test]
        public void WhileStatement() {
            Parse("while true do skip ", "");
        }

        [Test]
        public void WhileStatementWithExpression() {
            Parse("while 1+2 < 4+2 do ( skip )", "");
        }

        [Test]
        public void IfStatement() {
            Parse("if true then skip ", "");
        }

        [Test]
        public void IfStatementWithExpression() {
            Parse("if 1+3*4%2 <= 3 then (skip) ", "");
        }

        [Test]
        public void IfElseStatement() {
            Parse("if true then skip else skip ", "");
        }

        [Test]
        public void BlockStatement() {
            Parse("begin var x; var y; skip; skip end", "", new CommandLineOptions(new string[] { "/coursesyntax" }));
        }

        [Test]
        public void StatementSequence() {
            Parse("skip; skip; read x; begin var y; skip end; while true do skip od", "", new CommandLineOptions(new string[] { "/coursesyntax"}));
        }

        private void Parse(string src, string expectedOutput) {
            Assert.AreEqual(expectedOutput, Parse(src));
        }

        private void Parse(string src, string expectedOutput, CommandLineOptions options) {
            Assert.AreEqual(expectedOutput, Parse(src, options));
        }

    }
}
