@REM Build script for the While compiler, see http://while-language.googlecode.com
@set CC=%WINDIR%\Microsoft.NET\Framework\v2.0.50727\csc.exe 

@if "%1"=="clean" goto clean

@if exist %CC% goto start
@echo C# compiler was not found in its normal place, aborting...
@goto end

:start
@echo Compiling While Compiler...
@if not exist bin mkdir bin
@del /Q bin\*
%CC% /t:exe /nologo /out:bin\wc.exe /reference:lib\nunit.framework.dll /recurse:compiler\*.cs
echo.
@if %ERRORLEVEL%==0 (
	echo Compilation was successful, type 'bin\wc.exe' to start
) else (
	echo Failed to compile While Compiler
)

@echo. 
@echo Compiling example plugin...
@if not exist bin\plugins mkdir bin\plugins
%CC% /t:library /nologo /out:bin\plugins\ExamplePlugins.dll /reference:bin\wc.exe plugins\FoldConstantExpressions.cs
@echo.
@if %ERRORLEVEL%==0 (
	echo Compiled example plugin, run compiler with '/plugins:constexp' to use it.
) else (
	echo Failed to compile plugins
)
@goto end

:clean
@echo Cleaning bin folder...
@if exist bin rmdir /S /Q bin
:end
@echo.

