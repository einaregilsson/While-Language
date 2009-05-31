/**
 * $Id: AuthoringScope.cs 168 2009-05-28 10:24:29Z eboeg $ 
 * 
 * This file is part of the Process Language Runtime (PLR) 
 * and is licensed under the GPL v3.0.
 * 
 * Author: Einar Egilsson (einar@einaregilsson.com) 
 */
using System;
using System.Collections.Generic;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Package;
using Babel;

namespace CCS.LanguageService {

    public class AuthoringScope : Microsoft.VisualStudio.Package.AuthoringScope {
        public AuthoringScope(string source) {
            // how should this be set?
            this.resolver = new Resolver(source);
        }

        Resolver resolver;

        // ParseReason.QuickInfo
        public override string GetDataTipText(int line, int col, out TextSpan span) {
            span = new TextSpan();
            return null;
        }

        // ParseReason.CompleteWord
        // ParseReason.DisplayMemberList
        // ParseReason.MemberSelect
        // ParseReason.MemberSelectAndHilightBraces
        public override Microsoft.VisualStudio.Package.Declarations GetDeclarations(IVsTextView view, int line, int col, TokenInfo info, ParseReason reason) {
            IList<Declaration> declarations;
            switch (reason) {
                case ParseReason.CompleteWord:
                    declarations = resolver.FindCompletions(null, line, col);
                    break;
                case ParseReason.DisplayMemberList:
                case ParseReason.MemberSelect:
                case ParseReason.MemberSelectAndHighlightBraces:
                    declarations = resolver.FindMembers(null, line, col);
                    break;
                default:
                    throw new ArgumentException("reason");
            }

            return new Babel.Declarations(declarations);
        }

        // ParseReason.GetMethods
        public override Microsoft.VisualStudio.Package.Methods GetMethods(int line, int col, string name) {
            return null;// return new Babel.Methods(resolver.FindMethods(null, line, col, name));
        }

        // ParseReason.Goto
        public override string Goto(VSConstants.VSStd97CmdID cmd, IVsTextView textView, int line, int col, out TextSpan span) {
            // throw new System.NotImplementedException();
            span = new TextSpan();
            return null;
        }
    }
}