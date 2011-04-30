=A While language compiler for the .NET platform=

  * [#What_is_the_While_language? What is the While language?]
  * [#Why_a_compiler_for_a_toy_language? Why a compiler for a toy language?]
  * [#License License]
  * [#Command_line_options Command line options]
  * [#Course_Syntax_(the_/coursesyntax_switch) Course Syntax (the /coursesyntax_switch)]
  * [#Debugging_the_While_language Debugging the While language]
  * [#Plugin_Architecture Plugin Architecture]
  * [#While_Abstract_Syntax While Abstract Syntax]
    * [#Syntactic_categories Syntactic categories]
    * [#Abstract_syntax Abstract syntax]

==What is the While language?==

The While programming language is introduced in the book [http://www.amazon.com/Principles-Program-Analysis-Flemming-Nielson/dp/3540654100/ref=pd_bbs_sr_1?ie=UTF8&s=books&qid=1235741633&sr=8-1 Principles of Program Analysis] by Nielson, Nielson and Hankin. It is a simple imperative language with integer variables and integer and boolean expressions. For branching it has an if-else statement and for looping it has a while statement. Other statements it has is an assignment to assign integer expressions to variables, read and write statements to read input from user and print to screen, and a skip statement that has no effect. The language also contains procedures that can take 0-n parameters by value and 0 or 1 result parameter. All parameters are integers.


==Why a compiler for a toy language?==

My masters project at DTU involves writing compilers for the .NET platform and since I had never written a compiler before I decided to start with this simple language. This language is also used in the Program Analysis course that is taught at DTU by the authors of the book, and I thought it might be useful for students in the course to be able to try compiling their programs on a real platform, and see what difference their optimizations make in the final compiled executable. Another benefit is that the compiled programs can be debugged using a free .NET graphical debugger. As for the name, compilers are often named with the rule "abbreviation of language name + c" so for C# we have csc.exe, for Visual Basic we have vbc.exe. Using that rule I came up with the (rather unfortunate) name wc.exe.

[http://while-language.googlecode.com/files/wc-1.0-bin.zip Download the compiler v1.0]<br/>
[http://while-language.googlecode.com/files/wc-1.0-src.zip Download the compiler sources v1.0]

==License==

The source is licensed under the GPL v2.0, feel free to do what you want with it. The current version is written C#, the first version was written in the programming language Boo, which is a relatively new language for the .NET platform, similar to a statically typed Python. That version is available as a branch in the source repository, but is no longer updated. All future versions will be in C# only.

==Command line options==

The compiler is invoked as "`wc.exe [options] filename`". The possible options are:
<pre>
/? or /help            Print a help message

/out:<filename>        Specify the name of the compiled executable

/debug                 Include debug information in compiled file

/coursesyntax          Use the modified syntax used in the Program Analysis course at DTU.

/plugins:<p1>`[,p2...]`  Load the given plugins during compilation.
</pre>

The input file can also be specified as "STDIN", in which case the compiler reads the source from the standard input stream. This can be useful for example if you have some kind of preprocessor that takes the source file, analyzes it and spits out an optimized version. Then you could run the whole thing as something like:

{{{analyzer.exe somefile.while | wc.exe /out:somefile.exe STDIN}}}

	

==Course Syntax (the `/coursesyntax` switch)==

By default the compiler compiles While exactly as it is presented in the [http://www.amazon.com/Principles-Program-Analysis-Flemming-Nielson/dp/3540654100/ref=pd_bbs_sr_1?ie=UTF8&s=books&qid=1235741633&sr=8-1 Principles of Program Analysis] book. It can compile the examples from the book without modifications. This means that variables are not declared explicitly, and that () are used to group together multiple statements in if-else and while blocks. However, when I took the Program Analysis course at DTU and implemented some static analysis for it, we were given a slighly modified specification of the While language. In this spec variables had to be declared before use, and could only be declared inside begin-end blocks, and instead of using () in if and while statements, they ended with fi and od respectively. When the compiler is run with the /coursesyntax switch it will use this syntax and give errors on undeclared variables and if without fi and while ... do without od.

==Debugging the While language==

To debug While applications you should first download the Microsoft .NET Framework SDK v2.0. If you have Visual Studio 2005 or 2008 you might already have it installed. When it has installed, you can search for the file dbgclr.exe that should be somewhere under a GuiDebug folder. Start the debugger, go to the Debug menu, and select Program to Debug. Then locate your compiled .exe file and press OK. Press F10 to start debugging. If your source file is in the same folder as your executable it will load automatically, otherwise the debugger will prompt you for its location. Note that for this to work you must have compiled your program with the /debug switch.

==Plugin Architecture==
The compiler supports plugins to alter the syntax tree before compilation. Plugin writers simply create classes that implement the While.ICompilerPlugin interface, put their .dll's in the plugins subfolder under the folder where the compiler is, and can then run their plugins with the --plugins switch of the compiler. Loaded plugins get passed a copy of the abstract syntax tree and are free to modify it any way that they see fit. Plugins can even add their own nodes to it, since each node in the tree knows how to compile itself. There is also a handy Visitor class that plugins can inherit from, where they can override methods for the types of nodes they want to process, so they do not have to write the tree traversing code themselves. A simple plugin to fold constant expressions comes with the compiler and can be seen at [http://code.google.com/p/while-language/source/browse/trunk/plugins/FoldConstantExpressions.cs http://code.google.com/p/while-language/source/browse/trunk/plugins/FoldConstantExpressions.cs].

==While Abstract Syntax==

Below is a description of the While syntax, mostly taken from the [http://www.amazon.com/Principles-Program-Analysis-Flemming-Nielson/dp/3540654100/ref=pd_bbs_sr_1?ie=UTF8&s=books&qid=1235741633&sr=8-1 book], with the addition that concrete operators are shown and bitwise operations have been added.

<br />
===Syntactic categories===

<table>
  <tr>
    <td>_a_</td>
    <td>=</td>
    <td>Arithmetic expressions</td>
  </tr>
  <tr>
    <td>_b_</td>
    <td>=</td>
    <td>Boolean expressions</td>
  </tr>
  <tr>
    <td>_x,y_</td>
    <td>=</td>
    <td>Variables</td>
  </tr>
  <tr>
    <td>_n_</td>
    <td>=</td>
    <td>Numerals</td>
  </tr>
  <tr>
    <td>_p_</td>
    <td>=</td>
    <td>Procedures</td>
  </tr>
  <tr>
    <td>_S_</td>
    <td>=</td>
    <td>Statements</td>
  </tr>
  <tr>
    <td>_D_</td>
    <td>=</td>
    <td>Declarations</td>
  </tr>
  <tr>
    <td>_P_</td>
    <td>=</td>
    <td>Programs</td>
  </tr>
  <tr>
    <td>_op,,a,,_</td>
    <td>=</td>
    <td>Aerithmetic operators (+ - / * %)</td>
  </tr>
  <tr>
    <td>_op,,b,,_</td>
    <td>=</td>
    <td>Boolean operators (and or xor)</td>
  </tr>
  <tr>
    <td>_op,,r,,_</td>
    <td>=</td>
    <td>Relational operators (== != `<` `>` `<=` `>=`)</td>
  </tr>
  <tr>
    <td>_op,,bt,,_</td>
    <td>=</td>
    <td>Bitwise operators (`<<` `>>` & | ^)</td>
  </tr>
</table>

<br />
===Abstract syntax===

<table>
  <tr>
    <td>_a_</td>
    <td>::=</td>
    <td>_x_ | _n_ | _a,,1,, op,,a,, a,,2,,_ | _a,,1,, op,,bt,, a,,2,,_</td>
  </tr>
  <tr>
    <td>_b_</td>
    <td>::=</td>
    <td>*true* | *false* | *not* _b_ | _b,,1,, op,,b,, ,,2,,_ | _a,,1,, op,,r,, a,,2,,_</td>
  </tr>
  <tr>
    <td>_S_</td>
    <td>::=</td>
    <td>_x_ := _a_ | *skip* | _S,,1,,_;_S,,2,,_ | *if* _b_ *then* _S,,1,,_ *else* _S,,2,,_ | *while* _b_ *do* _S_ |</td>
  </tr>
  <tr>
    <td></td>
    <td></td>
    <td>*write* _a_ | *write* _b_ | *read* _x_ | *call* _p_(_x, y_)</td>
  </tr>
  <tr>
    <td>_D_</td>
    <td>::=</td>
    <td>*proc* _p_(*val* _x_, *res* _y_) *is* _S_ *end*</td>
  </tr>
  <tr>
    <td>_P_</td>
    <td>::=</td>
    <td>_S_ | *begin* _D ; S_ *end*</td>
  </tr>
</table>

When using the `/coursesyntax` these changes are made to the abstract syntax:

<table>
  <tr>
    <td>_V_</td>
    <td>=</td>
    <td>Variable Declaration</td>
  </tr>
</table>
<table>
  <tr>
    <td>_V_</td>
    <td>::=</td>
    <td>*var* _x_ | _V_;_V_</td>
  </tr>
  <tr>
    <td>_S_</td>
    <td>::=</td>
    <td>... | *begin* _V_;_S_ *end* | *if* _b_ *then* _S,,1,,_ *else* _S,,2,,_ *fi* | *while* _b_ *do* _S_ *od*</td>
  </tr>
</table>