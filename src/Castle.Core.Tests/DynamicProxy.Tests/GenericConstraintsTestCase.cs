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
	using Castle.DynamicProxy.Tests.GenericInterfaces;
	using Castle.DynamicProxy.Tests.Interceptors;

	using NUnit.Framework;

	[TestFixture]
	public class GenericConstraintsTestCase : BasePEVerifyTestCase
	{
		[Test]
		public void Can_proxy_generic_type_with_class_and_new_and_covariant_constraint()
		{
			generator.CreateInterfaceProxyWithoutTarget<IGenericTIsClassAndNewAndCovariant<object>>();
		}

		[Test]
		public void Can_proxy_generic_type_with_class_and_new_constraint()
		{
			generator.CreateInterfaceProxyWithoutTarget<IGenericTIsClassAndNew<object>>();
		}

		[Test]
		public void Can_proxy_generic_type_with_class_constraint()
		{
			generator.CreateInterfaceProxyWithoutTarget<IGenericTIsClass<string>>();
		}

		[Test]
		public void Can_proxy_generic_type_with_contravariant_constraint()
		{
			generator.CreateInterfaceProxyWithoutTarget<IGenericTIsContravariant<object>>();
		}

		[Test]
		public void Can_proxy_generic_type_with_covariant_constraint()
		{
			generator.CreateInterfaceProxyWithoutTarget<IGenericTIsCovariant<object>>();
		}

		[Test]
		public void Can_proxy_generic_type_with_new_constraint()
		{
			generator.CreateInterfaceProxyWithoutTarget<IGenericTIsNew<int>>();
		}

		[Test]
		public void Can_proxy_generic_type_with_struct_constraint()
		{
			generator.CreateInterfaceProxyWithoutTarget<IGenericTIsStruct<int>>();
		}

		[Test]
		public void Can_proxy_non_generic_type_with_generic_method_with_class_andnew_constraint()
		{
			var proxy =
				generator.CreateInterfaceProxyWithoutTarget<IHaveGenericMethodWhereTIsClassAndNew>(new DoNothingInterceptor());

			proxy.Execute<object>();
		}

		[Test]
		public void Can_proxy_non_generic_type_with_generic_method_with_class_constraint()
		{
			var proxy = generator.CreateInterfaceProxyWithoutTarget<IHaveGenericMethodWhereTIsClass>(new DoNothingInterceptor());

			proxy.Execute<string>();
		}

		[Test]
		public void Can_proxy_non_generic_type_with_generic_method_with_new_constraint()
		{
			var proxy = generator.CreateInterfaceProxyWithoutTarget<IHaveGenericMethodWhereTIsNew>(new DoNothingInterceptor());

			proxy.Execute<int>();
			proxy.Execute<object>();
		}

		[Test]
		public void Can_proxy_non_generic_type_with_generic_method_with_struct_constraint()
		{
			var proxy = generator.CreateInterfaceProxyWithoutTarget<IHaveGenericMethodWhereTIsStruct>(new DoNothingInterceptor());

			proxy.Execute<int>();
		}
	}
}