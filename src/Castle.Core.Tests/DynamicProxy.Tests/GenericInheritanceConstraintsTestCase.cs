namespace Castle.DynamicProxy.Tests
{
	using System.Collections.Generic;

	using Castle.DynamicProxy.Tests.GenericInterfaces;
	using Castle.DynamicProxy.Tests.Interfaces;

	using NUnit.Framework;

	[TestFixture]
	public class GenericInheritanceConstraintsTestCase:BasePEVerifyTestCase
	{
		[Test]
		public void Can_proxy_generic_type_with_simple_interface_constraint()
		{
			generator.CreateInterfaceProxyWithoutTarget<IGenericTIsIEmpty<Empty>>();
		}

		[Test]
		public void Can_proxy_generic_type_with_simple_class_constraint()
		{
			generator.CreateInterfaceProxyWithoutTarget<IGenericTIsEmpty<Empty>>();
		}

		[Test]
		public void Can_proxy_generic_type_with_simple_class_and_interface_constraint()
		{
			generator.CreateInterfaceProxyWithoutTarget<IGenericTIsOneAndITwo<SubOneTwo>>();
		}

		[Test]
		public void Can_proxy_generic_type_with_generic_closed_over_simple_interface_constraint()
		{
			generator.CreateInterfaceProxyWithoutTarget<IGenericTIsIEnumerableOfIEmpty<IList<Empty>>>();
		}

		[Test]
		public void Can_proxy_double_generic_type_where_TOne_is_TTwo()
		{
			generator.CreateInterfaceProxyWithoutTarget<IGenericTOneIsTTwo<Empty, IEmpty>>();
		}
	}
}