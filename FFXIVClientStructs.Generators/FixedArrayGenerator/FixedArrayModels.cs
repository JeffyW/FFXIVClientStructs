using System.Collections.Generic;

namespace FFXIVClientStructs.Generators.FixedArrayGenerator {
	internal class Struct {
		public string Name;
		public string Namespace;
		public string ArrayNamespace;
		public List<FixedArray> Arrays;
	}

	internal class FixedArray {
		public string Name;
		public string Type;
		public string Length;
		public string Offset;
	}
}