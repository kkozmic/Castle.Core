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

namespace Castle.DynamicProxy.Generators.Emitters
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Reflection;
	using System.Reflection.Emit;

	using Castle.Core;

	[DebuggerDisplay("{TypeBuilder}")]
	public class ClassEmitter : AbstractTypeEmitter
	{
		private readonly ModuleScope moduleScope;
		private const TypeAttributes DefaultAttributes = TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Serializable;

		public ClassEmitter(ModuleScope modulescope, String name, Type baseType, IEnumerable<Type> interfaces, params Type[] genericArguments)
			: this(modulescope, name, baseType, interfaces, DefaultAttributes, ShouldForceUnsigned(), genericArguments)
		{
		}

		public ClassEmitter(ModuleScope modulescope, String name, Type baseType, IEnumerable<Type> interfaces, TypeAttributes flags, params Type[] genericArguments)
			: this(modulescope, name, baseType, interfaces, flags, ShouldForceUnsigned(), genericArguments)
		{
		}

		public ClassEmitter(ModuleScope modulescope, String name, Type baseType, IEnumerable<Type> interfaces, TypeAttributes flags,
		                    bool forceUnsigned, params Type[] genericArguments)
			: this(CreateTypeBuilder(modulescope, name, baseType, interfaces, flags, forceUnsigned))
		{
			interfaces = InitializeGenericArgumentsFromBases(ref baseType, interfaces);

			if (interfaces != null)
			{
				foreach (Type inter in interfaces)
				{
					TypeBuilder.AddInterfaceImplementation(inter);
				}
			}
			var namingScope = modulescope.NamingScope.SafeSubScope();
			SetGenericParameters(baseType, interfaces, namingScope, genericArguments);
			TypeBuilder.SetParent(baseType);
			moduleScope = modulescope;
		}

		private void SetGenericParameters(Type baseType, IEnumerable<Type> interfaces, INamingScope namingScope, Type[] genericArguments)
		{
			if(genericArguments!=null && genericArguments.Length>0)
			{
				DefineGenericParameters(Array.ConvertAll(genericArguments, a => new Pair<Type, string>(a, a.Name)));
				return;
			}
			IList<Pair<Type, string>> cache = new List<Pair<Type, string>>();
			CollectGenericParameters(baseType, namingScope,cache);
			foreach (var @interface in interfaces)
			{
				CollectGenericParameters(@interface, namingScope,cache);
			}
			DefineGenericParameters(cache.ToArray());
		}

		private void DefineGenericParameters(Pair<Type, string>[] types)
		{
			if (types.Length == 0) return;
			var ownGenericParameters = TypeBuilder.DefineGenericParameters(Array.ConvertAll(types, t => t.Second));
			for (int i = 0; i < types.Length; i++)
			{
				var baseParameter = types[i].First;
				var ownParameter = ownGenericParameters[i];
				genericParameters.Add(baseParameter, ownParameter);
			}
			CopyGenericConstraints(genericParameters);
		}

		private void CopyGenericConstraints(Dictionary<Type, GenericTypeParameterBuilder> parameters)
		{
			foreach (var pair in parameters)
			{
				CopyGenericParameterAttributes(pair.Key, pair.Value);
				var types = pair.Key.GetGenericParameterConstraints();
#if SILVERLIGHT
				Type[] interfacesConstraints = Castle.Core.Extensions.SilverlightExtensions.FindAll(types, delegate(Type type) { return type.IsInterface; });

				Type baseClassConstraint = Castle.DynamicProxy.SilverlightExtensions.Extensions.Find(types, delegate(Type type) { return type.IsClass; });
#else
				var interfacesConstraints = Array.FindAll(types, type => type.IsInterface);
				var baseClassConstraint = Array.Find(types, type => type.IsClass);
#endif
				if(baseClassConstraint!=null)
				{
					pair.Value.SetBaseTypeConstraint(baseClassConstraint);
				}
				pair.Value.SetInterfaceConstraints(interfacesConstraints);
			}
		}

		private void CopyGenericParameterAttributes(Type baseParameter, GenericTypeParameterBuilder ownParameter)
		{
			// TODO: this should be based on GenericUtil.CopyGenericConstraints
			var attributes = baseParameter.GenericParameterAttributes;

			attributes = ResetVariance(attributes);
			ownParameter.SetGenericParameterAttributes(attributes);
		}

		private GenericParameterAttributes ResetVariance(GenericParameterAttributes attributes)
		{
			// we're building a class and only interfaces/delegates can have variance parameters
			// so we need to reset it before we set it.
			attributes &= ~GenericParameterAttributes.VarianceMask;
			return attributes;
		}

		private void CollectGenericParameters(Type type, INamingScope namingScope, IList<Pair<Type,string>> cache)
		{
			if (type == null || type.IsGenericTypeDefinition == false) return;
			var arguments = type.GetGenericArguments();
			foreach (var argument in arguments)
			{
				cache.Add(new Pair<Type, string>(argument, namingScope.GetUniqueName(argument.Name)));
			}
		}

		public ModuleScope ModuleScope
		{
			get { return moduleScope; }
		}

		private static TypeBuilder CreateTypeBuilder(ModuleScope modulescope, string name, Type baseType, IEnumerable<Type> interfaces, TypeAttributes flags, bool forceUnsigned)
		{
			bool isAssemblySigned = !forceUnsigned && !StrongNameUtil.IsAnyTypeFromUnsignedAssembly(baseType, interfaces);
			return modulescope.DefineType(isAssemblySigned, name, flags);
		}

		private static bool ShouldForceUnsigned()
		{
			return StrongNameUtil.CanStrongNameAssembly == false;
		}

		public ClassEmitter(TypeBuilder typeBuilder)
			: base(typeBuilder)
		{
		}

		// The ambivalent generic parameter handling of base type and interfaces has been removed from the ClassEmitter, it isn't used by the proxy
		// generators anyway. If a concrete user needs to support generic bases, a subclass can override this method (and not call this base
		// implementation), call CopyGenericParametersFromMethod and replace baseType and interfaces by versions bound to the newly created GenericTypeParams.
		protected virtual IEnumerable<Type> InitializeGenericArgumentsFromBases(ref Type baseType, IEnumerable<Type> interfaces)
		{
			if (baseType != null && baseType.IsGenericTypeDefinition)
			{
				throw new NotSupportedException("ClassEmitter does not support open generic base types. Type: " + baseType.FullName);
			}

			if (interfaces == null)
			{
				return interfaces;
			}

			return interfaces;
		}

		public Type[] GetOverridingGenericArguments(Type[] baseGenericArguments)
		{
			var types = new Type[baseGenericArguments.Length];
			for (int i = 0; i < types.Length; i++)
			{
				try
				{
					types[i] = genericParameters[baseGenericArguments[i]];
				}
				catch (KeyNotFoundException e)
				{
					throw new ArgumentException(string.Format("Could not find generic argument on type {0} corresponding to {1}.",
					                                          TypeBuilder, baseGenericArguments[i]), e);
				}
			}
			return types;
		}
	}
}