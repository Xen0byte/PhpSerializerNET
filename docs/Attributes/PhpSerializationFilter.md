
[back to overview](../README.md)

---

**Table of contents**
- [PhpSerializationFilter](#phpserializationfilter)
	- [Example](#example)

---

# PhpSerializationFilter

Abstract class that can be implemented to modifiy the serialization output on a given property or field.
To facilitate completely omitting fields, `null` returned from the `Serialize` method has special consideration in the
serializer.

## Example

**Implementation:**
```cs
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class PhpIgnoreNull : PhpSerializationFilter {
	public override string? Serialize(object key, object value, PhpSerializiationOptions options) {
		if (value != null) {
			return PhpSerialization.Serialize(key, options) + PhpSerialization.Serialize(value, options);
		}
		return null;
	}
}
```

**Usage:**
```cs
public struct PersonA {
	[PhpIgnoreNull]
	public string? Title { get; set; }
	public required string FirstName { get; set; } 
	public required string LastName { get; set; } 
}

var serialized = PhpSerialization.Serialize(new PersonA { FirstName = "John", LastName = "Johnson" });
// serialized = "a:2:{s:9:"FirstName";s:4:"John";s:8:"LastName";s:7:"Johnson"}"

// in contrast without the custom filter:
public struct PersonB {
	public string? Title { get; set; }
	public required string FirstName { get; set; } 
	public required string LastName { get; set; } 
}

var serialized = PhpSerialization.Serialize(new PersonB { FirstName = "John", LastName = "Johnson" });
// serialized = "a:3:{s:5:"Title";N;s:9:"FirstName";s:4:"John";s:8:"LastName";s:7:"Johnson"}"
```
