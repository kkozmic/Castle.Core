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
	using System.Linq;

	using Castle.DynamicProxy.Tests.GenInterfaces;
	using Castle.DynamicProxy.Tests.Interceptors;

	using NUnit.Framework;

	[TestFixture]
	public class GenericInvocationsTestCase : BasePEVerifyTestCase
	{
		[Test]
		public void GenericType_non_generic_method()
		{
			IInvocation invocation = null;
			var proxy =
				generator.CreateInterfaceProxyWithoutTarget<IGenericWithMethod<int>>(new WithCallbackInterceptor(i => invocation = i));
			proxy.DoStuff();

			Assert.IsNotNull(invocation);
		}

		[Test]
		public void GenericType_generic_method_with_generic_parameter()
		{
			IInvocation invocation = null;
			var proxy =
				generator.CreateInterfaceProxyWithoutTarget<IGenericWithGenericMethod<int>>(new WithCallbackInterceptor(i => invocation = i));
			proxy.DoStuff(4);

			Assert.IsNotNull(invocation);
			Assert.IsTrue(invocation.Method.GetParameters().Single().ParameterType.IsGenericParameter);
			Assert.AreEqual(typeof(int), invocation.GetConcreteMethod().GetParameters().Single().ParameterType);
		}
	}
}