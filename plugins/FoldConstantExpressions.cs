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
using While;
using While.AST;
using While.AST.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("Fold Constant Expressions")]
[assembly: AssemblyDescription("A plugin for the While.NET compiler")]
[assembly: AssemblyCopyright("Einar Egilsson")]
[assembly: AssemblyVersion("1.0.0")]

namespace While.Plugins {

    /// <summary>
    /// Example plugin for the While Compiler. Folds expressions that can be computed
    /// at compile time.
    /// </summary>
    public class FoldConstantExpressions : Visitor, ICompilerPlugin{
        public string Identifier {
            get { return "constexp"; }
        }

        public string Name {
            get { return "Fold Constant Expressions"; }
        }

        public void ProcessSyntaxTree(While.AST.WhileProgram program) {
            program.Accept(this);
            Console.WriteLine();
            Console.WriteLine("Program after optmizations:");
            Console.WriteLine(program);
        }

        public override bool VisitParentBeforeChildren {
            get { return false; }
        }

        //Fold arithmetic and bitwise operations
        public override void Visit(BinaryOp<int, TypedExpression<int>> node) {
            if (node.Left is Number && node.Right is Number) {
                Console.WriteLine("Folding {0} into {1}", node, node.TypedValue);
                int pos = node.Parent.ChildNodes.IndexOf(node);
                node.Parent[pos] = new Number(node.TypedValue);
            }
        }

        //Fold relational operations
        public override void Visit(BinaryOp<bool, TypedExpression<int>> node) {
            if (node.Left is Number && node.Right is Number) {
                Console.WriteLine("Folding {0} into {1}", node, node.TypedValue);
                int pos = node.Parent.ChildNodes.IndexOf(node);
                node.Parent[pos] = new Bool(node.TypedValue);
            }
        }

        //Fold logical operations
        public override void Visit(BinaryOp<bool, TypedExpression<bool>> node) {
            if (node.Left is Bool && node.Right is Bool) {
                Console.WriteLine("Folding {0} into {1}", node, node.TypedValue);
                int pos = node.Parent.ChildNodes.IndexOf(node);
                node.Parent[pos] = new Bool(node.TypedValue);
            }
        }
        
        //TODO: Fold unary ops...
    }
}
