using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Scriban;

namespace FFXIVClientStructs.Generators.FixedArrayGenerator {
	[Generator]
	internal class FixedArrayGenerator : ISourceGenerator {
		private Template _arrayTemplate;

		public void Initialize(GeneratorInitializationContext context) {
			context.RegisterForSyntaxNotifications(() => new FixedArraySyntaxContextReceiver());
			_arrayTemplate = Template.Parse(Templates.FixedArray);
		}

		public void Execute(GeneratorExecutionContext context) {
			if (context.SyntaxContextReceiver is not FixedArraySyntaxContextReceiver receiver)
				return;

			foreach (var structObj in receiver.Structs) {
				var filename = structObj.Namespace + "." + structObj.Name + ".FixedArrays.generated.cs";
				var source = _arrayTemplate.Render(new {Struct = structObj});
				context.AddSource(filename, SourceText.From(source, Encoding.UTF8));
			}
		}
	}
}