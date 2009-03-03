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
using While;
using While.AST;
using System.Reflection.Emit;
using IntExpression = While.AST.Expressions.TypedExpression<int>;
using BoolExpression = While.AST.Expressions.TypedExpression<bool>;

// This module contains all expression classes;
// of the abstract syntax tree. The expressions;
// have properties to evaluate them at compile;
// time (except variables). This could be used;
// to do some constant folding.

namespace While.AST.Expressions {

    public abstract class Expression : Node {
        public abstract object Value { get;}
    }

    public abstract class TypedExpression<T> : Expression {
        public override object Value { get { return TypedValue; } }
        public abstract T TypedValue { get; }
    }

    public class Bool : BoolExpression {

        private bool _value;

        public Bool(bool value) {
            _value = value;
        }
        public override bool TypedValue { get { return _value; } }

        public override string ToString() {
            return _value.ToString().ToLower();
        }

        public override void Compile(ILGenerator il) {
            il.Emit(_value ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
        }
    }

    public class Number : IntExpression {

        private int _nr;
        public override int TypedValue { get { return _nr; } }

        public Number(int nr) {
            _nr = nr;
        }

        public override string ToString() {
            return _nr.ToString();
        }

        public override void Compile(ILGenerator il) {
            il.Emit(OpCodes.Ldc_I4, _nr);
        }
    }

    public class Variable : IntExpression {

        private string _name;
        public string Name { get { return _name; } }

        private bool _isValueArg = false;
        public bool IsValueArg {
            get { return _isValueArg; }
            set { _isValueArg = value; }
        }

        private bool _isResultArg = false;
        public bool IsResultArg {
            get { return _isResultArg; }
            set { _isResultArg = value; }
        }

        public override int TypedValue {
            get { throw new WhileException("Variable {0} cannot be evaluated at compile time", _name); }
        }

        public Variable(string name) {
            _name = name;
        }

        public override string ToString() {
            return _name.ToString();
        }

        public override void Compile(ILGenerator il) {
            OpCode code = OpCodes.Ldloc;

            if (Options.BookVersion && !SymbolTable.IsInScope(_name)) {
                //Declare at first use
                SymbolTable.DefineVariable(_name);
                LocalBuilder lb = il.DeclareLocal(typeof(int));
                if (Options.Debug) {
                    lb.SetLocalSymInfo(_name);
                }
            }

            if (SymbolTable.IsArgument(_name)) {
                code = OpCodes.Ldarg;
            }
            il.Emit(code, SymbolTable.GetValue(_name));
        }

    }

    public abstract class BinaryOp<T, ChildType> : TypedExpression<T> where ChildType : Expression {

        public ChildType Left { get { return (ChildType)this[0]; } }
        public ChildType Right { get { return (ChildType)this[1]; } }
        protected OpCode _opCode;
        protected string _opString;

        public BinaryOp(ChildType left, ChildType right, OpCode opCode, string opString) {
            AddChild(left);
            AddChild(right);
            _opCode = opCode;
            _opString = opString;
        }

        public override string ToString() {
            return String.Format("({0} {1} {2})", Left, _opString, Right);
        }

        public override void Compile(ILGenerator il) {
            Left.Compile(il);
            Right.Compile(il);
            il.Emit(_opCode);
        }
    }

    public class Plus : BinaryOp<int, IntExpression> {
        public Plus(IntExpression left, IntExpression right)
            : base(left, right, OpCodes.Add_Ovf, "+") {
        }

        public override int TypedValue { 
            get { return Left.TypedValue + Right.TypedValue; } 
        }
    }

    public class Minus : BinaryOp<int, IntExpression> {
        public Minus(IntExpression left, IntExpression right)
            : base(left, right, OpCodes.Sub_Ovf, "-") {
        }

        public override int TypedValue { 
            get { return Left.TypedValue - Right.TypedValue; } 
        }
    }

    public class Multiplication : BinaryOp<int, IntExpression> {
        public Multiplication(IntExpression left, IntExpression right)
            : base(left, right, OpCodes.Mul_Ovf, "*") {
        }

        public override int TypedValue { 
            get { return Left.TypedValue * Right.TypedValue; } 
        }
    }

    public class Division : BinaryOp<int, IntExpression> {
        public Division(IntExpression left, IntExpression right)
            : base(left, right, OpCodes.Div, "/") {
        }

        public override int TypedValue { 
            get { return Left.TypedValue / Right.TypedValue; } 
        }
    }

    public class Modulo : BinaryOp<int, IntExpression> {
        public Modulo(IntExpression left, IntExpression right)
            : base(left, right, OpCodes.Rem, "%") {
        }

        public override int TypedValue { 
            get { return Left.TypedValue % Right.TypedValue; } 
        }
    }

    public class Equal : BinaryOp<bool, IntExpression> {
        public Equal(IntExpression left, IntExpression right)
            : base(left, right, OpCodes.Ceq, "==") {
        }

        public override bool TypedValue { 
            get { return Left.TypedValue == Right.TypedValue; } 
        }
    }

    public class NotEqual : BinaryOp<bool, IntExpression> {
        public NotEqual(IntExpression left, IntExpression right)
            : base(left, right, OpCodes.Nop, "!=") {
        }

        public override bool TypedValue { 
            get { return Left.TypedValue == Right.TypedValue; } 
        }

        public override void Compile(ILGenerator il) {
            Left.Compile(il);
            Right.Compile(il);
            il.Emit(OpCodes.Ceq);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ceq);
        }
    }

    public class GreaterThan : BinaryOp<bool, IntExpression> {
        public GreaterThan(IntExpression left, IntExpression right)
            : base(left, right, OpCodes.Cgt, ">") {
        }

        public override bool TypedValue { 
            get { return Left.TypedValue > Right.TypedValue; } 
        }
    }

    public class LessThan : BinaryOp<bool, IntExpression> {
        public LessThan(IntExpression left, IntExpression right)
            : base(left, right, OpCodes.Clt, "<") {
        }

        public override bool TypedValue { 
            get { return Left.TypedValue < Right.TypedValue; } 
        }
    }

    public class GreaterThanOrEqual : BinaryOp<bool, IntExpression> {
        public GreaterThanOrEqual(IntExpression left, IntExpression right)
            : base(left, right, OpCodes.Nop, ">=") {
        }

        public override bool TypedValue { 
            get { return Left.TypedValue >= Right.TypedValue; } 
        }

        public override void Compile(ILGenerator il) {
            Left.Compile(il);
            Right.Compile(il);
            il.Emit(OpCodes.Clt);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ceq);
        }
    }

    public class LessThanOrEqual : BinaryOp<bool, IntExpression> {
        public LessThanOrEqual (IntExpression left, IntExpression right)
            : base(left, right, OpCodes.Nop, "<=") {
        }

        public override bool TypedValue { 
            get { return Left.TypedValue <= Right.TypedValue; } 
        }

        public override void Compile(ILGenerator il) {
            Left.Compile(il);
            Right.Compile(il);
            il.Emit(OpCodes.Cgt);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ceq);
        }
    }

    public class BitwiseAnd : BinaryOp<int, IntExpression> {
        public BitwiseAnd(IntExpression left, IntExpression right)
            : base(left, right, OpCodes.And, "&") {
        }

        public override int TypedValue { 
            get { return Left.TypedValue & Right.TypedValue; } 
        }
    }

    public class BitwiseOr : BinaryOp<int, IntExpression> {
        public BitwiseOr(IntExpression left, IntExpression right)
            : base(left, right, OpCodes.Or, "|") {
        }

        public override int TypedValue { 
            get { return Left.TypedValue | Right.TypedValue; } 
        }
    }

    public class BitwiseXor : BinaryOp<int, IntExpression> {
        public BitwiseXor(IntExpression left, IntExpression right)
            : base(left, right, OpCodes.Xor, "^") {
        }

        public override int TypedValue { 
            get { return Left.TypedValue ^ Right.TypedValue; } 
        }
    }

    public class ShiftLeft : BinaryOp<int, IntExpression> {
        public ShiftLeft(IntExpression left, IntExpression right)
            : base(left, right, OpCodes.Shl, "<<") {
        }

        public override int TypedValue { 
            get { return Left.TypedValue << Right.TypedValue; } 
        }
    }

    public class ShiftRight : BinaryOp<int, IntExpression> {
        public ShiftRight(IntExpression left, IntExpression right)
            : base(left, right, OpCodes.Shr, ">>") {
        }

        public override int TypedValue { 
            get { return Left.TypedValue >> Right.TypedValue; } 
        }
    }


    public class LogicalAnd : BinaryOp<bool, BoolExpression> {
        public LogicalAnd(BoolExpression left, BoolExpression right)
            : base(left, right, OpCodes.And, "&&") {
        }

        public override bool TypedValue { 
            get { return Left.TypedValue && Right.TypedValue; } 
        }
    }

    public class LogicalOr : BinaryOp<bool, BoolExpression> {
        public LogicalOr(BoolExpression left, BoolExpression right)
            : base(left, right, OpCodes.Or, "||") {
        }

        public override bool TypedValue { 
            get { return Left.TypedValue || Right.TypedValue; } 
        }
    }
    
    public class LogicalXor : BinaryOp<bool, BoolExpression> {
        public LogicalXor(BoolExpression left, BoolExpression right)
            : base(left, right, OpCodes.Xor, "^") {
        }

        public override bool TypedValue { 
            get { return Left.TypedValue ^ Right.TypedValue; } 
        }
    }

    public abstract class UnaryOp<T, ExpressionType> : TypedExpression<T> where ExpressionType : Expression{
        public ExpressionType Expression {
            get { return (ExpressionType)this[0]; }
            set { this[0] = value; }
        }

        protected OpCode _opCode;
        protected string _opString;
        public UnaryOp(ExpressionType exp, OpCode opCode, string opString) {
            AddChild(exp);
            _opCode = opCode;
            _opString = opString;
        }

        public override void Compile(ILGenerator il) {
            Expression.Compile(il);
            il.Emit(_opCode);
        }

        public override string ToString() {
            return _opString + Expression.ToString();
        }
    }

    public class UnaryMinus : UnaryOp<int, IntExpression> {
        public UnaryMinus(IntExpression exp)
            : base(exp, OpCodes.Neg, "-") {
        }

        public override int TypedValue { 
            get { return -Expression.TypedValue; } 
        }
    }

    public class OnesComplement : UnaryOp<int, IntExpression> {
        public OnesComplement(IntExpression exp)
            : base(exp, OpCodes.Not, "~") {
        }

        public override int TypedValue { 
            get { return ~Expression.TypedValue; } 
        }
    }

    public class Not : UnaryOp<bool, BoolExpression> {
        
        public Not(BoolExpression exp)
            : base(exp, OpCodes.Not, "!") {
        }

        public override bool TypedValue { 
            get { return !Expression.TypedValue; } 
        }
        
        public override void Compile(ILGenerator il) {
            Expression.Compile(il);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ceq);
        }
    }
}