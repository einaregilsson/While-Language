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
using While.AST.Expressions;
using While.AST.Sequences;
using While.AST.Statements;

namespace While.AST {

    public abstract class Visitor {
        public abstract bool VisitParentBeforeChildren { get; }

        public void VisitNode(Node node) {
            if (node is Bool) {
                Visit((Bool)node);
            } else if (node is Number) {
                Visit((Number)node);
            } else if (node is Variable) {
                Visit((Variable)node);
            } else if (node is Plus) {
                Visit((Plus)node);
            } else if (node is Minus) {
                Visit((Minus)node);
            } else if (node is Multiplication) {
                Visit((Multiplication)node);
            } else if (node is Division) {
                Visit((Division)node);
            } else if (node is Modulo) {
                Visit((Modulo)node);
            } else if (node is Equal) {
                Visit((Equal)node);
            } else if (node is NotEqual) {
                Visit((NotEqual)node);
            } else if (node is GreaterThan) {
                Visit((GreaterThan)node);
            } else if (node is LessThan) {
                Visit((LessThan)node);
            } else if (node is GreaterThanOrEqual) {
                Visit((GreaterThanOrEqual)node);
            } else if (node is LessThanOrEqual) {
                Visit((LessThanOrEqual)node);
            } else if (node is BitwiseAnd) {
                Visit((BitwiseAnd)node);
            } else if (node is BitwiseOr) {
                Visit((BitwiseOr)node);
            } else if (node is BitwiseXor) {
                Visit((BitwiseXor)node);
            } else if (node is ShiftLeft) {
                Visit((ShiftLeft)node);
            } else if (node is ShiftRight) {
                Visit((ShiftRight)node);
            } else if (node is LogicalAnd) {
                Visit((LogicalAnd)node);
            } else if (node is LogicalOr) {
                Visit((LogicalOr)node);
            } else if (node is LogicalXor) {
                Visit((LogicalXor)node);
            } else if (node is UnaryMinus) {
                Visit((UnaryMinus)node);
            } else if (node is OnesComplement) {
                Visit((OnesComplement)node);
            } else if (node is Not) {
                Visit((Not)node);
            } else if (node is Procedure) {
                Visit((Procedure)node);
            } else if (node is ProcedureSequence) {
                Visit((ProcedureSequence)node);
            } else if (node is StatementSequence) {
                Visit((StatementSequence)node);
            } else if (node is VariableSequence) {
                Visit((VariableSequence)node);
            } else if (node is VariableDeclarationSequence) {
                Visit((VariableDeclarationSequence)node);
            } else if (node is VariableDeclaration) {
                Visit((VariableDeclaration)node);
            } else if (node is Assign) {
                Visit((Assign)node);
            } else if (node is Skip) {
                Visit((Skip)node);
            } else if (node is Call) {
                Visit((Call)node);
            } else if (node is Write) {
                Visit((Write)node);
            } else if (node is Read) {
                Visit((Read)node);
            } else if (node is Block) {
                Visit((Block)node);
            } else if (node is If) {
                Visit((If)node);
            } else if (node is While.AST.Statements.While) {
                Visit((While.AST.Statements.While)node);
            }

            //Second level
            if (node is BinaryOp<int, TypedExpression<int>>) {
                Visit((BinaryOp<int, TypedExpression<int>>)node);
            } else if (node is BinaryOp<bool, TypedExpression<int>>) {
                Visit((BinaryOp<bool, TypedExpression<int>>)node);
            } else if (node is BinaryOp<bool, TypedExpression<bool>>) {
                Visit((BinaryOp<bool, TypedExpression<bool>>)node);
            } else if (node is UnaryOp<int, TypedExpression<int>>) {
                Visit((UnaryOp<int, TypedExpression<int>>)node);
            } else if (node is UnaryOp<bool, TypedExpression<bool>>) {
                Visit((UnaryOp<bool, TypedExpression<bool>>)node);
            }

            //Third level in object hierarchy
            if (node is TypedExpression<int>) {
                Visit((TypedExpression<int>)node);
            } else if (node is TypedExpression<bool>) {
                Visit((TypedExpression<bool>)node);
            }

            //Fourth level
            if (node is Statement) {
                Visit((Statement)node);
            } else if (node is Expression) {
                Visit((Expression)node);
            }

            //Finally the root
            Visit(node);
        }

        #region Empty Visit overloads for all nodes
        public virtual void Visit(Bool node) { }
        public virtual void Visit(Number node) { }
        public virtual void Visit(Variable node) { }
        public virtual void Visit(Plus node) { }
        public virtual void Visit(Minus node) { }
        public virtual void Visit(Multiplication node) { }
        public virtual void Visit(Division node) { }
        public virtual void Visit(Modulo node) { }
        public virtual void Visit(Equal node) { }
        public virtual void Visit(NotEqual node) { }
        public virtual void Visit(GreaterThan node) { }
        public virtual void Visit(LessThan node) { }
        public virtual void Visit(GreaterThanOrEqual node) { }
        public virtual void Visit(LessThanOrEqual node) { }
        public virtual void Visit(BitwiseAnd node) { }
        public virtual void Visit(BitwiseOr node) { }
        public virtual void Visit(BitwiseXor node) { }
        public virtual void Visit(ShiftLeft node) { }
        public virtual void Visit(ShiftRight node) { }
        public virtual void Visit(LogicalAnd node) { }
        public virtual void Visit(LogicalOr node) { }
        public virtual void Visit(LogicalXor node) { }
        public virtual void Visit(UnaryMinus node) { }
        public virtual void Visit(OnesComplement node) { }
        public virtual void Visit(Not node) { }
        public virtual void Visit(SequencePoint node) { }
        public virtual void Visit(Procedure node) { }
        public virtual void Visit(ProcedureSequence node) { }
        public virtual void Visit(StatementSequence node) { }
        public virtual void Visit(VariableSequence node) { }
        public virtual void Visit(VariableDeclarationSequence node) { }
        public virtual void Visit(VariableDeclaration node) { }
        public virtual void Visit(Assign node) { }
        public virtual void Visit(Skip node) { }
        public virtual void Visit(Call node) { }
        public virtual void Visit(Write node) { }
        public virtual void Visit(Read node) { }
        public virtual void Visit(Block node) { }
        public virtual void Visit(If node) { }
        public virtual void Visit(While.AST.Statements.While node) { }
        public virtual void Visit(WhileProgram node) { }

        public virtual void Visit(TypedExpression<int> node) { }
        public virtual void Visit(TypedExpression<bool> node) { }
        public virtual void Visit(BinaryOp<int, TypedExpression<int>> node) { }
        public virtual void Visit(BinaryOp<bool, TypedExpression<int>> node) { }
        public virtual void Visit(BinaryOp<bool, TypedExpression<bool>> node) { }
        public virtual void Visit(UnaryOp<int, TypedExpression<int>> node) { }
        public virtual void Visit(UnaryOp<bool, TypedExpression<bool>> node) { }

        public virtual void Visit(Statement node) { }
        public virtual void Visit(Expression node) { }

        public virtual void Visit(Node node) { }


        #endregion
    }
}
