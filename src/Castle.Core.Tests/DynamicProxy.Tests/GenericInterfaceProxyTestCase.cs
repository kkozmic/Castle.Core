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
	using System.Reflection;

	using Castle.DynamicProxy.Generators;
	using Castle.DynamicProxy.Tests.GenInterfaces;
	using Castle.DynamicProxy.Tests.Interceptors;
	using Castle.DynamicProxy.Tests.Interfaces;

	using NUnit.Framework;

	[TestFixture]
	public class GenericInterfaceProxyTestCase : BasePEVerifyTestCase
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
			var proxy =
				generator.CreateInterfaceProxyWithTarget<GenInterface<int>>(
					new GenInterfaceImpl<int>(), logger);

			Assert.IsNotNull(proxy);

			Assert.AreEqual(1, proxy.DoSomething(1));

			Assert.AreEqual("DoSomething ", logger.LogContents);
		}

		[Test]
		public void Proxy_with_method_with_nested_generic_parameter()
		{
			var interceptors = new SetReturnValueInterceptor(null);
			var proxy =
				generator.CreateInterfaceProxyWithoutTarget<GenInterfaceWithMethodWithNestedGenericParameter>(interceptors);
			proxy.Foo<int>(null);
		}

		[Test]
		public void Proxy_with_method_with_nested_generic_parameter_by_ref()
		{
			var interceptors = new SetReturnValueInterceptor(null);
			var proxy =
				generator.CreateInterfaceProxyWithoutTarget<GenInterfaceWithMethodWithNestedGenericParameterByRef>(interceptors);
			IEnumerable<IComparable<int>> param = null;
			proxy.Foo(ref param);
		}

		[Test]
		public void Proxy_with_method_with_nested_generic_return()
		{
			var interceptors = new SetReturnValueInterceptor(null);
			var proxy =
				generator.CreateInterfaceProxyWithoutTarget<GenInterfaceWithMethodWithNestedGenericReturn>(interceptors);
			proxy.Foo<int>();
		}

		[Test]
		public void ProxyWithGenericArgumentAndGenericMethod()
		{
			var proxy =
				generator.CreateInterfaceProxyWithTarget<IGenericWithGenericMethodUsingT<int>>(
					new GenericWithGenericMethodUsingTImpl<int>(), logger);

			Assert.IsNotNull(proxy);

			proxy.Execute(10L, 1);

			Assert.AreEqual("DoSomething ", logger.LogContents);
		}

		[Test]
		public void ProxyWithGenericArgumentAndGenericMethodAndGenericReturn()
		{
			var proxy =
				generator.CreateInterfaceProxyWithTarget<GenInterfaceWithGenMethodsAndGenReturn<int>>(
					new GenInterfaceWithGenMethodsAndGenReturnImpl<int>(), logger);

			Assert.IsNotNull(proxy);

			Assert.AreEqual(10L, proxy.DoSomething(10L, 1));

			Assert.AreEqual("DoSomething ", logger.LogContents);
		}

		[Test]
		public void ProxyWithGenInterfaceWithGenericArrays()
		{
			var proxy =
				generator.CreateInterfaceProxyWithTarget<IGenInterfaceWithGenArray<int>>(
					new GenInterfaceWithGenArray<int>(), logger);

			Assert.IsNotNull(proxy);

			var items = new[] { 1, 2, 3 };
			proxy.CopyTo(items);
			items = proxy.CreateItems();
			Assert.IsNotNull(items);
			Assert.AreEqual(3, items.Length);

			Assert.AreEqual("CopyTo CreateItems ", logger.LogContents);
		}

		[Test]
		public void ProxyWithGenInterfaceWithBase()
		{
			var proxy =
				generator.CreateInterfaceProxyWithTarget<IGenInterfaceHierarchySpecialization<int>>(
					new GenInterfaceHierarchy<int>(), logger);

			Assert.IsNotNull(proxy);

			proxy.Add();
			proxy.Add(1);
			Assert.IsNotNull(proxy.FetchAll());

			Assert.AreEqual("Add Add FetchAll ", logger.LogContents);
		}

		[Test]
		public void ProxyWithGenExplicitImplementation()
		{
			var target = generator.CreateInterfaceProxyWithTarget<InterfaceWithExplicitImpl<int>>(
				new GenExplicitImplementation<int>(), logger);
			var enumerator = target.GetEnum1();
			Assert.IsNotNull(enumerator);
		}

		[Test]
		public void TwoGenericsInterfaceWithoutTarget()
		{
			generator.CreateInterfaceProxyWithoutTarget(typeof(GenInterface<object>),
														new[] { typeof(InterfaceWithExplicitImpl<int>) },
														new LogInvocationInterceptor());
		}

		[Test]
		public void NonGenInterfaceWithParentGenClassImplementingGenInterface()
		{
			generator.CreateInterfaceProxyWithoutTarget(typeof(IUserRepository),
														new[] { typeof(InterfaceWithExplicitImpl<int>) },
														new LogInvocationInterceptor());
		}

		[Test]
		public void WithoutTarget()
		{
			generator.CreateInterfaceProxyWithoutTarget(typeof(InterfaceWithExplicitImpl<int>), new LogInvocationInterceptor());
		}

		[Test]
		public void MethodInfoClosedInGenIfcGenMethodRefTypeNoTarget()
		{
			var interceptor = new KeepDataInterceptor();
			var proxy = generator.CreateInterfaceProxyWithoutTarget<IGenericWithGenericMethodUsingT<List<object>>>(interceptor);

			proxy.Execute(1, null);
			GenericTestUtility.CheckMethodInfoIsClosed(interceptor.Invocation.GetConcreteMethod(), typeof(void), typeof(int),
													   typeof(List<object>));
			Assert.AreEqual(null, interceptor.Invocation.GetConcreteMethodInvocationTarget());

			proxy.Execute(new List<object>(), new List<object>());
			GenericTestUtility.CheckMethodInfoIsClosed(interceptor.Invocation.GetConcreteMethod(), typeof(void),
													   typeof(List<object>), typeof(List<object>));
			Assert.AreEqual(null, interceptor.Invocation.GetConcreteMethodInvocationTarget());
		}

		[Test]
		public void MethodInfoClosedInGenIfcGenMethodValueTypeNoTarget()
		{
			var interceptor = new KeepDataInterceptor();
			var proxy =
				generator.CreateInterfaceProxyWithoutTarget<IGenericWithGenericMethodUsingT<int>>(interceptor);

			proxy.Execute(1, 1);
			GenericTestUtility.CheckMethodInfoIsClosed(interceptor.Invocation.GetConcreteMethod(), typeof(void), typeof(int),
													   typeof(int));

			proxy.Execute(new List<object>(), 1);
			GenericTestUtility.CheckMethodInfoIsClosed(interceptor.Invocation.GetConcreteMethod(), typeof(void),
													   typeof(List<object>), typeof(int));
		}

		[Test]
		public void MethodInfoClosedInGenIfcNongenMethodRefTypeNoTarget()
		{
			var interceptor = new KeepDataInterceptor();
			var proxy =
				generator.CreateInterfaceProxyWithoutTarget<IGenInterfaceHierarchyBase<List<object>>>(interceptor);

			proxy.Get();
			GenericTestUtility.CheckMethodInfoIsClosed(interceptor.Invocation.GetConcreteMethod(), typeof(List<object>));

			proxy.Add(null);
			GenericTestUtility.CheckMethodInfoIsClosed(interceptor.Invocation.GetConcreteMethod(), typeof(void),
													   typeof(List<object>));
		}

		[Test]
		public void MethodInfoClosedInGenIfcNongenMethodValueTypeNoTarget()
		{
			var interceptor = new KeepDataInterceptor();
			var proxy =
				generator.CreateInterfaceProxyWithoutTarget<IGenInterfaceHierarchyBase<int>>(interceptor);

			proxy.Get();
			GenericTestUtility.CheckMethodInfoIsClosed(interceptor.Invocation.GetConcreteMethod(), typeof(int));

			proxy.Add(0);
			GenericTestUtility.CheckMethodInfoIsClosed(interceptor.Invocation.GetConcreteMethod(), typeof(void), typeof(int));
		}

		[Test]
		public void MethodInfoClosedInNongenIfcGenMethodNoTarget()
		{
			var interceptor = new KeepDataInterceptor();
			var proxy = generator.CreateInterfaceProxyWithoutTarget<OnlyGenMethodsInterface>(interceptor);

			proxy.DoSomething(1);
			GenericTestUtility.CheckMethodInfoIsClosed(interceptor.Invocation.GetConcreteMethod(), typeof(int), typeof(int));

			proxy.DoSomething(new List<object>());
			GenericTestUtility.CheckMethodInfoIsClosed(interceptor.Invocation.GetConcreteMethod(), typeof(List<object>),
													   typeof(List<object>));
		}

		[Test]
		public void MethodInfoClosedInGenIfcGenMethodRefTypeWithTarget()
		{
			var interceptor = new KeepDataInterceptor();
			var target = new GenericWithGenericMethodUsingTImpl<List<object>>();
			var proxy =
				generator.CreateInterfaceProxyWithTarget<IGenericWithGenericMethodUsingT<List<object>>>(target, interceptor);

			proxy.Execute(1, null);
			GenericTestUtility.CheckMethodInfoIsClosed(interceptor.Invocation.GetConcreteMethod(), typeof(void), typeof(int),
													   typeof(List<object>));
			GenericTestUtility.CheckMethodInfoIsClosed(interceptor.Invocation.GetConcreteMethodInvocationTarget(), typeof(void),
													   typeof(int), typeof(List<object>));
			Assert.AreNotEqual(interceptor.Invocation.GetConcreteMethod(),
							   interceptor.Invocation.GetConcreteMethodInvocationTarget());

			proxy.Execute(new List<object>(), new List<object>());
			GenericTestUtility.CheckMethodInfoIsClosed(interceptor.Invocation.GetConcreteMethod(), typeof(void),
													   typeof(List<object>), typeof(List<object>));
			GenericTestUtility.CheckMethodInfoIsClosed(interceptor.Invocation.GetConcreteMethodInvocationTarget(), typeof(void),
													   typeof(List<object>), typeof(List<object>));
			Assert.AreNotEqual(interceptor.Invocation.GetConcreteMethod(),
							   interceptor.Invocation.GetConcreteMethodInvocationTarget());
		}

		[Test]
		public void MethodInfoClosedInGenIfcGenMethodValueTypeWithTarget()
		{
			var interceptor = new KeepDataInterceptor();
			IGenericWithGenericMethodUsingT<int> target = new GenericWithGenericMethodUsingTImpl<int>();
			var proxy =
				generator.CreateInterfaceProxyWithTarget(target, interceptor);

			proxy.Execute(1, 1);
			GenericTestUtility.CheckMethodInfoIsClosed(interceptor.Invocation.GetConcreteMethod(), typeof(void), typeof(int),
													   typeof(int));
			GenericTestUtility.CheckMethodInfoIsClosed(interceptor.Invocation.GetConcreteMethodInvocationTarget(), typeof(void),
													   typeof(int), typeof(int));
			Assert.AreNotEqual(interceptor.Invocation.GetConcreteMethod(),
							   interceptor.Invocation.GetConcreteMethodInvocationTarget());

			proxy.Execute(new List<object>(), 1);
			GenericTestUtility.CheckMethodInfoIsClosed(interceptor.Invocation.GetConcreteMethod(), typeof(void),
													   typeof(List<object>), typeof(int));
			GenericTestUtility.CheckMethodInfoIsClosed(interceptor.Invocation.GetConcreteMethodInvocationTarget(), typeof(void),
													   typeof(List<object>), typeof(int));
			Assert.AreNotEqual(interceptor.Invocation.GetConcreteMethod(),
							   interceptor.Invocation.GetConcreteMethodInvocationTarget());
		}

		[Test]
		public void MethodInfoClosedInGenIfcNongenMethodRefTypeWithTarget()
		{
			var interceptor = new KeepDataInterceptor();
			IGenInterfaceHierarchyBase<List<object>> target = new GenInterfaceHierarchy<List<object>>();
			var proxy =
				generator.CreateInterfaceProxyWithTarget(target, interceptor);

			proxy.Add(null);
			GenericTestUtility.CheckMethodInfoIsClosed(interceptor.Invocation.GetConcreteMethod(), typeof(void),
													   typeof(List<object>));
			GenericTestUtility.CheckMethodInfoIsClosed(interceptor.Invocation.GetConcreteMethodInvocationTarget(), typeof(void),
													   typeof(List<object>));
			Assert.AreNotEqual(interceptor.Invocation.GetConcreteMethod(),
							   interceptor.Invocation.GetConcreteMethodInvocationTarget());

			proxy.Get();
			GenericTestUtility.CheckMethodInfoIsClosed(interceptor.Invocation.GetConcreteMethod(), typeof(List<object>));
			GenericTestUtility.CheckMethodInfoIsClosed(interceptor.Invocation.GetConcreteMethodInvocationTarget(),
													   typeof(List<object>));
			Assert.AreNotEqual(interceptor.Invocation.GetConcreteMethod(),
							   interceptor.Invocation.GetConcreteMethodInvocationTarget());
		}

		[Test]
		public void MethodInfoClosedInGenIfcNongenMethodValueTypeWithTarget()
		{
			var interceptor = new KeepDataInterceptor();
			IGenInterfaceHierarchyBase<int> target = new GenInterfaceHierarchy<int>();
			var proxy =
				generator.CreateInterfaceProxyWithTarget(target, interceptor);

			proxy.Add(0);
			GenericTestUtility.CheckMethodInfoIsClosed(interceptor.Invocation.GetConcreteMethod(), typeof(void), typeof(int));
			GenericTestUtility.CheckMethodInfoIsClosed(interceptor.Invocation.GetConcreteMethodInvocationTarget(), typeof(void),
													   typeof(int));
			Assert.AreNotEqual(interceptor.Invocation.GetConcreteMethod(),
							   interceptor.Invocation.GetConcreteMethodInvocationTarget());

			proxy.Get();
			GenericTestUtility.CheckMethodInfoIsClosed(interceptor.Invocation.GetConcreteMethod(), typeof(int));
			GenericTestUtility.CheckMethodInfoIsClosed(interceptor.Invocation.GetConcreteMethodInvocationTarget(), typeof(int));
			Assert.AreNotEqual(interceptor.Invocation.GetConcreteMethod(),
							   interceptor.Invocation.GetConcreteMethodInvocationTarget());
		}

		[Test]
		public void MethodInfoClosedInNongenIfcGenMethodWithTarget()
		{
			var interceptor = new KeepDataInterceptor();
			OnlyGenMethodsInterface target = new OnlyGenMethodsInterfaceImpl();
			var proxy =
				generator.CreateInterfaceProxyWithTarget(target, interceptor);

			proxy.DoSomething(1);
			GenericTestUtility.CheckMethodInfoIsClosed(interceptor.Invocation.GetConcreteMethod(), typeof(int), typeof(int));
			GenericTestUtility.CheckMethodInfoIsClosed(interceptor.Invocation.GetConcreteMethodInvocationTarget(), typeof(int),
													   typeof(int));
			Assert.AreNotEqual(interceptor.Invocation.GetConcreteMethod(),
							   interceptor.Invocation.GetConcreteMethodInvocationTarget());

			proxy.DoSomething(new List<object>());
			GenericTestUtility.CheckMethodInfoIsClosed(interceptor.Invocation.GetConcreteMethod(), typeof(List<object>),
													   typeof(List<object>));
			GenericTestUtility.CheckMethodInfoIsClosed(interceptor.Invocation.GetConcreteMethodInvocationTarget(),
													   typeof(List<object>), typeof(List<object>));
			Assert.AreNotEqual(interceptor.Invocation.GetConcreteMethod(),
							   interceptor.Invocation.GetConcreteMethodInvocationTarget());
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void ThrowsWhenProxyingGenericTypeDefNoTarget()
		{
			var interceptor = new KeepDataInterceptor();
			var o = generator.CreateInterfaceProxyWithoutTarget(typeof(IGenInterfaceHierarchyBase<>), interceptor);
		}

		[Test(Description = "There is a strange CLR bug resulting from our loading the tokens of methods in generic types. "
		                    + "This test ensures we do not trigger it.")]
		public void TypeGetMethodsIsStable()
		{
			ProxyWithGenInterfaceWithBase();
			Assert.AreEqual(4, typeof(IGenInterfaceHierarchyBase<int>).GetMethods().Length);
		}

		[Test(Description = "There is a strange CLR bug resulting from our loading the tokens of methods in generic types. "
							+ "This test ensures we correctly work around it.")]
		public void MethodFinderIsStable()
		{
			ProxyWithGenInterfaceWithBase();
			Assert.AreEqual(4,
							MethodFinder.GetAllInstanceMethods(typeof(IGenInterfaceHierarchyBase<int>),
															   BindingFlags.Public | BindingFlags.Instance).Length);
		}

		[Test(Description = "There is a strange CLR bug resulting from our loading the tokens of methods in generic types. "
		                    +
		                    "This test ensures we do not trigger it across AppDomains. If we do, MethodFinder must provide a cross-AppDomain workaround."
			)]
#if SILVERLIGHT
		[Ignore("Cannot create seperate AppDomain in silverlight.")]
		public void TypeGetMethodsIsStableInDifferentAppDomains()
		{
		}
#else
		public void TypeGetMethodsIsStableInDifferentAppDomains()
		{
			ProxyWithGenInterfaceWithBase();
			var newDomain =
				AppDomain.CreateDomain("NewDomain", AppDomain.CurrentDomain.Evidence, AppDomain.CurrentDomain.SetupInformation);
			try
			{
				newDomain.DoCallBack(delegate { Assert.AreEqual(4, typeof (IGenInterfaceHierarchyBase<int>).GetMethods().Length); });
			}
			finally
			{
				AppDomain.Unload(newDomain);
			}
		}
#endif
	}
}
