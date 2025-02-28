﻿using System.Runtime.CompilerServices;

namespace Atata.UnitTests.DataProvision;

public static class DataVerificationProviderExtensionMethodTests
{
    public class Satisfy_Expression : ExtensionMethodTestFixture<string, Satisfy_Expression>
    {
        static Satisfy_Expression() =>
            For("abc123")
                .Pass(x => x.Satisfy(x => x.Contains("abc") && x.Contains("123")))
                .Fail(x => x.Satisfy(x => x == "xyz"));
    }

    public class Satisfy_Function : ExtensionMethodTestFixture<int, Satisfy_Function>
    {
        static Satisfy_Function() =>
            For(5)
                .Pass(x => x.Satisfy(x => x > 1 && x < 10, "..."))
                .Fail(x => x.Satisfy(x => x == 7, "..."));
    }

    public class Satisfy_IEnumerable_Expression : ExtensionMethodTestFixture<Subject<string>[], Satisfy_IEnumerable_Expression>
    {
        static Satisfy_IEnumerable_Expression() =>
            For(new[] { "a".ToSubject(), "b".ToSubject(), "c".ToSubject() })
                .Pass(x => x.Satisfy(x => x.Contains("a") && x.Contains("c")))
                .Fail(x => x.Satisfy(x => x.Any(y => y.Contains('z'))));
    }

    public abstract class ExtensionMethodTestFixture<TObject, TFixture>
        where TFixture : ExtensionMethodTestFixture<TObject, TFixture>
    {
        private static readonly TestSuiteData s_testSuiteData = new();

        private Subject<TObject> _sut;

        protected static TestSuiteBuilder For(TObject testObject)
        {
            s_testSuiteData.TestObject = testObject;
            return new TestSuiteBuilder(s_testSuiteData);
        }

        public static IEnumerable<TestCaseData> GetPassFunctionsTestCases(string testName) =>
            GetTestCases(s_testSuiteData.PassFunctions, testName);

        public static IEnumerable<TestCaseData> GetFailFunctionsTestCases(string testName) =>
            GetTestCases(s_testSuiteData.FailFunctions, testName);

        private static IEnumerable<TestCaseData> GetTestCases(
            List<Func<IObjectVerificationProvider<TObject, Subject<TObject>>, Subject<TObject>>> functions,
            string testName)
        {
            RuntimeHelpers.RunClassConstructor(typeof(TFixture).TypeHandle);

            return functions.Count == 1
                ? new[] { new TestCaseData(functions[0]).SetName(testName) }
                : functions.Select((x, i) => new TestCaseData(x).SetArgDisplayNames($"#{i + 1}"));
        }

        [OneTimeSetUp]
        public void SetUpFixture() =>
            _sut = s_testSuiteData.TestObject.ToSutSubject();

        [TestCaseSource(nameof(GetPassFunctionsTestCases), new object[] { nameof(Passes) })]
        public void Passes(Func<IObjectVerificationProvider<TObject, Subject<TObject>>, Subject<TObject>> function) =>
            Assert.DoesNotThrow(() =>
                function(_sut.Should));

        [TestCaseSource(nameof(GetFailFunctionsTestCases), new object[] { nameof(Fails) })]
        public void Fails(Func<IObjectVerificationProvider<TObject, Subject<TObject>>, Subject<TObject>> function) =>
            Assert.Throws<AssertionException>(() =>
                function(_sut.Should));

        [TestCaseSource(nameof(GetFailFunctionsTestCases), new object[] { nameof(Not_Passes) })]
        public void Not_Passes(Func<IObjectVerificationProvider<TObject, Subject<TObject>>, Subject<TObject>> function) =>
            Assert.DoesNotThrow(() =>
                function(_sut.Should.Not));

        [TestCaseSource(nameof(GetPassFunctionsTestCases), new object[] { nameof(Not_Fails) })]
        public void Not_Fails(Func<IObjectVerificationProvider<TObject, Subject<TObject>>, Subject<TObject>> function) =>
            Assert.Throws<AssertionException>(() =>
                function(_sut.Should.Not));

        public class TestSuiteData
        {
            public TObject TestObject { get; set; }

            public List<Func<IObjectVerificationProvider<TObject, Subject<TObject>>, Subject<TObject>>> PassFunctions { get; } =
                new List<Func<IObjectVerificationProvider<TObject, Subject<TObject>>, Subject<TObject>>>();

            public List<Func<IObjectVerificationProvider<TObject, Subject<TObject>>, Subject<TObject>>> FailFunctions { get; } =
                new List<Func<IObjectVerificationProvider<TObject, Subject<TObject>>, Subject<TObject>>>();
        }

        public class TestSuiteBuilder
        {
            private readonly TestSuiteData _context;

            public TestSuiteBuilder(TestSuiteData context) =>
                _context = context;

            public TestSuiteBuilder Pass(Func<IObjectVerificationProvider<TObject, Subject<TObject>>, Subject<TObject>> passFunction)
            {
                _context.PassFunctions.Add(passFunction);
                return this;
            }

            public TestSuiteBuilder Fail(Func<IObjectVerificationProvider<TObject, Subject<TObject>>, Subject<TObject>> failFunction)
            {
                _context.FailFunctions.Add(failFunction);
                return this;
            }
        }
    }
}
