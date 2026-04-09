#region Copyright
///////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) Phoenix Contact GmbH & Co KG
//  This software is licensed under Apache-2.0
//
///////////////////////////////////////////////////////////////////////////////
#endregion

using PlcNext.CommandLine;
using Shouldly;
using Xunit;

namespace Test.PlcNext.UnitTests
{
    public class TotalAccessTypeTests
    {
        [Fact]
        public void Equals_SameType_ReturnsTrue()
        {
            var a = new TotalAccessType(typeof(string));
            var b = new TotalAccessType(typeof(string));

            a.Equals(b).ShouldBeTrue();
        }

        [Fact]
        public void Equals_DifferentType_ReturnsFalse()
        {
            var a = new TotalAccessType(typeof(string));
            var b = new TotalAccessType(typeof(int));

            a.Equals(b).ShouldBeFalse();
        }

        [Fact]
        public void Equals_ObjectNull_ReturnsFalse()
        {
            var a = new TotalAccessType(typeof(string));

            a.Equals((object)null).ShouldBeFalse();
        }

        [Fact]
        public void Equals_ObjectOfDifferentType_ReturnsFalse()
        {
            var a = new TotalAccessType(typeof(string));

            a.Equals("not a TotalAccessType").ShouldBeFalse();
        }

        [Fact]
        public void Equals_ObjectSameType_ReturnsTrue()
        {
            var a = new TotalAccessType(typeof(string));
            object b = new TotalAccessType(typeof(string));

            a.Equals(b).ShouldBeTrue();
        }

        [Fact]
        public void GetHashCode_NullType_ReturnsZero()
        {
            var a = new TotalAccessType(null);

            a.GetHashCode().ShouldBe(0);
        }

        [Fact]
        public void GetHashCode_ValidType_ReturnsTypeHashCode()
        {
            var a = new TotalAccessType(typeof(string));

            a.GetHashCode().ShouldBe(typeof(string).GetHashCode());
        }

        [Fact]
        public void GetHashCode_SameType_ReturnsSameValue()
        {
            var a = new TotalAccessType(typeof(int));
            var b = new TotalAccessType(typeof(int));

            a.GetHashCode().ShouldBe(b.GetHashCode());
        }

        [Fact]
        public void OperatorEquals_SameType_ReturnsTrue()
        {
            var a = new TotalAccessType(typeof(string));
            var b = new TotalAccessType(typeof(string));

            (a == b).ShouldBeTrue();
        }

        [Fact]
        public void OperatorEquals_DifferentType_ReturnsFalse()
        {
            var a = new TotalAccessType(typeof(string));
            var b = new TotalAccessType(typeof(int));

            (a == b).ShouldBeFalse();
        }

        [Fact]
        public void OperatorNotEquals_SameType_ReturnsFalse()
        {
            var a = new TotalAccessType(typeof(string));
            var b = new TotalAccessType(typeof(string));

            (a != b).ShouldBeFalse();
        }

        [Fact]
        public void OperatorNotEquals_DifferentType_ReturnsTrue()
        {
            var a = new TotalAccessType(typeof(string));
            var b = new TotalAccessType(typeof(int));

            (a != b).ShouldBeTrue();
        }

        [Fact]
        public void TypeProperty_ReturnsConstructorArgument()
        {
            var a = new TotalAccessType(typeof(double));

            a.Type.ShouldBe(typeof(double));
        }
    }
}
