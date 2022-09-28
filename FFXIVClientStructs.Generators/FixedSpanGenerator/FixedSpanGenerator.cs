using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Scriban;

namespace FFXIVClientStructs.Generators.FixedSpanGenerator {
	[Generator]
	internal class FixedSpanGenerator : ISourceGenerator {
		private Template _spanTemplate;

		public void Initialize(GeneratorInitializationContext context) {
			context.RegisterForSyntaxNotifications(() => new FixedSpanSyntaxContextReceiver());
			_spanTemplate = Template.Parse(Templates.FixedSpan);
		}

		public void Execute(GeneratorExecutionContext context) {
			if (context.SyntaxContextReceiver is not FixedSpanSyntaxContextReceiver receiver)
				return;

			foreach (var structObj in receiver.Structs) {
				var filename = structObj.Namespace + "." + structObj.Name + ".FixedSpans.generated.cs";
				var source = _spanTemplate.Render(new {Struct = structObj});
				context.AddSource(filename, SourceText.From(source, Encoding.UTF8));
			}
		}
	}
}