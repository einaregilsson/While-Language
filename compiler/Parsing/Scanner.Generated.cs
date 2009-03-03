
using System;
using System.IO;
using System.Collections;

namespace While.Parsing {

//-----------------------------------------------------------------------------------
// Scanner
//-----------------------------------------------------------------------------------
public partial class Scanner {

	const int maxT = 49;
	const int noSym = 49;


	static Scanner() {
		start = new Hashtable(128);
		for (int i = 65; i <= 90; ++i) start[i] = 1;
		for (int i = 95; i <= 95; ++i) start[i] = 1;
		for (int i = 97; i <= 122; ++i) start[i] = 1;
		for (int i = 48; i <= 57; ++i) start[i] = 2;
		start[40] = 3; 
		start[41] = 4; 
		start[59] = 5; 
		start[44] = 6; 
		start[58] = 7; 
		start[60] = 26; 
		start[62] = 27; 
		start[61] = 11; 
		start[33] = 13; 
		start[124] = 15; 
		start[94] = 16; 
		start[38] = 17; 
		start[43] = 20; 
		start[45] = 21; 
		start[42] = 22; 
		start[47] = 23; 
		start[37] = 24; 
		start[126] = 25; 
		start[Buffer.EOF] = -1;

	}
	
	void NextCh() {
		if (oldEols > 0) { ch = EOL; oldEols--; } 
		else {
			pos = buffer.Pos;
			ch = buffer.Read(); col++;
			// replace isolated '\r' by '\n' in order to make
			// eol handling uniform across Windows, Unix and Mac
			if (ch == '\r' && buffer.Peek() != '\n') ch = EOL;
			if (ch == EOL) { line++; col = 0; }
		}

	}

	void AddCh() {
		if (tlen >= tval.Length) {
			char[] newBuf = new char[2 * tval.Length];
			Array.Copy(tval, 0, newBuf, 0, tval.Length);
			tval = newBuf;
		}
		if (ch != Buffer.EOF) {
			tval[tlen++] = (char) ch;
			NextCh();
		}
	}



	bool Comment0() {
		int level = 1, line0 = line;
		NextCh();
			for(;;) {
				if (ch == 10) {
					level--;
					if (level == 0) { oldEols = line - line0; NextCh(); return true; }
					NextCh();
				} else if (ch == Buffer.EOF) return false;
				else NextCh();
			}
	}

	bool Comment1() {
		int level = 1, pos0 = pos, line0 = line, col0 = col;
		NextCh();
		if (ch == '/') {
			NextCh();
			for(;;) {
				if (ch == 10) {
					level--;
					if (level == 0) { oldEols = line - line0; NextCh(); return true; }
					NextCh();
				} else if (ch == Buffer.EOF) return false;
				else NextCh();
			}
		} else {
			buffer.Pos = pos0; NextCh(); line = line0; col = col0;
		}
		return false;
	}

	bool Comment2() {
		int level = 1, pos0 = pos, line0 = line, col0 = col;
		NextCh();
		if (ch == '*') {
			NextCh();
			for(;;) {
				if (ch == '*') {
					NextCh();
					if (ch == '/') {
						level--;
						if (level == 0) { oldEols = line - line0; NextCh(); return true; }
						NextCh();
					}
				} else if (ch == '/') {
					NextCh();
					if (ch == '*') {
						level++; NextCh();
					}
				} else if (ch == Buffer.EOF) return false;
				else NextCh();
			}
		} else {
			buffer.Pos = pos0; NextCh(); line = line0; col = col0;
		}
		return false;
	}


	void CheckLiteral() {
		switch (t.val) {
			case "begin": t.kind = 3; break;
			case "end": t.kind = 4; break;
			case "proc": t.kind = 5; break;
			case "val": t.kind = 7; break;
			case "res": t.kind = 8; break;
			case "is": t.kind = 10; break;
			case "skip": t.kind = 13; break;
			case "write": t.kind = 14; break;
			case "read": t.kind = 15; break;
			case "var": t.kind = 16; break;
			case "if": t.kind = 18; break;
			case "then": t.kind = 19; break;
			case "else": t.kind = 20; break;
			case "fi": t.kind = 21; break;
			case "while": t.kind = 22; break;
			case "do": t.kind = 23; break;
			case "od": t.kind = 24; break;
			case "call": t.kind = 25; break;
			case "or": t.kind = 26; break;
			case "and": t.kind = 27; break;
			case "xor": t.kind = 28; break;
			case "not": t.kind = 46; break;
			case "true": t.kind = 47; break;
			case "false": t.kind = 48; break;
			default: break;
		}
	}

	Token NextToken() {
		while (ch == ' ' ||
			ch >= 9 && ch <= 10 || ch == 13
		) NextCh();
		if (ch == '#' && Comment0() ||ch == '/' && Comment1() ||ch == '/' && Comment2()) return NextToken();
		t = new Token();
		t.pos = pos; t.col = col; t.line = line; 
		int state;
		if (start.ContainsKey(ch)) { state = (int) start[ch]; }
		else { state = 0; }
		tlen = 0; AddCh();
		
		switch (state) {
			case -1: { t.kind = eofSym; break; } // NextCh already done
			case 0: { t.kind = noSym; break; }   // NextCh already done
			case 1:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 1;}
				else {t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}
			case 2:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 2;}
				else {t.kind = 2; break;}
			case 3:
				{t.kind = 6; break;}
			case 4:
				{t.kind = 9; break;}
			case 5:
				{t.kind = 11; break;}
			case 6:
				{t.kind = 12; break;}
			case 7:
				if (ch == '=') {AddCh(); goto case 8;}
				else {t.kind = noSym; break;}
			case 8:
				{t.kind = 17; break;}
			case 9:
				{t.kind = 31; break;}
			case 10:
				{t.kind = 32; break;}
			case 11:
				if (ch == '=') {AddCh(); goto case 12;}
				else {t.kind = noSym; break;}
			case 12:
				{t.kind = 33; break;}
			case 13:
				if (ch == '=') {AddCh(); goto case 14;}
				else {t.kind = noSym; break;}
			case 14:
				{t.kind = 34; break;}
			case 15:
				{t.kind = 35; break;}
			case 16:
				{t.kind = 36; break;}
			case 17:
				{t.kind = 37; break;}
			case 18:
				{t.kind = 38; break;}
			case 19:
				{t.kind = 39; break;}
			case 20:
				{t.kind = 40; break;}
			case 21:
				{t.kind = 41; break;}
			case 22:
				{t.kind = 42; break;}
			case 23:
				{t.kind = 43; break;}
			case 24:
				{t.kind = 44; break;}
			case 25:
				{t.kind = 45; break;}
			case 26:
				if (ch == '=') {AddCh(); goto case 9;}
				else if (ch == '<') {AddCh(); goto case 18;}
				else {t.kind = 29; break;}
			case 27:
				if (ch == '=') {AddCh(); goto case 10;}
				else if (ch == '>') {AddCh(); goto case 19;}
				else {t.kind = 30; break;}

		}
		t.val = new String(tval, 0, tlen);
		return t;
	}
} // end Scanner

}
