using System;
using Microsoft.VisualStudio.Package;
using Irony.Compiler;

namespace Demo
{
    public class LineScanner : IScanner
    {
        private LanguageCompiler compiler;
        private CompilerContext compilerContext;

        public LineScanner(Grammar grammar)
        {
            this.compiler = new LanguageCompiler(grammar);
            this.compilerContext = new CompilerContext(this.compiler);
            this.compilerContext.Mode = CompileMode.VsLineScan;
            this.compiler.Scanner.Prepare(this.compilerContext, null);
        }

        public bool ScanTokenAndProvideInfoAboutIt(TokenInfo tokenInfo, ref int state)
        {
            // Reads each token in a source line and performs syntax coloring.  It will continue to
            // be called for the source until false is returned.
            Token token = compiler.Scanner.VsReadToken(ref state);

            // !EOL and !EOF
            if (token != null && token.Terminal != Grammar.Eof && token.Category != TokenCategory.Error)
            {
                tokenInfo.StartIndex = token.Location.Position;
                tokenInfo.EndIndex = tokenInfo.StartIndex + token.Length - 1;
                tokenInfo.Color = (Microsoft.VisualStudio.Package.TokenColor)token.EditorInfo.Color;
                tokenInfo.Type = (Microsoft.VisualStudio.Package.TokenType)token.EditorInfo.Type;

                if (token.Symbol != null)
                {
                    tokenInfo.Trigger =
                        (Microsoft.VisualStudio.Package.TokenTriggers)token.Symbol.EditorInfo.Triggers;
                }
                else
                {
                    tokenInfo.Trigger =
                        (Microsoft.VisualStudio.Package.TokenTriggers)token.EditorInfo.Triggers;
                }

                return true;
            }

            return false;
        }

        public void SetSource(string source, int offset)
        {
            // Stores line of source to be used by ScanTokenAndProvideInfoAboutIt.
            compiler.Scanner.VsSetSource(source, offset);
        }
    }
}
