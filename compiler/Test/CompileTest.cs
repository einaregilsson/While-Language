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
using While.Parsing;
using NUnit.Framework;
using System.IO;
using System.Diagnostics;

namespace While.Test {


    [TestFixture]
    public class CompileTest : WhileTest {

        [Test]
        public void WriteStatement() {
            Compile("write 4 <= 3", "False");
        }


        [Test]
        public void Stack2() {
            Compile(@"
begin
	proc t1(val b, res v) is
		
		call t2(b);
		write b;
		call t2(v)
		
	end;
	proc t2(res v) is
		v := 23
	end;
	
	call t1(x, x);
	write x
end
",
    @"23
23
");
        }

        private void Compile(string src, string expected) {
            Parse(src);
            WhileProgram.Instance.Compile("test.exe");
            Process pr = new Process();
            pr.StartInfo.UseShellExecute = false;
            pr.StartInfo.RedirectStandardOutput = true;
            pr.StartInfo.FileName = "test.exe";
            pr.Start();
            string output = pr.StandardOutput.ReadToEnd();
            pr.WaitForExit();
            File.Delete("test.exe");
            System.Console.WriteLine("Output was: " + output);
            Assert.AreEqual(expected.Trim(), output.Trim());
        }
    }
}
