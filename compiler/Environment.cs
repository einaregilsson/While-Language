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
using System.Text;

namespace While {
    internal static class Environment {

        private static bool _test = false;
        public static bool Test {
            get { return _test; }
            set { _test = value; }
        }

        public static void Exit(int exitCode) {
            if (Environment.Test) {
                throw new WhileException("Program exited with status code {0}", exitCode);
            } else {
                System.Environment.Exit(exitCode);
            }
        }
    }
}
