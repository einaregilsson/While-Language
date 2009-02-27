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
using System;
using System.Diagnostics.SymbolStore;
using System.Reflection.Emit;
using System.Collections.Generic;

namespace While.AST {

    /// <summary>
    /// Class to keep track of what the debugger should highlight in the source file
    /// when debugging.
    /// </summary>
    public class SequencePoint {
        
        public SequencePoint(int startLine, int startCol, int endLine, int endCol) {
            _startLine = startLine;
            _startCol = startCol;
            _endLine = endLine;
            _endCol = endCol;
        }

        private int _startLine, _startCol, _endLine, _endCol;

        public int StartLine { get { return _startLine; } }
        public int StartCol { get { return _startCol; } }
        public int EndLine { get { return _endLine; } }
        public int EndCol { get { return _endLine; } }
    }

    /// <summary>
    /// The base for all nodes in the abstract syntax tree.
    /// Just contains some utility methods.
    /// </summary>
    public abstract class Node {


        public abstract void Compile(ILGenerator il);
        protected List<Node> _children = new List<Node>();
        public List<Node> ChildNodes { get { return _children; } }
        protected static SymbolTable _symTable = new SymbolTable();
        public static SymbolTable SymbolTable { get { return _symTable; } }
        protected static ISymbolDocumentWriter _debugWriter;
        protected List<SequencePoint> _sequencePoints = new List<SequencePoint>();
        
        protected void MarkSequencePoint(ILGenerator il, SequencePoint seq) {
            il.MarkSequencePoint(_debugWriter, seq.StartLine, seq.StartCol, seq.EndLine, seq.EndCol);
        }

        public Node this[int index] {
            get { return _children[index]; }
        }

        //public void AddSequencePoint(Token t) {
        //    _sequencePoints.Add(new SequencePoint(t.line, t.col, t.line, t.col + t.val.Length));
        //}

        public void AddSequencePoint(int startLine, int startCol, int endLine, int endCol) {
            _sequencePoints.Add(new SequencePoint(startLine, startCol, endLine, endCol));
        }

        public void EmitDebugInfo(ILGenerator il, int index, bool addNOP) {
            if (CompileOptions.Debug) {
                SequencePoint seq = _sequencePoints[index];
                MarkSequencePoint(il, _sequencePoints[index]);
                if (addNOP) {
                    il.Emit(OpCodes.Nop);
                }
            }
        }
    }
}
