using System;

namespace FFXIVClientStructs.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class FixedSpanAttribute : Attribute {
	public FixedSpanAttribute(Type type, string countField) {
		Type = type;
		CountField = countField;
		Count = 0;
	}

	public FixedSpanAttribute(Type type, int count) {
		Type = type;
		CountField = string.Empty;
		Count = count;
	}

	public Type Type { get; }
	public int Count { get; }
	public string CountField { get; }
}