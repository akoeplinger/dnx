// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Framework.Runtime.Json;
using Xunit;

namespace Microsoft.Framework.Runtime.Tests
{
	public class JsonDeserializerFacts
	{
		[Fact]
		public void DefaultConstructorSetPropertiesCorrectly()
		{
			var target = new JsonDeserializer();
		}
	}
}
