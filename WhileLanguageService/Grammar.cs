using System;
using System.Collections.Generic;
using System.Linq;
using Irony.Compiler;

namespace Demo
{
    [Language("While", "1.0", "While Programming Language")]
    public class Grammar : Irony.Compiler.Grammar
    {
        public Grammar()
        {
            #region Declare Terminals Here
            CommentTerminal blockComment = new CommentTerminal("block-comment", "/*", "*/");
            CommentTerminal lineComment = new CommentTerminal("line-comment", "//", "\r", "\n", "\u2085", "\u2028", "\u2029");
            CommentTerminal lineComment2 = new CommentTerminal("line-comment2", "#", "\r", "\n", "\u2085", "\u2028", "\u2029");
            NonGrammarTerminals.Add(blockComment);
            NonGrammarTerminals.Add(lineComment);
            NonGrammarTerminals.Add(lineComment2);

            NumberLiteral number = new NumberLiteral("number");
            //number.AddPrefix("0x", NumberFlags.Hex);
            //number.AddSuffixCodes("f", TypeCode.Single);

            IdentifierTerminal identifier = new IdentifierTerminal("identifier", IdFlags.IsNotKeyword);
            #endregion

            #region Declare NonTerminals Here
            var program = new NonTerminal("program");
            var procs = new NonTerminal("procs");
            var proc = new NonTerminal("proc");
            var statements = new NonTerminal("statements");
            var statement = new NonTerminal("statement");
            var block = new NonTerminal("block", "begin ... end");
            var parenParameters = new NonTerminal("paren-parameters");
            
            var variableDeclaration = new NonTerminal("variable-declaration");
            var variableDeclarations = new NonTerminal("variable-declarations");

            var assignment = new NonTerminal("assignment");
            var write = new NonTerminal("write-statement");
            var read = new NonTerminal("read-statement");
            var expression = new NonTerminal("expression");
            var ifStmt = new NonTerminal("if-statement");
            var whileStmt = new NonTerminal("if-statement");
            var callStmt = new NonTerminal("call-statement");
            //NonTerminal declarations = new NonTerminal("declaration");
            //NonTerminal declaration = new NonTerminal("declaration");
            //NonTerminal simpleDeclarations = new NonTerminal("simple-declarations");
            //NonTerminal simpleDeclaration = new NonTerminal("simple-declaration");
            //NonTerminal semiDeclaration = new NonTerminal("semi-declaration");
            
            //NonTerminal parameters = new NonTerminal("parameters");
            //NonTerminal classOption = new NonTerminal("class-option");
            //NonTerminal variableType = new NonTerminal("variable-type");
            //NonTerminal block = new NonTerminal("block");
            //NonTerminal blockContent = new NonTerminal("block-content");
            //NonTerminal statements = new NonTerminal("statements");
            //NonTerminal statement = new NonTerminal("statement");
            //NonTerminal parenExpressionAlways = new NonTerminal("paren-expression-always");
            //NonTerminal parenExpression = new NonTerminal("paren-expression");
            //NonTerminal forHeader = new NonTerminal("for-header");
            //NonTerminal forBlock = new NonTerminal("for-block");
            //NonTerminal semiStatement = new NonTerminal("semi-statement");
            //NonTerminal arguments = new NonTerminal("arguments");
            //NonTerminal parenArguments = new NonTerminal("paren-arguments");
            //NonTerminal assignExpression = new NonTerminal("assign-expression");
            //NonTerminal expression = new NonTerminal("expression");
            //NonTerminal booleanOperator = new NonTerminal("boolean-operator");
            //NonTerminal relationalExpression = new NonTerminal("relational-expression");
            //NonTerminal relationalOperator = new NonTerminal("relational-operator");
            //NonTerminal bitExpression = new NonTerminal("bit-expression");
            //NonTerminal bitOperator = new NonTerminal("bit-operator");
            //NonTerminal addExpression = new NonTerminal("add-expression");
            //NonTerminal addOperator = new NonTerminal("add-operator");
            //NonTerminal multiplyExpression = new NonTerminal("muliply-expression");
            //NonTerminal multiplyOperator = new NonTerminal("multiply-operator");
            //NonTerminal prefixExpression = new NonTerminal("prefix-expression");
            //NonTerminal prefixOperator = new NonTerminal("prefix-operator");
            //NonTerminal factor = new NonTerminal("factor");
            //NonTerminal identifierExpression = new NonTerminal("identifier-expression");
            #endregion

            #region Place Rules Here
            this.Root = program;

            program.Rule = statements;

            procs.Rule = MakeStarRule(procs, proc);
            statements.Rule = statements + ";" + statement | statement;

            proc.Rule = "proc" + identifier + "(" + ")" + "is" + statements + "end" + ";";
            statement.Rule = "skip" | block | assignment | write | read | ifStmt | callStmt | whileStmt | "(" + statements + ")";

            block.Rule = "begin" + variableDeclarations + statements + "end";

            variableDeclarations.Rule = 
                  variableDeclarations + variableDeclaration 
                | Empty;
            
            variableDeclaration.Rule = "var" + identifier + ";";
            
            //Statements
            assignment.Rule = identifier + ":=" + expression;
            read.Rule = "read" + (identifier | "(" + identifier + ")");
            write.Rule = "write" + expression;
            expression.Rule = identifier | number;
            whileStmt.Rule = "while" + expression + "do" + statements + "od";
            ifStmt.Rule = "if" + expression + "then" + statements + ("else" + statements | Empty) + "fi";
            callStmt.Rule = "call" + identifier + "(" + Empty + ")";
                                         
            //declaration.Rule
            //    = classOption + variableType + identifier + parameters + block
            //    | classOption + identifier + parenParameters + block
            //    | variableType + identifier + parenParameters + block
            //    | identifier + parenParameters + block
            //    | simpleDeclaration;

            //simpleDeclarations.Rule = MakePlusRule(simpleDeclarations, simpleDeclaration);

            //simpleDeclaration.Rule = semiDeclaration + ";";

            //semiDeclaration.Rule
            //    = semiDeclaration + "," + identifier
            //    | classOption + variableType + identifier
            //    | variableType + identifier;

            //parameters.Rule
            //    = parameters + "," + variableType + identifier
            //    | variableType + identifier;

            //parenParameters.Rule
            //    = Symbol("(") + ")"
            //    | "(" + parameters + ")";

            //classOption.Rule
            //    = Symbol("static")
            //    | "auto"
            //    | "extern";

            //variableType.Rule
            //    = Symbol("int")
            //    | "void";

            //block.Rule
            //    = Symbol("{") + "}"
            //    | "{" + blockContent + "}";

            //blockContent.Rule
            //    = simpleDeclarations + statements
            //    | simpleDeclarations
            //    | statements;

            //statements.Rule = MakePlusRule(statements, statement);

            //statement.Rule
            //    = semiStatement + ";"
            //    | "while" + parenExpression + statement
            //    | "for" + forHeader + statement
            //    | "if" + parenExpression + statement
            //    | "if" + parenExpression + statement + PreferShiftHere() + "else" + statement;

            //parenExpression.Rule = Symbol("(") + expression + ")";

            //forHeader.Rule = "(" + forBlock + ")";

            //forBlock.Rule = assignExpression + ";" + expression + ";" + assignExpression;

            //semiStatement.Rule
            //    = assignExpression
            //    | "return" + expression
            //    | "break"
            //    | "continue";

            //arguments.Rule
            //    = expression + "," + arguments
            //    | expression;

            //parenArguments.Rule
            //    = Symbol("(") + ")"
            //    | "(" + arguments + ")";

            //assignExpression.Rule
            //    = identifier + "=" + expression
            //    | expression;

            //expression.Rule
            //    = relationalExpression + booleanOperator + expression
            //    | relationalExpression;

            //booleanOperator.Rule
            //    = Symbol("&&")
            //    | "||";

            //relationalExpression.Rule
            //    = bitExpression + relationalOperator + bitExpression
            //    | bitExpression;

            //relationalOperator.Rule
            //    = Symbol(">")
            //    | ">="
            //    | "<"
            //    | "<="
            //    | "=="
            //    | "!=";

            //bitExpression.Rule
            //    = addExpression + bitOperator + bitExpression
            //    | addExpression;

            //bitOperator.Rule
            //    = Symbol("|")
            //    | "&"
            //    | "^";

            //addExpression.Rule
            //    = multiplyExpression + addOperator + addExpression
            //    | prefixExpression;

            //addOperator.Rule
            //    = Symbol("+") | "-";

            //multiplyExpression.Rule
            //    = prefixExpression + multiplyOperator + multiplyExpression
            //    | prefixExpression;

            //multiplyOperator.Rule
            //    = Symbol("*")
            //    | "/";

            //prefixExpression.Rule
            //    = prefixOperator + factor
            //    | factor;

            //prefixOperator.Rule = Symbol("!");

            //factor.Rule
            //    = identifierExpression + parenArguments
            //    | identifierExpression
            //    | number
            //    | parenExpression;

            //identifierExpression.Rule
            //    = identifier
            //    | identifierExpression + "." + identifier;
            #endregion

            #region Define Keywords and Register Symbols
            this.AddKeywords("begin", "end", "proc", "res", "val", "is", "skip",
                "write", "read", "if", "then", "else", "fi", "var", "while", "do", "od", "call",
                "or", "and", "xor", "true", "false");

            //this.RegisterBracePair("{", "}");
            this.RegisterBracePair("(", ")");

            this.RegisterOperators(1, "or");
            this.RegisterOperators(2, "and");
            this.RegisterOperators(3, "xor");
            this.RegisterOperators(4, "<", ">", "<=", ">=", "==", "!=");
            this.RegisterOperators(5, "|");
            this.RegisterOperators(6, "^");
            this.RegisterOperators(7, "&");
            this.RegisterOperators(8, "<<", ">>");
            this.RegisterOperators(9, "+", "-");
            this.RegisterOperators(10, "*", "/", "%");
            #endregion
        }
    }
}
