@echo off
REM Build script for the While compiler, see http://while-language.googlecode.com
set CC=%WINDIR%\Microsoft.NET\Framework\v2.0.50727\csc.exe 

if "%1"=="clean" goto clean

if exist %CC% goto start
echo C# compiler was not found in its normal place, aborting...
goto end

:start
echo Compiling While Compiler...
if not exist bin mkdir bin
del /Q bin\*
%CC% /t:exe /out:bin\wc.exe /reference:lib\nunit.framework.dll /recurse:compiler\*.cs
if %ERRORLEVEL%==0 (echo Compilation was successful, type 'bin\wc.exe' to start) else (echo Failed to compile While Compiler)

echo Compiling example plugin...
goto end

:clean
echo Cleaning bin folder...
if exist bin rmdir /S /Q bin
:end


