using System;
using System.Collections.Generic;
using System.Linq;

using Mono.Cecil;
using Mono.Cecil.Cil;

using System.Maui.Xaml;
using System.Maui.Build.Tasks;

namespace System.Maui.Core.XamlC
{
	class ColorTypeConverter : ICompiledTypeConverter
	{
		public IEnumerable<Instruction> ConvertFromString(string value, ILContext context, BaseNode node)
		{
			var module = context.Body.Method.Module;

			do {
				if (string.IsNullOrEmpty(value))
					break;

				value = value.Trim();

				if (value.StartsWith("#", StringComparison.Ordinal)) {
					var color = Color.FromHex(value);
					yield return Instruction.Create(OpCodes.Ldc_R8, color.R);
					yield return Instruction.Create(OpCodes.Ldc_R8, color.G);
					yield return Instruction.Create(OpCodes.Ldc_R8, color.B);
					yield return Instruction.Create(OpCodes.Ldc_R8, color.A);

					yield return Instruction.Create(OpCodes.Newobj, module.ImportCtorReference(("System.Maui.Core", "System.Maui", "Color"), parameterTypes: new[] {
						("mscorlib", "System", "Double"),
						("mscorlib", "System", "Double"),
						("mscorlib", "System", "Double"),
						("mscorlib", "System", "Double")}));
					yield break;
				}
				var parts = value.Split('.');
				if (parts.Length == 1 || (parts.Length == 2 && parts [0] == "Color")) {
					var color = parts [parts.Length - 1];
					if (color == "lightgrey")
						color = "lightgray";
					var fieldReference = module.ImportFieldReference(("System.Maui.Core", "System.Maui", "Color"),
																	 color,
																	 isStatic: true,
																	 caseSensitive: false);
					if (fieldReference != null) {
						yield return Instruction.Create(OpCodes.Ldsfld, fieldReference);
						yield break;
					}
					var propertyGetterReference = module.ImportPropertyGetterReference(("System.Maui.Core", "System.Maui", "Color"),
																					   color,
																					   isStatic: true,
																					   caseSensitive: false);
					if (propertyGetterReference != null) {
						yield return Instruction.Create(OpCodes.Call, propertyGetterReference);
						yield break;
					}
				}
			} while (false);

			throw new XamlParseException($"Cannot convert \"{value}\" into {typeof(Color)}", node);
		}
	}
}