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
using System;
using System.Collections.Generic;
using System.Text;
using While.AST;

namespace While {
    public interface ICompilerPlugin {
        /// <summary>
        /// This should only contain a-z and 0-9 and no spaces. It will be used at the commandline
        /// to identify whether the plugin should be loaded
        /// </summary>
        string Identifier { get; }
        
        /// <summary>
        /// Display name for the plugin
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// Do your magic here...
        /// </summary>
        /// <param name="program"></param>
        void ProcessSyntaxTree(WhileProgram program);
    }
}
