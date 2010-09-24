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
	using Castle.DynamicProxy.Tests.GenInterfaces;
	using Castle.DynamicProxy.Tests.Interceptors;

	using NUnit.Framework;

	[TestFixture]
	public class GenericProxiesTestCase : BasePEVerifyTestCase
	{
		[Test]
		public void Proxy_for_doublegeneric_type_is_generic()
		{
			var proxy = generator.CreateInterfaceProxyWithoutTarget<IEmptyGeneric<int, string>>();
			Assert.IsTrue(proxy.GetType().IsGenericType);
		}

		[Test]
		public void Proxy_for_doublegeneric_type_with_method_generates_correctly()
		{
			var proxy = generator.CreateInterfaceProxyWithoutTarget<IGenericWithMethod<int, string>>();
			Assert.IsTrue(proxy.GetType().IsGenericType);
		}

		[Test]
		public void Proxy_for_doublegeneric_type_with_method_works_correctly()
		{
			var proxy = generator.CreateInterfaceProxyWithoutTarget<IGenericWithMethod<int, string>>(new DoNothingInterceptor());
			proxy.DoStuff();
		}

		[Test]
		public void Proxy_for_generic_type_is_generic()
		{
			var proxy = generator.CreateInterfaceProxyWithoutTarget<IEmptyGeneric<int>>();
			Assert.IsTrue(proxy.GetType().IsGenericType);
		}

		[Test]
		public void Proxy_for_generic_type_with_method_generates_correctly()
		{
			var proxy = generator.CreateInterfaceProxyWithoutTarget<IGenericWithMethod<int>>();
			Assert.IsTrue(proxy.GetType().IsGenericType);
		}

		[Test]
		public void Proxy_for_generic_type_with_method_with_generic_parameter_generates_correctly()
		{
			var proxy = generator.CreateInterfaceProxyWithoutTarget<IGenericWithMethodWithGenericParameter<int>>();
			Assert.IsTrue(proxy.GetType().IsGenericType);
		}

		[Test]
		public void Proxy_for_generic_type_with_method_with_generic_parameter_works_correctly()
		{
			var proxy =
				generator.CreateInterfaceProxyWithoutTarget<IGenericWithMethodWithGenericParameter<int>>(new DoNothingInterceptor());
			proxy.DoStuff(2);
		}

		[Test]
		public void Proxy_for_generic_type_with_method_works_correctly()
		{
			var proxy = generator.CreateInterfaceProxyWithoutTarget<IGenericWithMethod<int>>(new DoNothingInterceptor());
			proxy.DoStuff();
		}

		[Test]
		public void Proxy_for_inherited_closed_generic_type_is_NOT_generic()
		{
			var proxy = generator.CreateInterfaceProxyWithoutTarget<IEmptyGenericOfInt>();
			Assert.IsFalse(proxy.GetType().IsGenericType);
		}

		[Test]
		public void Proxy_for_inherited_semi_closed_double_generic_type_is_single_generic()
		{
			var proxy = generator.CreateInterfaceProxyWithoutTarget<IEmptyGenericOfInt<string>>();
			Assert.IsTrue(proxy.GetType().IsGenericType);
			Assert.AreEqual(1, proxy.GetType().GetGenericArguments().Length);
		}
	}
}