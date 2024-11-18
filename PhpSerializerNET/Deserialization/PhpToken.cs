
using System.Runtime.InteropServices;

/**
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/
namespace PhpSerializerNET;
#nullable enable

/// <summary>
/// PHP data token. Holds the type, position (in the input string), length and value.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 1)]
internal readonly struct PhpToken {
	internal readonly PhpDataType Type;
	internal readonly int Position;
	internal readonly int Length;
	internal readonly ValueSpan Value;
	/// <summary>
	/// For <see cref="PhpDataType.Array"/> and <see cref="PhpDataType.Object"/> only. Holds the index of the last value
	/// token inside the respective array/object.
	/// </summary>
	/// <remarks>
	/// This does NOT reference the last value token. It could for example point to the last value token of an
	/// object inside an array, when the "last value" of the array would be the object itself.
	/// </remarks>
	internal readonly int LastValuePosition;

	internal PhpToken(
		in PhpDataType type,
		in int position,
		in ValueSpan value,
		int length = 0,
		int lastValuePosition = 0
	) {
		this.Type = type;
		this.Position = position;
		this.Value = value;
		this.Length = length;
		this.LastValuePosition = lastValuePosition;
	}
}
