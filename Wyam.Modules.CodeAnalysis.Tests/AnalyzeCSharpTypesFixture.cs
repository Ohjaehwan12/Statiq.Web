﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Wyam.Common;

namespace Wyam.Modules.CodeAnalysis.Tests
{
    [TestFixture]
    public class AnalyzeCSharpTypesFixture : AnalyzeCSharpFixtureBase
    {
        [Test]
        public void ReturnsAllTypes()
        {
            // Given
            string code = @"
                namespace Foo
                {
                    public class Blue
                    {
                    }

                    class Green
                    {
                        class Red
                        {
                        }
                    }

                    internal struct Yellow
                    {
                    }

                    enum Orange
                    {
                    }
                }
            ";
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
            IDocument document = Substitute.For<IDocument>();
            document.GetStream().Returns(stream);
            IExecutionContext context = Substitute.For<IExecutionContext>();
            context.GetNewDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(x => new TestDocument((IEnumerable<KeyValuePair<string, object>>)x[2]));
            IModule module = new AnalyzeCSharp();

            // When
            List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            CollectionAssert.AreEquivalent(new[] { string.Empty, "Foo", "Blue", "Green", "Red", "Yellow", "Orange" }, results.Select(x => x["Name"]));
            stream.Dispose();
        }

        [Test]
        public void MemberTypesReturnsNestedTypes()
        {
            // Given
            string code = @"
                namespace Foo
                {
                    class Green
                    {
                        public class Blue
                        {
                        }

                        private struct Red
                        {
                        }

                        enum Yellow
                        {
                        }
                    }
                }
            ";
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
            IDocument document = Substitute.For<IDocument>();
            document.GetStream().Returns(stream);
            IExecutionContext context = Substitute.For<IExecutionContext>();
            context.GetNewDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(x => new TestDocument((IEnumerable<KeyValuePair<string, object>>)x[2]));
            IModule module = new AnalyzeCSharp();

            // When
            List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            CollectionAssert.AreEquivalent(new[] { "Blue", "Red", "Yellow" }, 
                results.Single(x => x["Name"].Equals("Green")).Get<IEnumerable<IDocument>>("MemberTypes").Select(x => x["Name"]));
            stream.Dispose();
        }

        [Test]
        public void FullNameContainsContainingType()
        {
            // Given
            string code = @"
                namespace Foo
                {
                    public class Green
                    {
                        private class Blue
                        {
                        }
                    }

                    struct Red
                    {
                    }
                }

                namespace Foo.Bar
                {
                    enum Yellow
                    {
                    }
                }
            ";
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
            IDocument document = Substitute.For<IDocument>();
            document.GetStream().Returns(stream);
            IExecutionContext context = Substitute.For<IExecutionContext>();
            context.GetNewDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(x => new TestDocument((IEnumerable<KeyValuePair<string, object>>)x[2]));
            IModule module = new AnalyzeCSharp();

            // When
            List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            CollectionAssert.AreEquivalent(new[] { string.Empty, "Foo", "Green", "Green.Blue", "Red", "Yellow", "Bar" }, results.Select(x => x["FullName"]));
            stream.Dispose();
        }

        [Test]
        public void DisplayNameContainsContainingType()
        {
            // Given
            string code = @"
                namespace Foo
                {
                    public class Green
                    {
                        private class Blue
                        {
                        }
                    }

                    struct Red
                    {
                    }
                }

                namespace Foo.Bar
                {
                    enum Yellow
                    {
                    }
                }
            ";
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
            IDocument document = Substitute.For<IDocument>();
            document.GetStream().Returns(stream);
            IExecutionContext context = Substitute.For<IExecutionContext>();
            context.GetNewDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(x => new TestDocument((IEnumerable<KeyValuePair<string, object>>)x[2]));
            IModule module = new AnalyzeCSharp();

            // When
            List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            CollectionAssert.AreEquivalent(new[] { "global", "Foo", "Green", "Green.Blue", "Red", "Yellow", "Foo.Bar" }, results.Select(x => x["DisplayName"]));
            stream.Dispose();
        }

        [Test]
        public void QualifiedNameContainsNamespaceAndContainingType()
        {
            // Given
            string code = @"
                namespace Foo
                {
                    public class Green
                    {
                        private class Blue
                        {
                        }
                    }

                    struct Red
                    {
                    }
                }

                namespace Foo.Bar
                {
                    enum Yellow
                    {
                    }
                }
            ";
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
            IDocument document = Substitute.For<IDocument>();
            document.GetStream().Returns(stream);
            IExecutionContext context = Substitute.For<IExecutionContext>();
            context.GetNewDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(x => new TestDocument((IEnumerable<KeyValuePair<string, object>>)x[2]));
            IModule module = new AnalyzeCSharp();

            // When
            List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            CollectionAssert.AreEquivalent(new[] { String.Empty, "Foo", "Foo.Green", "Foo.Green.Blue", "Foo.Red", "Foo.Bar.Yellow", "Foo.Bar" }, results.Select(x => x["QualifiedName"]));
            stream.Dispose();
        }

        [Test]
        public void ContainingNamespaceIsCorrect()
        {
            // Given
            string code = @"
                namespace Foo
                {
                    class Green
                    {
                        class Blue
                        {
                        }
                    }

                    struct Red
                    {
                    }
                }

                namespace Foo.Bar
                {
                    enum Yellow
                    {
                    }
                }
            ";
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
            IDocument document = Substitute.For<IDocument>();
            document.GetStream().Returns(stream);
            IExecutionContext context = Substitute.For<IExecutionContext>();
            context.GetNewDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(x => new TestDocument((IEnumerable<KeyValuePair<string, object>>)x[2]));
            IModule module = new AnalyzeCSharp();

            // When
            List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            Assert.AreEqual("Foo", results.Single(x => x["Name"].Equals("Green")).Get<IDocument>("ContainingNamespace")["Name"]);
            Assert.AreEqual("Foo", results.Single(x => x["Name"].Equals("Blue")).Get<IDocument>("ContainingNamespace")["Name"]);
            Assert.AreEqual("Foo", results.Single(x => x["Name"].Equals("Red")).Get<IDocument>("ContainingNamespace")["Name"]);
            Assert.AreEqual("Bar", results.Single(x => x["Name"].Equals("Yellow")).Get<IDocument>("ContainingNamespace")["Name"]);
            stream.Dispose();
        }

        [Test]
        public void ContainingTypeIsCorrect()
        {
            // Given
            string code = @"
                namespace Foo
                {
                    class Green
                    {
                        class Blue
                        {
                        }
                    }

                    struct Red
                    {
                    }
                }

                namespace Foo.Bar
                {
                    enum Yellow
                    {
                    }
                }
            ";
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
            IDocument document = Substitute.For<IDocument>();
            document.GetStream().Returns(stream);
            IExecutionContext context = Substitute.For<IExecutionContext>();
            context.GetNewDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(x => new TestDocument((IEnumerable<KeyValuePair<string, object>>)x[2]));
            IModule module = new AnalyzeCSharp();

            // When
            List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            Assert.IsNull(results.Single(x => x["Name"].Equals("Green")).Get<IDocument>("ContainingType"));
            Assert.AreEqual("Green", results.Single(x => x["Name"].Equals("Blue")).Get<IDocument>("ContainingType")["Name"]);
            Assert.IsNull(results.Single(x => x["Name"].Equals("Red")).Get<IDocument>("ContainingType"));
            Assert.IsNull(results.Single(x => x["Name"].Equals("Yellow")).Get<IDocument>("ContainingType"));
            stream.Dispose();
        }

        [Test]
        public void KindIsCorrect()
        {
            // Given
            string code = @"
                namespace Foo
                {
                    public class Green
                    {
                        private class Blue
                        {
                        }
                    }

                    struct Red
                    {
                    }
                }

                namespace Foo.Bar
                {
                    enum Yellow
                    {
                    }
                }
            ";
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
            IDocument document = Substitute.For<IDocument>();
            document.GetStream().Returns(stream);
            IExecutionContext context = Substitute.For<IExecutionContext>();
            context.GetNewDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(x => new TestDocument((IEnumerable<KeyValuePair<string, object>>)x[2]));
            IModule module = new AnalyzeCSharp();

            // When
            List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            Assert.AreEqual("NamedType", results.Single(x => x["Name"].Equals("Green"))["Kind"]);
            Assert.AreEqual("NamedType", results.Single(x => x["Name"].Equals("Blue"))["Kind"]);
            Assert.AreEqual("NamedType", results.Single(x => x["Name"].Equals("Red"))["Kind"]);
            Assert.AreEqual("NamedType", results.Single(x => x["Name"].Equals("Yellow"))["Kind"]);
            stream.Dispose();
        }

        [Test]
        public void SpecificKindIsCorrect()
        {
            // Given
            string code = @"
                namespace Foo
                {
                    public class Green
                    {
                        private class Blue
                        {
                        }
                    }

                    struct Red
                    {
                    }
                }

                namespace Foo.Bar
                {
                    enum Yellow
                    {
                    }
                }
            ";
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
            IDocument document = Substitute.For<IDocument>();
            document.GetStream().Returns(stream);
            IExecutionContext context = Substitute.For<IExecutionContext>();
            context.GetNewDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(x => new TestDocument((IEnumerable<KeyValuePair<string, object>>)x[2]));
            IModule module = new AnalyzeCSharp();

            // When
            List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            Assert.AreEqual("Class", results.Single(x => x["Name"].Equals("Green"))["SpecificKind"]);
            Assert.AreEqual("Class", results.Single(x => x["Name"].Equals("Blue"))["SpecificKind"]);
            Assert.AreEqual("Struct", results.Single(x => x["Name"].Equals("Red"))["SpecificKind"]);
            Assert.AreEqual("Enum", results.Single(x => x["Name"].Equals("Yellow"))["SpecificKind"]);
            stream.Dispose();
        }

        [Test]
        public void BaseTypeIsCorrect()
        {
            // Given
            string code = @"
                namespace Foo
                {
                    public class Red
                    {
                    }

                    public class Green : Red
                    {
                    }

                    struct Blue
                    {
                    }

                    interface Yellow
                    {
                    }

                    enum Orange
                    {
                    }
                }
            ";
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
            IDocument document = Substitute.For<IDocument>();
            document.GetStream().Returns(stream);
            IExecutionContext context = Substitute.For<IExecutionContext>();
            context.GetNewDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(x => new TestDocument((IEnumerable<KeyValuePair<string, object>>)x[2]));
            IModule module = new AnalyzeCSharp();

            // When
            List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            Assert.AreEqual("Object", results.Single(x => x["Name"].Equals("Red")).Get<IDocument>("BaseType")["Name"]);
            Assert.AreEqual("Red", results.Single(x => x["Name"].Equals("Green")).Get<IDocument>("BaseType")["Name"]);
            Assert.AreEqual("ValueType", results.Single(x => x["Name"].Equals("Blue")).Get<IDocument>("BaseType")["Name"]);
            Assert.IsNull(results.Single(x => x["Name"].Equals("Yellow")).Get<IDocument>("BaseType"));
            Assert.AreEqual("Enum", results.Single(x => x["Name"].Equals("Orange")).Get<IDocument>("BaseType")["Name"]);
            stream.Dispose();
        }

        [Test]
        public void MembersReturnsAlMembers()
        {
            // Given
            string code = @"
                namespace Foo
                {
                    public class Blue
                    {
                        void Green()
                        {
                        }

                        int Red { get; }

                        string _yellow;

                        event ChangedEventHandler Changed;
                    }
                }
            ";
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
            IDocument document = Substitute.For<IDocument>();
            document.GetStream().Returns(stream);
            IExecutionContext context = Substitute.For<IExecutionContext>();
            context.GetNewDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(x => new TestDocument((IEnumerable<KeyValuePair<string, object>>)x[2]));
            IModule module = new AnalyzeCSharp();

            // When
            List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            CollectionAssert.AreEquivalent(new[] { "Green", "Red", "_yellow", "Changed" },
                GetClass(results, "Blue").Get<IReadOnlyList<IDocument>>("Members").Select(x => x["Name"]));
            stream.Dispose();
        }

        [Test]
        public void WritePathIsCorrect()
        {
            // Given
            string code = @"
                namespace Foo
                {
                    class Red
                    {
                    }

                    enum Green
                    {
                    }

                    namespace Bar
                    {
                        struct Blue
                        {
                        }
                    }
                }

                class Yellow
                {
                }
            ";
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
            IDocument document = Substitute.For<IDocument>();
            document.GetStream().Returns(stream);
            IExecutionContext context = Substitute.For<IExecutionContext>();
            context.GetNewDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(x => new TestDocument((IEnumerable<KeyValuePair<string, object>>)x[2]));
            IModule module = new AnalyzeCSharp();

            // When
            List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            CollectionAssert.AreEquivalent(new[] { "Foo\\414E2165\\index.html", "Foo.Bar\\92C5B5C5\\index.html", "439037DE\\index.html", "Foo\\53AB53EF\\index.html" },
                results.Where(x => x["Kind"].Equals("NamedType")).Select(x => x["WritePath"]));
            stream.Dispose();
        }

        [Test]
        public void GetDocumentForExternalBaseType()
        {
            // Given
            string code = @"
                namespace Foo
                {
                    class Red
                    {
                    }
                }
            ";
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
            IDocument document = Substitute.For<IDocument>();
            document.GetStream().Returns(stream);
            IExecutionContext context = Substitute.For<IExecutionContext>();
            context.GetNewDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(x => new TestDocument((IEnumerable<KeyValuePair<string, object>>)x[2]));
            IModule module = new AnalyzeCSharp();

            // When
            List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            Assert.AreEqual("Object", GetClass(results, "Red").Get<IDocument>("BaseType")["Name"]);
            stream.Dispose();
        }

        [Test]
        public void GetDocumentsForExternalInterfaces()
        {
            // Given
            string code = @"
                namespace Foo
                {
                    class Red : IBlue, IFoo
                    {
                    }

                    interface IBlue
                    {
                    }
                }
            ";
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(code));
            IDocument document = Substitute.For<IDocument>();
            document.GetStream().Returns(stream);
            IExecutionContext context = Substitute.For<IExecutionContext>();
            context.GetNewDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<KeyValuePair<string, object>>>())
                .Returns(x => new TestDocument((IEnumerable<KeyValuePair<string, object>>)x[2]));
            IModule module = new AnalyzeCSharp();

            // When
            List<IDocument> results = module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

            // Then
            CollectionAssert.AreEquivalent(new[] { string.Empty, "Foo", "Red", "IBlue" }, results.Select(x => x["Name"]));
            CollectionAssert.AreEquivalent(new [] { "IBlue", "IFoo" }, GetClass(results, "Red").Get<IEnumerable<IDocument>>("AllInterfaces").Select(x => x["Name"]));
            stream.Dispose();
        }

        // TODO: Test that Name does not contain generic type parameters
        // TODO: Test that FullName contains generic type parameters
        // TODO: Test that QualifiedName contains generic type parameters
    }
}
