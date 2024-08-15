/**
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/

using System;

namespace PhpSerializerNET;

#nullable enable

/// <summary>
/// Base attribute class for serialization filters. Filters can be used to customize the serialization output
/// on struct and class members. For instance by omitting null values or serializing <see cref="DateTime"/>` as an
/// integer unix timestamp.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public abstract class PhpSerializationFilter : Attribute {

	public PhpSerializationFilter() {
	}

	/// <summary>
	/// Serialize a class or struct member.
	/// </summary>
	/// <param name="key"> The key that identifies the member. </param>
	/// <param name="value"> The value that is to be serialized. </param>
	/// <param name="options"> The options used during the serialization. </param>
	/// <returns>
	/// The serialized member. e.g. <c>s:3:"foo";i:42;"</c>. <br/>
	/// </returns>
	/// <remarks>
	/// If the default serialization is desired, then return <br/>
	/// <c>PhpSerialization.Serialize(key, options) + PhpSerialization.Serialize(value, options);</c>
	/// </remarks>
	public abstract string? Serialize(object key, object? value, PhpSerializiationOptions options);
}