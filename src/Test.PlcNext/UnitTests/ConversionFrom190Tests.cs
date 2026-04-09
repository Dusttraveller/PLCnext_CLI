#region Copyright
///////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) Phoenix Contact GmbH & Co KG
//  This software is licensed under Apache-2.0
//
///////////////////////////////////////////////////////////////////////////////
#endregion

using System;
using PlcNext.Migration;
using Shouldly;
using Xunit;

namespace Test.PlcNext.UnitTests
{
    public class ConversionFrom190Tests
    {
        [Fact]
        public void BaseVersion_Returns19_0()
        {
            var step = new ConversionFrom190();

            step.BaseVersion.ShouldBe(new Version(19, 0));
        }

        [Fact]
        public void Execute_DoesNotThrow()
        {
            var step = new ConversionFrom190();

            Should.NotThrow(() => step.Execute("/some/path"));
        }

        [Fact]
        public void Execute_WithEmptyPath_DoesNotThrow()
        {
            var step = new ConversionFrom190();

            Should.NotThrow(() => step.Execute(string.Empty));
        }
    }
}
