// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Castle.DynamicProxy.Internal
{
	using System;
	using System.Reflection;

	internal static class Check
	{
		public static Type CheckOpenIfGeneric(this Type target, string name)
		{
			if(target.IsGenericType == false)
			{
				return target;
			}
			if (target.IsGenericTypeDefinition)
			{
				return target;
			}

			throw new ArgumentException(
				string.Format("Type '{0}' isn't an open generic type. Only open generics and non-generics are valid at this point.", target),
				name);
		}

		public static MethodInfo CheckOpenIfGeneric(this MethodInfo target, string name)
		{
			if (target == null)
			{
				return target;
			}
			try
			{
				CheckOpenIfGeneric(target.DeclaringType, "DeclaringType");
				return target;
			}
			catch (ArgumentException e)
			{
				throw new ArgumentException(
					string.Format(
						"Method '{0}' is not defined in an open generic type. Only methods from open generics or non-generics are valid at this point.",
						target),
					name,
					e);
			}
		}
	}
}