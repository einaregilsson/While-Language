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
 * 
 * Generated with the compiler generator Coco/R.
 * Coco/R is Copyright (c) 1990, 2004 Hanspeter Moessenboeck, University of Linz
 */
using System;
using While.AST;
using While.AST.Expressions;
using While.AST.Sequences;
using While.AST.Statements;

namespace While.Parsing {

    public partial class Parser {

        const bool T = true;
        const bool x = false;
        const int minErrDist = 2;

        public Scanner scanner;
        public Errors errors;

        public Token t;    // last recognized token
        public Token la;   // lookahead token
        int errDist = minErrDist;
        private CommandLineOptions _options;

        public Parser(Scanner scanner, CommandLineOptions options) {
            this.scanner = scanner;
            errors = new Errors();
            _options = options;
            Node.Options = options;
        }

        public CommandLineOptions Options {
            get { return _options; }
        }
        void SynErr(int n) {
            if (errDist >= minErrDist) errors.SynErr(la.line, la.col, n);
            errDist = 0;
        }

        public void SemErr(string msg) {
            if (errDist >= minErrDist) errors.SemErr(t.line, t.col, msg);
            errDist = 0;
        }

        void Expect(int n) {
            if (la.kind == n) Get(); else { SynErr(n); }
        }

        bool StartOf(int s) {
            return set[s, la.kind];
        }

        private bool ExpectBool(Expression exp, Token t, bool isRightHandSide) {
            if (!(exp is TypedExpression<bool>)) {
                errors.SemErr(t.line, t.col, string.Format("'{0}' expects a boolean expression on its {1} side", t.val, isRightHandSide ? "right" : "left"));
                return false;
            }
            return true;
        }

        private bool ExpectInt(Expression exp, Token t, bool isRightHandSide) {
            if (!(exp is TypedExpression<int>)) {
                errors.SemErr(t.line, t.col, string.Format("'{0}' expects an integer expression on its {1} side", t.val, isRightHandSide ? "right" : "left"));
                return false;
            }
            return true;
        }

        private void ExpectIntArg(Expression exp, Token t) {
            if (!(exp is TypedExpression<int>)) {
                errors.SemErr(t.line, t.col, "Arguments to procedures can only be integer expressions");
            }
        }

        private StatementSequence ToStatementSequence(Statement s) {
            if (s is StatementSequence) {
                return (StatementSequence)s;
            } else {
                StatementSequence seq = new StatementSequence();
                seq.AddStatement(s);
                return seq;
            }
        }

        private SymbolTable SymbolTable {
            get { return While.AST.WhileProgram.SymbolTable; }
        }
        private bool IsStartOfResultArg() {
            Token t = scanner.Peek();
            scanner.ResetPeek();
            return t.val == "res";
        }

        private bool IsProcProgram() {
            Token t = scanner.Peek();
            scanner.ResetPeek();
            return t.val == "proc";
        }

    } // end Parser


    public partial class Errors {
        public int count = 0;                                    // number of errors detected
        public System.IO.TextWriter errorStream = Console.Out;   // error messages go to this stream
        public string errMsgFormat = "-- line {0} col {1}: {2}"; // 0=line, 1=column, 2=text

        public void SynErr(int line, int col, int n) {
            string s;
            s = GetErrorMessage(n);
            if (s == null) {
                s = "error " + n;
            }
            errorStream.WriteLine(errMsgFormat, line, col, s);
            count++;
        }

        public void SemErr(int line, int col, string s) {
            errorStream.WriteLine(errMsgFormat, line, col, s);
            count++;
        }

        public void SemErr(string s) {
            errorStream.WriteLine(s);
            count++;
        }

        public void Warning(int line, int col, string s) {
            errorStream.WriteLine(errMsgFormat, line, col, s);
        }

        public void Warning(string s) {
            errorStream.WriteLine(s);
        }
    } // Errors


    public class FatalError : Exception {
        public FatalError(string m) : base(m) { }
    }
}
