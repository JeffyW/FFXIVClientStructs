using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FFXIVClientStructs.Generators.FixedSpanGenerator {
	internal class FixedSpanSyntaxContextReceiver : ISyntaxContextReceiver {
		private readonly SymbolDisplayFormat _format = new(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces, miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

		public List<Struct> Structs = new();

		public void OnVisitSyntaxNode(GeneratorSyntaxContext context) {
			if (context.Node is not StructDeclarationSyntax sds)
				return;
			if (sds.Modifiers.All(m => m.Text != "partial"))
				return;

			var fields = sds.ChildNodes().OfType<FieldDeclarationSyntax>()
				.Where(f => f.AttributeLists.SelectMany(al => al.Attributes)
					.Any(a => a.Name.ToString() == "FixedSpan")).ToList();

			if (context.SemanticModel.GetDeclaredSymbol(sds) is not INamedTypeSymbol structType)
				return;

			var structObj = new Struct {
				Name = structType.Name,
				Namespace = structType.ContainingNamespace.ToDisplayString(_format),
				Spans = new List<FixedSpan>()
			};

			foreach (var f in fields) {
				var attributes = f.AttributeLists.SelectMany(al => al.Attributes).ToList();
				if (attributes.FirstOrDefault(a => a.Name.ToString() == "FixedSpan") is not { } spanAttr)
					continue;
				
				var name = f.Declaration.Variables.FirstOrDefault()?.Identifier.ValueText;
				if (string.IsNullOrWhiteSpace(name)) continue;

				var isFixedArray = f.Declaration.Variables.FirstOrDefault()?.ArgumentList?.Arguments.Count > 0;

				var type = GetTypeOfArgument(ref context, spanAttr, 0);
				if (type == null) continue;
				
				var typeString = type.ToDisplayString();
				if (type is IPointerTypeSymbol pointer)
					typeString = $"Pointer<{pointer.PointedAtType.ToDisplayString()}>";

				var length = GetStringArgument(ref context, spanAttr, 1);
				if (string.IsNullOrWhiteSpace(length))
					length = GetIntArgument(ref context, spanAttr, 1);
				if (string.IsNullOrWhiteSpace(length)) continue;

				var arrObj = new FixedSpan {
					Name = FixSpanName(name),
					FieldName = isFixedArray ? $"{name}[0]" : name,
					Length = length,
					Type = typeString,
				};

				structObj.Spans.Add(arrObj);
			}

			if (structObj.Spans.Count > 0)
				Structs.Add(structObj);
		}

		private static string FixSpanName(string name) {
			if (name.StartsWith("_") && name.Length > 1)
				return $"{char.ToUpper(name[1])}{name.Substring(2)}";
			if (name.Contains("Array"))
				return name.Replace("Array", "Span");
			if (name.Contains("List"))
				return name.Replace("List", "Span");
			return $"{name}Span";
		}

		private static ITypeSymbol GetTypeOfArgument(ref GeneratorSyntaxContext context, AttributeSyntax attr, int index) {
			if (attr.ArgumentList?.Arguments[index].Expression is not TypeOfExpressionSyntax typeEx)
				return null;
			return context.SemanticModel.GetTypeInfo(typeEx.Type).Type;
		}

		private static string GetStringArgument(ref GeneratorSyntaxContext context, AttributeSyntax attr, int index) {
			var ex = attr.ArgumentList?.Arguments[index].Expression;
			if (ex is InvocationExpressionSyntax invEx)
				return context.SemanticModel.GetConstantValue(invEx).Value?.ToString();
			if (ex is LiteralExpressionSyntax litEx)
				return context.SemanticModel.GetConstantValue(litEx).Value?.ToString();
			return null;
		}

		private static string GetIntArgument(ref GeneratorSyntaxContext context, AttributeSyntax attr, int index) {
			if (attr.ArgumentList?.Arguments[index].Expression is not LiteralExpressionSyntax litEx)
				return null;
			if (context.SemanticModel.GetConstantValue(litEx).Value is int intValue)
				return intValue.ToString();
			return null;
		}
	}
}