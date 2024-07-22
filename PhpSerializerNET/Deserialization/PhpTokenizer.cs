/**
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace PhpSerializerNET;

public ref struct PhpTokenizer {
	private int _position;
	private readonly Encoding _inputEncoding;
	private Span<PhpToken> _tokens;
	private int _tokenPosition = 0;

	private readonly ReadOnlySpan<byte> _input;

	private PhpTokenizer(ReadOnlySpan<byte> input, Encoding inputEncoding, Span<PhpToken> array) {
		this._tokens = array;
		this._inputEncoding = inputEncoding;
		this._input = input;
		this._position = 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private PhpDataType GetDataType() {
		var result = this._input[this._position] switch {
			(byte)'N' => PhpDataType.Null,
			(byte)'b' => PhpDataType.Boolean,
			(byte)'s' => PhpDataType.String,
			(byte)'i' => PhpDataType.Integer,
			(byte)'d' => PhpDataType.Floating,
			(byte)'a' => PhpDataType.Array,
			(byte)'O' => PhpDataType.Object,
			_ => throw new UnreachableException(),
		};
		this._position++;
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void Advance() {
		this._position++;
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void Advance(int positons) {
		this._position += positons;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private string GetNumbers() {
		int start = this._position;
		var span = this._input.Slice(this._position);
		span = span.Slice(0, span.IndexOf((byte)';'));
		this._position += span.Length;
		return span.Utf8Substring(this._inputEncoding);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private int GetLength() {
		int length = 0;
		for (; this._input[this._position] != ':'; this._position++) {
			length = length * 10 + (_input[_position] - 48);
		}
		return length;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private string GetBoolean() {
		string result = this._input[this._position] switch {
			(byte)'1' => "1",
			(byte)'0' => "0",
			_ => throw new UnreachableException()
		};
		this._position++;
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private string GetNCharacters(int length) {
		int start = this._position;
		this._position += length;
		return this._input.Slice(start, length).Utf8Substring(this._inputEncoding);
	}

	internal void GetToken() {
		switch (this.GetDataType()) {
			case PhpDataType.Boolean:
				this.GetBooleanToken();
				break;
			case PhpDataType.Null:
				this.GetNullToken();
				break;
			case PhpDataType.String:
				this.GetStringToken();
				break;
			case PhpDataType.Integer:
				this.GetIntegerToken();
				break;
			case PhpDataType.Floating:
				this.GetFloatingToken();
				break;
			case PhpDataType.Array:
				this.GetArrayToken();
				break;
			case PhpDataType.Object:
				this.GetObjectToken();
				break;
		};
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void GetNullToken() {
		this._tokens[this._tokenPosition++] = new PhpToken(PhpDataType.Null, _position-1);
		this.Advance();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void GetBooleanToken() {
		this.Advance();
		this._tokens[this._tokenPosition++] = new PhpToken(
			PhpDataType.Boolean,
			_position - 2,
			this.GetBoolean(),
			0
		);
		this.Advance();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void GetStringToken() {
		int position = _position -1;
		this.Advance();
		int length = this.GetLength();
		this.Advance(2);
		this._tokens[this._tokenPosition++] = new PhpToken(
			PhpDataType.String,
			position,
			this.GetNCharacters(length)
		);
		this.Advance(2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void GetIntegerToken() {
		this.Advance();
		this._tokens[this._tokenPosition++] = new PhpToken(
			PhpDataType.Integer,
			this._position-2,
			this.GetNumbers()
		);
		this.Advance();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void GetFloatingToken() {
		this.Advance();
		this._tokens[this._tokenPosition++] = new PhpToken(
			PhpDataType.Floating,
			this._position - 2,
			this.GetNumbers()
		);
		this.Advance();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void GetArrayToken() {
		int position = _position - 1;
		this.Advance();
		int length = this.GetLength();
		this._tokens[this._tokenPosition++] = new PhpToken(
			PhpDataType.Array,
			position,
			"",
			length
		);
		this.Advance(2);
		for (int i = 0; i < length * 2; i++) {
			this.GetToken();
		}
		this.Advance();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void GetObjectToken() {
		int position = _position -1;
		this.Advance();
		int classNamelength = this.GetLength();
		this.Advance(2);
		string className = this.GetNCharacters(classNamelength);
		this.Advance(2);
		int propertyCount = this.GetLength();
		this._tokens[this._tokenPosition++] = new PhpToken(
			PhpDataType.Object,
			position,
			className,
			propertyCount
		);
		this.Advance(2);
		for (int i = 0; i < propertyCount * 2; i++) {
			this.GetToken();
		}
		this.Advance();
	}

	internal static void Tokenize(ReadOnlySpan<byte> inputBytes, Encoding inputEncoding, Span<PhpToken> tokens) {
		new PhpTokenizer(inputBytes, inputEncoding, tokens).GetToken();
	}
}