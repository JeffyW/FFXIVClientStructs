using System.Collections.Generic;

namespace FFXIVClientStructs.Generators.FixedSpanGenerator {
	internal class Struct {
		public string Name;
		public string Namespace;
		public List<FixedSpan> Spans;
	}

	internal class FixedSpan {
		public string Name;
		public string FieldName;
		public string Type;
		public string Length;
	}
}