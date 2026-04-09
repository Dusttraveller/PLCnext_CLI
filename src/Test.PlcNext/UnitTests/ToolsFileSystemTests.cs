#region Copyright
///////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) Phoenix Contact GmbH & Co KG
//  This software is licensed under Apache-2.0
//
///////////////////////////////////////////////////////////////////////////////
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NSubstitute;
using PlcNext.Common.Tools;
using PlcNext.Common.Tools.FileSystem;
using Shouldly;
using Xunit;

namespace Test.PlcNext.UnitTests
{
    public class FormattableIoExceptionTests
    {
        [Fact]
        public void Constructor_WithMessage_SetsMessage()
        {
            var ex = new FormattableIoException("some IO error");

            ex.Message.ShouldBe("some IO error");
        }

        [Fact]
        public void Constructor_WithIoException_SetsMessageAndInnerException()
        {
            var inner = new IOException("disk full");
            var ex = new FormattableIoException(inner);

            ex.Message.ShouldBe("disk full");
            ex.InnerException.ShouldBe(inner);
        }

        [Fact]
        public void Constructor_WithNullIoException_DoesNotThrow()
        {
            IOException nullEx = null;

            Should.NotThrow(() => new FormattableIoException(nullEx));
        }

        [Fact]
        public void Constructor_WithUnauthorizedAccessException_SetsMessageAndInnerException()
        {
            var inner = new UnauthorizedAccessException("access denied");
            var ex = new FormattableIoException(inner);

            ex.Message.ShouldBe("access denied");
            ex.InnerException.ShouldBe(inner);
        }

        [Fact]
        public void Constructor_WithNullUnauthorizedAccessException_DoesNotThrow()
        {
            UnauthorizedAccessException nullEx = null;

            Should.NotThrow(() => new FormattableIoException(nullEx));
        }
    }

    public class FileSystemExtensionsTests
    {
        #region CleanPath

        [Fact]
        public void CleanPath_NullInput_ReturnsEmpty()
        {
            string result = ((string)null).CleanPath();

            result.ShouldBe(string.Empty);
        }

        [Fact]
        public void CleanPath_PathWithQuotes_TrimsQuotes()
        {
            string result = "\"my/path\"".CleanPath();

            result.ShouldNotContain("\"");
        }

        [Fact]
        public void CleanPath_PathWithLeadingAndTrailingSpaces_Trims()
        {
            string result = "  /some/path  ".CleanPath();

            result.ShouldNotStartWith(" ");
            result.ShouldNotEndWith(" ");
        }

        [Fact]
        public void CleanPath_BackslashesReplaced_WithDirectorySeparator()
        {
            string result = @"some\path\file.txt".CleanPath();

            // All separators are normalized to Path.DirectorySeparatorChar
            result.ShouldContain(Path.DirectorySeparatorChar.ToString());
            result.Replace(Path.DirectorySeparatorChar, '|').ShouldNotContain("/");
        }

        [Fact]
        public void CleanPath_ForwardSlashesReplaced_WithDirectorySeparator()
        {
            string result = "some/path/file.txt".CleanPath();

            result.ShouldContain(Path.DirectorySeparatorChar.ToString());
        }

        [Fact]
        public void CleanPath_EmptyString_ReturnsEmpty()
        {
            string result = string.Empty.CleanPath();

            result.ShouldBe(string.Empty);
        }

        #endregion

        #region Format

        [Fact]
        public void Format_IOException_ReturnsFormattableIoException()
        {
            var ex = new IOException("io error");

            Exception result = ex.Format();

            result.ShouldBeOfType<FormattableIoException>();
            result.Message.ShouldBe("io error");
        }

        [Fact]
        public void Format_UnauthorizedAccessException_ReturnsFormattableIoException()
        {
            var ex = new UnauthorizedAccessException("not allowed");

            Exception result = ex.Format();

            result.ShouldBeOfType<FormattableIoException>();
            result.Message.ShouldBe("not allowed");
        }

        [Fact]
        public void Format_OtherException_ReturnsSameException()
        {
            var ex = new InvalidOperationException("something else");

            Exception result = ex.Format();

            result.ShouldBeSameAs(ex);
        }

        [Fact]
        public void Format_FormattableException_ReturnsSameException()
        {
            var ex = new FormattableException("formattable");

            Exception result = ex.Format();

            result.ShouldBeSameAs(ex);
        }

        #endregion
    }

    public class VirtualEntryTests
    {
        private static IDirectoryContentResolver MakeDirectoryResolver(string fullName = "/root")
        {
            var resolver = Substitute.For<IDirectoryContentResolver>();
            resolver.FullName.Returns(fullName);
            resolver.GetContent().Returns(Enumerable.Empty<VirtualEntry>());
            resolver.Create<VirtualFile>(Arg.Any<string>()).Returns(ci =>
            {
                var fr = Substitute.For<IFileContentResolver>();
                fr.FullName.Returns($"{fullName}/{ci.Arg<string>()}");
                return new VirtualFile(ci.Arg<string>(), fr);
            });
            resolver.Create<VirtualDirectory>(Arg.Any<string>()).Returns(ci =>
            {
                var dr = MakeDirectoryResolver($"{fullName}/{ci.Arg<string>()}");
                return new VirtualDirectory(ci.Arg<string>(), dr, StringComparison.Ordinal);
            });
            return resolver;
        }

        private static VirtualDirectory MakeDirectory(string name = "root", string fullName = "/root")
        {
            return new VirtualDirectory(name, MakeDirectoryResolver(fullName), StringComparison.Ordinal);
        }

        [Fact]
        public void ToString_ReturnsFullName()
        {
            var dir = MakeDirectory("root", "/my/root");

            dir.ToString().ShouldBe("/my/root");
        }

        [Fact]
        public void Equals_SameFullName_ReturnsTrue()
        {
            var dir1 = MakeDirectory("root", "/same/path");
            var dir2 = MakeDirectory("root", "/same/path");

            dir1.Equals(dir2).ShouldBeTrue();
        }

        [Fact]
        public void Equals_DifferentFullName_ReturnsFalse()
        {
            var dir1 = MakeDirectory("root", "/path/a");
            var dir2 = MakeDirectory("root", "/path/b");

            dir1.Equals(dir2).ShouldBeFalse();
        }

        [Fact]
        public void Equals_Null_ReturnsFalse()
        {
            var dir = MakeDirectory();

            dir.Equals(null).ShouldBeFalse();
        }

        [Fact]
        public void Equals_SameReference_ReturnsTrue()
        {
            var dir = MakeDirectory();

            dir.Equals(dir).ShouldBeTrue();
        }

        [Fact]
        public void Equals_DifferentType_ReturnsFalse()
        {
            var dir = MakeDirectory("root", "/root");
            var fileResolver = Substitute.For<IFileContentResolver>();
            fileResolver.FullName.Returns("/root");
            var file = new VirtualFile("root", fileResolver);

            dir.Equals(file).ShouldBeFalse();
        }

        [Fact]
        public void GetHashCode_SameFullName_ReturnsSameHash()
        {
            var dir1 = MakeDirectory("root", "/same");
            var dir2 = MakeDirectory("root", "/same");

            dir1.GetHashCode().ShouldBe(dir2.GetHashCode());
        }

        [Fact]
        public void OperatorEquals_SameFullName_ReturnsTrue()
        {
            VirtualEntry dir1 = MakeDirectory("root", "/same");
            VirtualEntry dir2 = MakeDirectory("root", "/same");

            (dir1 == dir2).ShouldBeTrue();
        }

        [Fact]
        public void OperatorNotEquals_DifferentFullName_ReturnsTrue()
        {
            VirtualEntry dir1 = MakeDirectory("root", "/a");
            VirtualEntry dir2 = MakeDirectory("root", "/b");

            (dir1 != dir2).ShouldBeTrue();
        }

        [Fact]
        public void Name_ReturnsConstructorValue()
        {
            var dir = MakeDirectory("mydir");

            dir.Name.ShouldBe("mydir");
        }
    }

    public class VirtualDirectoryTests
    {
        private static IDirectoryContentResolver MakeResolver(
            string fullName = "/root",
            IEnumerable<VirtualEntry> initialContent = null)
        {
            var resolver = Substitute.For<IDirectoryContentResolver>();
            resolver.FullName.Returns(fullName);
            resolver.GetContent().Returns(initialContent ?? Enumerable.Empty<VirtualEntry>());
            resolver.SplitPath(Arg.Any<string>()).Returns(ci =>
                ci.Arg<string>().Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries));
            resolver.Create<VirtualFile>(Arg.Any<string>()).Returns(ci =>
            {
                var fr = Substitute.For<IFileContentResolver>();
                fr.FullName.Returns($"{fullName}/{ci.Arg<string>()}");
                return new VirtualFile(ci.Arg<string>(), fr);
            });
            resolver.Create<VirtualDirectory>(Arg.Any<string>()).Returns(ci =>
            {
                var name = ci.Arg<string>();
                var sub = Substitute.For<IDirectoryContentResolver>();
                sub.FullName.Returns($"{fullName}/{name}");
                sub.GetContent().Returns(Enumerable.Empty<VirtualEntry>());
                sub.Create<VirtualFile>(Arg.Any<string>()).Returns(ci2 =>
                {
                    var fr = Substitute.For<IFileContentResolver>();
                    fr.FullName.Returns($"{fullName}/{name}/{ci2.Arg<string>()}");
                    return new VirtualFile(ci2.Arg<string>(), fr);
                });
                return new VirtualDirectory(name, sub, StringComparison.Ordinal);
            });
            return resolver;
        }

        [Fact]
        public void FileExists_WhenFileInEntries_ReturnsTrue()
        {
            var fileResolver = Substitute.For<IFileContentResolver>();
            fileResolver.FullName.Returns("/root/test.txt");
            var file = new VirtualFile("test.txt", fileResolver);

            var resolver = MakeResolver(initialContent: new VirtualEntry[] { file });
            var dir = new VirtualDirectory("root", resolver, StringComparison.Ordinal);

            dir.FileExists("test.txt").ShouldBeTrue();
        }

        [Fact]
        public void FileExists_WhenFileNotInEntries_ReturnsFalse()
        {
            var resolver = MakeResolver();
            var dir = new VirtualDirectory("root", resolver, StringComparison.Ordinal);

            dir.FileExists("missing.txt").ShouldBeFalse();
        }

        [Fact]
        public void DirectoryExists_WhenDirectoryInEntries_ReturnsTrue()
        {
            var subResolver = MakeResolver("/root/sub");
            var subDir = new VirtualDirectory("sub", subResolver, StringComparison.Ordinal);

            var resolver = MakeResolver(initialContent: new VirtualEntry[] { subDir });
            var dir = new VirtualDirectory("root", resolver, StringComparison.Ordinal);

            dir.DirectoryExists("sub").ShouldBeTrue();
        }

        [Fact]
        public void DirectoryExists_WhenDirectoryNotInEntries_ReturnsFalse()
        {
            var resolver = MakeResolver();
            var dir = new VirtualDirectory("root", resolver, StringComparison.Ordinal);

            dir.DirectoryExists("nonexistent").ShouldBeFalse();
        }

        [Fact]
        public void Directory_WithNullParts_ThrowsArgumentNullException()
        {
            var resolver = MakeResolver();
            var dir = new VirtualDirectory("root", resolver, StringComparison.Ordinal);

            Should.Throw<ArgumentNullException>(() => dir.Directory(null));
        }

        [Fact]
        public void Directory_WithEmptyParts_ReturnsSelf()
        {
            var resolver = MakeResolver();
            var dir = new VirtualDirectory("root", resolver, StringComparison.Ordinal);

            var result = dir.Directory(Array.Empty<string>());

            result.ShouldBeSameAs(dir);
        }

        [Fact]
        public void Directory_CreatesSubdirectory()
        {
            var resolver = MakeResolver();
            var dir = new VirtualDirectory("root", resolver, StringComparison.Ordinal);

            var sub = dir.Directory("src");

            sub.ShouldNotBeNull();
            sub.Name.ShouldBe("src");
        }

        [Fact]
        public void Directory_ReturnsSameSubdirectoryOnSecondCall()
        {
            var resolver = MakeResolver();
            var dir = new VirtualDirectory("root", resolver, StringComparison.Ordinal);

            var sub1 = dir.Directory("src");
            var sub2 = dir.Directory("src");

            sub1.ShouldBeSameAs(sub2);
        }

        [Fact]
        public void File_CreatesFileEntry()
        {
            var resolver = MakeResolver();
            var dir = new VirtualDirectory("root", resolver, StringComparison.Ordinal);

            var file = dir.File("main.cpp");

            file.ShouldNotBeNull();
            file.Name.ShouldBe("main.cpp");
        }

        [Fact]
        public void File_ReturnsSameFileOnSecondCall()
        {
            var resolver = MakeResolver();
            var dir = new VirtualDirectory("root", resolver, StringComparison.Ordinal);

            var f1 = dir.File("main.cpp");
            var f2 = dir.File("main.cpp");

            f1.ShouldBeSameAs(f2);
        }

        [Fact]
        public void Files_WithWildcard_ReturnsAllFiles()
        {
            var fileResolver1 = Substitute.For<IFileContentResolver>();
            fileResolver1.FullName.Returns("/root/a.cpp");
            var fileResolver2 = Substitute.For<IFileContentResolver>();
            fileResolver2.FullName.Returns("/root/b.hpp");
            var file1 = new VirtualFile("a.cpp", fileResolver1);
            var file2 = new VirtualFile("b.hpp", fileResolver2);

            var resolver = MakeResolver(initialContent: new VirtualEntry[] { file1, file2 });
            var dir = new VirtualDirectory("root", resolver, StringComparison.Ordinal);

            dir.Files("*").ShouldBe(new[] { file1, file2 }, ignoreOrder: true);
        }

        [Fact]
        public void Files_WithExtensionFilter_ReturnsMatchingFiles()
        {
            var fileResolver1 = Substitute.For<IFileContentResolver>();
            fileResolver1.FullName.Returns("/root/a.cpp");
            var fileResolver2 = Substitute.For<IFileContentResolver>();
            fileResolver2.FullName.Returns("/root/b.hpp");
            var file1 = new VirtualFile("a.cpp", fileResolver1);
            var file2 = new VirtualFile("b.hpp", fileResolver2);

            var resolver = MakeResolver(initialContent: new VirtualEntry[] { file1, file2 });
            var dir = new VirtualDirectory("root", resolver, StringComparison.Ordinal);

            dir.Files("*.cpp").ShouldBe(new[] { file1 });
        }

        [Fact]
        public void Files_WithNullSearchString_ThrowsArgumentNullException()
        {
            var resolver = MakeResolver();
            var dir = new VirtualDirectory("root", resolver, StringComparison.Ordinal);

            Should.Throw<ArgumentNullException>(() => dir.Files(null).ToList());
        }

        [Fact]
        public void DeleteIfEmpty_WhenEmpty_DeletesDirectory()
        {
            var resolver = MakeResolver();
            var dir = new VirtualDirectory("root", resolver, StringComparison.Ordinal);

            Should.NotThrow(() => dir.DeleteIfEmpty());

            resolver.Received().Delete(false);
        }

        [Fact]
        public void DeleteIfEmpty_WhenNotEmpty_DoesNotDelete()
        {
            var fileResolver = Substitute.For<IFileContentResolver>();
            fileResolver.FullName.Returns("/root/file.txt");
            var file = new VirtualFile("file.txt", fileResolver);

            var resolver = MakeResolver(initialContent: new VirtualEntry[] { file });
            var dir = new VirtualDirectory("root", resolver, StringComparison.Ordinal);

            dir.DeleteIfEmpty();

            resolver.DidNotReceive().Delete(true);
        }
    }

    public class VirtualFileTests
    {
        private static VirtualFile MakeFile(string name = "test.txt", string fullName = "/root/test.txt",
                                            Stream readStream = null)
        {
            var resolver = Substitute.For<IFileContentResolver>();
            resolver.FullName.Returns(fullName);
            resolver.GetContent(false, false).Returns(readStream ?? new MemoryStream());
            resolver.GetContent(true, false).Returns(new MemoryStream());
            resolver.GetContent(true, true).Returns(new MemoryStream());
            return new VirtualFile(name, resolver);
        }

        [Fact]
        public void CopyTo_NullDestination_ThrowsArgumentNullException()
        {
            var file = MakeFile();

            Should.Throw<ArgumentNullException>(() => file.CopyTo(null));
        }

        [Fact]
        public void CopyTo_FileAlreadyExistsInDestination_ThrowsFormattableIoException()
        {
            // Build a destination directory that already contains a file with the same name
            var existingFileResolver = Substitute.For<IFileContentResolver>();
            existingFileResolver.FullName.Returns("/dest/test.txt");
            var existingFile = new VirtualFile("test.txt", existingFileResolver);

            var destResolver = Substitute.For<IDirectoryContentResolver>();
            destResolver.FullName.Returns("/dest");
            destResolver.GetContent().Returns(new VirtualEntry[] { existingFile });
            var destDir = new VirtualDirectory("dest", destResolver, StringComparison.Ordinal);

            var file = MakeFile("test.txt", "/root/test.txt");

            Should.Throw<FormattableIoException>(() => file.CopyTo(destDir));
        }

        [Fact]
        public void CopyTo_NewFile_CopiesContentToDestination()
        {
            byte[] data = { 1, 2, 3, 4, 5 };
            var readStream = new MemoryStream(data);
            var file = MakeFile("source.txt", "/root/source.txt", readStream);

            var writeStream = new MemoryStream();
            var destFileResolver = Substitute.For<IFileContentResolver>();
            destFileResolver.FullName.Returns("/dest/source.txt");
            destFileResolver.GetContent(true, false).Returns(writeStream);
            destFileResolver.GetContent(true, true).Returns(writeStream);

            var destResolver = Substitute.For<IDirectoryContentResolver>();
            destResolver.FullName.Returns("/dest");
            destResolver.GetContent().Returns(Enumerable.Empty<VirtualEntry>());
            destResolver.Create<VirtualFile>("source.txt").Returns(new VirtualFile("source.txt", destFileResolver));
            var destDir = new VirtualDirectory("dest", destResolver, StringComparison.Ordinal);

            VirtualFile result = file.CopyTo(destDir);

            result.ShouldNotBeNull();
            result.Name.ShouldBe("source.txt");
            writeStream.ToArray().ShouldBe(data);
        }

        [Fact]
        public void Name_ReturnsConstructorValue()
        {
            var file = MakeFile("myfile.cpp");

            file.Name.ShouldBe("myfile.cpp");
        }
    }
}
