using While.AST;
using While.AST.Expressions;
using While.AST.Sequences;
using While.AST.Statements;
using System.Collections.Generic;

using System;

namespace While.Parsing {



public partial class Parser {
	public const int _EOF = 0;
	public const int _ident = 1;
	public const int _number = 2;
	public const int maxT = 49;




	void Get () {
		for (;;) {
			t = la;
			la = scanner.Scan();
			if (la.kind <= maxT) { ++errDist; break; }

			la = t;
		}
	}
	
	void Program() {
		StatementSequence statements = null;
		ProcedureSequence procs = new ProcedureSequence(); 
		Token startTok = null, endTok = null; 
		if (IsProcProgram()) {
			Expect(3);
			startTok = t; 
			Proc(procs);
			while (la.kind == 5) {
				Proc(procs);
			}
			StmtSeq(out statements);
			Expect(4);
			endTok = t; 
		} else if (StartOf(1)) {
			StmtSeq(out statements);
		} else SynErr(50);
		WhileProgram.Instance = new WhileProgram(procs, statements);
		if (startTok != null && endTok != null) {
			WhileProgram.Instance.AddSequencePoint(startTok);
			WhileProgram.Instance.AddSequencePoint(endTok);
		}
		
	}

	void Proc(ProcedureSequence procs) {
		StatementSequence statements;
		string name;
		VariableSequence valArgs = new VariableSequence();
		Variable resultArg = null; 
		Expect(5);
		Token ptok = t; 
		Expect(1);
		name = t.val; 
		Expect(6);
		if (la.kind == 7 || la.kind == 8) {
			if (la.kind == 7) {
				Get();
				Expect(1);
				Variable v = new Variable(t.val);
				v.IsValueArg = true;
				valArgs.AddVariable(v);
				SymbolTable.DefineArgument(t.val); 
				if (la.kind == 12) {
					Args(valArgs, out resultArg);
				}
			} else {
				Get();
				Expect(1);
				resultArg = new Variable(t.val);
				resultArg.IsResultArg = true;
				SymbolTable.DefineResultArgument(t.val); 
			}
		}
		Expect(9);
		SequencePoint seq1 = new SequencePoint(ptok.line,ptok.col, t.line,t.col+t.val.Length); 
		Expect(10);
		StmtSeq(out statements);
		Expect(4);
		SequencePoint seq2 = new SequencePoint(t.line, t.col, t.line, t.col+t.val.Length); 
		Expect(11);
		if (procs.ContainsProcedure(name)) {
		errors.SemErr(ptok.line, ptok.col, "Procedure '" + name + "' is already declared");
		} else {
			Procedure proc = new Procedure(name, valArgs, resultArg, statements);
			proc.AddSequencePoint(seq1);
			proc.AddSequencePoint(seq2);
			procs.AddProcedure(proc);
		}
		SymbolTable.Clear();
		
	}

	void StmtSeq(out StatementSequence statements) {
		Statement stmt;
		statements = new StatementSequence();
		
		Stmt(out stmt);
		statements.AddStatement(stmt); 
		while (la.kind == 11) {
			Get();
			Stmt(out stmt);
			statements.AddStatement(stmt); 
		}
	}

	void Args(VariableSequence valArgs, out Variable resultArg) {
		resultArg = null; 
		if (IsStartOfResultArg()) {
			Expect(12);
			Expect(8);
			Expect(1);
			resultArg = new Variable(t.val);
			resultArg.IsResultArg = true;
			if (SymbolTable.IsDeclaredInCurrentScope(t.val)) {
				errors.SemErr(t.line, t.col, string.Format("Argument '{0}' is already declared in this scope",t.val));
			} else {
				SymbolTable.DefineResultArgument(t.val);
			}
			
		} else if (la.kind == 12) {
			Get();
			Expect(1);
			Variable v = new Variable(t.val);
			v.IsValueArg = true;
			valArgs.AddVariable(v);
			if (SymbolTable.IsDeclaredInCurrentScope(t.val)) {
				errors.SemErr(t.line, t.col, string.Format("Argument '{0}' is already declared in this scope",t.val));
			} else {
				SymbolTable.DefineArgument(t.val);
			}
			
			if (la.kind == 12) {
				Args(valArgs, out resultArg);
			}
		} else SynErr(51);
	}

	void Stmt(out Statement stmt) {
		Expression exp = null;
		                                     stmt = null;
		int sl = la.line, sc = la.col;
		StatementSequence stmtSeq; 
		Token bf; 
		switch (la.kind) {
		case 1: {
			AssignStmt(out stmt);
			stmt.AddSequencePoint(sl,sc, t.line,t.col+t.val.Length); 
			break;
		}
		case 13: {
			Get();
			stmt = new Skip();stmt.AddSequencePoint(t); 
			break;
		}
		case 3: {
			BlockStmt(out stmt);
			break;
		}
		case 18: {
			IfStmt(out stmt);
			break;
		}
		case 22: {
			WhileStmt(out stmt);
			break;
		}
		case 15: {
			ReadStmt(out stmt);
			stmt.AddSequencePoint(sl,sc, t.line,t.col+t.val.Length);
			break;
		}
		case 14: {
			Get();
			Expr(out exp);
			stmt = new Write(exp); stmt.AddSequencePoint(sl,sc, t.line,t.col+t.val.Length); 
			break;
		}
		case 25: {
			CallProc(out stmt);
			stmt.AddSequencePoint(sl,sc, t.line,t.col+t.val.Length); 
			break;
		}
		case 6: {
			Get();
			bf = t; 
			StmtSeq(out stmtSeq);
			Expect(9);
			stmt = stmtSeq;
			stmt.AddSequencePoint(bf);
			stmt.AddSequencePoint(t); 
			break;
		}
		default: SynErr(52); break;
		}
	}

	void AssignStmt(out Statement assign) {
		Expression exp;
		Variable var; 
		assign = null; 
		Expect(1);
		var = new Variable(t.val);
		if (!SymbolTable.IsInScope(t.val) && !Options.BookVersion) {
			errors.SemErr(t.line, t.col, string.Format("Assignment to undeclared variable '{0}'",t.val));
		}
		
		Expect(17);
		Token tok = t; 
		Expr(out exp);
		if (!ExpectInt(exp, tok, true)) {
		return;
		} 
		
		assign = new Assign(var, (TypedExpression<int>)exp); 
	}

	void BlockStmt(out Statement block) {
		Expect(3);
		if (Options.BookVersion) {
		errors.SemErr(t.line, t.col, "Variable declarations are only allowed when using the /coursesyntax switch. Type 'wc.exe /help' for more information");
		While.Environment.Exit(1);
		}
		VariableDeclarationSequence vars = new VariableDeclarationSequence();
		SymbolTable.PushScope();
		int sl = t.line;
		int sc = t.col;
		int el = t.line;
		int ec = t.col+t.val.Length;
		
		if (la.kind == 16) {
			VarDecStmt(out vars);
		}
		StatementSequence statements; 
		StmtSeq(out statements);
		Expect(4);
		block = new Block(vars, statements);
		block.AddSequencePoint(sl,sc,el,ec);
		block.AddSequencePoint(t);
		SymbolTable.PopScope(); 
	}

	void IfStmt(out Statement ifStmt) {
		StatementSequence ifBranch = null; 
		StatementSequence elseBranch = null;
		Statement tmpStmt = null;
		Expression exp = null;
		ifStmt = null;
		
		Expect(18);
		int sl = t.line; int sc = t.col; Token tok = t; 
		Expr(out exp);
		if (!ExpectBool(exp, tok, true)) {
		                             return;
		                         } 
		Expect(19);
		int el = t.line; int ec = t.col+t.val.Length; 
		if (Options.BookVersion) {
			Stmt(out tmpStmt);
			ifBranch = ToStatementSequence(tmpStmt); 
			if (la.kind == 20) {
				Get();
				Stmt(out tmpStmt);
				elseBranch = ToStatementSequence(tmpStmt); 
			}
		} else if (StartOf(1)) {
			StmtSeq(out ifBranch);
			if (la.kind == 20) {
				Get();
				StmtSeq(out elseBranch);
			}
			Expect(21);
			ifBranch.AddSequencePoint(t);
			if (elseBranch != null) {
			    elseBranch.AddSequencePoint(t);
			} 
			
		} else SynErr(53);
		ifStmt = new If((TypedExpression<bool>)exp, ifBranch, elseBranch);
		ifStmt.AddSequencePoint(sl,sc,el,ec);
		
	}

	void WhileStmt(out Statement whileStmt) {
		Expression exp = null;
		StatementSequence whileBranch = null;
		Statement branchStmt = null;
		whileStmt = null;
		                                      
		Expect(22);
		int sl = t.line; int sc = t.col; Token tok = t; 
		Expr(out exp);
		if (!ExpectBool(exp, tok, true)) { return; } 
		Expect(23);
		int el = t.line; int ec = t.col+t.val.Length; 
		if (Options.BookVersion) {
			Stmt(out branchStmt);
			whileBranch = ToStatementSequence(branchStmt); 
		} else if (StartOf(1)) {
			StmtSeq(out whileBranch);
			Expect(24);
			whileBranch.AddSequencePoint(t); 
		} else SynErr(54);
		whileStmt = new While.AST.Statements.While((TypedExpression<bool>)exp, whileBranch);
		whileStmt.AddSequencePoint(sl,sc,el,ec);
		
	}

	void ReadStmt(out Statement stmt) {
		stmt = null; 
		Expect(15);
		if (la.kind == 1) {
			Get();
			stmt = new Read(new Variable(t.val)); 
		} else if (la.kind == 6) {
			Get();
			Expect(1);
			stmt = new Read(new Variable(t.val)); 
			Expect(9);
		} else SynErr(55);
	}

	void Expr(out Expression exp) {
		LogicOr(out exp);
	}

	void CallProc(out Statement callStmt) {
		Expression exp;
		List<Expression> expressions = new List<Expression>();	
		Expect(25);
		Token callToken = t;
		Token exprToken = null;
		Expect(1);
		string proc = t.val; 
		Expect(6);
		if (StartOf(2)) {
			exprToken = la; 
			Expr(out exp);
			expressions.Add(exp); ExpectIntArg(exp, exprToken); 
			while (la.kind == 12) {
				Get();
				exprToken = la; 
				Expr(out exp);
				expressions.Add(exp); ExpectIntArg(exp, exprToken); 
			}
		}
		Expect(9);
		callStmt = new Call(proc, expressions, callToken, exprToken); 
	}

	void VarDecStmt(out VariableDeclarationSequence vars) {
		vars = new VariableDeclarationSequence(); 
		VarDec(vars);
		while (la.kind == 16) {
			VarDec(vars);
		}
	}

	void VarDec(VariableDeclarationSequence vars) {
		Expect(16);
		int sl = t.line; 
		int sc = t.col;
		int el = la.line;
		int ec = la.col+la.val.Length; 
		Expect(1);
		if (SymbolTable.IsDeclaredInCurrentScope(t.val)) {
		errors.SemErr(t.line, t.col, string.Format("Variable '{0}' is already declared in this scope", t.val));
		} else if (SymbolTable.IsInScope(t.val)) {
			errors.Warning(t.line, t.col, string.Format("Variable '{0}' hides variable with same name in outer block",t.val));
			SymbolTable.DefineVariable(t.val);
		} else {
			SymbolTable.DefineVariable(t.val);
		}
		VariableDeclaration vd = new VariableDeclaration(new Variable(t.val));
		vd.AddSequencePoint(sl,sc,el,ec);
		vars.AddVariableDeclaration(vd); 
		Expect(11);
	}

	void LogicOr(out Expression exp) {
		Expression second; 
		LogicAnd(out exp);
		while (la.kind == 26) {
			Get();
			Token tok = t; 
			LogicAnd(out second);
			if (!ExpectBool(exp, tok, false)) { return; } 
			if (!ExpectBool(second, tok, true)) { return; } 
			exp = new LogicalOr((TypedExpression<bool>)exp, (TypedExpression<bool>)second); 
		}
	}

	void LogicAnd(out Expression exp) {
		Expression second; 
		LogicXor(out exp);
		while (la.kind == 27) {
			Get();
			Token tok = t; 
			LogicXor(out second);
			if (!ExpectBool(exp, tok, false)) { return; } 
			if (!ExpectBool(second, tok, true)) { return; } 
			exp = new LogicalAnd((TypedExpression<bool>) exp, (TypedExpression<bool>) second); 
		}
	}

	void LogicXor(out Expression exp) {
		Expression second; 
		Comparison(out exp);
		while (la.kind == 28) {
			Get();
			Token tok = t; 
			Comparison(out second);
			if (!ExpectBool(exp, tok, false)) { return; } 
			if (!ExpectBool(second, tok, true)) { return; } 
			exp = new LogicalXor((TypedExpression<bool>)exp, (TypedExpression<bool>)second); 
		}
	}

	void Comparison(out Expression exp) {
		Expression second; 
		BitOr(out exp);
		if (StartOf(3)) {
			switch (la.kind) {
			case 29: {
				Get();
				break;
			}
			case 30: {
				Get();
				break;
			}
			case 31: {
				Get();
				break;
			}
			case 32: {
				Get();
				break;
			}
			case 33: {
				Get();
				break;
			}
			case 34: {
				Get();
				break;
			}
			}
			Token tok = t; 
			Comparison(out second);
			if (!ExpectInt(exp, tok, false)) { return; };
			if (!ExpectInt(second, tok, true)) { return; };
			if (tok.val == "<") {
			    exp = new LessThan((TypedExpression<int>)exp, (TypedExpression<int>)second);
			} else if (tok.val == ">") {
			    exp = new GreaterThan((TypedExpression<int>)exp, (TypedExpression<int>)second); 
			} else if (tok.val == "<=") {
			    exp = new LessThanOrEqual((TypedExpression<int>)exp, (TypedExpression<int>)second); 
			} else if (tok.val == ">=") {
			    exp = new GreaterThanOrEqual((TypedExpression<int>)exp, (TypedExpression<int>)second); 
			} else if (tok.val == "==") {
			    exp = new Equal((TypedExpression<int>)exp, (TypedExpression<int>)second); 
			} else if (tok.val == "!=") {
			    exp = new NotEqual((TypedExpression<int>)exp, (TypedExpression<int>)second); 
			}
			
		}
	}

	void BitOr(out Expression exp) {
		Expression second; 
		BitXor(out exp);
		while (la.kind == 35) {
			Get();
			Token tok = t; 
			BitXor(out second);
			if (!ExpectInt(exp, tok, false)) { return; } 
			if (!ExpectInt(second, tok, true)) { return; } 
			exp = new BitwiseOr((TypedExpression<int>)exp, (TypedExpression<int>)second); 
		}
	}

	void BitXor(out Expression exp) {
		Expression second; 
		BitAnd(out exp);
		while (la.kind == 36) {
			Get();
			Token tok = t; 
			BitAnd(out second);
			if (!ExpectInt(exp, tok, false)) { return; } 
			if (!ExpectInt(second, tok, true)) { return; } 
			exp = new BitwiseXor((TypedExpression<int>)exp, (TypedExpression<int>)second); 
		}
	}

	void BitAnd(out Expression exp) {
		Expression second; 
		BitShift(out exp);
		while (la.kind == 37) {
			Get();
			Token tok = t; 
			BitShift(out second);
			if (!ExpectInt(exp, tok, false)) { return; } 
			if (!ExpectInt(second, tok, true)) { return; } 
			exp = new BitwiseAnd((TypedExpression<int>)exp, (TypedExpression<int>)second); 
		}
	}

	void BitShift(out Expression exp) {
		Expression second; 
		PlusMinus(out exp);
		while (la.kind == 38 || la.kind == 39) {
			if (la.kind == 38) {
				Get();
			} else {
				Get();
			}
			Token tok = t; 
			PlusMinus(out second);
			if (!ExpectInt(exp, tok, false)) { return; } 
			if (!ExpectInt(second, tok, true)) { return; } 
			if (tok.val == "<<") {
			    exp = new ShiftLeft((TypedExpression<int>)exp, (TypedExpression<int>)second);
			} else if (tok.val == ">>") {
			    exp = new ShiftRight((TypedExpression<int>)exp, (TypedExpression<int>)second);
			}
			
		}
	}

	void PlusMinus(out Expression exp) {
		Expression second; 
		MulDivMod(out exp);
		while (la.kind == 40 || la.kind == 41) {
			if (la.kind == 40) {
				Get();
			} else {
				Get();
			}
			Token tok = t; 
			MulDivMod(out second);
			if (!ExpectInt(exp, tok, false)) { return; } 
			if (!ExpectInt(second, tok, true)) { return; } 
			if (tok.val == "+") {
			    exp = new Plus((TypedExpression<int>)exp, (TypedExpression<int>)second);
			} else if (tok.val == "-") {
			    exp = new Minus((TypedExpression<int>)exp, (TypedExpression<int>)second);
			}
			 
		}
	}

	void MulDivMod(out Expression exp) {
		Expression second; 
		UnaryOperator(out exp);
		while (la.kind == 42 || la.kind == 43 || la.kind == 44) {
			if (la.kind == 42) {
				Get();
			} else if (la.kind == 43) {
				Get();
			} else {
				Get();
			}
			Token tok = t; 
			UnaryOperator(out second);
			if (!ExpectInt(exp, tok, false)) { return; } 
			if (!ExpectInt(second, tok, true)) { return; } 
			if (tok.val == "*") {
			    exp = new Multiplication((TypedExpression<int>)exp, (TypedExpression<int>)second);
			} else if (tok.val == "/") {
			    exp = new Division((TypedExpression<int>)exp, (TypedExpression<int>)second);
			} else if (tok.val == "%") {
			    exp = new Modulo((TypedExpression<int>)exp, (TypedExpression<int>)second);
			}
			
		}
	}

	void UnaryOperator(out Expression exp) {
		Token tok = null; string op = null; 
		if (la.kind == 41 || la.kind == 45 || la.kind == 46) {
			if (la.kind == 41) {
				Get();
				tok = t; op = t.val; 
			} else if (la.kind == 45) {
				Get();
				tok = t; op = t.val; 
			} else {
				Get();
				tok = t; op = t.val; 
			}
		}
		Terminal(out exp);
		if (op == "-") {
		if (!ExpectInt(exp, tok, true)) { return; }
		exp = new UnaryMinus((TypedExpression<int>)exp);
		} else if (op == "~") {
			if (!ExpectInt((TypedExpression<int>)exp, tok, true)) { return; }
			exp = new OnesComplement((TypedExpression<int>)exp);
		} else if (op == "not") {
			if (!ExpectBool(exp, tok, true)) { return; }
			exp = new Not((TypedExpression<bool>)exp);
		}
		
	}

	void Terminal(out Expression exp) {
		exp = null; 
		if (la.kind == 1) {
			Get();
			exp = new Variable(t.val);
			if (!SymbolTable.IsInScope(t.val) && !Options.BookVersion) {
				errors.SemErr(t.line, t.col, string.Format("Undeclared variable '{0}'", t.val)); 
			}
			
		} else if (la.kind == 2) {
			Get();
			exp = new Number(int.Parse(t.val)); 
		} else if (la.kind == 47) {
			Get();
			exp = new Bool(true); 
		} else if (la.kind == 48) {
			Get();
			exp = new Bool(false); 
		} else if (la.kind == 6) {
			Get();
			Expr(out exp);
			Expect(9);
		} else SynErr(56);
	}



	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		Program();

    Expect(0);
	}
	
	static readonly bool[,] set = {
		{T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
		{x,T,x,T, x,x,T,x, x,x,x,x, x,T,T,T, x,x,T,x, x,x,T,x, x,T,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
		{x,T,T,x, x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,T,T,T, T,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,T, T,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x}

	};
} // end Parser


public partial class Errors {
    private string GetErrorMessage(int n) {
        string s = null;
        switch(n) {
			case 0: s = "EOF expected"; break;
			case 1: s = "ident expected"; break;
			case 2: s = "number expected"; break;
			case 3: s = "\"begin\" expected"; break;
			case 4: s = "\"end\" expected"; break;
			case 5: s = "\"proc\" expected"; break;
			case 6: s = "\"(\" expected"; break;
			case 7: s = "\"val\" expected"; break;
			case 8: s = "\"res\" expected"; break;
			case 9: s = "\")\" expected"; break;
			case 10: s = "\"is\" expected"; break;
			case 11: s = "\";\" expected"; break;
			case 12: s = "\",\" expected"; break;
			case 13: s = "\"skip\" expected"; break;
			case 14: s = "\"write\" expected"; break;
			case 15: s = "\"read\" expected"; break;
			case 16: s = "\"var\" expected"; break;
			case 17: s = "\":=\" expected"; break;
			case 18: s = "\"if\" expected"; break;
			case 19: s = "\"then\" expected"; break;
			case 20: s = "\"else\" expected"; break;
			case 21: s = "\"fi\" expected"; break;
			case 22: s = "\"while\" expected"; break;
			case 23: s = "\"do\" expected"; break;
			case 24: s = "\"od\" expected"; break;
			case 25: s = "\"call\" expected"; break;
			case 26: s = "\"or\" expected"; break;
			case 27: s = "\"and\" expected"; break;
			case 28: s = "\"xor\" expected"; break;
			case 29: s = "\"<\" expected"; break;
			case 30: s = "\">\" expected"; break;
			case 31: s = "\"<=\" expected"; break;
			case 32: s = "\">=\" expected"; break;
			case 33: s = "\"==\" expected"; break;
			case 34: s = "\"!=\" expected"; break;
			case 35: s = "\"|\" expected"; break;
			case 36: s = "\"^\" expected"; break;
			case 37: s = "\"&\" expected"; break;
			case 38: s = "\"<<\" expected"; break;
			case 39: s = "\">>\" expected"; break;
			case 40: s = "\"+\" expected"; break;
			case 41: s = "\"-\" expected"; break;
			case 42: s = "\"*\" expected"; break;
			case 43: s = "\"/\" expected"; break;
			case 44: s = "\"%\" expected"; break;
			case 45: s = "\"~\" expected"; break;
			case 46: s = "\"not\" expected"; break;
			case 47: s = "\"true\" expected"; break;
			case 48: s = "\"false\" expected"; break;
			case 49: s = "??? expected"; break;
			case 50: s = "invalid Program"; break;
			case 51: s = "invalid Args"; break;
			case 52: s = "invalid Stmt"; break;
			case 53: s = "invalid IfStmt"; break;
			case 54: s = "invalid WhileStmt"; break;
			case 55: s = "invalid ReadStmt"; break;
			case 56: s = "invalid Terminal"; break;

        }
        return s;
    }
} // Errors

}