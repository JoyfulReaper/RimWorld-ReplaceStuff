using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace TD.Utilities
{
	static class PatchCompilerGenerated
	{
		public static void PatchGeneratedMethod(this Harmony harmony, Type masterType, Predicate<MethodInfo> check,
		HarmonyMethod prefix = null, HarmonyMethod postfix = null, HarmonyMethod transpiler = null)
		{
			Stack<Type> nestedTypes = new Stack<Type>(masterType.GetNestedTypes(BindingFlags.NonPublic));

			while (nestedTypes.Count > 0)
			{
				Type type = nestedTypes.Pop();
				foreach (var nested in type.GetNestedTypes(BindingFlags.NonPublic))
					nestedTypes.Push(nested);

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
