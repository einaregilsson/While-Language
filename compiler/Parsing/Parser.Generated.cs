using While.AST;
using While.AST.Expressions;
using While.AST.Sequences;
using While.AST.Statements;

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
		StatementSequence statements;
		ProcedureSequence procs = new ProcedureSequence(); 
		Token startTok, endTok; 
		if (IsProcProgram()) {
			Expect(3);
			startTok = t; 
			Proc(procs);
			while (la.kind == 5) {
				Proc(procs);
			}
			StmtSeq(statements);
			Expect(4);
			endTok = t; 
		} else if (StartOf(1)) {
			StmtSeq(statements);
		} else SynErr(50);
		WhileProgram.Instance = new WhileProgram(statements, procs);
		if (startTok != null && endTok != null) {
			WhileProgram.Instance.AddSequencePoint(startTok);
			WhileProgram.Instance.AddSequencePoint(endTok);
		}
		
	}

	void Proc(ProcedureSequence procs) {
		StatementSequence statements;
		string name;
		List<Variable> valArgs = new List<Variable>();
		Variable resultArg; 
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
				valArgs.Add(v);
				SymbolTable.DefineArgument(t.val) 
				if (la.kind == 12) {
					Args(valArgs, resultArg);
				}
			} else {
				Get();
				Expect(1);
				Variable resultArg = new Variable(t.val);
				resultArg.IsResultArg = true;
				SymbolTable.DefineResultArgument(t.val); 
			}
		}
		Expect(9);
		SequencePoint seq1 = new SequencePoint(ptok.line,ptok.col, t.line,t.col+t.val.Length); 
		Expect(10);
		StmtSeq(statements);
		Expect(4);
		seq2 = new SequencePoint(t.line, t.col, t.line, t.col+t.val.Length); 
		Expect(11);
		if (procs.ContainsProcedure(name)) {
		errors.SemErr(ptok.line, ptok.col, "Procedure '" + name + "' is already declared");
		} else {
			proc = Procedure(name, valArgs, resultArg, statements);
			proc.AddSequencePoint(seq1);
			proc.AddSequencePoint(seq2);
			procs.Add(name, proc);
		}
		SymbolTable.Clear();
		
	}

	void StmtSeq(ref StatementSequence statements) {
		Statement stmt;
		statements = new StatementSequence();
		
		Stmt(stmt);
		statements.AddStatement(stmt); 
		while (la.kind == 11) {
			Get();
			Stmt(stmt);
			statements.AddStatement(stmt); 
		}
	}

	void Args(VariableSequence vars, ref Variable resultArg) {
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
				errors.SemErr(t.line, t.col, string.Format("Argument '${0}' is already declared in this scope",t.val));
			} else {
				SymbolTable.DefineArgument(t.val);
			}
			
			if (la.kind == 12) {
				Args(valArgs, resultArg);
			}
		} else SynErr(51);
	}

	void Stmt(ref Statement stmt) {
		Expression exp;
		int sl = la.line, sc = la.col;
		StatementSequence stmtSeq; 
		Token bf; 
		switch (la.kind) {
		case 1: {
			AssignStmt(stmt);
			stmt.AddSequencePoint((sl,sc, t.line,t.col+t.val.Length)); 
			break;
		}
		case 13: {
			Get();
			stmt = new Skip();stmt.AddSequencePoint(t); 
			break;
		}
		case 3: {
			BlockStmt(stmt);
			break;
		}
		case 18: {
			IfStmt(stmt);
			break;
		}
		case 22: {
			WhileStmt(stmt);
			break;
		}
		case 15: {
			ReadStmt(stmt);
			stmt.AddSequencePoint((sl,sc, t.line,t.col+t.val.Length));
			break;
		}
		case 14: {
			Get();
			Expr(exp);
			stmt = new Write(exp); stmt.AddSequencePoint((sl,sc, t.line,t.col+t.val.Length)); 
			break;
		}
		case 25: {
			CallProc(stmt);
			stmt.AddSequencePoint((sl,sc, t.line,t.col+t.val.Length)); 
			break;
		}
		case 6: {
			Get();
			bf = t; 
			StmtSeq(stmtSeq);
			Expect(9);
			stmt = stmtSeq;
			stmt.AddSequencePoint(bf);
			stmt.AddSequencePoint(t); 
			break;
		}
		default: SynErr(52); break;
		}
	}

	void AssignStmt(ref Statement assign) {
		Expression exp;
		Variable var; 
		Expect(1);
		var = new Variable(t.val);
		if (!SymbolTable.IsInScope(t.val) and not CompileOptions.BookVersion) {
			errors.SemErr(t.line, t.col, string.Format("Assignment to undeclared variable '{0}'",t.val));
		}
		
		Expect(17);
		Token tok = t 
		Expr(exp);
		if (!ExpectInt(exp, tok, true)) {
		return;
		} 
		
		assign = new Assign(var, exp); 
	}

	void BlockStmt(ref block as Statement) {
		Expect(3);
		if (CompileOptions.BookVersion) {
		errors.SemErr(t.line, t.col, "Variable declarations are only allowed when using the /coursesyntax switch. Type 'wc.exe /help' for more information")M
		System.Environment.Exit(1);
		}
		VariableDeclarationSequence vars = new VariableDeclarationSequence();
		SymbolTable.PushScope();
		int sl = t.line;
		int sc = t.col;
		int el = t.line;
		int ec = t.col+t.val.Length;
		
		if (la.kind == 16) {
			VarDecStmt(vars);
		}
		StatementSequence statements; 
		StmtSeq(statements);
		Expect(4);
		Block block = new Block(vars, statements);
		block.AddSequencePoint((sl,sc,el,ec));
		block.AddSequencePoint(t);
		SymbolTable.PopScope(); 
	}

	void IfStmt(ref ifStmt as Statement) {
		ifBranch as StatementSequence; 
		elseBranch as StatementSequence
		tmpStmt as Statement
		exp as Expression
		sl as int
		sc as int
		el as int
		ec as int
		
		Expect(18);
		sl,sc,tok = t.line, t.col, t 
		Expr(exp);
		return unless ExpectBool(exp, tok, true) 
		Expect(19);
		el,ec = t.line, t.col+t.val.Length 
		if (CompileOptions.BookVersion) {
			Stmt(tmpStmt);
			ifBranch = ToStmtSeq(tmpStmt) 
			if (la.kind == 20) {
				Get();
				Stmt(tmpStmt);
				elseBranch = ToStmtSeq(tmpStmt) 
			}
		} else if (StartOf(1)) {
			StmtSeq(ifBranch);
			if (la.kind == 20) {
				Get();
				StmtSeq(elseBranch);
			}
			Expect(21);
			ifBranch.AddSequencePoint(t)
			elseBranch.AddSequencePoint(t) if elseBranch
			
		} else SynErr(53);
		ifStmt = If(exp, ifBranch, elseBranch) 
		ifStmt.AddSequencePoint((sl,sc,el,ec))
		
	}

	void WhileStmt(ref whileStmt as Statement) {
		exp as Expression
		whileBranch as StatementSequence
		branchStmt as Statement
		sl as int
		sc as int
		el as int
		ec as int
		Expect(22);
		sl,sc,tok = t.line, t.col,t 
		Expr(exp);
		return unless ExpectBool(exp, tok, true) 
		Expect(23);
		el,ec = t.line, t.col+t.val.Length 
		if (CompileOptions.BookVersion) {
			Stmt(branchStmt);
			whileBranch = ToStmtSeq(branchStmt) 
		} else if (StartOf(1)) {
			StmtSeq(whileBranch);
			Expect(24);
			whileBranch.AddSequencePoint(t) 
		} else SynErr(54);
		whileStmt = While(exp, whileBranch) 
		whileStmt.AddSequencePoint((sl,sc,el,ec))
		
	}

	void ReadStmt(ref stmt as Statement) {
		Expect(15);
		if (la.kind == 1) {
			Get();
			stmt = new Read(Variable(t.val)); 
		} else if (la.kind == 6) {
			Get();
			Expect(1);
			stmt = new Read(Variable(t.val)); 
			Expect(9);
		} else SynErr(55);
	}

	void Expr(ref exp as Expression) {
		LogicOr(exp);
	}

	void CallProc(ref callStmt as Statement) {
		exp as Expression
		list = List[of Expression]()	
		Expect(25);
		callToken = t 
		exprToken as Token
		Expect(1);
		proc = t.val 
		Expect(6);
		if (StartOf(2)) {
			exprToken = la 
			Expr(exp);
			list.Add(exp); ExpectIntArg(exp, exprToken) 
			while (la.kind == 12) {
				Get();
				exprToken = la 
				Expr(exp);
				list.Add(exp); ExpectIntArg(exp, exprToken) 
			}
		}
		Expect(9);
		callStmt = Call(proc, list, callToken, exprToken) 
	}

	void VarDecStmt(ref VariableDeclarationSequence vars) {
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
		vd = VariableDeclaration(Variable(t.val));
		vd.AddSequencePoint((sl,sc,el,ec));
		vars.AddVariableDeclaration(vd); 
		Expect(11);
	}

	void LogicOr(ref exp as Expression) {
		second as Expression 
		LogicAnd(exp);
		while (la.kind == 26) {
			Get();
			tok = t 
			LogicAnd(second);
			return unless ExpectBool(exp, tok, false) 
			return unless ExpectBool(second, tok, true) 
			exp = LogicBinaryOp(exp, second, LogicBinaryOp.Or) 
		}
	}

	void LogicAnd(ref exp as Expression) {
		second as Expression 
		LogicXor(exp);
		while (la.kind == 27) {
			Get();
			tok = t 
			LogicXor(second);
			return unless ExpectBool(exp, tok, false) 
			return unless ExpectBool(second, tok, true) 
			exp = LogicBinaryOp(exp, second, LogicBinaryOp.And) 
		}
	}

	void LogicXor(ref exp as Expression) {
		second as Expression 
		Comparison(exp);
		while (la.kind == 28) {
			Get();
			tok = t 
			Comparison(second);
			return unless ExpectBool(exp, tok, false) 
			return unless ExpectBool(second, tok, true) 
			exp = LogicBinaryOp(exp, second, LogicBinaryOp.Xor) 
		}
	}

	void Comparison(ref exp as Expression) {
		second as Expression 
		op as string 
		BitOr(exp);
		if (StartOf(3)) {
			switch (la.kind) {
			case 29: {
				Get();
				op = ComparisonBinaryOp.LessThan 
				break;
			}
			case 30: {
				Get();
				op = ComparisonBinaryOp.GreaterThan 
				break;
			}
			case 31: {
				Get();
				op = ComparisonBinaryOp.LessThanOrEqual 
				break;
			}
			case 32: {
				Get();
				op = ComparisonBinaryOp.GreaterThanOrEqual 
				break;
			}
			case 33: {
				Get();
				op = ComparisonBinaryOp.Equal
				break;
			}
			case 34: {
				Get();
				op = ComparisonBinaryOp.NotEqual 
				break;
			}
			}
			tok = t 
			Comparison(second);
			return unless ExpectInt(exp, tok, false) 
			return unless ExpectInt(second, tok, true) 
			exp = ComparisonBinaryOp(exp, second, op) 
		}
	}

	void BitOr(ref exp as Expression) {
		second as Expression 
		BitXor(exp);
		while (la.kind == 35) {
			Get();
			tok = t 
			BitXor(second);
			return unless ExpectInt(exp, tok, false) 
			return unless ExpectInt(second, tok, true) 
			exp = BitBinaryOp(exp, second, BitBinaryOp.Or) 
		}
	}

	void BitXor(ref exp as Expression) {
		second as Expression 
		BitAnd(exp);
		while (la.kind == 36) {
			Get();
			tok = t 
			BitAnd(second);
			return unless ExpectInt(exp, tok, false) 
			return unless ExpectInt(second, tok, true) 
			exp = BitBinaryOp(exp, second, BitBinaryOp.Xor) 
		}
	}

	void BitAnd(ref exp as Expression) {
		second as Expression 
		BitShift(exp);
		while (la.kind == 37) {
			Get();
			tok = t 
			BitShift(second);
			return unless ExpectInt(exp, tok, false) 
			return unless ExpectInt(second, tok, true) 
			exp = BitBinaryOp(exp, second, BitBinaryOp.And) 
		}
	}

	void BitShift(ref exp as Expression) {
		second as Expression
		op as string 
		PlusMinus(exp);
		while (la.kind == 38 || la.kind == 39) {
			if (la.kind == 38) {
				Get();
				op = BitBinaryOp.ShiftLeft 
			} else {
				Get();
				op = BitBinaryOp.ShiftRight 
			}
			tok = t 
			PlusMinus(second);
			return unless ExpectInt(exp, tok, false) 
			return unless ExpectInt(second, tok, true) 
			exp = BitBinaryOp(exp, second, op)
		}
	}

	void PlusMinus(ref exp as Expression) {
		second as Expression 
		op as string
		MulDivMod(exp);
		while (la.kind == 40 || la.kind == 41) {
			if (la.kind == 40) {
				Get();
				op = ArithmeticBinaryOp.Plus 
			} else {
				Get();
				op = ArithmeticBinaryOp.Minus 
			}
			tok = t 
			MulDivMod(second);
			return unless ExpectInt(exp, tok, false) 
			return unless ExpectInt(second, tok, true) 
			exp = ArithmeticBinaryOp(exp, second, op) 
		}
	}

	void MulDivMod(ref exp as Expression) {
		second as Expression 
		UnaryOperator(exp);
		while (la.kind == 42 || la.kind == 43 || la.kind == 44) {
			if (la.kind == 42) {
				Get();
				op = ArithmeticBinaryOp.Multiplication 
			} else if (la.kind == 43) {
				Get();
				op = ArithmeticBinaryOp.Division 
			} else {
				Get();
				op = ArithmeticBinaryOp.Modulo 
			}
			tok = t 
			UnaryOperator(second);
			return unless ExpectInt(exp, tok, false) 
			return unless ExpectInt(second, tok, true) 
			exp = ArithmeticBinaryOp(exp, second, op) 
		}
	}

	void UnaryOperator(ref exp as Expression) {
		op as string = null 
		if (la.kind == 41 || la.kind == 45 || la.kind == 46) {
			if (la.kind == 41) {
				Get();
				op = t.val 
			} else if (la.kind == 45) {
				Get();
				op = t.val 
			} else {
				Get();
				op = t.val 
				tok = t 
			}
		}
		Terminal(exp);
		if op in ('-','~'): 
		return unless ExpectInt(exp, tok, true)
		exp = IntUnaryOp(exp, op)
		elif op == 'not':
			return unless ExpectBool(exp, tok, true)
			exp = NotUnaryOp(exp) 
	}

	void Terminal(ref exp as Expression) {
		if (la.kind == 1) {
			Get();
			exp = Variable(t.val) 
			if not SymbolTable.IsInScope(t.val) and not CompileOptions.BookVersion:
				errors.SemErr(t.line, t.col, "Undeclared variable '${t.val}'") 
		} else if (la.kind == 2) {
			Get();
			exp = Number(int.Parse(t.val)) 
		} else if (la.kind == 47) {
			Get();
			exp = Bool(true) 
		} else if (la.kind == 48) {
			Get();
			exp = Bool(false) 
		} else if (la.kind == 6) {
			Get();
			Expr(exp);
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