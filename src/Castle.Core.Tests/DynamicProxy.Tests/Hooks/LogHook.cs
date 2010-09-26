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

namespace Castle.DynamicProxy.Tests.Hooks
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;

	using NUnit.Framework;

#if !SILVERLIGHT
	[Serializable]
#endif
	public class LogHook : IProxyGenerationHook
	{
		private readonly IList<MemberInfo> askedMembers = new List<MemberInfo>();
		private readonly IList<MemberInfo> nonVirtualMembers = new List<MemberInfo>();
		private readonly bool screeningEnabled;
		private readonly Type targetTypeToAssert;
		private bool completed;

		public LogHook(Type targetTypeToAssert, bool screeningEnabled)
		{
			this.targetTypeToAssert = targetTypeToAssert;
			this.screeningEnabled = screeningEnabled;
		}

		public IList<MemberInfo> AskedMembers
		{
			get { return askedMembers; }
		}

		public bool Completed
		{
			get { return completed; }
		}

		public IList<MemberInfo> NonVirtualMembers
		{
			get { return nonVirtualMembers; }
		}

		public void MethodsInspected()
		{
			completed = true;
		}

		public void NonProxyableMemberNotification(Type type, MemberInfo memberInfo)
		{
			Assert.AreEqual(targetTypeToAssert, type);

			nonVirtualMembers.Add(memberInfo);
		}

		public bool ShouldInterceptMethod(Type type, MethodInfo memberInfo)
		{
			Assert.AreEqual(targetTypeToAssert, type);

			askedMembers.Add(memberInfo);

			if (screeningEnabled && memberInfo.Name.StartsWith("Sum"))
			{
				return false;
			}

			return true;
		}
	}
}