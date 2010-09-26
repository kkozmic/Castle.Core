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
	using System.Collections.Generic;

	using Castle.DynamicProxy.Tests.Interceptors;
	using Castle.DynamicProxy.Tests.Interfaces;

	using NUnit.Framework;

	[TestFixture]
	public class GenericInheritanceConstraintsTestCase : BasePEVerifyTestCase
	{
		private IInterceptor noop;

		[Test]
		public void Can_proxy_double_generic_type_where_TOne_is_Generic_closed_over_TTwo()
		{
			var proxy = generator.CreateInterfaceProxyWithoutTarget<IGenericTOneIsIEnumerableTTwo<Empty[], IEmpty>>(noop);

			proxy.Execute();
		}

		[Test]
		public void Can_proxy_double_generic_type_where_TOne_is_TTwo()
		{
			var proxy = generator.CreateInterfaceProxyWithoutTarget<IGenericTOneIsTTwo<Empty, IEmpty>>(noop);

			proxy.Execute();
		}

		[Test]
		public void Can_proxy_generic_type_with_generic_closed_over_simple_interface_constraint()
		{
			var proxy = generator.CreateInterfaceProxyWithoutTarget<IGenericTIsIEnumerableOfIEmpty<IList<Empty>>>(noop);

			proxy.Execute();
		}

		[Test]
		public void Can_proxy_generic_type_with_simple_class_and_interface_constraint()
		{
			var proxy = generator.CreateInterfaceProxyWithoutTarget<IGenericTIsOneAndITwo<SubOneTwo>>(noop);

			proxy.Execute();
		}

		[Test]
		public void Can_proxy_generic_type_with_simple_class_constraint()
		{
			var proxy = generator.CreateInterfaceProxyWithoutTarget<IGenericTIsEmpty<Empty>>(noop);

			proxy.Execute();
		}

		[Test]
		public void Can_proxy_generic_type_with_simple_interface_constraint()
		{
			var proxy = generator.CreateInterfaceProxyWithoutTarget<IGenericTIsIEmpty<Empty>>(noop);

			proxy.Execute();
		}

		[SetUp]
		public void SetUp()
		{
			noop = new DoNothingInterceptor();
		}
	}
}