@echo off
REM Build script for the While compiler, see http://while-language.googlecode.com
set depends=call make
set all=compiler, plugins
set CC=%WINDIR%\Microsoft.NET\Framework\v2.0.50727\csc.exe 


if exist %CC% goto check
echo C# compiler was not found in its normal place, aborting...
goto end

:check
if "%1"=="" goto default

:start
for %%c in (%all%) do (
	if %1==%%c goto %1
)
echo Unknown target '%1'
echo Available targets are: %all%
goto end

:compiler
	echo Compiling While Compiler...
	%CC% /t:exe /out:bin\wc.exe /reference:bin\nunit.framework.dll /recurse:compiler\*.cs
	if %ERRORLEVEL%==0 (echo Compilation was successful, type 'bin\wc.exe' to start) else (echo Failed to compile While Compiler)
	goto nexttarget

:plugins
	%depends% compiler
	echo TODO: Create plugins
	goto nexttarget

:default
	%depends% compiler

:nexttarget
if "%2"=="" goto end
shift
goto start

:end


