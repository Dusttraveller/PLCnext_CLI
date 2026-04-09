#region Copyright
///////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) Phoenix Contact GmbH & Co KG
//  This software is licensed under Apache-2.0
//
///////////////////////////////////////////////////////////////////////////////
#endregion

using System;
using System.IO;
using System.Threading.Tasks;
using PlcNext.Common.Tools.IO;
using Shouldly;
using Xunit;

namespace Test.PlcNext.UnitTests
{
    public class UnsupportedArchiveFormatExceptionTests
    {
        [Fact]
        public void Constructor_MessageContainsArchiveName()
        {
            var ex = new UnsupportedArchiveFormatException("archive.rar");

            ex.Message.ShouldContain("archive.rar");
        }

        [Fact]
        public void Constructor_WithInnerException_SetsInnerException()
        {
            var inner = new Exception("inner");
            var ex = new UnsupportedArchiveFormatException("archive.zip", inner);

            ex.InnerException.ShouldBe(inner);
            ex.Message.ShouldContain("archive.zip");
        }

        [Fact]
        public void Constructor_WithoutInnerException_InnerExceptionIsNull()
        {
            var ex = new UnsupportedArchiveFormatException("archive.tar.xz");

            ex.InnerException.ShouldBeNull();
        }
    }

    public class StreamExtensionsTests
    {
        #region CopyToAsync

        [Fact]
        public async Task CopyToAsync_NullSource_ThrowsArgumentNullException()
        {
            using var destination = new MemoryStream();

            await Should.ThrowAsync<ArgumentNullException>(() =>
                StreamExtensions.CopyToAsync(null, destination));
        }

        [Fact]
        public async Task CopyToAsync_NonReadableSource_ThrowsArgumentException()
        {
            using var source = new NonReadableStream();
            using var destination = new MemoryStream();

            await Should.ThrowAsync<ArgumentException>(() =>
                StreamExtensions.CopyToAsync(source, destination));
        }

        [Fact]
        public async Task CopyToAsync_NullDestination_ThrowsArgumentNullException()
        {
            using var source = new MemoryStream(new byte[] { 1, 2, 3 });

            await Should.ThrowAsync<ArgumentNullException>(() =>
                StreamExtensions.CopyToAsync(source, null));
        }

        [Fact]
        public async Task CopyToAsync_NonWritableDestination_ThrowsArgumentException()
        {
            using var source = new MemoryStream(new byte[] { 1, 2, 3 });
            using var destination = new NonWritableStream();

            await Should.ThrowAsync<ArgumentException>(() =>
                StreamExtensions.CopyToAsync(source, destination));
        }

        [Fact]
        public async Task CopyToAsync_CopiesAllBytes()
        {
            byte[] data = { 1, 2, 3, 4, 5 };
            using var source = new MemoryStream(data);
            using var destination = new MemoryStream();

            await source.CopyToAsync(destination);

            destination.ToArray().ShouldBe(data);
        }

        [Fact]
        public async Task CopyToAsync_ReportsProgressWithTotalBytesRead()
        {
            byte[] data = new byte[1024];
            new Random(42).NextBytes(data);
            using var source = new MemoryStream(data);
            using var destination = new MemoryStream();

            long lastReported = -1;
            await source.CopyToAsync(destination, bytes => lastReported = bytes);

            lastReported.ShouldBe(data.Length);
        }

        [Fact]
        public async Task CopyToAsync_NullProgress_DoesNotThrow()
        {
            using var source = new MemoryStream(new byte[] { 1, 2, 3 });
            using var destination = new MemoryStream();

            await Should.NotThrowAsync(() => source.CopyToAsync(destination, null));
        }

        [Fact]
        public async Task CopyToAsync_EmptySource_CopiesNothing()
        {
            using var source = new MemoryStream();
            using var destination = new MemoryStream();

            await source.CopyToAsync(destination);

            destination.Length.ShouldBe(0);
        }

        #endregion

        #region ReadToEnd

        [Fact]
        public void ReadToEnd_NullStream_ThrowsArgumentNullException()
        {
            Should.Throw<ArgumentNullException>(() => StreamExtensions.ReadToEnd(null));
        }

        [Fact]
        public void ReadToEnd_SeekableStream_ReadsAllBytes()
        {
            byte[] data = { 10, 20, 30, 40, 50 };
            using var stream = new MemoryStream(data);

            byte[] result = stream.ReadToEnd();

            result.ShouldBe(data);
        }

        [Fact]
        public void ReadToEnd_SeekableStream_DoesNotChangePosition()
        {
            byte[] data = { 1, 2, 3 };
            using var stream = new MemoryStream(data);
            stream.Position = 2;

            stream.ReadToEnd();

            stream.Position.ShouldBe(2);
        }

        [Fact]
        public void ReadToEnd_EmptyStream_ReturnsEmptyArray()
        {
            using var stream = new MemoryStream();

            byte[] result = stream.ReadToEnd();

            result.ShouldBeEmpty();
        }

        [Fact]
        public void ReadToEnd_LargeStream_ReadsAllBytes()
        {
            // Larger than one buffer to exercise the buffer expansion path (buffer size is 4096)
            byte[] data = new byte[4096 + 100];
            new Random(1).NextBytes(data);
            using var stream = new MemoryStream(data);

            byte[] result = stream.ReadToEnd();

            result.ShouldBe(data);
        }

        #endregion

        // Helpers
        private class NonReadableStream : Stream
        {
            public override bool CanRead => false;
            public override bool CanSeek => false;
            public override bool CanWrite => true;
            public override long Length => 0;
            public override long Position { get => 0; set { } }
            public override void Flush() { }
            public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) { }
            public override void Write(byte[] buffer, int offset, int count) { }
        }

        private class NonWritableStream : Stream
        {
            public override bool CanRead => true;
            public override bool CanSeek => false;
            public override bool CanWrite => false;
            public override long Length => 0;
            public override long Position { get => 0; set { } }
            public override void Flush() { }
            public override int Read(byte[] buffer, int offset, int count) => 0;
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) { }
            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        }
    }

    public class PageBufferOptionsTests
    {
        [Fact]
        public void Constructor_SetsCapacityAndMinimalCapacity()
        {
            var options = new PageBufferOptions(1024, 64);

            options.Capacity.ShouldBe(1024L);
            options.MinimalCapacity.ShouldBe(64L);
        }

        [Fact]
        public void Constructor_DefaultBufferTypeIsFile()
        {
            var options = new PageBufferOptions(1024, 64);

            options.BufferType.ShouldBe(PageBufferType.File);
        }

        [Fact]
        public void Constructor_DefaultEnableDoubleBufferIsTrue()
        {
            var options = new PageBufferOptions(1024, 64);

            options.EnableDoubleBuffer.ShouldBeTrue();
        }

        [Fact]
        public void BufferType_CanBeChangedToMemory()
        {
            var options = new PageBufferOptions(1024, 64) { BufferType = PageBufferType.Memory };

            options.BufferType.ShouldBe(PageBufferType.Memory);
        }

        [Fact]
        public void EnableDoubleBuffer_CanBeSetToFalse()
        {
            var options = new PageBufferOptions(1024, 64) { EnableDoubleBuffer = false };

            options.EnableDoubleBuffer.ShouldBeFalse();
        }
    }

    public class PageBufferTypeTests
    {
        [Fact]
        public void PageBufferType_HasExpectedValues()
        {
            ((int)PageBufferType.Memory).ShouldBe(0);
            ((int)PageBufferType.File).ShouldBe(1);
        }
    }

    public class BuddyAllocatorTests
    {
        [Fact]
        public void Constructor_NullStream_ThrowsArgumentNullException()
        {
            Should.Throw<ArgumentNullException>(() => new BuddyAllocator(null, 1024, 64));
        }

        [Fact]
        public void Allocate_WithSufficientCapacity_ReturnsPage()
        {
            using var stream = new MemoryStream();
            var allocator = new BuddyAllocator(stream, 1024, 0);

            IPage page = allocator.Allocate(64);

            page.ShouldNotBeNull();
        }

        [Fact]
        public void Allocate_ZeroCapacityAndZeroMinimum_ReturnsNull()
        {
            using var stream = new MemoryStream();
            var allocator = new BuddyAllocator(stream, 1024, 0);

            IPage page = allocator.Allocate(0);

            page.ShouldBeNull();
        }

        [Fact]
        public void Allocate_AfterDispose_ThrowsObjectDisposedException()
        {
            var stream = new MemoryStream();
            var allocator = new BuddyAllocator(stream, 1024, 0);
            allocator.Dispose();

            Should.Throw<ObjectDisposedException>(() => allocator.Allocate(64));
        }

        [Fact]
        public void Clear_ResetsFreeSpace()
        {
            using var stream = new MemoryStream();
            var allocator = new BuddyAllocator(stream, 1024, 0);

            allocator.Allocate(512);
            allocator.Clear();

            // After clear, full capacity should be allocatable again
            IPage page = allocator.Allocate(512);
            page.ShouldNotBeNull();
        }

        [Fact]
        public void FlushStream_DoesNotThrow()
        {
            using var stream = new MemoryStream();
            var allocator = new BuddyAllocator(stream, 1024, 0);

            Should.NotThrow(() => allocator.FlushStream());
        }

        [Fact]
        public void Dispose_CalledTwice_DoesNotThrow()
        {
            var stream = new MemoryStream();
            var allocator = new BuddyAllocator(stream, 1024, 0);

            allocator.Dispose();

            Should.NotThrow(() => allocator.Dispose());
        }
    }

    public class BlockTests
    {
        [Fact]
        public void Allocate_RequestedCapacityFitsInBlock_ReturnsBlock()
        {
            var block = new Block { Capacity = 1024 };

            Block result = block.Allocate(512);

            result.ShouldNotBeNull();
            result.IsAllocated.ShouldBeTrue();
        }

        [Fact]
        public void Allocate_RequestedCapacityLargerThanBlock_ReturnsNull()
        {
            var block = new Block { Capacity = 64 };

            Block result = block.Allocate(128);

            result.ShouldBeNull();
        }

        [Fact]
        public void Allocate_FitsInHalfBlock_SplitsBlock()
        {
            var block = new Block { Capacity = 1024 };

            // Requesting 256 causes recursive halving: 1024→512→256
            Block result = block.Allocate(256);

            result.ShouldNotBeNull();
            result.IsAllocated.ShouldBeTrue();
            result.Capacity.ShouldBe(256L);
        }

        [Fact]
        public void Allocate_ExactCapacity_AllocatesBlock()
        {
            var block = new Block { Capacity = 128 };

            Block result = block.Allocate(128);

            result.ShouldNotBeNull();
            result.IsAllocated.ShouldBeTrue();
            result.Capacity.ShouldBe(128L);
        }

        [Fact]
        public void Release_SetsIsAllocatedToFalse()
        {
            var block = new Block { Capacity = 128, IsAllocated = true };

            block.Release();

            block.IsAllocated.ShouldBeFalse();
        }

        [Fact]
        public void Release_WithNoCompanion_DoesNotThrow()
        {
            var block = new Block { Capacity = 128, IsAllocated = true, Companion = null };

            Should.NotThrow(() => block.Release());
        }

        [Fact]
        public void Allocate_AlreadyAllocated_SearchesCompanionBuddies()
        {
            var block = new Block { Capacity = 1024 };
            // First allocation splits the block
            Block first = block.Allocate(256);
            first.ShouldNotBeNull();

            // Second allocation should find a free buddy
            Block second = block.Allocate(256);
            second.ShouldNotBeNull();
        }
    }
}
