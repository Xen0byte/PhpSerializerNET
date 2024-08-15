/**
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/
namespace PhpSerializerNET;

using System;
using System.Globalization;
using System.Text;

internal readonly struct ValueSpan {
	private static ValueSpan _empty = new ValueSpan(0, 0);
	internal readonly int Start;
	internal readonly int Length;

	internal ValueSpan(int start, int length) {
		this.Start = start;
		this.Length = length;
	}

	internal static ValueSpan Empty => _empty;

	internal ReadOnlySpan<byte> GetSlice(ReadOnlySpan<byte> input) => input.Slice(this.Start, this.Length);

	internal double GetDouble(ReadOnlySpan<byte> input) {
		var value = input.Slice(Start, Length);
		return value switch {
		[(byte)'I', (byte)'N', (byte)'F'] => double.PositiveInfinity,
		[(byte)'-', (byte)'I', (byte)'N', (byte)'F'] => double.NegativeInfinity,
		[(byte)'N', (byte)'A', (byte)'N'] => double.NaN,
			_ => double.Parse(value, CultureInfo.InvariantCulture),
		};
	}

	internal bool GetBool(ReadOnlySpan<byte> input) => input[this.Start] == '1';

	internal long GetLong(ReadOnlySpan<byte> input) {
		// All the PHP integers we deal with here can only be the number characters and an optional "-".
		// See also the Validator code.
		// 'long.Parse()' has to take into account that we can skip here, making this manual approach faster.
		var span = input.Slice(this.Start, this.Length);
		int i = 0;
		bool isNegative = false;
		if (span[0] == '-') {
			i++;
			isNegative = true;
		}
		long result = 0;
		for (; i < span.Length; i++) {
			result = result * 10 + (span[i] - 48);
		}
		return isNegative ? result * -1 : result;
	}

	internal string GetString(ReadOnlySpan<byte> input, Encoding inputEncoding) {
		return inputEncoding.GetString(input.Slice(this.Start, this.Length));
	}
}