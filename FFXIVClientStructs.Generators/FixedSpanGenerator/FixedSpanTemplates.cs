namespace FFXIVClientStructs.Generators.FixedSpanGenerator {
	internal static class Templates {
		public const string FixedSpan = @"using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using FFXIVClientStructs.STD;

namespace {{ struct.namespace }} {
	public unsafe partial struct {{ struct.name }} {
		{{~ for span in struct.spans ~}}
		public Span<{{ span.type }}> {{ span.name }} => new(Unsafe.AsPointer(ref {{ span.field_name }}), {{ span.length }});
		{{ end }}
	}
}
";
	}
}