#region Copyright
///////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) Phoenix Contact GmbH & Co KG
//  This software is licensed under Apache-2.0
//
///////////////////////////////////////////////////////////////////////////////
#endregion

using System;
using PlcNext.Common.Installation;
using Shouldly;
using Xunit;

namespace Test.PlcNext.UnitTests
{
    public class InvalidUriFormatExceptionTests
    {
        #region TryCreateUri (absolute)

        [Fact]
        public void TryCreateUri_ValidAbsoluteUri_ReturnsUri()
        {
            Uri result = InvalidUriFormatException.TryCreateUri("https://example.com/path");

            result.ShouldNotBeNull();
            result.Scheme.ShouldBe("https");
            result.Host.ShouldBe("example.com");
        }

        [Fact]
        public void TryCreateUri_AbsoluteUriWithoutTrailingSlash_AddsTrailingSlash()
        {
            Uri result = InvalidUriFormatException.TryCreateUri("https://example.com/path");

            result.ToString().ShouldEndWith("/");
        }

        [Fact]
        public void TryCreateUri_AbsoluteUriWithTrailingSlash_DoesNotDoubleSlash()
        {
            Uri result = InvalidUriFormatException.TryCreateUri("https://example.com/path/");

            result.ToString().ShouldEndWith("/");
            result.ToString().ShouldNotEndWith("//");
        }

        [Fact]
        public void TryCreateUri_InvalidAbsoluteUri_ThrowsInvalidUriFormatException()
        {
            Should.Throw<InvalidUriFormatException>(() =>
                InvalidUriFormatException.TryCreateUri("not a valid uri :::"));
        }

        [Fact]
        public void TryCreateUri_InvalidAbsoluteUri_ExceptionHasInnerUriFormatException()
        {
            InvalidUriFormatException ex = Should.Throw<InvalidUriFormatException>(() =>
                InvalidUriFormatException.TryCreateUri("not a valid uri :::"));

            ex.InnerException.ShouldBeOfType<UriFormatException>();
        }

        #endregion

        #region TryCreateUri (relative)

        [Fact]
        public void TryCreateUri_ValidRelativeUri_ReturnsUri()
        {
            var baseUri = new Uri("https://example.com/base/");
            Uri result = InvalidUriFormatException.TryCreateUri("resource.zip", baseUri);

            result.ShouldNotBeNull();
            result.ToString().ShouldContain("resource.zip");
        }

        [Fact]
        public void TryCreateUri_RelativeUriWithLeadingSlash_RemovesLeadingSlash()
        {
            var baseUri = new Uri("https://example.com/base/");
            Uri result = InvalidUriFormatException.TryCreateUri("/resource.zip", baseUri);

            result.ShouldNotBeNull();
            // Leading slash is stripped — the path should not double the base
            result.ToString().ShouldNotBe("https://example.com/base//resource.zip");
        }

        [Fact]
        public void TryCreateUri_RelativeUri_CombinesWithBase()
        {
            var baseUri = new Uri("https://example.com/downloads/");
            Uri result = InvalidUriFormatException.TryCreateUri("sdk-22.0.zip", baseUri);

            result.Host.ShouldBe("example.com");
            result.AbsolutePath.ShouldContain("sdk-22.0.zip");
        }

        #endregion

        #region Constructor message

        [Fact]
        public void Constructor_WithAbsoluteUri_MessageContainsUri()
        {
            const string uri = "https://bad-uri-example";
            var ex = new InvalidUriFormatException(uri, false);

            ex.Message.ShouldContain(uri);
        }

        [Fact]
        public void Constructor_WithRelativeUri_MessageContainsUri()
        {
            const string uri = "relative/bad/path";
            var ex = new InvalidUriFormatException(uri, true);

            ex.Message.ShouldContain(uri);
        }

        [Fact]
        public void Constructor_WithInnerException_SetsInnerException()
        {
            var inner = new UriFormatException("bad format");
            var ex = new InvalidUriFormatException("https://example.com", false, inner);

            ex.InnerException.ShouldBe(inner);
        }

        #endregion
    }
}
