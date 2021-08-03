﻿using System;
using System.IO;
using NUnit.Framework;

namespace Atata.Tests.DataProvision
{
    [TestFixture]
    public class DirectorySubjectTests
    {
        [Test]
        public void Name() =>
            new DirectorySubject(Path.Combine("Parent", "Dir"))
                .Name.Should.Equal("Dir");

        [Test]
        public void FullName() =>
            new DirectorySubject(Path.Combine("Parent", "Dir"))
                .FullName.Should.Equal(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Parent", "Dir"));

        [TestFixture]
        public static class Exists
        {
            [Test]
            public static void True() =>
                new DirectorySubject(AppDomain.CurrentDomain.BaseDirectory)
                    .Exists.Should.BeTrue();

            [Test]
            public static void False() =>
                new DirectorySubject(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MissingDirectory"))
                    .Exists.Should.BeFalse();
        }

        [TestFixture]
        public static class Directories
        {
            private static DirectoryFixture s_directoryFixture;

            private static DirectorySubject s_sut;

            [OneTimeSetUp]
            public static void SetUpFixture()
            {
                s_directoryFixture = DirectoryFixture.CreateUniqueDirectory()
                    .CreateDirectory("dir1")
                    .CreateDirectory(Path.Combine("dir1", "dir1_1"))
                    .CreateDirectory(Path.Combine("dir1", "dir1_2"))
                    .CreateDirectory(Path.Combine("dir1", "dir1_3"))
                    .CreateDirectory("dir2");

                s_sut = new DirectorySubject(s_directoryFixture.DirectoryPath, "sut");
            }

            [OneTimeTearDown]
            public static void TearDownFxture() =>
                s_directoryFixture.Dispose();

            [Test]
            public static void Count() =>
                s_sut.Directories.Count().Should.Equal(2);

            [Test]
            public static void Count_ProviderName() =>
                s_sut.Directories.Count().ProviderName.ToResultSubject()
                    .Should.Equal("sut.Directories.Count()");

            [Test]
            public static void IntIndexer() =>
                s_sut.Directories[0].Name.Should.Equal("dir1");

            [Test]
            public static void IntIndexer_ProviderName() =>
                s_sut.Directories[0].ProviderName.ToResultSubject()
                    .Should.Equal("sut.Directories[0]");

            [Test]
            public static void StringIndexer() =>
                s_sut.Directories["dir1"].Should.Exist();

            [Test]
            public static void StringIndexer_OfMissingDirectory() =>
                new DirectorySubject(Guid.NewGuid().ToString()).Directories["missing"].Should.Not.Exist();

            [Test]
            public static void StringIndexer_ProviderName() =>
                s_sut.Directories["dir1"].ProviderName.ToResultSubject()
                    .Should.Equal("sut.Directories[\"dir1\"]");

            [Test]
            public static void StringIndexer_ForSubDirectories() =>
                s_sut.Directories["dir1"].Directories["dir1_2"].Should.Exist();

            [Test]
            public static void StringIndexer_ForSubDirectories_ProviderName() =>
                s_sut.Directories["dir1"].Directories["dir1_2"].ProviderName.ToResultSubject()
                    .Should.Equal("sut.Directories[\"dir1\"].Directories[\"dir1_2\"]");

            [Test]
            public static void SubDirectoriesCount() =>
                s_sut.Directories[0].Directories.Count().Should.Equal(3);

            [Test]
            public static void SubDirectoriesCount_ProviderName() =>
                s_sut.Directories[0].Directories.Count().ProviderName.ToSubject()
                    .Should.Equal("sut.Directories[0].Directories.Count()");

            [Test]
            public static void Names() =>
                s_sut.Directories["dir1"].Directories.Names
                    .Should.EqualSequence("dir1_1", "dir1_2", "dir1_3");

            [Test]
            public static void Names_ProviderName() =>
                s_sut.Directories["dir1"].Directories.Names.ProviderName.ToResultSubject()
                    .Should.Equal("sut.Directories[\"dir1\"].Directories.Names");
        }

        [TestFixture]
        public static class Files
        {
            private static DirectoryFixture s_directoryFixture;

            private static DirectorySubject s_sut;

            [OneTimeSetUp]
            public static void SetUpFixture()
            {
                s_directoryFixture = DirectoryFixture.CreateUniqueDirectory()
                    .CreateFile("1.txt")
                    .CreateFile("2.txt");

                s_sut = new DirectorySubject(s_directoryFixture.DirectoryPath, "sut");
            }

            [OneTimeTearDown]
            public static void TearDownFxture() =>
                s_directoryFixture.Dispose();

            [Test]
            public static void Count() =>
                s_sut.Files.Count().Should.Equal(2);

            [Test]
            public static void Count_ProviderName() =>
                s_sut.Files.Count().ProviderName.ToResultSubject()
                    .Should.Equal("sut.Files.Count()");

            [Test]
            public static void IntIndexer() =>
                s_sut.Files[0].Name.Should.Equal("1.txt");

            [Test]
            public static void StringIndexer() =>
                s_sut.Files["1.txt"].Should.Exist();

            [Test]
            public static void StringIndexer_ProviderName() =>
                s_sut.Files["1.txt"].ProviderName.ToResultSubject()
                    .Should.Equal("sut.Files[\"1.txt\"]");

            [Test]
            public static void Where_First() =>
                s_sut.Files.Where(x => x.Extension != ".ext").First()
                    .Name.Should.Equal("1.txt");

            [Test]
            public static void Where_First_ProviderName() =>
                s_sut.Files.Where(x => x.Extension != ".ext").First()
                    .ProviderName.ToResultSubject().Should.Equal("sut.Files.Where(x => x.Extension != \".ext\").First()");

            [Test]
            public static void Names() =>
                s_sut.Files.Names
                    .Should.BeEquivalent("1.txt", "2.txt");

            [Test]
            public static void Names_ProviderName() =>
                s_sut.Files.Names.ProviderName.ToResultSubject()
                    .Should.Equal("sut.Files.Names");

            [Test]
            public static void ThruMissingSubDirectory() =>
                s_sut.Directories["missing"].Files["missing.txt"].Should.Not.Exist();
        }
    }
}
