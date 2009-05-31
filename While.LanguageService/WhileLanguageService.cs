/**
 * $Id: CCSLanguage.cs 171 2009-05-29 11:43:19Z eboeg $ 
 * 
 * This file is part of the Process Language Runtime (PLR) 
 * and is licensed under the GPL v3.0.
 * 
 * Author: Einar Egilsson (einar@einaregilsson.com) 
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Package;
using System.Text.RegularExpressions;
using While.Parsing;
namespace CCS.LanguageService {
    [Guid("2AB66092-EABE-48ad-8895-9A02699AA083")]
    class WhileLanguageService : Babel.BabelLanguageService {
        public override string GetFormatFilterList() {
            return "While File (*.w)\n*.w";
        }

        public override Microsoft.VisualStudio.Package.AuthoringScope ParseSource(ParseRequest req) {
            Babel.Source source = (Babel.Source)this.GetSource(req.FileName);
            bool yyparseResult = false;

            // req.DirtySpan seems to be set even though no changes have occurred
            // source.IsDirty also behaves strangely
            // might be possible to use source.ChangeCount to sync instead

            if (req.DirtySpan.iStartIndex != req.DirtySpan.iEndIndex
                || req.DirtySpan.iStartLine != req.DirtySpan.iEndLine) {
//                ErrorHandler handler = new ErrorHandler();
                //Scanner scanner = new Scanner(); // string interface
                //Parser parser = new Parser();  // use noarg constructor
                //parser.scanner = scanner;
                //scanner.Handler = handler;
                //parser.SetHandler(handler);
                //scanner.SetSource(req.Text, 0);

                //parser.MBWInit(req);
                //yyparseResult = parser.Parse();
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                StreamWriter writer = new StreamWriter(ms);
                writer.Write(req.Text);
                ms.Seek(0, SeekOrigin.Begin);
                While.Parsing.Scanner scanner2 = new While.Parsing.Scanner(ms);
                While.Parsing.Parser p = new While.Parsing.Parser(scanner2, new While.CommandLineOptions(new string[]{}));
                p.Parse();

                // store the parse results
                // source.ParseResult = aast;
                source.ParseResult = null;
                //source.Braces = parser.Braces;

                foreach (While.Parsing.Error e in p.errors.errors) {
                    TextSpan span = new TextSpan();
                    span.iStartLine = span.iEndLine = e.Line;
                    span.iStartIndex = e.Column; ;
                    span.iEndIndex = e.Column + 4;
                    req.Sink.AddError(req.FileName, e.Message, span, Severity.Error);
                    
                }
                // for the time being, just pull errors back from the error handler
                //if (handler.ErrNum > 0) {
                //    foreach (Babel.Parser.Error error in handler.SortedErrorList()) {
                //        TextSpan span = new TextSpan();
                //        span.iStartLine = span.iEndLine = error.line - 1;
                //        span.iStartIndex = error.column;
                //        span.iEndIndex = error.column + error.length;
                //        req.Sink.AddError(req.FileName, error.message, span, Severity.Error);
                //    }
                //}
            }
            switch (req.Reason) {
                case ParseReason.Check:
                case ParseReason.HighlightBraces:
                case ParseReason.MatchBraces:
                case ParseReason.MemberSelectAndHighlightBraces:
                    // send matches to sink
                    // this should (probably?) be filtered on req.Line / col
                    if (source.Braces != null) {
                        foreach (TextSpan[] brace in source.Braces) {
                            if (brace.Length == 2)
                                req.Sink.MatchPair(brace[0], brace[1], 1);
                            else if (brace.Length >= 3)
                                req.Sink.MatchTriple(brace[0], brace[1], brace[2], 1);
                        }
                    }
                    break;
                default:
                    break;
            }

            return new AuthoringScope(req.Text);
        }

        private class TokenInfo {
            public int start, end, type;
            public string text;
            public TokenInfo(int start, int end, int type, string text) { this.start = start; this.end = end; this.type = type; this.text = text; }
        }
        public override int ValidateBreakpointLocation(IVsTextBuffer buffer, int line, int col, TextSpan[] pCodeSpan) {

            if (pCodeSpan != null) {
                pCodeSpan[0].iStartLine = line;
                pCodeSpan[0].iEndLine = line;
                //List<TokenInfo> tokens = new List<TokenInfo>();
                //if (buffer != null) {
                //    int length;
                //    buffer.GetLengthOfLine(line, out length);
                //    IVsTextLines lines = (IVsTextLines)buffer;
                //    string text;
                //    lines.GetLineText(line, 0, line, length, out text);

                //    //Scanner scanner = new Scanner();
                //    scanner.SetSource(text, 0);
                //    int state = 0, start, end;
                //    int tokenType = -1;
                //    while (tokenType != ((int)Tokens.EOF)) {
                //        tokenType = scanner.GetNext(ref state, out start, out end);
                //        tokens.Add(new TokenInfo(start, end, tokenType, scanner.yytext));
                //    }

                //    for (int i = 0; i < tokens.Count; i++) {
                //        TokenInfo t = tokens[i];
                //        if (t.type == (int)Tokens.OUTACTION
                //            || t.type == (int)Tokens.METHOD
                //            || t.type == (int)Tokens.LCASEIDENT
                //            || (t.type == (int)Tokens.NUMBER && t.text == "0")) {
                //            pCodeSpan[0].iStartIndex = t.start;
                //            pCodeSpan[0].iEndIndex = t.end + 1;
                //            return Microsoft.VisualStudio.VSConstants.S_OK;
                //        } else if (t.type == (int)Tokens.PROC) {
                //            //Just a heuristic, if there is a '=' somewhere after this, then we don't want it
                //            //Most likely it is Foo = proc or Foo(x,y,z) = proc and we don't want to highlight that
                //            bool canUseProc = true;
                //            for (int j = i + 1; j < tokens.Count; j++) {
                //                if (tokens[j].type == (int)'=') {
                //                    i = j;
                //                    canUseProc = false;
                //                    break;
                //                }
                //            }
                //            if (canUseProc) {
                //                pCodeSpan[0].iStartIndex = t.start;
                //                pCodeSpan[0].iEndIndex = t.end + 1;
                //                return Microsoft.VisualStudio.VSConstants.S_OK;
                //            }
                //        }

     //               }
       //         }
            }
            return Microsoft.VisualStudio.VSConstants.S_FALSE;
        }
    }
}
