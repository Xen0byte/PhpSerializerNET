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
	private readonly Encoding _inputEncoding;
	private readonly ReadOnlySpan<byte> _input;
	private Span<PhpToken> _tokens;
	private int _position;
	private int _tokenPosition;

	private PhpTokenizer(ReadOnlySpan<byte> input, Encoding inputEncoding, Span<PhpToken> array) {
		this._inputEncoding = inputEncoding;
		this._input = input;
		this._tokens = array;
		this._position = 0;
		this._tokenPosition = 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private PhpDataType GetDataType() {
		return this._input[this._position++] switch {
			(byte)'N' => PhpDataType.Null,
			(byte)'b' => PhpDataType.Boolean,
			(byte)'s' => PhpDataType.String,
			(byte)'i' => PhpDataType.Integer,
			(byte)'d' => PhpDataType.Floating,
			(byte)'a' => PhpDataType.Array,
			(byte)'O' => PhpDataType.Object,
			_ => throw new UnreachableException(),
		};
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
		int length = this._input.Slice(this._position).IndexOf((byte)';');
		this._position += length;
		return this._inputEncoding.GetString(this._input.Slice(start, length));
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
	private string GetNCharacters(int length) {
		return _inputEncoding.GetString(this._input.Slice(this._position, length));
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
			this._input[this._position++] == (byte)'1'
				? "1"
				: "0",
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
		this.Advance(2+length);
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
		int classNameLength = this.GetLength();
		this.Advance(2);
		string className = this.GetNCharacters(classNameLength);
		this.Advance(2+classNameLength);
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