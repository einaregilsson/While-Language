#Build script for the While compiler, see http://while-language.googlecode.com
wc:
	@echo Compiling While compiler
	gmcs /t:exe /out:bin/wc.exe /reference:bin/nunit.framework.dll /recurse:compiler/*.cs
	@echo Type \'mono bin/wc.exe\' to start the compiler

plugins: wc 
	@echo TODO: Create plugins
