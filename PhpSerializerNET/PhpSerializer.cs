/**
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PhpSerializerNET;

internal class PhpSerializer {
	private readonly PhpSerializiationOptions _options;
	private readonly List<object> _seenObjects;

	public PhpSerializer(PhpSerializiationOptions options = null) {
		this._options = options ?? PhpSerializiationOptions.DefaultOptions;
		this._seenObjects = new();
	}

	public string Serialize(object input) {
		switch (input) {
			case Enum enumValue:
				if (this._options.NumericEnums) {
					return $"i:{enumValue.GetNumericString()};";
				} else {
					return this.Serialize(enumValue.ToString());
				}

			case long longValue:
				return string.Concat("i:", longValue.ToString(CultureInfo.InvariantCulture), ";");

			case int integerValue:
				return string.Concat("i:", integerValue.ToString(CultureInfo.InvariantCulture), ";");

			case double floatValue:
				if (double.IsPositiveInfinity(floatValue)) {
					return $"d:INF;";
				}
				if (double.IsNegativeInfinity(floatValue)) {
					return $"d:-INF;";
				}
				if (double.IsNaN(floatValue)) {
					return $"d:NAN;";
				}
				return string.Concat("d:", floatValue.ToString(CultureInfo.InvariantCulture), ";");

			case string stringValue:
				// Use the UTF8 byte count, because that's what the PHP implementation does:
				string length = Encoding.UTF8.GetByteCount(stringValue).ToString(CultureInfo.InvariantCulture);
				return string.Concat("s:", length, ":\"", stringValue, "\";");

			case bool boolValue:
				return boolValue ? "b:1;" : "b:0;";

			case null:
				return "N;";

			default:
				return this.SerializeComplex(input);
		}
	}

	private string SerializeComplex(object input) {
		if (this._seenObjects.Contains(input)) {
			if (this._options.ThrowOnCircularReferences) {
				throw new ArgumentException("Input object has a circular reference.");
			}
			return "N;";
		}
		this._seenObjects.Add(input);

		switch (input) {
			case PhpDynamicObject dynamicObject: {
					var className = dynamicObject.GetClassName() ?? "stdClass";
					ICollection<string> memberNames = dynamicObject.GetDynamicMemberNames();
					string preamble = $"O:{className.Length}:\"{className}\":{memberNames.Count}:{{";
					string[] entryStrings = new string[memberNames.Count * 2];
					int entryIndex = 0;
					foreach (var memberName in memberNames) {
						entryStrings[entryIndex] = this.Serialize(memberName);
						entryStrings[entryIndex + 1] = this.Serialize(dynamicObject.GetMember(memberName));
						entryIndex += 2;
					}
					return string.Concat(preamble, string.Concat(entryStrings), "}");
				}
			case ExpandoObject expando: {
					var dictionary = (IDictionary<string, object>)expando;
					string preamble = $"O:8:\"stdClass\":{dictionary.Keys.Count}:{{";

					string[] entryStrings = new string[dictionary.Count * 2];
					int entryIndex = 0;
					foreach (var entry in dictionary) {
						entryStrings[entryIndex] = this.Serialize(entry.Key);
						entryStrings[entryIndex + 1] = this.Serialize(entry.Value);
						entryIndex += 2;
					}
					return string.Concat(preamble, string.Concat(entryStrings), "}");
				}
			case IDynamicMetaObjectProvider:
				throw new NotSupportedException(
					"Serialization support for dynamic objects is limited to PhpSerializerNET.PhpDynamicObject and System.Dynamic.ExpandoObject in this version."
				);
			case IDictionary dictionary: {
					string preamble;
					if (input is IPhpObject phpObject) {
						string className = phpObject.GetClassName();
						preamble = $"O:{className.Length}:\"{className}\":{dictionary.Count}:{{";
					} else {
						var dictionaryType = dictionary.GetType();
						if (dictionaryType.GenericTypeArguments.Length > 0) {
							var keyType = dictionaryType.GenericTypeArguments[0];
							if (!keyType.IsIConvertible() && keyType != typeof(object)) {
								throw new Exception($"Can not serialize into associative array with key type {keyType.FullName}");
							}
						}
						preamble = $"a:{dictionary.Count}:{{";
					}

					string[] entryStrings = new string[dictionary.Count * 2];
					int entryIndex = 0;
					foreach (DictionaryEntry entry in dictionary) {
						entryStrings[entryIndex] = this.Serialize(entry.Key);
						entryStrings[entryIndex + 1] = this.Serialize(entry.Value);
						entryIndex += 2;
					}
					return string.Concat(preamble, string.Concat(entryStrings), "}");
				}
			case IList collection:
				string[] itemStrings = new string[collection.Count * 2];
				for (int i = 0; i < itemStrings.Length; i += 2) {
					itemStrings[i] = string.Concat("i:", (i / 2).ToString(CultureInfo.InvariantCulture), ";");
					itemStrings[i + 1] = this.Serialize(collection[i / 2]);
				}
				return string.Concat(
					$"a:{collection.Count}:{{",
					string.Concat(itemStrings),
					"}"
				);
			default: {
					StringBuilder output = new StringBuilder();
					var inputType = input.GetType();

					if (typeof(IPhpObject).IsAssignableFrom(inputType) || inputType.GetCustomAttribute<PhpClass>() != null) {
						return this.SerializeToObject(input);
					}

					List<MemberInfo> members = new();
					if (inputType.IsValueType) {
						foreach (FieldInfo field in inputType.GetFields()) {
							if (field.IsPublic) {
								var attribute = Attribute.GetCustomAttribute(field, typeof(PhpIgnoreAttribute), false);
								if (attribute == null) {
									members.Add(field);
								}
							}
						}
					} else {
						foreach (PropertyInfo property in inputType.GetProperties()) {
							if (property.CanRead) {
								var ignoreAttribute = Attribute.GetCustomAttribute(
									property,
									typeof(PhpIgnoreAttribute),
									false
								);
								if (ignoreAttribute == null) {
									members.Add(property);
								}
							}
						}
					}
					output.Append($"a:{members.Count}:");
					output.Append('{');
					foreach (var member in members) {
						output.Append(this.SerializeMember(member, input));
					}
					output.Append('}');
					return output.ToString();
				}
		}
	}

	private string SerializeToObject(object input) {
		string className;
		if (input is IPhpObject phpObject) {
			className = phpObject.GetClassName();
		} else {
			className = input.GetType().GetCustomAttribute<PhpClass>()?.Name;
		}

		if (string.IsNullOrEmpty(className)) {
			className = "stdClass";
		}
		StringBuilder output = new StringBuilder();
		List<PropertyInfo> properties = new();
		foreach (var property in input.GetType().GetProperties()) {
			if (property.CanRead) {
				var ignoreAttribute = Attribute.GetCustomAttribute(
					property,
					typeof(PhpIgnoreAttribute),
					false
				);
				if (ignoreAttribute == null) {
					properties.Add(property);
				}
			}
		}

		output.Append("O:")
			.Append(className.Length)
			.Append(":\"")
			.Append(className)
			.Append("\":")
			.Append(properties.Count)
			.Append(":{");
		foreach (PropertyInfo property in properties) {
			output.Append(this.SerializeMember(property, input));
		}
		output.Append('}');
		return output.ToString();
	}

	private string SerializeMember(MemberInfo member, object input) {
		PhpPropertyAttribute attribute = (PhpPropertyAttribute)Attribute.GetCustomAttribute(
			member,
			typeof(PhpPropertyAttribute),
			false
		);

		if (attribute != null) {
			if (attribute.IsInteger == true) {
				return $"{this.Serialize(attribute.Key)}{this.Serialize(member.GetValue(input))}";
			}
			return $"{this.Serialize(attribute.Name)}{this.Serialize(member.GetValue(input))}";
		}
		return $"{this.Serialize(member.Name)}{this.Serialize(member.GetValue(input))}";
	}
}