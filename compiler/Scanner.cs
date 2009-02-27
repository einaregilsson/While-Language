/*
 * While Compiler
 * http://code.google.com/p/while-language/
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
 * $HeadURL: https://while-language.googlecode.com/svn/branches/Boo/Scanner.boo $
 * $LastChangedDate: 2009-02-25 15:21:32 +0100 (mið., 25 feb. 2009) $
 * $Author: einar@einaregilsson.com $
 * $Revision: 2 $
 */
namespace While


import System
import System.IO
import System.Collections.Generic

public class Token:

	public kind as int

	// token kind
	public pos as int

	// token position in the source text (starting at 0)
	public col as int

	// token column (starting at 1)
	public line as int

	// token line (starting at 1)
	public val as string

	// token value
	public next as Token
	// ML 2005-03-11 Tokens are kept in linked list


//-----------------------------------------------------------------------------------
// Buffer
//-----------------------------------------------------------------------------------
public class Buffer:

	// This Buffer supports the following cases:
	// 1) seekable stream (file)
	//    a) whole stream in buffer
	//    b) part of stream in buffer
	// 2) non seekable stream (network, console)
	
	public static final EOF as int = (cast(int, char.MaxValue) + 1)

	private static final MIN_BUFFER_LENGTH = 1024

	// 1KB
	private static final MAX_BUFFER_LENGTH as int = (MIN_BUFFER_LENGTH * 64)

	// 64KB
	private buf as (byte)

	// input buffer
	private bufStart as int

	// position of first byte in buffer relative to input stream
	private bufLen as int

	// length of buffer
	private fileLen as int

	// length of input stream (may change if the stream is no file)
	private bufPos as int

	// current position in buffer
	private stream as Stream

	// input stream (seekable)
	private isUserStream as bool

	// was the stream opened by the user?
	public def constructor(s as Stream, isUserStream as bool):
		stream = s
		self.isUserStream = isUserStream
		
		if stream.CanSeek:
			fileLen = cast(int, stream.Length)
			bufLen = Math.Min(fileLen, MAX_BUFFER_LENGTH)
			bufStart = Int32.MaxValue
		else:
			// nothing in the buffer so far
			fileLen = (bufLen = (bufStart = 0))
		
		buf = array(byte, (bufLen if (bufLen > 0) else MIN_BUFFER_LENGTH))
		if fileLen > 0:
			Pos = 0
		else:
			// setup buffer to position 0 (start)
			bufPos = 0
		// index 0 is already after the file, thus Pos = 0 is invalid
		if (bufLen == fileLen) and stream.CanSeek:
			Close()

	
	protected def constructor(b as Buffer):
		// called in UTF8Buffer constructor
		buf = b.buf
		bufStart = b.bufStart
		bufLen = b.bufLen
		fileLen = b.fileLen
		bufPos = b.bufPos
		stream = b.stream
		// keep destructor from closing the stream
		b.stream = null
		isUserStream = b.isUserStream

	def destructor():
		
		Close()

	
	protected def Close():
		if (not isUserStream) and (stream is not null):
			stream.Close()
			stream = null

	
	public virtual def Read() as int:
		if bufPos < bufLen:
			return buf[(bufPos++)]
		elif Pos < fileLen:
			Pos = Pos
			// shift buffer start to Pos
			return buf[(bufPos++)]
		elif ((stream is not null) and (not stream.CanSeek)) and (ReadNextStreamChunk() > 0):
			return buf[(bufPos++)]

		else:
			return EOF

	
	public def Peek() as int:
		curPos as int = Pos
		ch as int = Read()
		Pos = curPos
		return ch

	
	public def GetString(beg as int, end as int) as string:
		length as int = (end - beg)
		buf as (char) = array(char, length)
		oldPos as int = Pos
		Pos = beg
		for i in range(0, length):
			buf[i] = cast(char, Read())
		Pos = oldPos
		return String(buf)

	
	public Pos as int:
		get:
			return (bufPos + bufStart)
		set:
			if ((value >= fileLen) and (stream is not null)) and (not stream.CanSeek):
				// Wanted position is after buffer and the stream
				// is not seek-able e.g. network or console,
				// thus we have to read the stream manually till
				// the wanted position is in sight.
				while (value >= fileLen) and (ReadNextStreamChunk() > 0):
					pass
			
			if (value < 0) or (value > fileLen):
				raise FatalError(('buffer out of bounds access, position: ' + value))
			
			if (value >= bufStart) and (value < (bufStart + bufLen)):
				// already in buffer
				bufPos = (value - bufStart)
			elif stream is not null:
				// must be swapped in
				stream.Seek(value, SeekOrigin.Begin)
				bufLen = stream.Read(buf, 0, buf.Length)
				bufStart = value
				bufPos = 0
			else:
				// set the position to the end of the file, Pos will return fileLen.
				bufPos = (fileLen - bufStart)

	
	// Read the next chunk of bytes from the stream, increases the buffer
	// if needed and updates the fields fileLen and bufLen.
	// Returns the number of bytes read.
	private def ReadNextStreamChunk() as int:
		free as int = (buf.Length - bufLen)
		if free == 0:
			// in the case of a growing input stream
			// we can neither seek in the stream, nor can we
			// foresee the maximum length, thus we must adapt
			// the buffer size on demand.
			newBuf as (byte) = array(byte, (bufLen * 2))
			Array.Copy(buf, newBuf, bufLen)
			buf = newBuf
			free = bufLen
		read as int = stream.Read(buf, bufLen, free)
		if read > 0:
			fileLen = (bufLen = (bufLen + read))
			return read
		// end of stream reached
		return 0


//-----------------------------------------------------------------------------------
// UTF8Buffer
//-----------------------------------------------------------------------------------
public class UTF8Buffer(Buffer):

	public def constructor(b as Buffer):
		super(b)

	
	public override def Read() as int:
		// until we find a uft8 start (0xxxxxxx or 11xxxxxx)
		// nothing to do, first 127 chars are the same in ascii and utf8
		// 0xxxxxxx or end of file character
		// 11110xxx 10xxxxxx 10xxxxxx 10xxxxxx
		c2 as int
		c1 as int
		c3 as int
		ch as int
		while true:
			ch = super.Read()
			break  unless (((ch >= 128) and ((ch & 192) != 192)) and (ch != EOF))
		if (ch < 128) or (ch == EOF):
			pass
		elif (ch & 240) == 240:
			c1 = (ch & 7)
			ch = super.Read()
			c2 = (ch & 63)
			ch = super.Read()
			c3 = (ch & 63)
			ch = super.Read()
			c4 as int = (ch & 63)
			ch = ((((((c1 << 6) | c2) << 6) | c3) << 6) | c4)
		elif (ch & 224) == 224:
			// 1110xxxx 10xxxxxx 10xxxxxx
			c1 = (ch & 15)
			ch = super.Read()
			c2 = (ch & 63)
			ch = super.Read()
			c3 = (ch & 63)
			ch = ((((c1 << 6) | c2) << 6) | c3)
		elif (ch & 192) == 192:
			// 110xxxxx 10xxxxxx
			c1 = (ch & 31)
			ch = super.Read()
			c2 = (ch & 63)
			ch = ((c1 << 6) | c2)
		return ch


//-----------------------------------------------------------------------------------
// Scanner
//-----------------------------------------------------------------------------------
public class Scanner:

	private static final EOL = char('\n')

	private static final eofSym = 0

	/* pdt */
	static final maxT as int = 49
	static final noSym as int = 49

	
	public buffer as Buffer

	// scanner buffer
	private t as Token

	// current token
	private ch as int

	// current input character
	private pos as int

	// byte position of current character
	private col as int

	// column number of current character
	private line as int

	// line number of current character
	private oldEols as int

	// EOLs that appeared in a comment;
	private start as Dictionary[of int, int]

	// maps first token character to start state
	private tokens as Token

	// list of tokens already peeked (first token is a dummy)
	private pt as Token

	// current peek token
	private tval as (char) = array(char, 128)

	// text of current token
	private tlen as int

	// length of current token
	public def constructor(fileName as string):
		try:
			stream as Stream = FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read)
			buffer = Buffer(stream, false)
			Init()
		except ex as IOException:
			raise FatalError(('Cannot open file ' + fileName))

	
	public def constructor(s as Stream):
		buffer = Buffer(s, true)
		Init()

	
	private def Init():
		pos = (-1)
		line = 1
		col = 0
		oldEols = 0
		NextCh()
		if ch == 239:
			// check optional byte order mark for UTF-8
			NextCh()
			ch1 as int = ch
			NextCh()
			ch2 as int = ch
			if (ch1 != 187) or (ch2 != 191):
				raise FatalError(String.Format('illegal byte order mark: EF {0,2:X} {1,2:X}', ch1, ch2))
			buffer = UTF8Buffer(buffer)
			col = 0
			NextCh()
		start = Dictionary[of int, int](128)
		for i in range(65, 91): start[i] = 1
		for i in range(95, 96): start[i] = 1
		for i in range(97, 123): start[i] = 1
		for i in range(48, 58): start[i] = 2
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

		pt = (tokens = Token())
		// first token is a dummy

	
	private def NextCh():
		if oldEols > 0:
			ch = cast(int,EOL)
			oldEols -= 1
		else:
			pos = buffer.Pos
			ch = buffer.Read()
			col += 1
			// replace isolated '\r' by '\n' in order to make
			// eol handling uniform across Windows, Unix and Mac
			if (ch == char('\r')) and (buffer.Peek() != char('\n')):
				ch = cast(int,EOL)
			if ch == cast(int,EOL):
				line += 1
				col = 0


	
	private def AddCh():
		if tlen >= tval.Length:
			newBuf as (char) = array(char, (2 * tval.Length))
			Array.Copy(tval, 0, newBuf, 0, tval.Length)
			tval = newBuf
		if ch != Buffer.EOF:
			tval[tlen++] = cast(char,ch)

			NextCh()

	
	

	def Comment0() as bool:
		level as int = 1
		line0 as int = line
		NextCh()
		while true:
			if ch == 10:
				level--
				if level == 0:
					oldEols = line - line0
					NextCh()
					return true
				NextCh()
			elif ch == Buffer.EOF:
				return false
			else:
				NextCh()

	def Comment1() as bool:
		level as int = 1
		line0 as int = line
		pos0 as int = pos
		col0 as int = col
		NextCh()
		if ch == char('/'):
			NextCh()
			while true:
				if ch == 10:
					level--
					if level == 0:
						oldEols = line - line0
						NextCh()
						return true
					NextCh()
				elif ch == Buffer.EOF:
					return false
				else:
					NextCh()
		else:
			buffer.Pos = pos0
			NextCh()
			line = line0
			col = col0
		return false

	def Comment2() as bool:
		level as int = 1
		line0 as int = line
		pos0 as int = pos
		col0 as int = col
		NextCh()
		if ch == char('*'):
			NextCh()
			while true:
				if ch == char('*'):
					NextCh()
					if ch == char('/'):
						level--
						if level == 0:
							oldEols = line - line0
							NextCh()
							return true
						NextCh()
				elif ch == char('/'):
					NextCh()
					if ch == char('*'):
						level++
						NextCh()
				elif ch == Buffer.EOF:
					return false
				else:
					NextCh()
		else:
			buffer.Pos = pos0
			NextCh()
			line = line0
			col = col0
		return false

	
	private def CheckLiteral():
		tokString as string =  t.val
		if tokString == "begin": t.kind = 3
		elif tokString == "end": t.kind = 4
		elif tokString == "proc": t.kind = 5
		elif tokString == "val": t.kind = 7
		elif tokString == "res": t.kind = 8
		elif tokString == "is": t.kind = 10
		elif tokString == "skip": t.kind = 13
		elif tokString == "write": t.kind = 14
		elif tokString == "read": t.kind = 15
		elif tokString == "var": t.kind = 16
		elif tokString == "if": t.kind = 18
		elif tokString == "then": t.kind = 19
		elif tokString == "else": t.kind = 20
		elif tokString == "fi": t.kind = 21
		elif tokString == "while": t.kind = 22
		elif tokString == "do": t.kind = 23
		elif tokString == "od": t.kind = 24
		elif tokString == "call": t.kind = 25
		elif tokString == "or": t.kind = 26
		elif tokString == "and": t.kind = 27
		elif tokString == "xor": t.kind = 28
		elif tokString == "not": t.kind = 46
		elif tokString == "true": t.kind = 47
		elif tokString == "false": t.kind = 48


	
	private def NextToken() as Token:
		while ch == char(' ') or ch >= 9 and ch <= 10 or ch == 13:
			NextCh()
		if ch == char('#') and Comment0() or ch == char('/') and Comment1() or ch == char('/') and Comment2():
			return NextToken()
		t = Token()
		t.pos = pos
		t.col = col
		t.line = line
		state as int
		try:
			state = start[ch]
		except ex as KeyNotFoundException:
			state = 0
		tlen = 0
		AddCh()
		
		while true:
			if state == (-1):
				t.kind = eofSym
			elif state == 0:
			// NextCh already done
				t.kind = noSym
			// NextCh already done
			elif state == 1:
				if ch >= char('0') and ch <= char('9') or ch >= char('A') and ch <= char('Z') or ch == char('_') or ch >= char('a') and ch <= char('z'):
					AddCh()
					state = 1
					continue
				else:
					t.kind = 1 
					t.val = String(tval, 0, tlen)
					CheckLiteral()
					return t
			elif state == 2:
				if ch >= char('0') and ch <= char('9'):
					AddCh()
					state = 2
					continue
				else:
					t.kind = 2 
			elif state == 3:
				t.kind = 6 
			elif state == 4:
				t.kind = 9 
			elif state == 5:
				t.kind = 11 
			elif state == 6:
				t.kind = 12 
			elif state == 7:
				if ch == char('='):
					AddCh()
					state = 8
					continue
				else:
					t.kind = noSym
			elif state == 8:
				t.kind = 17 
			elif state == 9:
				t.kind = 31 
			elif state == 10:
				t.kind = 32 
			elif state == 11:
				if ch == char('='):
					AddCh()
					state = 12
					continue
				else:
					t.kind = noSym
			elif state == 12:
				t.kind = 33 
			elif state == 13:
				if ch == char('='):
					AddCh()
					state = 14
					continue
				else:
					t.kind = noSym
			elif state == 14:
				t.kind = 34 
			elif state == 15:
				t.kind = 35 
			elif state == 16:
				t.kind = 36 
			elif state == 17:
				t.kind = 37 
			elif state == 18:
				t.kind = 38 
			elif state == 19:
				t.kind = 39 
			elif state == 20:
				t.kind = 40 
			elif state == 21:
				t.kind = 41 
			elif state == 22:
				t.kind = 42 
			elif state == 23:
				t.kind = 43 
			elif state == 24:
				t.kind = 44 
			elif state == 25:
				t.kind = 45 
			elif state == 26:
				if ch == char('='):
					AddCh()
					state = 9
					continue
				elif ch == char('<'):
					AddCh()
					state = 18
					continue
				else:
					t.kind = 29 
			elif state == 27:
				if ch == char('='):
					AddCh()
					state = 10
					continue
				elif ch == char('>'):
					AddCh()
					state = 19
					continue
				else:
					t.kind = 30 

			break unless false
			
		t.val = String(tval, 0, tlen)
		return t

	
	// get the next token (possibly a token already seen during peeking)
	public def Scan() as Token:
		if tokens.next is null:
			return NextToken()
		else:
			pt = (tokens = tokens.next)
			return tokens

	
	// peek for the next token, ignore pragmas
	public def Peek() as Token:
		if pt.next is null:
			while true:
				pt = (pt.next = NextToken())
				break  unless (pt.kind > maxT)
		else:
			// skip pragmas
			while true:
				pt = pt.next
				break  unless (pt.kind > maxT)
		return pt

	
	// make sure that peeking starts at the current scan position
	public def ResetPeek():
		pt = tokens
	

// end Scanner

