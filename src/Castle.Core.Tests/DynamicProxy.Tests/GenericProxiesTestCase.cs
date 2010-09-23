namespace Castle.DynamicProxy.Tests
{
	using Castle.DynamicProxy.Tests.GenInterfaces;

	using NUnit.Framework;

	[TestFixture]
	public class GenericProxiesTestCase : BasePEVerifyTestCase
	{
		[Test]
		public void Proxy_for_generic_type_is_generic()
		{
			var proxy = generator.CreateInterfaceProxyWithoutTarget<IEmptyGeneric<int>>();
			Assert.IsTrue(proxy.GetType().IsGenericType);
		}

		[Test]
		public void Proxy_for_doublegeneric_type_is_generic()
		{
			var proxy = generator.CreateInterfaceProxyWithoutTarget<IEmptyGeneric<int, string>>();
			Assert.IsTrue(proxy.GetType().IsGenericType);
		}

		[Test]
		public void Proxy_for_generic_type_with_non_generic_method_generates_correctly()
		{
			var proxy = generator.CreateInterfaceProxyWithoutTarget<IGenericWithMethod<int>>();
			Assert.IsTrue(proxy.GetType().IsGenericType);
		}
	}
}