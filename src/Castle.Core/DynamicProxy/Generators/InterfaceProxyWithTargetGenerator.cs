// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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

namespace Castle.DynamicProxy.Generators
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Reflection;
	using System.Xml.Serialization;

	using Castle.Core.Internal;
	using Castle.DynamicProxy.Contributors;
	using Castle.DynamicProxy.Generators.Emitters;
	using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
	using Castle.DynamicProxy.Serialization;

	public class InterfaceProxyWithTargetGenerator : BaseProxyGenerator
	{
		private readonly Type[] genericArguments;
		private readonly Type openInterface;
		private readonly Type proxyTargetType;
		protected readonly Type[] additionalInterfacesToProxy;
		protected FieldReference targetField;

		public InterfaceProxyWithTargetGenerator(ModuleScope scope, Type @interface, Type proxyTargetType,
		                                         Type[] additionalInterfacesToProxy,
		                                         ProxyGenerationOptions proxyGenerationOptions)
			: base(scope, @interface, proxyGenerationOptions)
		{

			this.proxyTargetType = proxyTargetType;
			this.additionalInterfacesToProxy = additionalInterfacesToProxy;
			// TODO: this should be refactored 
			AdjustTargetTypes(ref targetType, ref this.proxyTargetType);
			if (targetType.IsGenericTypeDefinition)
			{
				genericArguments = @interface.GetGenericArguments();
				openInterface = @interface.GetGenericTypeDefinition();
			}
		}

		protected virtual void AdjustTargetTypes(ref Type proxyTargetInterface, ref Type targetFieldType)
		{
			proxyTargetInterface = GetTargetType(proxyTargetInterface, additionalInterfacesToProxy ?? Type.EmptyTypes,
												 ProxyGenerationOptions);
			targetFieldType = proxyTargetInterface;
		}

		protected virtual bool AllowChangeTarget
		{
			get { return false; }
		}

		protected virtual string GeneratorType
		{
			get { return ProxyTypeConstants.InterfaceWithTarget; }
		}

		public override Type GetProxyType()
		{
			return GenerateCode(proxyTargetType, additionalInterfacesToProxy, ProxyGenerationOptions);
		}

		protected Type GenerateCode(Type proxyTargetType, Type[] interfaces, ProxyGenerationOptions options)
		{
			// make sure ProxyGenerationOptions is initialized
			options.Initialize();
			CheckNotGenericTypeDefinitions(interfaces, "interfaces");
			EnsureValidBaseType(options.BaseTypeForInterfaceProxy);

			interfaces = TypeUtil.GetAllInterfaces(interfaces).ToArray();
			var cacheKey = new CacheKey(proxyTargetType, targetType, interfaces, options, AllowChangeTarget);

			return ObtainProxyType(cacheKey, (n, s) => GenerateType(n, proxyTargetType, interfaces, s));
		}

		protected override ClassEmitter BuildClassEmitter(string typeName, Type baseType, Type[] interfaces)
		{
			var emitter = base.BuildClassEmitter(typeName, baseType, interfaces);
			if (openInterface != null)
			{
				emitter.CopyGenericParametersFromType(openInterface);
			}

			return emitter;
		}

		protected override Type ObtainProxyType(CacheKey cacheKey, Func<string, INamingScope, Type> factory)
		{
			var type = base.ObtainProxyType(cacheKey, factory);
			Debug.Assert(type.IsGenericType == (genericArguments != null));
			if (genericArguments != null)
			{
				var proxyType = type.MakeGenericType(genericArguments);
				InitializeStaticFields(proxyType);
				return proxyType;
			}
			return type;
		}

		protected virtual ITypeContributor AddMappingForTargetType(IDictionary<Type, ITypeContributor> typeImplementerMapping,
		                                                           Type proxyTargetType, ICollection<Type> targetInterfaces,
		                                                           ICollection<Type> additionalInterfaces,
		                                                           INamingScope namingScope)
		{
			var contributor = new InterfaceProxyWithTargetInterfaceTargetContributor(proxyTargetType, AllowChangeTarget, namingScope)
				                  {Logger = Logger};
			var proxiedInterfaces = targetType.GetAllInterfaces();
			foreach (var @interface in proxiedInterfaces)
			{
				contributor.AddInterfaceToProxy(@interface);
				AddMappingNoCheck(@interface, contributor, typeImplementerMapping);
			}

			foreach (var @interface in additionalInterfaces)
			{
				if (!ImplementedByTarget(targetInterfaces, @interface) || proxiedInterfaces.Contains(@interface))
				{
					continue;
				}

				contributor.AddInterfaceToProxy(@interface);
				AddMappingNoCheck(@interface, contributor, typeImplementerMapping);
			}
			return contributor;
		}

#if (!SILVERLIGHT)

		protected override void CreateTypeAttributes(ClassEmitter emitter)
		{
			base.CreateTypeAttributes(emitter);
			emitter.DefineCustomAttribute<SerializableAttribute>();
		}
#endif

		protected virtual Type GenerateType(string typeName, Type proxyTargetType, Type[] interfaces, INamingScope namingScope)
		{
			ITypeContributor[] contributors;
			var allInterfaces = GetTypeImplementerMapping(interfaces, proxyTargetType, out contributors, namingScope);

			ClassEmitter emitter;
			FieldReference interceptorsField;
			var baseType = Init(typeName, out emitter, proxyTargetType, out interceptorsField, allInterfaces);

			var model = new MetaType();
			// Collect methods
			foreach (var contributor in contributors)
			{
				contributor.CollectElementsToProxy(ProxyGenerationOptions.Hook, model);
			}

			ProxyGenerationOptions.Hook.MethodsInspected();

			// Constructor

			var cctor = GenerateStaticConstructor(emitter);
			var ctorArguments = new List<FieldReference>();

			foreach (var contributor in contributors)
			{
				contributor.Generate(emitter, ProxyGenerationOptions);

				// TODO: redo it
				if (contributor is MixinContributor)
				{
					ctorArguments.AddRange((contributor as MixinContributor).Fields);
				}
			}

			ctorArguments.Add(interceptorsField);
			ctorArguments.Add(targetField);
			var selector = emitter.GetField("__selector");
			if (selector != null)
			{
				ctorArguments.Add(selector);
			}

			GenerateConstructors(emitter, baseType, ctorArguments.ToArray());

			// Complete type initializer code body
			CompleteInitCacheMethod(cctor.CodeBuilder);

			// Crosses fingers and build type
			var generatedType = emitter.BuildType();

			InitializeStaticFields(generatedType);
			return generatedType;
		}

		protected virtual InterfaceProxyWithoutTargetContributor GetContributorForAdditionalInterfaces(
			INamingScope namingScope)
		{
			return new InterfaceProxyWithOptionalTargetContributor(namingScope, GetTargetExpression, GetTarget) { Logger = Logger };
		}

		private Reference GetTarget(ClassEmitter @class, MethodInfo method)
		{
			if (method.DeclaringType.IsAssignableFrom(proxyTargetType))
			{
				return @class.GetField("target");
			}
			return new AsTypeReference(@class.GetField("__target"), method.DeclaringType);
		}

		private Expression GetTargetExpression(ClassEmitter @class, MethodInfo method)
		{
			return GetTarget(@class, method).ToExpression();
		}

		protected virtual Type[] GetTypeImplementerMapping(Type[] interfaces, Type proxyTargetType,
		                                                   out ITypeContributor[] contributors, INamingScope namingScope)
		{
			IDictionary<Type, ITypeContributor> typeImplementerMapping = new Dictionary<Type, ITypeContributor>();
			var mixins = new MixinContributor(namingScope, AllowChangeTarget) {Logger = Logger};
			// Order of interface precedence:
			// 1. first target
			var targetInterfaces = proxyTargetType.GetAllInterfaces();
			var additionalInterfaces = TypeUtil.GetAllInterfaces(interfaces);
			var target = AddMappingForTargetType(typeImplementerMapping, proxyTargetType, targetInterfaces, additionalInterfaces,
			                                     namingScope);

			// 2. then mixins
			if (ProxyGenerationOptions.HasMixins)
			{
				foreach (var mixinInterface in ProxyGenerationOptions.MixinData.MixinInterfaces)
				{
					if (targetInterfaces.Contains(mixinInterface))
					{
						// OK, so the target implements this interface. We now do one of two things:
						if (additionalInterfaces.Contains(mixinInterface))
						{
							// we intercept the interface, and forward calls to the target type
							AddMapping(mixinInterface, target, typeImplementerMapping);
						}
						// we do not intercept the interface
						mixins.AddEmptyInterface(mixinInterface);
					}
					else
					{
						if (!typeImplementerMapping.ContainsKey(mixinInterface))
						{
							mixins.AddInterfaceToProxy(mixinInterface);
							typeImplementerMapping.Add(mixinInterface, mixins);
						}
					}
				}
			}

			var additionalInterfacesContributor = GetContributorForAdditionalInterfaces(namingScope);
			// 3. then additional interfaces
			foreach (var @interface in additionalInterfaces)
			{
				if (typeImplementerMapping.ContainsKey(@interface))
				{
					continue;
				}
				if (ProxyGenerationOptions.MixinData.ContainsMixin(@interface))
				{
					continue;
				}

				additionalInterfacesContributor.AddInterfaceToProxy(@interface);
				AddMappingNoCheck(@interface, additionalInterfacesContributor, typeImplementerMapping);
			}

			// 4. plus special interfaces
			var instance = new InterfaceProxyInstanceContributor(targetType, GeneratorType, interfaces);
			AddMappingForISerializable(typeImplementerMapping, instance);
			try
			{
				AddMappingNoCheck(typeof (IProxyTargetAccessor), instance, typeImplementerMapping);
			}
			catch (ArgumentException)
			{
				HandleExplicitlyPassedProxyTargetAccessor(targetInterfaces, additionalInterfaces);
			}

			contributors = new[]
				               {
					               target,
					               additionalInterfacesContributor,
					               mixins,
					               instance
				               };
			return typeImplementerMapping.Keys.ToArray();
		}

		protected virtual Type Init(string typeName, out ClassEmitter emitter, Type proxyTargetType,
		                            out FieldReference interceptorsField, Type[] interfaces)
		{
			var baseType = ProxyGenerationOptions.BaseTypeForInterfaceProxy;

			emitter = BuildClassEmitter(typeName, baseType, interfaces);

			CreateFields(emitter, proxyTargetType);
			CreateTypeAttributes(emitter);

			interceptorsField = emitter.GetField("__interceptors");
			return baseType;
		}

		private void CreateFields(ClassEmitter emitter, Type proxyTargetType)
		{
			base.CreateFields(emitter);
			CreateTargetField(emitter, proxyTargetType);
		}

		protected virtual void CreateTargetField(ClassEmitter emitter, Type proxyTargetType)
		{
			targetField = emitter.CreateField("__target", proxyTargetType);

			emitter.DefineCustomAttributeFor<XmlIgnoreAttribute>(targetField);
		}

		private void EnsureValidBaseType(Type type)
		{
			if (type == null)
			{
				throw new ArgumentException(
					"Base type for proxy is null reference. Please set it to System.Object or some other valid type.");
			}

			if (!type.IsClass)
			{
				ThrowInvalidBaseType(type, "it is not a class type");
			}

			if (type.IsSealed)
			{
				ThrowInvalidBaseType(type, "it is sealed");
			}

			var constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
			                                      null, Type.EmptyTypes, null);

			if (constructor == null || constructor.IsPrivate)
			{
				ThrowInvalidBaseType(type, "it does not have accessible parameterless constructor");
			}
		}

		private bool ImplementedByTarget(ICollection<Type> targetInterfaces, Type @interface)
		{
			return targetInterfaces.Contains(@interface);
		}

		private void ThrowInvalidBaseType(Type type, string doesNotHaveAccessibleParameterlessConstructor)
		{
			var format =
				"Type {0} is not valid base type for interface proxy, because {1}. Only a non-sealed class with non-private default constructor can be used as base type for interface proxy. Please use some other valid type.";
			throw new ArgumentException(string.Format(format, type, doesNotHaveAccessibleParameterlessConstructor));
		}

		protected static Type GetTargetType(Type @interface, Type[] additionalInterfaces, ProxyGenerationOptions options)
		{
			options.Initialize();
			if (@interface.IsGenericType && additionalInterfaces.None(i => i.IsGenericType) &&
				options.MixinData.MixinInterfaces.None(m => m.IsGenericType))
			{
				return @interface.GetGenericTypeDefinition();
			}
			return @interface;
		}
	}
}