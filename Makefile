#Build script for the While compiler, see http://while-language.googlecode.com
all: wc plugins

wc:
	@echo Compiling While compiler...
	@mkdir -p bin
	@rm -f -r bin/*
	gmcs /t:exe /out:bin/wc.exe /reference:lib/nunit.framework.dll /recurse:compiler/*.cs
	@echo Type \'mono bin/wc.exe\' to start the compiler

plugins: wc 
	@echo Compiling example plugin...
	@mkdir -p bin/plugins
	@rm -f bin/plugins/*
	gmcs /t:library /out:bin/plugins/ExamplePlugins.dll /reference:bin/wc.exe plugins/FoldConstantExpressions.cs
	@echo Compiled example plugin, run compiler with '--plugins=constexp' to use it.

clean:
	@echo Cleaning bin folder...
	@rm -r -f bin
