/**
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/

using System.Text;

namespace PhpSerializerNET;

/// <summary>
/// Options for deserializing PHP data.
/// </summary>
public class PhpDeserializationOptions {
	/// <summary>
	/// Whether properties are matched case-sensitive. Default true.
	/// </summary>
	public bool CaseSensitiveProperties { get; init; } = true;

	/// <summary>
	/// If true, keys present in the array but not on the target class will be ignored.
	/// Otherwise an exception will be thrown.
	/// Default is false.
	/// </summary>
	public bool AllowExcessKeys { get; init; } = false;

	/// <summary>
	/// Determines how and when associative arrays are deserialized into <see cref="System.Collections.Generic.List{object}"/>
	/// instead of a dictionary. Defaults to <see cref="ListOptions.Default"/>.
	/// </summary>
	public ListOptions UseLists { get; init; } = ListOptions.Default;

	/// <summary>
	/// On deserializing an IConvertible from a PHP string, treat an empty string as the default value of the target type
	/// i.e. "" => 0 for an integer.
	/// </summary>
	public bool EmptyStringToDefault { get; init; } = true;

	/// <summary>
	/// Whether to convert strings "1"` and "0" to boolean.
	/// Default is false.
	/// </summary>
	public bool NumberStringToBool { get; init; }

	/// <summary>
	/// Encoding of the input. Default is UTF-8. Encoding can make a difference in string lenghts and selecting the wrong
	/// encoding for a given input can cause the deserialization to fail.
	/// </summary>
	public Encoding InputEncoding { get; init; } = Encoding.UTF8;

	/// <summary>
	/// Target datatype for objects of type "stdClass".
	/// Default: <see cref="StdClassOption.Dictionary"/>.
	/// Note: This does not affect use of PhpSerialization.Deserialize<T>()
	/// </summary>
	public StdClassOption StdClass { get; init; } = StdClassOption.Dictionary;

	/// <summary>
	/// Enable or disable lookup in currently loaded assemblies for target classes and structs to deserialize objects into.
	/// i.E. `o:8:"UserInfo":...` being mapped to a UserInfo class.
	/// Note: This does not affect use of PhpSerialization.Deserialize<T>()
	/// </summary>
	public bool EnableTypeLookup { get; init; } = true;

	public TypeCacheFlag TypeCache { get; init; } = TypeCacheFlag.ClassNames;

	public static PhpDeserializationOptions DefaultOptions { get; } = new();
}
