/**
 * $Id: Configuration.cs 168 2009-05-28 10:24:29Z eboeg $ 
 * 
 * This file is part of the Process Language Runtime (PLR) 
 * and is licensed under the GPL v3.0.
 * 
 * Author: Einar Egilsson (einar@einaregilsson.com) 
 */
using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Babel //Must be since this is partial against the ManagedBabel source
{
	public static partial class Configuration
	{
        public const string Name = "While";
        public const string Extension = ".w";

        static CommentInfo ccsCommentInfo;
        public static CommentInfo MyCommentInfo { get { return ccsCommentInfo; ; } }

        static Configuration()
        {
            ccsCommentInfo.BlockEnd = "\n";
            ccsCommentInfo.BlockStart = "#";
            ccsCommentInfo.LineStart = "??";
            ccsCommentInfo.UseLineComments = true;

            // default colors - currently, these need to be declared
            CreateColor("Keyword", COLORINDEX.CI_BLUE, COLORINDEX.CI_USERTEXT_BK);
            CreateColor("Comment", COLORINDEX.CI_DARKGREEN, COLORINDEX.CI_USERTEXT_BK);
            CreateColor("Identifier", COLORINDEX.CI_SYSPLAINTEXT_FG, COLORINDEX.CI_USERTEXT_BK);
            CreateColor("String", COLORINDEX.CI_RED, COLORINDEX.CI_USERTEXT_BK);
            CreateColor("Number", COLORINDEX.CI_SYSPLAINTEXT_FG, COLORINDEX.CI_USERTEXT_BK);
            CreateColor("Text", COLORINDEX.CI_SYSPLAINTEXT_FG, COLORINDEX.CI_USERTEXT_BK);

            TokenColor error = CreateColor("Error", COLORINDEX.CI_RED, COLORINDEX.CI_USERTEXT_BK, false, false);
            TokenColor proc = CreateColor("Proc", COLORINDEX.CI_AQUAMARINE, COLORINDEX.CI_USERTEXT_BK, true, false);
            TokenColor outaction = CreateColor("OutAction", COLORINDEX.CI_DARKGRAY, COLORINDEX.CI_USERTEXT_BK, true, false);
            TokenColor inaction = CreateColor("InAction", COLORINDEX.CI_BLACK, COLORINDEX.CI_USERTEXT_BK);
            TokenColor method = CreateColor("Method", COLORINDEX.CI_MAGENTA, COLORINDEX.CI_USERTEXT_BK);
            TokenColor fullclass = CreateColor("FullClass", COLORINDEX.CI_MAROON, COLORINDEX.CI_USERTEXT_BK);
            TokenColor stringColor = CreateColor("String", COLORINDEX.CI_MAROON, COLORINDEX.CI_USERTEXT_BK);
            TokenColor fatParens = CreateColor("FatParens", COLORINDEX.CI_BLACK, COLORINDEX.CI_USERTEXT_BK, true, false);
            //
            // map tokens to color classes
            //
            //ColorToken((int)Tokens.LCASEIDENT, TokenType.Keyword, inaction, TokenTriggers.None);
            //ColorToken((int)Tokens.OUTACTION, TokenType.Keyword, outaction, TokenTriggers.None);
            //ColorToken((int)Tokens.PROC, TokenType.Keyword, proc, TokenTriggers.None);
            //ColorToken((int)Tokens.NUMBER, TokenType.Literal, TokenColor.String, TokenTriggers.None);
            //ColorToken((int)Tokens.KWUSE, TokenType.Keyword, TokenColor.Keyword, TokenTriggers.None);
            //ColorToken((int)Tokens.KWIF, TokenType.Keyword, TokenColor.Keyword, TokenTriggers.None);
            //ColorToken((int)Tokens.KWELSE, TokenType.Keyword, TokenColor.Keyword, TokenTriggers.None);
            //ColorToken((int)Tokens.KWTHEN, TokenType.Keyword, TokenColor.Keyword, TokenTriggers.None);
            //ColorToken((int)Tokens.KWAND, TokenType.Keyword, TokenColor.Keyword, TokenTriggers.None);
            //ColorToken((int)Tokens.KWOR, TokenType.Keyword, TokenColor.Keyword, TokenTriggers.None);
            //ColorToken((int)Tokens.KWXOR, TokenType.Keyword, TokenColor.Keyword, TokenTriggers.None);
            //ColorToken((int)Tokens.KWTRUE, TokenType.Keyword, TokenColor.Keyword, TokenTriggers.None);
            //ColorToken((int)Tokens.KWFALSE, TokenType.Keyword, TokenColor.Keyword, TokenTriggers.None);
            //ColorToken((int)Tokens.METHOD, TokenType.Keyword, method, TokenTriggers.None);
            //ColorToken((int)Tokens.FULLCLASS, TokenType.Keyword, fullclass, TokenTriggers.None);
            //ColorToken((int)Tokens.STRING, TokenType.Keyword, stringColor, TokenTriggers.None);

            //ColorToken((int)'(', TokenType.Delimiter, TokenColor.Text, TokenTriggers.MatchBraces);
            //ColorToken((int)')', TokenType.Delimiter, TokenColor.Text, TokenTriggers.MatchBraces);
            //ColorToken((int)'{', TokenType.Delimiter, fatParens, TokenTriggers.MatchBraces);
            //ColorToken((int)'}', TokenType.Delimiter, fatParens, TokenTriggers.MatchBraces);
            //ColorToken((int)'[', TokenType.Delimiter, fatParens, TokenTriggers.MatchBraces);
            //ColorToken((int)']', TokenType.Delimiter, fatParens, TokenTriggers.MatchBraces);

            ////// Extra token values internal to the scanner
            //ColorToken((int)Tokens.LEX_ERROR, TokenType.Text, error, TokenTriggers.None);
            //ColorToken((int)Tokens.LEX_COMMENT, TokenType.Text, TokenColor.Comment, TokenTriggers.None);
        }
    }
}