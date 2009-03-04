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
using System.IO;
using While;
using While.Parsing;
using NUnit.Framework;

namespace While.Test {

    public abstract class WhileTest {

        public WhileTest() {
            global::While.Environment.Test = true;
        }

        protected string Parse(string src) {
            return Parse(src, new CommandLineOptions(new string[] { }));
        }

        protected string Parse(string src, CommandLineOptions options) {
            StreamWriter writer = new StreamWriter(new MemoryStream());
            writer.Write(src);
            writer.Flush();
            writer.BaseStream.Seek(0, SeekOrigin.Begin);

            Parser p = new Parser(new Scanner(writer.BaseStream), options);
            StringWriter result = new StringWriter();
            p.errors.errorStream = result;
            p.Parse();
            return result.ToString();
        }
    }
}
