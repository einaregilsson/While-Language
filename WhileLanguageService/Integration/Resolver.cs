using System;
using System.Collections.Generic;
using System.Text;
using Irony.Compiler;

namespace Demo
{
    public class Resolver : Demo.IASTResolver
    {
        #region IASTResolver Members


        public IList<Demo.Declaration> FindCompletions(object result, int line, int col)
        {
            // Used for intellisense.
            List<Demo.Declaration> declarations = new List<Demo.Declaration>();

            // Add keywords defined by grammar
            foreach (string keyword in Configuration.Grammar.Keywords)
            {
                declarations.Add(new Declaration("", keyword, 206, keyword));
            }

            declarations.Sort();
            return declarations;
        }

        public IList<Demo.Declaration> FindMembers(object result, int line, int col)
        {
            List<Demo.Declaration> members = new List<Demo.Declaration>();

            return members;
        }

        public string FindQuickInfo(object result, int line, int col)
        {
            return "unknown";
        }

        public IList<Demo.Method> FindMethods(object result, int line, int col, string name)
        {
            return new List<Demo.Method>();
        }

        #endregion
    }
}
