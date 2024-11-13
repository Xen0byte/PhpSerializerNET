/**
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/

using System;
using System.Runtime.CompilerServices;

#nullable enable

namespace PhpSerializerNET;

public ref struct PhpTokenizer {
	private readonly Span<PhpToken> _tokens;
	private readonly ReadOnlySpan<byte> _input;
	private int _position;
	private int _tokenPosition;

	private PhpTokenizer(ReadOnlySpan<byte> input, Span<PhpToken> array) {
		this._input = input;
		this._tokens = array;
		this._position = 0;
		this._tokenPosition = 0;
	}

	private void Advance(int positions) {
		this._position += positions;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private ValueSpan GetNumbers() {
		int start = this._position;
		while (this._input[++this._position] != (byte)';') { }
		return new ValueSpan(start, this._position - start);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private int GetLength() {
		int result = 0;
		for (; this._input[this._position] != ':'; this._position++) {
			result = result * 10 + (this._input[this._position] - 48);
		}
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	private void GetToken() {
		switch (this._input[this._position++]) {
			case (byte)'b':
				this.GetBooleanToken();
				break;
			case (byte)'N':
				this._tokens[this._tokenPosition++] = new PhpToken(PhpDataType.Null, this._position - 1, ValueSpan.Empty);
				this._position++;
				break;
			case (byte)'s':
				this.GetStringToken();
				break;
			case (byte)'i':
				this.GetIntegerToken();
				break;
			case (byte)'d':
				this.GetFloatingToken();
				break;
			case (byte)'a':
				this.GetArrayToken();
				break;
			case (byte)'O':
				this.GetObjectToken();
				break;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void GetBooleanToken() {
		this._position++;
		this._tokens[this._tokenPosition++] = new PhpToken(
			PhpDataType.Boolean,
			this._position - 2,
			new ValueSpan(this._position++, 1)
		);
		this._position++;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void GetStringToken() {
		int position = this._position - 1;
		this._position++;
		int length = this.GetLength();
		this._position += 2;
		this._tokens[this._tokenPosition++] = new PhpToken(
			PhpDataType.String,
			position,
			new ValueSpan(this._position, length)
		);
		this._position += 2 + length;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void GetIntegerToken() {
		this._position++;
		this._tokens[this._tokenPosition++] = new PhpToken(
			PhpDataType.Integer,
			this._position - 2,
			this.GetNumbers()
		);
		this._position++;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void GetFloatingToken() {
		this._position++;
		this._tokens[this._tokenPosition++] = new PhpToken(
			PhpDataType.Floating,
			this._position - 2,
			this.GetNumbers()
		);
		this._position++;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void GetArrayToken() {
		int position = this._position - 1;
		this._position++;
		int length = this.GetLength();
		this._tokens[this._tokenPosition++] = new PhpToken(
			PhpDataType.Array,
			position,
			ValueSpan.Empty,
			length
		);
		this.Advance(2);
		for (int i = 0; i < length * 2; i++) {
			this.GetToken();
		}
		this._position++;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void GetObjectToken() {
		int position = this._position - 1;
		this._position++;
		int classNameLength = this.GetLength();
		this.Advance(2);
		ValueSpan classNameSpan = new ValueSpan(this._position, classNameLength);
		this.Advance(2 + classNameLength);
		int propertyCount = this.GetLength();
		this._tokens[this._tokenPosition++] = new PhpToken(
			PhpDataType.Object,
			position,
			classNameSpan,
			propertyCount
		);
		this.Advance(2);
		for (int i = 0; i < propertyCount * 2; i++) {
			this.GetToken();
		}
		this._position++;
	}

	internal static void Tokenize(ReadOnlySpan<byte> inputBytes, Span<PhpToken> tokens) {
		new PhpTokenizer(inputBytes, tokens).GetToken();
	}
}