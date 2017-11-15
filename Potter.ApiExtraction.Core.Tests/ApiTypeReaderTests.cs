using System;
using System.Collections.Generic;
using System.Linq;
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
            Expectation expectation = ExpectedTypes.EmptyClass;

            // Assert
            ReadExpectation(apiTypeReader, expectation);
        }

        [TestMethod]
        public void Read_SimpleProperties()
        {
            // Arrange
            var apiTypeReader = new ApiTypeReader();
            Expectation expectation = ExpectedTypes.SimpleProperties;

            // Assert
            ReadExpectation(apiTypeReader, expectation);
        }

        [TestMethod]
        public void Read_SimpleEvents()
        {
            // Arrange
            var apiTypeReader = new ApiTypeReader();
            Expectation expectation = ExpectedTypes.SimpleEvents;

            // Assert
            ReadExpectation(apiTypeReader, expectation);
        }

        [TestMethod]
        public void Read_IndexerProperties()
        {
            // Arrange
            var apiTypeReader = new ApiTypeReader();
            Expectation expectation = ExpectedTypes.IndexerProperties;

            // Assert
            ReadExpectation(apiTypeReader, expectation);
        }

        [TestMethod]
        public void Read_SimpleMethods()
        {
            // Arrange
            var apiTypeReader = new ApiTypeReader();
            Expectation expectation = ExpectedTypes.SimpleMethods;

            // Assert
            ReadExpectation(apiTypeReader, expectation);
        }

        [TestMethod]
        public void Read_GenericMethods()
        {
            // Arrange
            var apiTypeReader = new ApiTypeReader();
            Expectation expectation = ExpectedTypes.GenericMethods;

            // Assert
            ReadExpectation(apiTypeReader, expectation);
        }

        [TestMethod]
        public void Read_GenericMethodsWithConstraints()
        {
            // Arrange
            var apiTypeReader = new ApiTypeReader();
            Expectation expectation = ExpectedTypes.GenericMethodsWithConstraints;

            // Assert
            ReadExpectation(apiTypeReader, expectation);
        }

        [TestMethod]
        public void Read_GenericClassOf1()
        {
            // Arrange
            var apiTypeReader = new ApiTypeReader();
            Expectation expectation = ExpectedTypes.GenericClassOf1;

            // Assert
            ReadExpectation(apiTypeReader, expectation);
        }

        [TestMethod]
        public void Read_GenericClassOf2()
        {
            // Arrange
            var apiTypeReader = new ApiTypeReader();
            Expectation expectation = ExpectedTypes.GenericClassOf2;

            // Assert
            ReadExpectation(apiTypeReader, expectation);
        }

        [TestMethod]
        public void Read_GenericClassWithClassConstraint()
        {
            // Arrange
            var apiTypeReader = new ApiTypeReader();
            Expectation expectation = ExpectedTypes.GenericClassWithClassConstraint;

            // Assert
            ReadExpectation(apiTypeReader, expectation);
        }

        [TestMethod]
        public void Read_GenericClassWithValueConstraint()
        {
            // Arrange
            var apiTypeReader = new ApiTypeReader();
            Expectation expectation = ExpectedTypes.GenericClassWithValueConstraint;

            // Assert
            ReadExpectation(apiTypeReader, expectation);
        }

        [TestMethod]
        public void Read_GenericClassWithMultipleConstraints()
        {
            // Arrange
            var apiTypeReader = new ApiTypeReader();
            Expectation expectation = ExpectedTypes.GenericClassWithMultipleConstraints;

            // Assert
            ReadExpectation(apiTypeReader, expectation);
        }

        [TestMethod]
        public void Read_ClassWithConstructors()
        {
            // Arrange
            var apiTypeReader = new ApiTypeReader();
            Expectation expectation = ExpectedTypes.ClassWithConstructors;

            // Assert
            ReadExpectation(apiTypeReader, expectation);
        }

        [TestMethod]
        public void Read_GenericClassWithConstructors()
        {
            // Arrange
            var apiTypeReader = new ApiTypeReader();
            Expectation expectation = ExpectedTypes.GenericClassWithConstructors;

            // Assert
            ReadExpectation(apiTypeReader, expectation);
        }

        [TestMethod]
        public void Read_ClassWithStaticMembers()
        {
            // Arrange
            var apiTypeReader = new ApiTypeReader();
            Expectation expectation = ExpectedTypes.ClassWithStaticMembers;

            // Assert
            ReadExpectation(apiTypeReader, expectation);
        }

        [TestMethod]
        public void Read_EmptyStaticClass()
        {
            // Arrange
            var apiTypeReader = new ApiTypeReader();
            Expectation expectation = ExpectedTypes.EmptyStaticClass;

            // Assert
            ReadExpectation(apiTypeReader, expectation);
        }

        #region Helpers

        public static void ReadExpectation(ApiTypeReader typeReader, Expectation expectation)
        {
            var typeNameResolver = new TypeNameResolver
            {
                SimplifyNamespaces = true,
            };

            // Act
            var compilationUnit = typeReader.ReadCompilationUnit(expectation.Type, typeNameResolver);

            // Assert
            AssertCompilationUnit(expectation.CompilationUnit, compilationUnit);
        }

        public static void AssertCompilationUnit(CompilationUnitExpectation expected, CompilationUnitSyntax actual)
        {
            // Compare usings.
            AssertSequence(expected.Usings, actual.Usings, (expectedUsing, actualUsing) =>
            {
                Assert.AreEqual(expectedUsing, actualUsing.ToString());
            });

            // Compare namespaces.
            AssertSequence(expected.Namespaces, actual.Members, (expectedMember, actualMember) =>
            {
                if (actualMember is NamespaceDeclarationSyntax actualNamespace)
                {
                    AssertNamespaceDeclaration(expectedMember, actualNamespace);
                }

                Assert.IsInstanceOfType(actualMember, typeof(NamespaceDeclarationSyntax));
            });
        }

        public static void AssertNamespaceDeclaration(NamespaceExpectation expected, NamespaceDeclarationSyntax actual)
        {
            Assert.AreEqual(expected.Namespace, actual.Name.ToString());

            AssertSequence(expected.Types, actual.Members, (expectedMember, actualMember) =>
            {
                if (actualMember is InterfaceDeclarationSyntax actualInterface)
                {
                    AssertInterfaceDeclaration(expectedMember, actualInterface);
                }

                Assert.IsInstanceOfType(actualMember, typeof(InterfaceDeclarationSyntax));
            });
        }

        public static void AssertInterfaceDeclaration(TypeExpectation expected, InterfaceDeclarationSyntax actual)
        {
            Assert.AreEqual(expected.Declaration, getDeclaration(actual));
            AssertConstraints(expected.Constraints, actual.ConstraintClauses);

            AssertSequence(expected.Members, actual.Members, AssertMemberDeclaration);
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
            AssertSequence(expected, actual, (expectedConstraint, actualConstraint) =>
            {
                Assert.AreEqual(expectedConstraint, actualConstraint.ToString());
            });
        }

        public static void AssertSequence<TExpected, TActual>(IEnumerable<TExpected> expected, IEnumerable<TActual> actual, Action<TExpected, TActual> assert)
        {
            IEnumerator<TExpected> expectedEnumerator = expected.GetEnumerator();
            IEnumerator<TActual> actualEnumerator = actual.GetEnumerator();

            while (expectedEnumerator.MoveNext())
            {
                if (actualEnumerator.MoveNext() == false)
                {
                    Assert.AreEqual(expected.Count(), actual.Count(), "Too few items.");
                }

                assert(expectedEnumerator.Current, actualEnumerator.Current);
            }

            if (actualEnumerator.MoveNext())
            {
                Assert.AreEqual(expected.Count(), actual.Count(), "Too many items.");
            }
        }

        #endregion
    }
}
