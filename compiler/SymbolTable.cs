/*
 * While Compiler
 * http://while-language.googlecode.com
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
using While;
using System.Collections.Generic;

namespace While {

    /// <summary>
    ///Symbol table to keep track of variables and their scope.
    ///Used both in the parsing phase, and again in the compilation phase.
    /// </summary>
    public class SymbolTable {

        private List<Dictionary<string, int>> _stack = new List<Dictionary<string, int>>();
        private int _nr = 0;
        private int _resultArgIndex = -1;
        private List<string> _args = new List<string>();

        public void PushScope() {
            _stack.Add(new Dictionary<string, int>());
        }

        public void PopScope() {
            _stack.RemoveAt(_stack.Count - 1);
        }

        public void Clear() {
            _stack.Clear();
            _args.Clear();
            _nr = 0;
            _resultArgIndex = -1;
        }

        public void DefineVariable(string name) {
            if (_stack.Count == 0) {
                throw new WhileException("Stack is empty, cannot define variable {0}", name);
            }
            if (_stack[_stack.Count - 1].ContainsKey(name)) {
                throw new WhileException("Variable {0} is already defined in this scope!", name);
            }
            _stack[_stack.Count - 1].Add(name, _nr);
            _nr++;
        }

        public void DefineArgument(string name) {
            _args.Add(name);
        }

        public void DefineResultArgument(string name) {
            _args.Add(name);
            _resultArgIndex = _args.Count - 1;
        }

        public bool IsArgument(string name) {
            return FindScopeForVariable(name) == null && _args.Contains(name);
        }

        public bool IsResultArgument(string name) {
            return IsArgument(name) && _args.IndexOf(name) == _resultArgIndex;
        }

        /// <summary>
        /// Returns the number for the variable (since the CLR only understands numbers, not names) 
        /// </summary>
        /// <param name="name">The variable name</param>
        /// <returns></returns>
        public int GetValue(string name) {

            Dictionary<string, int> scope = FindScopeForVariable(name);
            if (scope == null) {
                int nr = _args.IndexOf(name);
                if (nr == -1) {
                    throw new WhileException("Variable {0} is not in scope!",name);
                }
                return nr;
            }
            return scope[name];
        }

        public bool IsInScope(string name) {
            Dictionary<string, int> scope = FindScopeForVariable(name);
            if (scope == null) {
                return _args.Contains(name);
            }
            return true;
        }

        public bool IsDeclaredInCurrentScope(string name) {
            return _stack.Count > 0 && _stack[_stack.Count - 1].ContainsKey(name) || _args.Contains(name);
        }

        public Dictionary<string, int> FindScopeForVariable(string name) {
            for (int i = _stack.Count - 1; i >= 0; i--) {
                if (_stack[i].ContainsKey(name)) {
                    return _stack[i];
                }
            }
            return null;
        }
    }
}


