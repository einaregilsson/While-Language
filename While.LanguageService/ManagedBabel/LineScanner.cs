/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.IO;
using Microsoft.VisualStudio.Package;

namespace Babel
{
	/// <summary>
	/// LineScanner wraps the GPLEX scanner to provide the IScanner interface
	/// required by the Managed Package Framework. This includes mapping tokens
	/// to color definitions.
	/// </summary>
	public class LineScanner : IScanner
	{
		Babel.ParserGenerator.IColorScan lex = null;

		public LineScanner()
		{
			//this.lex = new Babel.Lexer.Scanner();
		}

        While.Parsing.Scanner s;
		public bool ScanTokenAndProvideInfoAboutIt(TokenInfo tokenInfo, ref int state)
		{
			int start, end;
            int token = 0;
			//int token = lex.GetNext(ref state, out start, out end);
            While.Parsing.Token t = s.Scan();
            
			// !EOL and !EOF
			if (t.kind != While.Parsing.Parser._EOF)
			{
				//Configuration.TokenDefinition definition = Configuration.GetDefinition(token);
				tokenInfo.StartIndex = t.pos;
				tokenInfo.EndIndex = t.pos+t.val.Length;
				tokenInfo.Color = TokenColor.Comment;
				tokenInfo.Type = TokenType.Keyword;
                tokenInfo.Trigger = TokenTriggers.None;

				return true;
			}
			else
			{
				return false;
			}
		}

		public void SetSource(string source, int offset)
		{
            MemoryStream ms = new MemoryStream();
            StreamWriter w = new StreamWriter(ms);
            w.Write(source.Substring(offset));
            ms.Seek(0, SeekOrigin.Begin);
            s = new While.Parsing.Scanner(ms);
			//lex.SetSource(source, offset);
		}
	}
}