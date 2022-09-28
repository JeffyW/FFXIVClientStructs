namespace FFXIVClientStructs.Generators.FixedArrayGenerator {
	internal static class Templates {
		public const string FixedArray = @"using System;
using System.Runtime.InteropServices;

namespace {{ struct.namespace }} {
	public unsafe partial struct {{ struct.name }} {
		{{~ for arr in struct.arrays ~}}
		[FieldOffset({{ arr.offset }})] public {{ struct.array_namespace }}.{{ arr.name }} {{ arr.name }};
		{{ end }}
	}
}

namespace {{ struct.array_namespace }} {
	{{~ for arr in struct.arrays ~}}
	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct {{ arr.name }} {
		public int Length => {{ arr.length }};
	
		public ref {{ arr.type }} this[int index] {
			get {
				if (index < 0 || index > Length)
					throw new IndexOutOfRangeException($""Index out of Range: {index}"");
				fixed(void* ptr = &this)
					return ref *({{ arr.type }}*)((byte*)ptr + index * sizeof({{ arr.type }}));
			}
		}
	}
	{{ end }}
}
";
	}
}