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

namespace Castle.DynamicProxy.Tests
{
	using Castle.DynamicProxy.Tests.GenClasses;
	using Castle.DynamicProxy.Tests.GenInterfaces;
	using Castle.DynamicProxy.Tests.InterClasses;
	using Castle.DynamicProxy.Tests.Interceptors;
	using Castle.DynamicProxy.Tests.Interfaces;

	using NUnit.Framework;

	[TestFixture]
	public class GenericMethodsProxyTestCase : BasePEVerifyTestCase
	{
		[Test]
		public void GenericMethod_WithArrayOfGenericOfGenericArgument()
		{
			var proxy = generator.CreateClassProxy<ClassWithMethodWithArrayOfListOfT>();
			proxy.GenericMethodWithListArrayArgument<string>(null);
		}

		[Test]
		public void Can_proxy_non_generic_type_with_double_generic_method_where_one_generic_arg_constraints_the_other()
		{
			var interceptor = new KeepDataInterceptor();
			var proxy = generator.CreateInterfaceProxyWithoutTarget<IHaveGenericMethodWhereTOneIsTTwo>(
				interceptor);

			proxy.Execute<object, string>();

			var expectedMethod = typeof(IHaveGenericMethodWhereTOneIsTTwo)
				.GetMethod("RegisterType")
				.MakeGenericMethod(typeof(object), typeof(string));

			Assert.AreEqual(expectedMethod, interceptor.Invocation.Method);
		}

		[Test]
		public void Can_proxy_generic_type_with_generic_method_where_TMethod_is_TType()
		{
			IInvocation invocation = null;
			var expectedMethod = typeof(IGenericWithGenericMethodWhereTMethodIsTType<object>)
				.GetMethod("Execute").
				MakeGenericMethod(typeof(string));

			var proxy =
				generator.CreateInterfaceProxyWithoutTarget<IGenericWithGenericMethodWhereTMethodIsTType<object>>(
					new WithCallbackInterceptor(i => invocation = i));

			proxy.Execute<string>();
			
			Assert.IsNotNull(invocation);
			Assert.AreEqual(expectedMethod, invocation.GetConcreteMethod());
		}

		[Test]
		public void GenericMethod_WithGenericOfGenericArgument()
		{
			var proxy = generator.CreateClassProxy<ClassWithMethodWithGenericOfGenericOfT>();
			proxy.GenericMethodWithGenericOfGenericArgument<string>(null);
		}

		[Test]
		public void ProxyAdditionalInterfaceWithGenericMethods()
		{
			var proxy = (IService)generator.CreateInterfaceProxyWithoutTarget(
				typeof(IService), new[] { typeof(OnlyGenMethodsInterface) },
				new StandardInterceptor());

			Assert.IsNotNull(proxy);
		}

		[Test]
		public void ProxyInterfaceWithGenericMethodWithTwoGenericParametersWhereOneIsBaseToAnother()
		{
			generator.CreateInterfaceProxyWithoutTarget<GenericMethodWhereOneGenParamInheritsTheOther>();
		}
	}
}