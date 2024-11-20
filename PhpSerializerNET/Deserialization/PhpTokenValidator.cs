/**
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/

using System;
using System.Runtime.CompilerServices;

namespace PhpSerializerNET;

#nullable enable

internal ref struct PhpTokenValidator {
	private int _position;
	private int _tokenCount;
	private readonly ReadOnlySpan<byte> _input;
	private readonly int _lastIndex;

	internal PhpTokenValidator(in ReadOnlySpan<byte> input) {
		this._tokenCount = 1;
		this._input = input;
		this._position = 0;
		this._lastIndex = this._input.Length - 1;
	}

	internal void GetToken() {
		switch (this._input[this._position++]) {
			case (byte)'b':
				this.GetCharacter(':');
				this.GetBoolean();
				this.GetCharacter(';');
				break;
			case (byte)'N':
				this.GetCharacter(';');
				break;
			case (byte)'s':
				this.GetCharacter(':');
				int length = 0;
				this.GetLength(PhpDataType.String, ref length);
				this.GetCharacter(':');
				this.GetCharacter('"');
				this.GetNCharacters(length);
				this.GetCharacter('"');
				this.GetCharacter(';');
				break;
			case (byte)'i':
				this.GetCharacter(':');
				this.GetInteger();
				this.GetCharacter(';');
				break;
			case (byte)'d':
				this.GetCharacter(':');
				this.GetFloat();
				this.GetCharacter(';');
				break;
			case (byte)'a':
				this.GetArrayToken();
				break;
			case (byte)'O':
				this.GetObjectToken();
				break;
			default:
				throw new DeserializationException(
					$"Unexpected token '{this.GetCharAt(this._position - 1)}' at position {this._position - 1}."
				);
		};
	}

	private char GetCharAt(int position) {
		return (char)this._input[position];
	}

	private void GetCharacter(char character) {
		if (this._lastIndex < this._position) {
			throw new DeserializationException(
				$"Unexpected end of input. Expected '{character}' at index {this._position}, but input ends at index {this._lastIndex}"
			);
		}
		if (this._input[this._position++] != character) {
			throw new DeserializationException(
				$"Unexpected token at index {this._position - 1}. Expected '{character}' but found '{this.GetCharAt(this._position - 1)}' instead."
			);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void GetFloat() {
		int i = this._position;
		for (; this._input[i] != (byte)';' && i < this._lastIndex; i++) {
			_ = this._input[this._position] switch {
				>= (byte)'0' and <= (byte)'9' => true,
				(byte)'+' => true,
				(byte)'-' => true,
				(byte)'.' => true,
				(byte)'E' or (byte)'e' => true, // exponents.
				(byte)'I' or (byte)'F' => true, // infinity.
				(byte)'N' or (byte)'A' => true, // NaN.
				_ => throw new DeserializationException(
					$"Unexpected token at index {this._position}. " +
					$"'{this.GetCharAt(this._position)}' is not a valid part of a floating point number."
				),
			};
		}
		if (i == this._position) {
			throw new DeserializationException(
				$"Unexpected token at index {i}: Expected floating point number, but found ';' instead."
			);
		}
		this._position = i;

		// Edgecase: input ends here without a delimeter following. Normal handling would give a misleading exception:
		if (this._lastIndex == this._position && this._input[this._position] != (byte)';') {
			throw new DeserializationException(
				$"Unexpected end of input. Expected ':' at index {this._position}, but input ends at index {this._lastIndex}"
			);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void GetInteger() {
		int i = this._position;
		for (; this._input[i] != ';' && i < this._lastIndex; i++) {
			_ = this._input[i] switch {
				>= (byte)'0' and <= (byte)'9' => true,
				(byte)'+' => true,
				(byte)'-' => true,
				_ => throw new DeserializationException(
					$"Unexpected token at index {i}. " +
					$"'{this.GetCharAt(i)}' is not a valid part of a number."
				),
			};
		}
		if (i == this._position) {
			throw new DeserializationException(
				$"Unexpected token at index {i}: Expected number, but found ';' instead."
			);
		}
		this._position = i;

		// Edgecase: input ends here without a delimeter following. Normal handling would give a misleading exception:
		if (this._lastIndex == this._position && this._input[this._position] != (byte)';') {
			throw new DeserializationException(
				$"Unexpected end of input. Expected ':' at index {this._position}, but input ends at index {this._lastIndex}"
			);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void GetLength(PhpDataType dataType, ref int length) {
		for (; this._input[this._position] != ':' && this._position < this._lastIndex; this._position++) {
			length = this._input[this._position] switch {
				>= (byte)'0' and <= (byte)'9' => length * 10 + (this._input[this._position] - 48),
				_ => throw new DeserializationException(
					$"{dataType} at position {this._position} has illegal, missing or malformed length."
				),
			};
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void GetBoolean() {
		if (this._lastIndex < this._position) {
			throw new DeserializationException(
				$"Unexpected end of input. Expected '0' or '1' at index {this._position}, but input ends at index {this._lastIndex}"
			);
		}
		var item = this._input[this._position++];
		if (item != 48 && item != 49) {
			throw new DeserializationException(
				$"Unexpected token in boolean at index {this._position - 1}. "
				+ $"Expected either '1' or '0', but found '{(char)item}' instead."
			);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void GetNCharacters(int length) {
		if (this._position + length > this._lastIndex) {
			throw new DeserializationException(
				$"Illegal length of {length}. The string at position {this._position} points to out of bounds index {this._position + length}."
			);
		}
		this._position += length;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void GetObjectToken() {
		int position = this._position - 1;
		int classNamelength = 0;
		int propertyCount = 0;
		this.GetCharacter(':');
		this.GetLength(PhpDataType.Object, ref classNamelength);
		this.GetCharacter(':');
		this.GetCharacter('"');
		this.GetNCharacters(classNamelength);
		this.GetCharacter('"');
		this.GetCharacter(':');
		this.GetLength(PhpDataType.Object, ref propertyCount);
		this.GetCharacter(':');
		this.GetCharacter('{');
		int i = 0;
		while (this._input[this._position] != '}') {
			this.GetToken();
			this.GetToken();
			i++;
			if (i > propertyCount) {
				throw new DeserializationException(
					$"Object at position {position} should have {propertyCount} properties, " +
					$"but actually has {i} or more properties."
				);
			}
		}
		this._tokenCount += propertyCount * 2;
		this.GetCharacter('}');
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void GetArrayToken() {
		int position = this._position - 1;
		this.GetCharacter(':');
		int length = 0;
		this.GetLength(PhpDataType.Array, ref length);
		this.GetCharacter(':');
		this.GetCharacter('{');
		int i = 0;
		while (this._input[this._position] != '}') {
			this.GetToken();
			this.GetToken();
			i++;
			if (i > length) {
				throw new DeserializationException(
					$"Array at position {position} should be of length {length}, " +
					$"but actual length is {i} or more."
				);
			}
		}
		this._tokenCount += length * 2;
		this.GetCharacter('}');
	}

	/// <summary>
	/// Validate the PHP data and return the number of tokens found.
	/// </summary>
	/// <param name="input"> The raw UTF8 bytes of PHP data to validate. </param>
	/// <returns> The number of tokens found. </returns>
	/// <exception cref="DeserializationException"></exception>
	internal static int Validate(ReadOnlySpan<byte> input) {
		var validatior = new PhpTokenValidator(input);
		validatior.GetToken();
		if (validatior._position <= validatior._lastIndex) {
			throw new DeserializationException($"Unexpected token '{(char)input[validatior._position]}' at position {validatior._position}.");
		}
		return validatior._tokenCount;
	}
}
