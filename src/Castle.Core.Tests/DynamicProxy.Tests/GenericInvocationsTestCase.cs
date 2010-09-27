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

	using Castle.DynamicProxy.Tests.Interceptors;
	using Castle.DynamicProxy.Tests.Interfaces;

	using NUnit.Framework;

	[TestFixture]
	public class GenericInvocationsTestCase : BasePEVerifyTestCase
	{
		private CaptureInvocationInterceptor interceptor;

		[Test]
		public void Invocation_Method_DeclaringType_is_closed_when_non_generic_method_using_T_on_generic_proxy()
		{
			var proxy = generator.CreateInterfaceProxyWithoutTarget<IGenericWithMethodUsingT<int>>(interceptor);

			proxy.Execute(4);

			Assert.IsNotNull(interceptor.Invocation);
			var method = interceptor.Invocation.Method;
			Assert.IsFalse(method.DeclaringType.IsGenericTypeDefinition);
			Assert.IsFalse(method.GetParameters().Single().ParameterType.IsGenericParameter);
			Assert.AreEqual(typeof(int), method.GetParameters().Single().ParameterType);
		}

		[Test]
		public void Invocation_Method_DeclaringType_is_closed_when_non_generic_simple_method_on_generic_proxy()
		{
			var proxy = generator.CreateInterfaceProxyWithoutTarget<IGenericWithMethod<int>>(interceptor);

			proxy.Execute();

			Assert.IsNotNull(interceptor.Invocation);
			Assert.IsFalse(interceptor.Invocation.Method.DeclaringType.IsGenericTypeDefinition);
		}

		[SetUp]
		public void SetUp()
		{
			interceptor = new CaptureInvocationInterceptor();
		}
	}
}