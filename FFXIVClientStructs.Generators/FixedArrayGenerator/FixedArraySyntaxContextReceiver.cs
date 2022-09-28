using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FFXIVClientStructs.Generators.FixedArrayGenerator {
	internal class FixedArraySyntaxContextReceiver : ISyntaxContextReceiver {
		private readonly SymbolDisplayFormat _format = new(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces, miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes);
		
		public List<Struct> Structs = new();

		public void OnVisitSyntaxNode(GeneratorSyntaxContext context) {
			if (context.Node is not StructDeclarationSyntax sds)
				return;
			if (sds.Modifiers.All(m => m.Text != "partial"))
				return;

			var fields = sds.ChildNodes().OfType<FieldDeclarationSyntax>()
				.Where(f => f.AttributeLists.SelectMany(al => al.Attributes)
					.Any(a => a.Name.ToString() == "FixedArray")).ToList();
			if (context.SemanticModel.GetDeclaredSymbol(sds) is not INamedTypeSymbol structType)
				return;

			var structObj = new Struct {
				Name = structType.Name,
				Namespace = structType.ContainingNamespace.ToDisplayString(_format),
				ArrayNamespace = $"FFXIVClientStructs.Arrays.{structType.Name}Arrays",
				Arrays = new List<FixedArray>()
			};
			
			foreach (var f in fields) {
				var attributes = f.AttributeLists.SelectMany(al => al.Attributes).ToList();
				if (attributes.FirstOrDefault(a => a.Name.ToString() == "FixedArray") is not { } arrayAttr)
					continue;

				if (attributes.FirstOrDefault(a => a.Name.ToString() == "FieldOffset") is not { } offsetAttr)
					continue;

				var name = f.Declaration.Variables.FirstOrDefault()?.Identifier.ValueText;
				if (string.IsNullOrWhiteSpace(name)) continue;

				var offset = GetIntArgument(ref context, offsetAttr, 0);
				if (offset < 0) continue;

				var type = GetTypeOfArgument(ref context, arrayAttr, 0);
				if (type == null) continue;
				
				var length = GetIntArgument(ref context, arrayAttr, 1);
				if (length < 0) continue;

				var arrObj = new FixedArray {
					Name = FixArrayName(name),
					Length = $"{length}",
					Type = type.ToDisplayString(),
					Offset = $"0x{offset:X2}"
				};

				structObj.Arrays.Add(arrObj);
			}

			if (structObj.Arrays.Count > 0)
				Structs.Add(structObj);
		}

		private static string FixArrayName(string name) {
			if (name.StartsWith("_") && name.Length > 1)
				return $"{char.ToUpper(name[1])}{name.Substring(2)}";
			if (name.Contains("Array"))
				return name.Replace("Array", "List");
			if (name.Contains("List"))
				return name.Replace("List", "Array");
			return $"{name}Array";
		}

		private static ITypeSymbol GetTypeOfArgument(ref GeneratorSyntaxContext context, AttributeSyntax attr, int index) {
			if (attr.ArgumentList?.Arguments[index].Expression is not TypeOfExpressionSyntax typeEx)
				return null;
			return context.SemanticModel.GetTypeInfo(typeEx.Type).Type;
		}

		private static int GetIntArgument(ref GeneratorSyntaxContext context, AttributeSyntax attr, int index) {
			if (attr.ArgumentList?.Arguments[index].Expression is not LiteralExpressionSyntax litEx)
				return -1;
			if (context.SemanticModel.GetConstantValue(litEx).Value is int intValue)
				return intValue;
			return -1;
		}
	}
}