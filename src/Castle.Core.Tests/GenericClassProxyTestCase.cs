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
	using System;
	using System.Collections.Generic;

	using Castle.DynamicProxy.Tests.GenClasses;
	using Castle.DynamicProxy.Tests.Interceptors;

	using NUnit.Framework;

	[TestFixture]
	public class GenericClassProxyTestCase : BasePEVerifyTestCase
	{
		private LogInvocationInterceptor logger;

		public override void Init()
		{
			base.Init();
			logger = new LogInvocationInterceptor();
		}

		[Test]
		public void ProxyWithGenericArgument()
		{
			var proxy = generator.CreateClassProxy<ClassWithGenArgs<int>>(logger);

			Assert.IsNotNull(proxy);

			proxy.DoSomething();

			Assert.IsTrue(proxy.Invoked);

			proxy.AProperty = true;
			Assert.IsTrue(proxy.AProperty);

			Assert.AreEqual("DoSomething set_AProperty get_AProperty ", logger.LogContents);
		}

		[Test]
		public void ProxyWithGenericArguments()
		{
			var proxy = generator.CreateClassProxy<ClassWithGenArgs<int, string>>(logger);

			Assert.IsNotNull(proxy);

			proxy.DoSomething();

			Assert.IsTrue(proxy.Invoked);

			proxy.AProperty = true;
			Assert.IsTrue(proxy.AProperty);

			Assert.AreEqual("DoSomething set_AProperty get_AProperty ", logger.LogContents);
		}

		[Test]
		public void ProxyWithGenericArgumentsWithBaseGenericClass()
		{
			var proxy =
				generator.CreateClassProxy<SubClassWithGenArgs<int, string, int>>(logger);

			Assert.IsNotNull(proxy);

			proxy.DoSomething();

			Assert.IsTrue(proxy.Invoked);

			proxy.AProperty = true;
			Assert.IsTrue(proxy.AProperty);

			Assert.AreEqual("DoSomething set_AProperty get_AProperty ", logger.LogContents);
		}

		[Test]
		public void ProxyWithGenericArgumentsAndArgumentConstraints()
		{
			var proxy = generator.CreateClassProxy<GenClassWithConstraints<int>>(logger);

			Assert.IsNotNull(proxy);

			proxy.DoSomething();

			Assert.IsTrue(proxy.Invoked);

			Assert.AreEqual("DoSomething ", logger.LogContents);
		}

		[Test]
		public void GenericProxyWithIndexer()
		{
			object proxy = generator.CreateClassProxy<ClassWithIndexer<string, int>>(logger);

			Assert.IsNotNull(proxy);

			var type = (ClassWithIndexer<string, int>)proxy;

			type["name"] = 10;
			Assert.AreEqual(10, type["name"]);

			Assert.AreEqual("set_Item get_Item ", logger.LogContents);
		}

		[Test]
		public void ProxyWithMethodReturningGenericOfGenericOfT()
		{
			var proxy = generator.CreateClassProxy<ClassWithMethodWithReturnArrayOfListOfT>();
			proxy.GenericMethodReturnsListArray<string>();
			proxy.GenericMethodReturnsGenericOfGenericType<int>();
		}

		[Test]
		public void ProxyWithGenericArgumentsAndMethodGenericArguments()
		{
			var proxy =
				generator.CreateClassProxy<GenClassWithGenMethods<List<object>>>(logger);

			Assert.IsNotNull(proxy);

			proxy.DoSomething("z param");

			Assert.IsTrue(proxy.Invoked);
			Assert.AreEqual("z param", proxy.SavedParam);
			Assert.AreEqual("DoSomething ", logger.LogContents);
		}

		[Test]
		public void ProxyWithGenericArgumentsAndMethodGenericArgumentsWithConstraints()
		{
			var proxy =
				generator.CreateClassProxy<GenClassWithGenMethodsConstrained<List<object>>>(logger);

			Assert.IsNotNull(proxy);

			proxy.DoSomething("z param");

			Assert.IsTrue(proxy.Invoked);
			Assert.AreEqual("z param", proxy.SavedParam);
			Assert.AreEqual("DoSomething ", logger.LogContents);
		}

		[Test]
		public void ProxyWithGenericArgumentsAndMethodGenericArgumentsWithOneNotDefinedOnType()
		{
			var proxy =
				generator.CreateClassProxy<GenClassWithGenMethods<List<object>>>(logger);

			Assert.IsNotNull(proxy);

			var value1 = 10;

			proxy.DoSomethingElse(param1 => param1.ToString(), value1);

			Assert.IsTrue(proxy.Invoked);
			Assert.AreEqual("10", proxy.SavedParam);
			Assert.AreEqual("DoSomethingElse ", logger.LogContents);
		}

		[Test]
		public void ProxyWithGenericArgumentsAndMethodGenericReturn()
		{
			var proxy =
				generator.CreateClassProxy<GenClassWithGenReturn<List<object>, List<object>>>(logger);

			Assert.IsNotNull(proxy);

			object ret1 = proxy.DoSomethingT();
			object ret2 = proxy.DoSomethingZ();

			Assert.IsInstanceOf(typeof(List<object>), ret1);
			Assert.IsInstanceOf(typeof(List<object>), ret2);
			Assert.AreEqual("DoSomethingT DoSomethingZ ", logger.LogContents);
		}

		[Test]
		public void GenericMethodArgumentsAndTypeGenericArgumentsWithSameName()
		{
			var proxy =
				generator.CreateClassProxy<GenClassNameClash<List<object>, List<object>>>(logger);

			Assert.IsNotNull(proxy);

			proxy.DoSomethingT(1);
			proxy.DoSomethingZ(1L);
			proxy.DoSomethingTX(1, "a");
			proxy.DoSomethingZX(1L, "b");

			Assert.AreEqual("DoSomethingT DoSomethingZ DoSomethingTX DoSomethingZX ", logger.LogContents);
		}

		[Test]
		public void ClassWithGenMethodOnly()
		{
			var proxy =
				generator.CreateClassProxy<OnlyGenMethodsClass>(logger);

			Assert.IsNotNull(proxy);

			proxy.DoSomething(new List<object>());

			Assert.IsTrue(proxy.Invoked);
			Assert.AreEqual("DoSomething ", logger.LogContents);
		}

		[Test]
		public void MethodInfoClosedInGenTypeGenMethodRefType()
		{
			var interceptor = new KeepDataInterceptor();
			var proxy = generator.CreateClassProxy<GenClassWithGenMethods<List<object>>>(interceptor);

			proxy.DoSomething(1);
			GenericTestUtility.CheckMethodInfoIsClosed(interceptor.Invocation.GetConcreteMethod(), typeof(List<object>),
			                                           typeof(int));

			proxy.DoSomething(new List<object>());
			GenericTestUtility.CheckMethodInfoIsClosed(interceptor.Invocation.GetConcreteMethod(), typeof(List<object>),
			                                           typeof(List<object>));
		}

		[Test]
		public void MethodInfoClosedInGenTypeGenMethodValueType()
		{
			var interceptor = new KeepDataInterceptor();
			var proxy = generator.CreateClassProxy<GenClassWithGenMethods<int>>(interceptor);

			proxy.DoSomething(1);
			GenericTestUtility.CheckMethodInfoIsClosed(interceptor.Invocation.GetConcreteMethod(), typeof(int), typeof(int));

			proxy.DoSomething(new List<object>());
			GenericTestUtility.CheckMethodInfoIsClosed(interceptor.Invocation.GetConcreteMethod(), typeof(int),
			                                           typeof(List<object>));
		}

		[Test]
		public void MethodInfoClosedInGenTypeNongenMethodRefTypeRefType()
		{
			var interceptor = new KeepDataInterceptor();
			var proxy = generator.CreateClassProxy<GenClassWithGenReturn<List<object>, List<object>>>(interceptor);

			proxy.DoSomethingT();
			GenericTestUtility.CheckMethodInfoIsClosed(interceptor.Invocation.GetConcreteMethod(), typeof(List<object>));

			proxy.DoSomethingZ();
			GenericTestUtility.CheckMethodInfoIsClosed(interceptor.Invocation.GetConcreteMethod(), typeof(List<object>));
		}

		[Test]
		public void MethodInfoClosedInGenTypeNongenMethodValueTypeValueType()
		{
			var interceptor = new KeepDataInterceptor();
			var proxy = generator.CreateClassProxy<GenClassWithGenReturn<int, int>>(interceptor);

			proxy.DoSomethingT();
			GenericTestUtility.CheckMethodInfoIsClosed(interceptor.Invocation.GetConcreteMethod(), typeof(int));
			Assert.AreEqual(interceptor.Invocation.GetConcreteMethod(),
			                interceptor.Invocation.GetConcreteMethodInvocationTarget().GetBaseDefinition());

			proxy.DoSomethingZ();
			GenericTestUtility.CheckMethodInfoIsClosed(interceptor.Invocation.GetConcreteMethod(), typeof(int));
			Assert.AreEqual(interceptor.Invocation.GetConcreteMethod(),
			                interceptor.Invocation.GetConcreteMethodInvocationTarget().GetBaseDefinition());
		}

		[Test]
		public void MethodInfoClosedInGenTypeNongenMethodValueTypeRefType()
		{
			var interceptor = new KeepDataInterceptor();
			var proxy =
				generator.CreateClassProxy<GenClassWithGenReturn<int, List<object>>>(interceptor);

			proxy.DoSomethingT();
			GenericTestUtility.CheckMethodInfoIsClosed(interceptor.Invocation.GetConcreteMethod(), typeof(int));
			Assert.AreEqual(interceptor.Invocation.GetConcreteMethod(),
			                interceptor.Invocation.GetConcreteMethodInvocationTarget().GetBaseDefinition());

			proxy.DoSomethingZ();
			GenericTestUtility.CheckMethodInfoIsClosed(interceptor.Invocation.GetConcreteMethod(), typeof(List<object>));
			Assert.AreEqual(interceptor.Invocation.GetConcreteMethod(),
			                interceptor.Invocation.GetConcreteMethodInvocationTarget().GetBaseDefinition());
		}

		[Test]
		public void MethodInfoClosedInNongenTypeGenMethod()
		{
			var interceptor = new KeepDataInterceptor();
			var proxy = generator.CreateClassProxy<OnlyGenMethodsClass>(interceptor);

			proxy.DoSomething(1);
			GenericTestUtility.CheckMethodInfoIsClosed(interceptor.Invocation.GetConcreteMethod(), typeof(int), typeof(int));

			proxy.DoSomething(new List<object>());
			GenericTestUtility.CheckMethodInfoIsClosed(interceptor.Invocation.GetConcreteMethod(), typeof(List<object>),
			                                           typeof(List<object>));
		}

		[Test]
		public void TypeWithGenericMethodHavingArgumentBeingGenericArrayOfT()
		{
			var proxy = generator.CreateClassProxy<MethodWithArgumentBeingArrayOfGenericTypeOfT>();
			Assert.IsNotNull(proxy);
			proxy.Method(new Action<string>[0]);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void ThrowsWhenProxyingGenericTypeDefNoTarget()
		{
			var interceptor = new KeepDataInterceptor();
			var o = generator.CreateClassProxy(typeof(GenClassWithGenReturn<,>), interceptor);
		}
	}
}