using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Potter.ApiExtraction.Core.Generation;
using Potter.ApiExtraction.Core.Tests.Constants;

namespace Potter.ApiExtraction.Core.Tests
{
    [TestClass]
    public class ApiTypeReaderTests
    {
        [TestMethod]
        public void Read_EmptyClass()
        {
            // Arrange
            var apiTypeReader = new ApiTypeReader();
            (Type type, CompilationUnitExpectation expectation) = ExpectedTypes.EmptyClass;

            // Assert
            ReadExpectation(apiTypeReader, type, expectation);
        }

        [TestMethod]
        public void Read_SimpleProperties()
        {
            // Arrange
            var apiTypeReader = new ApiTypeReader();
            (Type type, CompilationUnitExpectation expectation) = ExpectedTypes.SimpleProperties;

            // Assert
            ReadExpectation(apiTypeReader, type, expectation);
        }

        [TestMethod]
        public void Read_SimpleEvents()
        {
            // Arrange
            var apiTypeReader = new ApiTypeReader();
            (Type type, CompilationUnitExpectation expectation) = ExpectedTypes.SimpleEvents;

            // Assert
            ReadExpectation(apiTypeReader, type, expectation);
        }

        [TestMethod]
        public void Read_IndexerProperties()
        {
            // Arrange
            var apiTypeReader = new ApiTypeReader();
            (Type type, CompilationUnitExpectation expectation) = ExpectedTypes.IndexerProperties;

            // Assert
            ReadExpectation(apiTypeReader, type, expectation);
        }

        [TestMethod]
        public void Read_SimpleMethods()
        {
            // Arrange
            var apiTypeReader = new ApiTypeReader();
            (Type type, CompilationUnitExpectation expectation) = ExpectedTypes.SimpleMethods;

            // Assert
            ReadExpectation(apiTypeReader, type, expectation);
        }

        [TestMethod]
        public void Read_GenericMethods()
        {
            // Arrange
            var apiTypeReader = new ApiTypeReader();
            (Type type, CompilationUnitExpectation expectation) = ExpectedTypes.GenericMethods;

            // Assert
            ReadExpectation(apiTypeReader, type, expectation);
        }

        [TestMethod]
        public void Read_GenericMethodsWithConstraints()
        {
            // Arrange
            var apiTypeReader = new ApiTypeReader();
            (Type type, CompilationUnitExpectation expectation) = ExpectedTypes.GenericMethodsWithConstraints;

            // Assert
            ReadExpectation(apiTypeReader, type, expectation);
        }

        #region Helpers

        public static void ReadExpectation(ApiTypeReader typeReader, Type type, CompilationUnitExpectation expectation)
        {
            var typeNameResolver = new TypeNameResolver
            {
                SimplifyNamespaces = true,
            };

            // Act
            var compilationUnit = typeReader.ReadCompilationUnit(type, typeNameResolver);

            // Assert
            AssertCompilationUnit(expectation, compilationUnit);
        }

        public static void AssertCompilationUnit(CompilationUnitExpectation expected, CompilationUnitSyntax actual)
        {
            // Compare usings.
            IEnumerator<string> expectedUsingEnumerator = expected.Usings.GetEnumerator();
            IEnumerator<UsingDirectiveSyntax> actualUsingEnumerator = ((IEnumerable<UsingDirectiveSyntax>) actual.Usings).GetEnumerator();

            while (expectedUsingEnumerator.MoveNext())
            {
                Assert.IsTrue(actualUsingEnumerator.MoveNext());

                Assert.AreEqual(expectedUsingEnumerator.Current, actualUsingEnumerator.Current.ToString());
            }

            Assert.IsFalse(actualUsingEnumerator.MoveNext());

            // Compare namespaces.
            IEnumerator<NamespaceExpectation> expectedMemberEnumerator = expected.Namespaces.GetEnumerator();
            IEnumerator<MemberDeclarationSyntax> actualMemberEnumerator = ((IEnumerable<MemberDeclarationSyntax>) actual.Members).GetEnumerator();

            while (expectedMemberEnumerator.MoveNext())
            {
                Assert.IsTrue(actualMemberEnumerator.MoveNext());

                if (actualMemberEnumerator.Current is NamespaceDeclarationSyntax actualNamespace)
                {
                    AssertNamespaceDeclaration(expectedMemberEnumerator.Current, actualNamespace);
                }

                Assert.IsInstanceOfType(actualMemberEnumerator.Current, typeof(NamespaceDeclarationSyntax));
            }

            Assert.IsFalse(actualMemberEnumerator.MoveNext());
        }

        public static void AssertNamespaceDeclaration(NamespaceExpectation expected, NamespaceDeclarationSyntax actual)
        {
            Assert.AreEqual(expected.Namespace, actual.Name.ToString());

            IEnumerator<TypeExpectation> expectedEnumerator = expected.Types.GetEnumerator();
            IEnumerator<MemberDeclarationSyntax> actualEnumerator = ((IEnumerable<MemberDeclarationSyntax>) actual.Members).GetEnumerator();

            while (expectedEnumerator.MoveNext())
            {
                Assert.IsTrue(actualEnumerator.MoveNext());

                if (actualEnumerator.Current is InterfaceDeclarationSyntax actualInterface)
                {
                    AssertInterfaceDeclaration(expectedEnumerator.Current, actualInterface);
                }

                Assert.IsInstanceOfType(actualEnumerator.Current, typeof(InterfaceDeclarationSyntax));
            }

            Assert.IsFalse(actualEnumerator.MoveNext());
        }

        public static void AssertInterfaceDeclaration(TypeExpectation expected, InterfaceDeclarationSyntax actual)
        {
            Assert.AreEqual(expected.Declaration, getDeclaration(actual));
            AssertConstraints(expected.Constraints, actual.ConstraintClauses);

            IEnumerator<MemberExpectation> expectedEnumerator = expected.Members.GetEnumerator();
            IEnumerator<MemberDeclarationSyntax> actualEnumerator = ((IEnumerable<MemberDeclarationSyntax>) actual.Members).GetEnumerator();

            while (expectedEnumerator.MoveNext())
            {
                Assert.IsTrue(actualEnumerator.MoveNext());

                AssertMemberDeclaration(expectedEnumerator.Current, actualEnumerator.Current);
            }

            if (actualEnumerator.MoveNext())
            {
                Assert.AreEqual(expected.Members.Count, actual.Members.Count);
            }
        }

        private static string getDeclaration(InterfaceDeclarationSyntax actual)
        {
            int totalLength = actual.FullSpan.Length;
            int constraintLength = actual.ConstraintClauses.FullSpan.Length;
            int bodyLength = actual.CloseBraceToken.FullSpan.End - actual.OpenBraceToken.FullSpan.Start;

            return actual.ToString().Substring(0, totalLength - constraintLength - bodyLength);
        }

        public static void AssertMemberDeclaration(MemberExpectation expected, MemberDeclarationSyntax actual)
        {
            Assert.AreEqual(expected.Declaration, getDeclaration(actual));

            if (actual is MethodDeclarationSyntax actualMethod)
            {
                Assert.IsFalse(actualMethod.SemicolonToken.Span.IsEmpty, $"The method does not end with a semicolon. ({actualMethod})");
                AssertConstraints(expected.Constraints, actualMethod.ConstraintClauses);
            }
        }

        private static string getDeclaration(MemberDeclarationSyntax actual)
        {
            if (actual is MethodDeclarationSyntax actualMethod)
            {
                int declarationEnd = actualMethod.ConstraintClauses.Span.IsEmpty ? actualMethod.SemicolonToken.SpanStart : actualMethod.ConstraintClauses.Span.Start;
                int declarationLength = declarationEnd - actualMethod.Span.Start;

                return actualMethod.ToString().Substring(0, declarationLength);
            }

            return actual.ToString();
        }

        public static void AssertConstraints(ICollection<string> expected, SyntaxList<TypeParameterConstraintClauseSyntax> actual)
        {
            Assert.AreEqual(expected.Count, actual.Count, "Wrong number of parameter constraints.");

            if (expected.Count == 0)
            {
                return;
            }

            IEnumerator<string> expectedEnumerator = expected.GetEnumerator();
            IEnumerator<TypeParameterConstraintClauseSyntax> actualEnumerator = ((IEnumerable<TypeParameterConstraintClauseSyntax>) actual).GetEnumerator();

            while (expectedEnumerator.MoveNext())
            {
                Assert.IsTrue(actualEnumerator.MoveNext());

                Assert.AreEqual(expectedEnumerator.Current, actualEnumerator.Current.ToString());
            }

            Assert.IsFalse(actualEnumerator.MoveNext());
        }

        #endregion
    }
}
