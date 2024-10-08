
/**
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/

using System;

namespace PhpSerializerNET;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class PhpPropertyAttribute : Attribute {
	public string Name { get; set; }
	public int Key { get; set; }
	public bool IsInteger { get; private set; } = false;

	/// <summary>
	/// Define the property key, as a string, for de/serialization.
	/// </summary>
	public PhpPropertyAttribute(string name) {
		this.Name = name;
	}

	/// <summary>
	/// Define an integer key for de/serialization.
	/// Note: This also affects serialization into object notation, as that is a legal way of representing an object.
	/// </summary>
	/// <remark>
	/// Deserialization of objects and arrays with mixed keys may yield unexpected results and or exceptions on certain target types.
	/// </remarks>
	public PhpPropertyAttribute(int key) {
		this.Key = key;
		this.IsInteger = true;
	}
}
