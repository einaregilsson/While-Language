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
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace While {
    public class PluginLoader {

        public void Load(List<string> pluginNames) {
            if (pluginNames.Count == 0) {
                return;
            }
            string path;
            path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase);
            path = Path.Combine(path, "plugins");
            path = Regex.Replace(path,@"^file\:(/|\\)+", "");
            if (!Directory.Exists(path)) {
                Console.Error.WriteLine("No plugins folder found, no plugins will be loaded!");
                return;
            }
            string[] files = Directory.GetFiles(path);
            foreach (string file in files) {
                if (file.ToLower().EndsWith(".dll")) {
                    LoadPluginTypes(pluginNames, file);
                }
            }

            //Check for missing plugins...
            foreach (string name in pluginNames) {
                bool found = false;
                foreach (ICompilerPlugin plugin in _loadedPlugins) {
                    if (plugin.Identifier == name) {
                        found = true;
                    }
                }
                if (!found) {
                    Console.Error.WriteLine("Failed to load plugin '{0}', it is not in any dll's in the plugins folder.", name);
                }
            }
        }

        private void LoadPluginTypes(List<string> pluginNames, string assemblyName) {

            try {
                Assembly pluginAssembly = Assembly.LoadFile(assemblyName);
                foreach (Type t in pluginAssembly.GetTypes()) {
                    if (typeof(ICompilerPlugin).IsAssignableFrom(t)) {
                        ConstructorInfo constructor = t.GetConstructor(new Type[] { });
                        if (constructor == null) {
                            Console.Error.WriteLine("Cannot load plugin type '{0}' because it doesn't have a parameterless constructor", t.Name);
                            continue;
                        }
                        ICompilerPlugin plugin = (ICompilerPlugin)Activator.CreateInstance(t);
                        if (pluginNames.Contains(plugin.Identifier)) {
                            Console.WriteLine("Loaded plugin '{0}'", plugin.Name);
                            _loadedPlugins.Add(plugin);
                        }
                    }
                }
            } catch (Exception ex) {
                Console.Error.WriteLine("Failed to load assembly '{0}', {1}", assemblyName, ex.Message);
            }
        }

        private List<ICompilerPlugin> _loadedPlugins = new List<ICompilerPlugin>();
        public List<ICompilerPlugin> LoadedPlugins {
            get { return _loadedPlugins; }
        }
    }
}
