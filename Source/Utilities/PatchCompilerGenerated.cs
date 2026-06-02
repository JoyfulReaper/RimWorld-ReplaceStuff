using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace TD.Utilities
{
	static class PatchCompilerGenerated
	{
		public static void PatchGeneratedMethod(this Harmony harmony, Type masterType, Predicate<MethodInfo> check,
		HarmonyMethod prefix = null, HarmonyMethod postfix = null, HarmonyMethod transpiler = null)
		{
			// Use a proper Stack
			Stack<Type> nestedTypes = new Stack<Type>(masterType.GetNestedTypes(BindingFlags.NonPublic));

			while (nestedTypes.Count > 0)
			{
				Type type = nestedTypes.Pop();

				// Add next level of nesting
				foreach (var nested in type.GetNestedTypes(BindingFlags.NonPublic))
					nestedTypes.Push(nested);

				// Search methods
				foreach (MethodInfo method in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic))
				{
					if (method.DeclaringType != type) continue;

					if (check(method))
					{
						harmony.Patch(method, prefix, postfix, transpiler);
					}
				}
			}
		}
	}
}
