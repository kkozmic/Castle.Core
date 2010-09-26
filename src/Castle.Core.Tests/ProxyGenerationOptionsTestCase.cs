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

	using Castle.DynamicProxy.Tests.Hooks;
	using Castle.DynamicProxy.Tests.Mixins;
	using NUnit.Framework;

	[TestFixture]
	public class ProxyGenerationOptionsTestCase
	{
		private ProxyGenerationOptions options1;
		private ProxyGenerationOptions options2;

		[SetUp]
		public void Init()
		{
			options1 = new ProxyGenerationOptions();
			options2 = new ProxyGenerationOptions();
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void MixinData_NeedsInitialize()
		{
#pragma warning disable 219
			MixinData data = options1.MixinData;
#pragma warning restore 219
		}

		[Test]
		public void MixinData()
		{
			options1.Initialize();
			MixinData data = options1.MixinData;
			Assert.AreEqual(0, new List<object>(data.Mixins).Count);
		}

		[Test]
		public void MixinData_WithMixins()
		{
			options1.AddMixinInstance(new SimpleMixin());
			options1.Initialize();
			MixinData data = options1.MixinData;
			Assert.AreEqual(1, new List<object>(data.Mixins).Count);
		}

		[Test]
		public void MixinData_NoReInitializeWhenNothingChanged()
		{
			options1.AddMixinInstance(new SimpleMixin());
			options1.Initialize();

			MixinData data1 = options1.MixinData;
			options1.Initialize();
			MixinData data2 = options1.MixinData;
			Assert.AreSame(data1, data2);
		}

		[Test]
		public void MixinData_ReInitializeWhenMixinsChanged()
		{
			options1.AddMixinInstance(new SimpleMixin());
			options1.Initialize();

			MixinData data1 = options1.MixinData;

			options1.AddMixinInstance(new OtherMixin());
			options1.Initialize();
			MixinData data2 = options1.MixinData;
			Assert.AreNotSame(data1, data2);

			Assert.AreEqual (1, new List<object>(data1.Mixins).Count);
			Assert.AreEqual (2, new List<object>(data2.Mixins).Count);
		}

		[Test]
		public void Equals_EmptyOptions()
		{
			Assert.AreEqual(options1, options2);
		}

		[Test]
		public void Equals_EqualNonEmptyOptions()
		{
			options1 = new ProxyGenerationOptions();
			options2 = new ProxyGenerationOptions();

			options1.BaseTypeForInterfaceProxy = typeof (IConvertible);
			options2.BaseTypeForInterfaceProxy = typeof (IConvertible);

			SimpleMixin mixin = new SimpleMixin();
			options1.AddMixinInstance(mixin);
			options2.AddMixinInstance(mixin);

			IProxyGenerationHook hook = new AllMethodsHook();
			options1.Hook = hook;
			options2.Hook = hook;

			IInterceptorSelector selector = new AllInterceptorSelector();
			options1.Selector = selector;
			options2.Selector = selector;

			Assert.AreEqual(options1, options2);
		}

		[Test]
		public void Equals_DifferentOptions_BaseTypeForInterfaceProxy()
		{
			options1.BaseTypeForInterfaceProxy = typeof (IConvertible);
			options2.BaseTypeForInterfaceProxy = typeof (object);

			Assert.AreNotEqual(options1, options2);
		}

		[Test]
		public void Equals_DifferentOptions_AddMixinInstance()
		{
			SimpleMixin mixin = new SimpleMixin();
			options1.AddMixinInstance(mixin);

			Assert.AreNotEqual(options1, options2);
		}

		[Test]
		public void Equals_DifferentOptions_Hook()
		{
			IProxyGenerationHook hook = new LogHook(typeof(object), true);
			options1.Hook = hook;

			Assert.AreNotEqual(options1, options2);
		}

		[Test]
		public void Equals_DifferentOptions_Selector()
		{
			options1.Selector = new AllInterceptorSelector();

			Assert.AreNotEqual(options1, options2);
		}

		[Test]
		public void Equals_ComparesMixinTypesNotInstances()
		{
			options1.AddMixinInstance(new SimpleMixin());
			options2.AddMixinInstance(new SimpleMixin());

			Assert.AreEqual(options1, options2);
		}

		[Test]
		public void Equals_ComparesSortedMixinTypes()
		{
			options1.AddMixinInstance(new SimpleMixin());
			options1.AddMixinInstance(new ComplexMixin());

			options2.AddMixinInstance(new ComplexMixin());
			options2.AddMixinInstance(new SimpleMixin());

			Assert.AreEqual(options1, options2);
		}

		[Test]
		public void Equals_Compares_selectors_existence()
		{
			options1.Selector = new AllInterceptorSelector();
			options2.Selector = new TypeInterceptorSelector<StandardInterceptor>();

			Assert.AreEqual(options1, options2);

			options2.Selector = null;
			Assert.AreNotEqual(options1, options2);

			options1.Selector = null;
			Assert.AreEqual(options1, options2);
		}

		[Test]
		public void GetHashCode_EmptyOptions()
		{
			Assert.AreEqual(options1.GetHashCode(), options2.GetHashCode());
		}

		[Test]
		public void GetHashCode_EqualNonEmptyOptions()
		{
			options1 = new ProxyGenerationOptions();
			options2 = new ProxyGenerationOptions();

			options1.BaseTypeForInterfaceProxy = typeof (IConvertible);
			options2.BaseTypeForInterfaceProxy = typeof (IConvertible);

			SimpleMixin mixin = new SimpleMixin();
			options1.AddMixinInstance(mixin);
			options2.AddMixinInstance(mixin);

			
			IProxyGenerationHook hook = new AllMethodsHook();
			options1.Hook = hook;
			options2.Hook = hook;

			IInterceptorSelector selector = new AllInterceptorSelector();
			options1.Selector = selector;
			options2.Selector = selector;

			Assert.AreEqual(options1.GetHashCode(), options2.GetHashCode());
		}

		[Test]
		public void GetHashCode_EqualOptions_DifferentMixinInstances()
		{
			options1.AddMixinInstance(new SimpleMixin());
			options2.AddMixinInstance(new SimpleMixin());

			Assert.AreEqual(options1.GetHashCode(), options2.GetHashCode());
		}

		[Test]
		public void GetHashCode_DifferentOptions_BaseTypeForInterfaceProxy()
		{
			options1.BaseTypeForInterfaceProxy = typeof (IConvertible);
			options2.BaseTypeForInterfaceProxy = typeof (object);

			Assert.AreNotEqual(options1.GetHashCode(), options2.GetHashCode());
		}

		[Test]
		public void GetHashCode_DifferentOptions_AddMixinInstance()
		{
			SimpleMixin mixin = new SimpleMixin();
			options1.AddMixinInstance(mixin);

			Assert.AreNotEqual(options1.GetHashCode(), options2.GetHashCode());
		}

		[Test]
		public void GetHashCode_DifferentOptions_Hook()
		{
			IProxyGenerationHook hook = new LogHook(typeof (object), true);
			options1.Hook = hook;

			Assert.AreNotEqual(options1.GetHashCode(), options2.GetHashCode());
		}

		[Test]
		public void GetHashCode_DifferentOptions_Selector()
		{
			options1.Selector = new AllInterceptorSelector();

			Assert.AreNotEqual(options1.GetHashCode(), options2.GetHashCode());
		}
	}
}