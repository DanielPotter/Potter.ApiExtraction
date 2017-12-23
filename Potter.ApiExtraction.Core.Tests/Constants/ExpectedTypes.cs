using System;
using Potter.ApiExtraction.Core.Configuration;
using Potter.ApiExtraction.Core.Tests.Constants;

#region Type Expectations

namespace Potter.ApiExtraction.Core.Tests
{
    public static partial class ExpectedTypes
    {
        private static ApiConfiguration createConfigurationForType(Type type)
        {
            return new ApiConfiguration
            {
                SimplifyNamespaces = true,
                Assemblies = new AssemblyConfiguration[]
                {
                    new AssemblyConfiguration
                    {
                        Name = type.Assembly.FullName,
                        Location = type.Assembly.Location,
                    },
                },
                Groups = new GroupConfiguration[]
                {
                    new GroupConfiguration
                    {
                        Name = "TestGroup",
                        Types = new TypeConfiguration
                        {
                            Mode = TypeMode.Whitelist,
                            Items = new MemberSelector[]
                            {
                                new TypeSelector
                                {
                                    Name = type.Name,
                                },
                            },
                        },
                    },
                },
            };
        }

        public static Expectation EmptyClass { get; } = new Expectation
        {
            Type = typeof(Types.EmptyClass),
            Configuration = createConfigurationForType(typeof(Types.EmptyClass)),
            CompilationUnit = new CompilationUnitExpectation
            {
                Namespaces =
                {
                    new NamespaceExpectation
                    {
                        Namespace = typeof(Types.EmptyClass).Namespace,
                        Types =
                        {
                            new TypeExpectation(TypeKind.Interface)
                            {
                                Declaration = $"publicinterfaceI{nameof(Types.EmptyClass)}",
                            },
                            new TypeExpectation(TypeKind.Interface)
                            {
                                Declaration = $"publicinterfaceI{nameof(Types.EmptyClass)}Factory",
                                Members =
                                {
                                    new MemberExpectation(MemberType.Method)
                                    {
                                        Declaration = $"I{nameof(Types.EmptyClass)}Create{nameof(Types.EmptyClass)}()",
                                    },
                                },
                            },
                        },
                    },
                },
            },
        };

        public static Expectation SimpleFields { get; } = new Expectation
        {
            Type = typeof(Types.SimpleFields),
            Configuration = createConfigurationForType(typeof(Types.SimpleFields)),
            CompilationUnit = new CompilationUnitExpectation
            {
                Namespaces =
                {
                    new NamespaceExpectation
                    {
                        Namespace = typeof(Types.SimpleFields).Namespace,
                        Types =
                        {
                            new TypeExpectation(TypeKind.Interface)
                            {
                                Declaration = $"publicinterfaceI{nameof(Types.SimpleFields)}",
                                Members =
                                {
                                    new MemberExpectation(MemberType.Property)
                                    {
                                        Declaration = $"int{nameof(Types.SimpleFields.Value)}{{get;set;}}",
                                    },
                                    new MemberExpectation(MemberType.Property)
                                    {
                                        Declaration = $"int{nameof(Types.SimpleFields.ReadOnlyValue)}{{get;}}",
                                    },
                                },
                            },
                            new TypeExpectation(TypeKind.Interface)
                            {
                                Declaration = $"publicinterfaceI{nameof(Types.SimpleFields)}Factory",
                                Members =
                                {
                                    new MemberExpectation(MemberType.Method)
                                    {
                                        Declaration = $"I{nameof(Types.SimpleFields)}Create{nameof(Types.SimpleFields)}()",
                                    },
                                },
                            },
                        },
                    },
                },
            },
        };

        public static Expectation SimpleProperties { get; } = new Expectation
        {
            Type = typeof(Types.SimpleProperties),
            Configuration = createConfigurationForType(typeof(Types.SimpleProperties)),
            CompilationUnit = new CompilationUnitExpectation
            {
                Namespaces =
                {
                    new NamespaceExpectation
                    {
                        Namespace = typeof(Types.SimpleProperties).Namespace,
                        Types =
                        {
                            new TypeExpectation(TypeKind.Interface)
                            {
                                Declaration = $"publicinterfaceI{nameof(Types.SimpleProperties)}",
                                Members =
                                {
                                    new MemberExpectation(MemberType.Property)
                                    {
                                        Declaration = $"int{nameof(Types.SimpleProperties.GettableValue)}{{get;}}",
                                    },
                                    new MemberExpectation(MemberType.Property)
                                    {
                                        Declaration = $"int{nameof(Types.SimpleProperties.SettableValue)}{{set;}}",
                                    },
                                    new MemberExpectation(MemberType.Property)
                                    {
                                        Declaration = $"int{nameof(Types.SimpleProperties.Value)}{{get;set;}}",
                                    },
                                },
                            },
                            new TypeExpectation(TypeKind.Interface)
                            {
                                Declaration = $"publicinterfaceI{nameof(Types.SimpleProperties)}Factory",
                                Members =
                                {
                                    new MemberExpectation(MemberType.Method)
                                    {
                                        Declaration = $"I{nameof(Types.SimpleProperties)}Create{nameof(Types.SimpleProperties)}()",
                                    },
                                },
                            },
                        },
                    },
                },
            },
        };

        public static Expectation SimpleEvents { get; } = new Expectation
        {
            Type = typeof(Types.SimpleEvents),
            Configuration = createConfigurationForType(typeof(Types.SimpleEvents)),
            CompilationUnit = new CompilationUnitExpectation
            {
                Usings =
                {
                    "usingSystem;"
                },
                Namespaces =
                {
                    new NamespaceExpectation
                    {
                        Namespace = typeof(Types.SimpleEvents).Namespace,
                        Types =
                        {
                            new TypeExpectation(TypeKind.Interface)
                            {
                                Declaration = $"publicinterfaceI{nameof(Types.SimpleEvents)}",
                                Members =
                                {
                                    new MemberExpectation(MemberType.Event)
                                    {
                                        Declaration = $"eventEventHandler{nameof(Types.SimpleEvents.EventField)};",
                                    },
                                    new MemberExpectation(MemberType.Event)
                                    {
                                        Declaration = $"eventEventHandler{nameof(Types.SimpleEvents.EventProperty)};",
                                    },
                                },
                            },
                            new TypeExpectation(TypeKind.Interface)
                            {
                                Declaration = $"publicinterfaceI{nameof(Types.SimpleEvents)}Factory",
                                Members =
                                {
                                    new MemberExpectation(MemberType.Method)
                                    {
                                        Declaration = $"I{nameof(Types.SimpleEvents)}Create{nameof(Types.SimpleEvents)}()",
                                    },
                                },
                            },
                        },
                    },
                },
            },
        };

        public static Expectation IndexerProperties { get; } = new Expectation
        {
            Type = typeof(Types.IndexerProperties),
            Configuration = createConfigurationForType(typeof(Types.IndexerProperties)),
            CompilationUnit = new CompilationUnitExpectation
            {
                Namespaces =
                {
                    new NamespaceExpectation
                    {
                        Namespace = typeof(Types.IndexerProperties).Namespace,
                        Types =
                        {
                            new TypeExpectation(TypeKind.Interface)
                            {
                                Declaration = $"publicinterfaceI{nameof(Types.IndexerProperties)}",
                                Members =
                                {
                                    new MemberExpectation(MemberType.Indexer)
                                    {
                                        Declaration = $"stringthis[intarg1]{{get;set;}}",
                                    },
                                    new MemberExpectation(MemberType.Indexer)
                                    {
                                        Declaration = $"stringthis[intarg1,boolarg2]{{get;set;}}",
                                    },
                                    new MemberExpectation(MemberType.Indexer)
                                    {
                                        Declaration = $"stringthis[bytearg1]{{get;}}",
                                    },
                                    new MemberExpectation(MemberType.Indexer)
                                    {
                                        Declaration = $"stringthis[stringarg1]{{set;}}",
                                    },
                                },
                            },
                            new TypeExpectation(TypeKind.Interface)
                            {
                                Declaration = $"publicinterfaceI{nameof(Types.IndexerProperties)}Factory",
                                Members =
                                {
                                    new MemberExpectation(MemberType.Method)
                                    {
                                        Declaration = $"I{nameof(Types.IndexerProperties)}Create{nameof(Types.IndexerProperties)}()",
                                    },
                                },
                            },
                        },
                    },
                },
            },
        };

        public static Expectation SimpleMethods { get; } = new Expectation
        {
            Type = typeof(Types.SimpleMethods),
            Configuration = createConfigurationForType(typeof(Types.SimpleMethods)),
            CompilationUnit = new CompilationUnitExpectation
            {
                Namespaces =
                {
                    new NamespaceExpectation
                    {
                        Namespace = typeof(Types.SimpleMethods).Namespace,
                        Types =
                        {
                            new TypeExpectation(TypeKind.Interface)
                            {
                                Declaration = $"publicinterfaceI{nameof(Types.SimpleMethods)}",
                                Members =
                                {
                                    new MemberExpectation(MemberType.Method)
                                    {
                                        Declaration = $"void{nameof(Types.SimpleMethods.Action)}()",
                                    },
                                    new MemberExpectation(MemberType.Method)
                                    {
                                        Declaration = $"void{nameof(Types.SimpleMethods.Action)}(boolarg1)",
                                    },
                                    new MemberExpectation(MemberType.Method)
                                    {
                                        Declaration = $"void{nameof(Types.SimpleMethods.Action)}(boolarg1,intarg2)",
                                    },
                                    new MemberExpectation(MemberType.Method)
                                    {
                                        Declaration = $"object{nameof(Types.SimpleMethods.Function)}()",
                                    },
                                    new MemberExpectation(MemberType.Method)
                                    {
                                        Declaration = $"object{nameof(Types.SimpleMethods.Function)}(boolarg1)",
                                    },
                                    new MemberExpectation(MemberType.Method)
                                    {
                                        Declaration = $"object{nameof(Types.SimpleMethods.Function)}(boolarg1,intarg2)",
                                    },
                                },
                            },
                            new TypeExpectation(TypeKind.Interface)
                            {
                                Declaration = $"publicinterfaceI{nameof(Types.SimpleMethods)}Factory",
                                Members =
                                {
                                    new MemberExpectation(MemberType.Method)
                                    {
                                        Declaration = $"I{nameof(Types.SimpleMethods)}Create{nameof(Types.SimpleMethods)}()",
                                    },
                                },
                            },
                        },
                    },
                },
            },
        };

        public static Expectation ReferenceMethods { get; } = new Expectation
        {
            Type = typeof(Types.ReferenceMethods),
            Configuration = createConfigurationForType(typeof(Types.ReferenceMethods)),
            CompilationUnit = new CompilationUnitExpectation
            {
                Namespaces =
                {
                    new NamespaceExpectation
                    {
                        Namespace = typeof(Types.ReferenceMethods).Namespace,
                        Types =
                        {
                            new TypeExpectation(TypeKind.Interface)
                            {
                                Declaration = $"publicinterfaceI{nameof(Types.ReferenceMethods)}",
                                Members =
                                {
                                    new MemberExpectation(MemberType.Method)
                                    {
                                        Declaration = $"void{nameof(Types.ReferenceMethods.Out)}(outintvalue)",
                                    },
                                    new MemberExpectation(MemberType.Method)
                                    {
                                        Declaration = $"void{nameof(Types.ReferenceMethods.Ref)}(refintvalue)",
                                    },
                                },
                            },
                            new TypeExpectation(TypeKind.Interface)
                            {
                                Declaration = $"publicinterfaceI{nameof(Types.ReferenceMethods)}Factory",
                                Members =
                                {
                                    new MemberExpectation(MemberType.Method)
                                    {
                                        Declaration = $"I{nameof(Types.ReferenceMethods)}Create{nameof(Types.ReferenceMethods)}()",
                                    },
                                },
                            },
                        },
                    },
                },
            },
        };

        public static Expectation GenericMethods { get; } = new Expectation
        {
            Type = typeof(Types.GenericMethods),
            Configuration = createConfigurationForType(typeof(Types.GenericMethods)),
            CompilationUnit = new CompilationUnitExpectation
            {
                Namespaces =
                {
                    new NamespaceExpectation
                    {
                        Namespace = typeof(Types.GenericMethods).Namespace,
                        Types =
                        {
                            new TypeExpectation(TypeKind.Interface)
                            {
                                Declaration = $"publicinterfaceI{nameof(Types.GenericMethods)}",
                                Members =
                                {
                                    new MemberExpectation(MemberType.Method)
                                    {
                                        Declaration = $"void{nameof(Types.GenericMethods.Action)}<T>()",
                                    },
                                    new MemberExpectation(MemberType.Method)
                                    {
                                        Declaration = $"void{nameof(Types.GenericMethods.Action)}<T1>(T1arg1)",
                                    },
                                    new MemberExpectation(MemberType.Method)
                                    {
                                        Declaration = $"void{nameof(Types.GenericMethods.Action)}<T1,T2>(T1arg1,T2arg2)",
                                    },
                                    new MemberExpectation(MemberType.Method)
                                    {
                                        Declaration = $"object{nameof(Types.GenericMethods.Function)}<T>()",
                                    },
                                    new MemberExpectation(MemberType.Method)
                                    {
                                        Declaration = $"object{nameof(Types.GenericMethods.Function)}<T1>(T1arg1)",
                                    },
                                    new MemberExpectation(MemberType.Method)
                                    {
                                        Declaration = $"object{nameof(Types.GenericMethods.Function)}<T1,T2>(T1arg1,T2arg2)",
                                    },
                                },
                            },
                            new TypeExpectation(TypeKind.Interface)
                            {
                                Declaration = $"publicinterfaceI{nameof(Types.GenericMethods)}Factory",
                                Members =
                                {
                                    new MemberExpectation(MemberType.Method)
                                    {
                                        Declaration = $"I{nameof(Types.GenericMethods)}Create{nameof(Types.GenericMethods)}()",
                                    },
                                },
                            },
                        },
                    },
                },
            },
        };

        public static Expectation GenericMethodsWithConstraints { get; } = new Expectation
        {
            Type = typeof(Types.GenericMethodsWithConstraints),
            Configuration = createConfigurationForType(typeof(Types.GenericMethodsWithConstraints)),
            CompilationUnit = new CompilationUnitExpectation
            {
                Namespaces =
                {
                    new NamespaceExpectation
                    {
                        Namespace = typeof(Types.GenericMethodsWithConstraints).Namespace,
                        Types =
                        {
                            new TypeExpectation(TypeKind.Interface)
                            {
                                Declaration = $"publicinterfaceI{nameof(Types.GenericMethodsWithConstraints)}",
                                Members =
                                {
                                    new MemberExpectation(MemberType.Method)
                                    {
                                        Declaration = $"void{nameof(Types.GenericMethodsWithConstraints.ClassParameter)}<T>()",
                                        Constraints =
                                        {
                                            "whereT:class",
                                        },
                                    },
                                    new MemberExpectation(MemberType.Method)
                                    {
                                        Declaration = $"void{nameof(Types.GenericMethodsWithConstraints.ValueParameter)}<T>()",
                                        Constraints =
                                        {
                                            "whereT:struct",
                                        },
                                    },
                                    new MemberExpectation(MemberType.Method)
                                    {
                                        Declaration = $"void{nameof(Types.GenericMethodsWithConstraints.MultipleParameters)}<T1,T2>()",
                                        Constraints =
                                        {
                                            "whereT1:class",
                                            "whereT2:struct",
                                        },
                                    },
                                },
                            },
                            new TypeExpectation(TypeKind.Interface)
                            {
                                Declaration = $"publicinterfaceI{nameof(Types.GenericMethodsWithConstraints)}Factory",
                                Members =
                                {
                                    new MemberExpectation(MemberType.Method)
                                    {
                                        Declaration = $"I{nameof(Types.GenericMethodsWithConstraints)}Create{nameof(Types.GenericMethodsWithConstraints)}()",
                                    },
                                },
                            },
                        },
                    },
                },
            },
        };

        public static Expectation GenericClassOf1 { get; } = new Expectation
        {
            Type = typeof(Types.GenericClass<>),
            Configuration = createConfigurationForType(typeof(Types.GenericClass<>)),
            CompilationUnit = new CompilationUnitExpectation
            {
                Namespaces =
                {
                    new NamespaceExpectation
                    {
                        Namespace = typeof(Types.GenericClass<>).Namespace,
                        Types =
                        {
                            new TypeExpectation(TypeKind.Interface)
                            {
                                Declaration = $"publicinterfaceI{nameof(Types.GenericClass<int>)}<T1>",
                            },
                            new TypeExpectation(TypeKind.Interface)
                            {
                                Declaration = $"publicinterfaceI{nameof(Types.GenericClass<int>)}Factory<T1>",
                                Members =
                                {
                                    new MemberExpectation(MemberType.Method)
                                    {
                                        Declaration = $"I{nameof(Types.GenericClass<int>)}<T1>Create{nameof(Types.GenericClass<int>)}()",
                                    },
                                },
                            },
                        },
                    },
                },
            },
        };

        public static Expectation GenericClassOf2 { get; } = new Expectation
        {
            Type = typeof(Types.GenericClass<,>),
            Configuration = createConfigurationForType(typeof(Types.GenericClass<,>)),
            CompilationUnit = new CompilationUnitExpectation
            {
                Namespaces =
                {
                    new NamespaceExpectation
                    {
                        Namespace = typeof(Types.GenericClass<,>).Namespace,
                        Types =
                        {
                            new TypeExpectation(TypeKind.Interface)
                            {
                                Declaration = $"publicinterfaceI{nameof(Types.GenericClass<int, int>)}<T1,T2>",
                            },
                            new TypeExpectation(TypeKind.Interface)
                            {
                                Declaration = $"publicinterfaceI{nameof(Types.GenericClass<int, int>)}Factory<T1,T2>",
                                Members =
                                {
                                    new MemberExpectation(MemberType.Method)
                                    {
                                        Declaration = $"I{nameof(Types.GenericClass<int, int>)}<T1,T2>Create{nameof(Types.GenericClass<int, int>)}()",
                                    },
                                },
                            },
                        },
                    },
                },
            },
        };

        public static Expectation GenericClassWithClassConstraint { get; } = new Expectation
        {
            Type = typeof(Types.GenericClassWithClassConstraint<>),
            Configuration = createConfigurationForType(typeof(Types.GenericClassWithClassConstraint<>)),
            CompilationUnit = new CompilationUnitExpectation
            {
                Namespaces =
                {
                    new NamespaceExpectation
                    {
                        Namespace = typeof(Types.GenericClassWithClassConstraint<>).Namespace,
                        Types =
                        {
                            new TypeExpectation(TypeKind.Interface)
                            {
                                Declaration = $"publicinterfaceI{nameof(Types.GenericClassWithClassConstraint<object>)}<T1>",
                                Constraints =
                                {
                                    "whereT1:class",
                                },
                            },
                            new TypeExpectation(TypeKind.Interface)
                            {
                                Declaration = $"publicinterfaceI{nameof(Types.GenericClassWithClassConstraint<object>)}Factory<T1>",
                                Constraints =
                                {
                                    "whereT1:class",
                                },
                                Members =
                                {
                                    new MemberExpectation(MemberType.Method)
                                    {
                                        Declaration = $"I{nameof(Types.GenericClassWithClassConstraint<object>)}<T1>Create{nameof(Types.GenericClassWithClassConstraint<object>)}()",
                                    },
                                },
                            },
                        },
                    },
                },
            },
        };

        public static Expectation GenericClassWithValueConstraint { get; } = new Expectation
        {
            Type = typeof(Types.GenericClassWithValueConstraint<>),
            Configuration = createConfigurationForType(typeof(Types.GenericClassWithValueConstraint<>)),
            CompilationUnit = new CompilationUnitExpectation
            {
                Namespaces =
                {
                    new NamespaceExpectation
                    {
                        Namespace = typeof(Types.GenericClassWithValueConstraint<>).Namespace,
                        Types =
                        {
                            new TypeExpectation(TypeKind.Interface)
                            {
                                Declaration = $"publicinterfaceI{nameof(Types.GenericClassWithValueConstraint<int>)}<T1>",
                                Constraints =
                                {
                                    "whereT1:struct",
                                },
                            },
                            new TypeExpectation(TypeKind.Interface)
                            {
                                Declaration = $"publicinterfaceI{nameof(Types.GenericClassWithValueConstraint<int>)}Factory<T1>",
                                Constraints =
                                {
                                    "whereT1:struct",
                                },
                                Members =
                                {
                                    new MemberExpectation(MemberType.Method)
                                    {
                                        Declaration = $"I{nameof(Types.GenericClassWithValueConstraint<int>)}<T1>Create{nameof(Types.GenericClassWithValueConstraint<int>)}()",
                                    },
                                },
                            },
                        },
                    },
                },
            },
        };

        public static Expectation GenericClassWithMultipleConstraints { get; } = new Expectation
        {
            Type = typeof(Types.GenericClassWithMultipleConstraints<,>),
            Configuration = createConfigurationForType(typeof(Types.GenericClassWithMultipleConstraints<,>)),
            CompilationUnit = new CompilationUnitExpectation
            {
                Namespaces =
                {
                    new NamespaceExpectation
                    {
                        Namespace = typeof(Types.GenericClassWithMultipleConstraints<,>).Namespace,
                        Types =
                        {
                            new TypeExpectation(TypeKind.Interface)
                            {
                                Declaration = $"publicinterfaceI{nameof(Types.GenericClassWithMultipleConstraints<object, int>)}<T1,T2>",
                                Constraints =
                                {
                                    "whereT1:class",
                                    "whereT2:struct",
                                },
                            },
                            new TypeExpectation(TypeKind.Interface)
                            {
                                Declaration = $"publicinterfaceI{nameof(Types.GenericClassWithMultipleConstraints<object, int>)}Factory<T1,T2>",
                                Constraints =
                                {
                                    "whereT1:class",
                                    "whereT2:struct",
                                },
                                Members =
                                {
                                    new MemberExpectation(MemberType.Method)
                                    {
                                        Declaration = $"I{nameof(Types.GenericClassWithMultipleConstraints<object, int>)}<T1,T2>Create{nameof(Types.GenericClassWithMultipleConstraints<object, int>)}()",
                                    },
                                },
                            },
                        },
                    },
                },
            },
        };

        public static Expectation ClassWithConstructors { get; } = new Expectation
        {
            Type = typeof(Types.ClassWithConstructors),
            Configuration = createConfigurationForType(typeof(Types.ClassWithConstructors)),
            CompilationUnit = new CompilationUnitExpectation
            {
                Namespaces =
                {
                    new NamespaceExpectation
                    {
                        Namespace = typeof(Types.ClassWithConstructors).Namespace,
                        Types =
                        {
                            new TypeExpectation(TypeKind.Interface)
                            {
                                Declaration = $"publicinterfaceI{nameof(Types.ClassWithConstructors)}",
                            },
                            new TypeExpectation(TypeKind.Interface)
                            {
                                Declaration = $"publicinterfaceI{nameof(Types.ClassWithConstructors)}Factory",
                                Members =
                                {
                                    new MemberExpectation(MemberType.Method)
                                    {
                                        Declaration = $"I{nameof(Types.ClassWithConstructors)}Create{nameof(Types.ClassWithConstructors)}()",
                                    },
                                    new MemberExpectation(MemberType.Method)
                                    {
                                        Declaration = $"I{nameof(Types.ClassWithConstructors)}Create{nameof(Types.ClassWithConstructors)}(boolarg1)",
                                    },
                                    new MemberExpectation(MemberType.Method)
                                    {
                                        Declaration = $"I{nameof(Types.ClassWithConstructors)}Create{nameof(Types.ClassWithConstructors)}(boolarg1,intarg2)",
                                    },
                                },
                            },
                        },
                    },
                },
            },
        };

        public static Expectation GenericClassWithConstructors { get; } = new Expectation
        {
            Type = typeof(Types.GenericClassWithConstructors<,>),
            Configuration = createConfigurationForType(typeof(Types.GenericClassWithConstructors<,>)),
            CompilationUnit = new CompilationUnitExpectation
            {
                Namespaces =
                {
                    new NamespaceExpectation
                    {
                        Namespace = typeof(Types.GenericClassWithConstructors<,>).Namespace,
                        Types =
                        {
                            new TypeExpectation(TypeKind.Interface)
                            {
                                Declaration = $"publicinterfaceI{nameof(Types.GenericClassWithConstructors<int, int>)}<T1,T2>",
                            },
                            new TypeExpectation(TypeKind.Interface)
                            {
                                Declaration = $"publicinterfaceI{nameof(Types.GenericClassWithConstructors<int, int>)}Factory<T1,T2>",
                                Members =
                                {
                                    new MemberExpectation(MemberType.Method)
                                    {
                                        Declaration = $"I{nameof(Types.GenericClassWithConstructors<int, int>)}<T1,T2>Create{nameof(Types.GenericClassWithConstructors<int, int>)}()",
                                    },
                                    new MemberExpectation(MemberType.Method)
                                    {
                                        Declaration = $"I{nameof(Types.GenericClassWithConstructors<int, int>)}<T1,T2>Create{nameof(Types.GenericClassWithConstructors<int, int>)}(T1arg1)",
                                    },
                                    new MemberExpectation(MemberType.Method)
                                    {
                                        Declaration = $"I{nameof(Types.GenericClassWithConstructors<int, int>)}<T1,T2>Create{nameof(Types.GenericClassWithConstructors<int, int>)}(T1arg1,T2arg2)",
                                    },
                                },
                            },
                        },
                    },
                },
            },
        };

        public static Expectation ClassWithStaticMembers { get; } = new Expectation
        {
            Type = typeof(Types.ClassWithStaticMembers),
            Configuration = createConfigurationForType(typeof(Types.ClassWithStaticMembers)),
            CompilationUnit = new CompilationUnitExpectation
            {
                Namespaces =
                {
                    new NamespaceExpectation
                    {
                        Namespace = typeof(Types.ClassWithStaticMembers).Namespace,
                        Types =
                        {
                            new TypeExpectation(TypeKind.Interface)
                            {
                                Declaration = $"publicinterfaceI{nameof(Types.ClassWithStaticMembers)}",
                            },
                            new TypeExpectation(TypeKind.Interface)
                            {
                                Declaration = $"publicinterfaceI{nameof(Types.ClassWithStaticMembers)}Factory",
                                Members =
                                {
                                    new MemberExpectation(MemberType.Method)
                                    {
                                        Declaration = $"I{nameof(Types.ClassWithStaticMembers)}Create{nameof(Types.ClassWithStaticMembers)}()",
                                    },
                                },
                            },
                            new TypeExpectation(TypeKind.Interface)
                            {
                                Declaration = $"publicinterfaceI{nameof(Types.ClassWithStaticMembers)}Manager",
                                Members =
                                {
                                    new MemberExpectation(MemberType.Method)
                                    {
                                        Declaration = "voidAction()",
                                    },
                                    new MemberExpectation(MemberType.Property)
                                    {
                                        Declaration = "intValue{get;set;}",
                                    },
                                },
                            },
                        },
                    },
                },
            },
        };

        public static Expectation EmptyStaticClass { get; } = new Expectation
        {
            Type = typeof(Types.EmptyStaticClass),
            Configuration = createConfigurationForType(typeof(Types.EmptyStaticClass)),
            CompilationUnit = new CompilationUnitExpectation
            {
                Namespaces =
                {
                    new NamespaceExpectation
                    {
                        Namespace = typeof(Types.EmptyStaticClass).Namespace,
                        Types =
                        {
                            new TypeExpectation(TypeKind.Interface)
                            {
                                Declaration = $"publicinterfaceI{nameof(Types.EmptyStaticClass)}Manager",
                            },
                        },
                    },
                },
            },
        };

        public static Expectation StructWithPublicMembers { get; } = new Expectation
        {
            Type = typeof(Types.StructWithPublicMembers),
            Configuration = createConfigurationForType(typeof(Types.StructWithPublicMembers)),
            CompilationUnit = new CompilationUnitExpectation
            {
                Usings =
                {
                    "usingSystem;"
                },
                Namespaces =
                {
                    new NamespaceExpectation
                    {
                        Namespace = typeof(Types.StructWithPublicMembers).Namespace,
                        Types =
                        {
                            new TypeExpectation(TypeKind.Interface)
                            {
                                Declaration = $"publicinterfaceI{nameof(Types.StructWithPublicMembers)}",
                                Members =
                                {
                                    new MemberExpectation(MemberType.Property)
                                    {
                                        Declaration = $"void{nameof(Types.StructWithPublicMembers.Run)}()",
                                    },
                                    new MemberExpectation(MemberType.Property)
                                    {
                                        Declaration = $"object{nameof(Types.StructWithPublicMembers.Value)}{{get;set;}}",
                                    },
                                    new MemberExpectation(MemberType.Property)
                                    {
                                        Declaration = $"eventEventHandler{nameof(Types.StructWithPublicMembers.Changed)};",
                                    },
                                },
                            },
                            new TypeExpectation(TypeKind.Interface)
                            {
                                Declaration = $"publicinterfaceI{nameof(Types.StructWithPublicMembers)}Factory",
                                Members =
                                {
                                    new MemberExpectation(MemberType.Method)
                                    {
                                        Declaration = $"I{nameof(Types.StructWithPublicMembers)}Create{nameof(Types.StructWithPublicMembers)}()",
                                    },
                                },
                            },
                        },
                    },
                },
            },
        };

        public static Expectation InterfaceWithPublicMembers { get; } = new Expectation
        {
            Type = typeof(Types.IInterfaceWithPublicMembers),
            Configuration = createConfigurationForType(typeof(Types.IInterfaceWithPublicMembers)),
            CompilationUnit = new CompilationUnitExpectation
            {
                Usings =
                {
                    "usingSystem;"
                },
                Namespaces =
                {
                    new NamespaceExpectation
                    {
                        Namespace = typeof(Types.IInterfaceWithPublicMembers).Namespace,
                        Types =
                        {
                            new TypeExpectation(TypeKind.Interface)
                            {
                                Declaration = $"publicinterface{nameof(Types.IInterfaceWithPublicMembers)}",
                                Members =
                                {
                                    new MemberExpectation(MemberType.Property)
                                    {
                                        Declaration = $"void{nameof(Types.IInterfaceWithPublicMembers.Run)}()",
                                    },
                                    new MemberExpectation(MemberType.Property)
                                    {
                                        Declaration = $"object{nameof(Types.IInterfaceWithPublicMembers.Value)}{{get;set;}}",
                                    },
                                    new MemberExpectation(MemberType.Property)
                                    {
                                        Declaration = $"eventEventHandler{nameof(Types.IInterfaceWithPublicMembers.Changed)};",
                                    },
                                },
                            },
                        },
                    },
                },
            },
        };

        public static Expectation SimpleEnum { get; } = new Expectation
        {
            Type = typeof(Types.SimpleEnum),
            Configuration = createConfigurationForType(typeof(Types.SimpleEnum)),
            CompilationUnit = new CompilationUnitExpectation
            {
                Namespaces =
                {
                    new NamespaceExpectation
                    {
                        Namespace = typeof(Types.SimpleEnum).Namespace,
                        Types =
                        {
                            new TypeExpectation(TypeKind.Enum)
                            {
                                Declaration = $"publicenum{nameof(Types.SimpleEnum)}",
                                Members =
                                {
                                    new MemberExpectation(MemberType.EnumMember)
                                    {
                                        Declaration = $"{nameof(Types.SimpleEnum.One)}",
                                    },
                                    new MemberExpectation(MemberType.EnumMember)
                                    {
                                        Declaration = $"{nameof(Types.SimpleEnum.Two)}",
                                    },
                                    new MemberExpectation(MemberType.EnumMember)
                                    {
                                        Declaration = $"{nameof(Types.SimpleEnum.Three)}",
                                    },
                                },
                            },
                        },
                    },
                },
            },
        };
    }
}

namespace Potter.ApiExtraction.Types
{
    public class EmptyClass
    {
    }

    public class SimpleFields
    {
        public int Value;

        public readonly int ReadOnlyValue;
    }

    public class SimpleProperties
    {
        public int GettableValue { get; }

        public int SettableValue { set { } }

        public int Value { get; set; }
    }

    public class SimpleEvents
    {
        public event EventHandler EventField;

        public event EventHandler EventProperty
        {
            add { }
            remove { }
        }
    }

    public class IndexerProperties
    {
        public string this[int arg1]
        {
            get { return null; }
            set { }
        }

        public string this[int arg1, bool arg2]
        {
            get { return null; }
            set { }
        }

        public string this[byte arg1]
        {
            get { return null; }
        }

        public string this[string arg1]
        {
            set { }
        }
    }

    public class SimpleMethods
    {
        public void Action()
        {
        }

        public void Action(bool arg1)
        {
        }

        public void Action(bool arg1, int arg2)
        {
        }

        public object Function()
        {
            return null;
        }

        public object Function(bool arg1)
        {
            return null;
        }

        public object Function(bool arg1, int arg2)
        {
            return null;
        }
    }

    public class ReferenceMethods
    {
        public void Out(out int value)
        {
            value = 0;
        }

        public void Ref(ref int value)
        {
        }
    }

    public class GenericMethods
    {
        public void Action<T>()
        {
        }

        public void Action<T1>(T1 arg1)
        {
        }

        public void Action<T1, T2>(T1 arg1, T2 arg2)
        {
        }

        public object Function<T>()
        {
            return null;
        }

        public object Function<T1>(T1 arg1)
        {
            return null;
        }

        public object Function<T1, T2>(T1 arg1, T2 arg2)
        {
            return null;
        }
    }

    public class GenericMethodsWithConstraints
    {
        public void ClassParameter<T>()
            where T : class
        {
        }

        public void ValueParameter<T>()
            where T : struct
        {
        }

        public void MultipleParameters<T1, T2>()
            where T1 : class
            where T2 : struct
        {
        }
    }

    public class GenericClass<T1>
    {
    }

    public class GenericClass<T1, T2>
    {
    }

    public class GenericClassWithClassConstraint<T1>
            where T1 : class
    {
    }

    public class GenericClassWithValueConstraint<T1>
            where T1 : struct
    {
    }

    public class GenericClassWithMultipleConstraints<T1, T2>
            where T1 : class
            where T2 : struct
    {
    }

    public class ClassWithConstructors
    {
        public ClassWithConstructors()
        {
        }

        public ClassWithConstructors(bool arg1)
        {
        }

        public ClassWithConstructors(bool arg1, int arg2)
        {
        }
    }

    public class GenericClassWithConstructors<T1, T2>
    {
        public GenericClassWithConstructors()
        {
        }

        public GenericClassWithConstructors(T1 arg1)
        {
        }

        public GenericClassWithConstructors(T1 arg1, T2 arg2)
        {
        }
    }

    public class ClassWithStaticMembers
    {
        public static int Value { get; set; }

        public static void Action()
        {
        }
    }

    public static class EmptyStaticClass
    {
    }

    public class ClassWithPublicMembers
    {
        public object Value { get; set; }

        public event EventHandler Changed { add { } remove { } }

        public void Run()
        {
        }
    }

    public class ClassWithPublicVirtualMembers
    {
        public virtual object Value { get; set; }

        public virtual event EventHandler Changed { add { } remove { } }

        public virtual void Run()
        {
        }
    }

    public class ClassWithPublicMembersHidingBase : ClassWithPublicMembers
    {
        public new object Value { get; set; }

        public new event EventHandler Changed { add { } remove { } }

        public new void Run()
        {
        }
    }

    public class ClassWithPublicMembersHidingVirtualBase : ClassWithPublicVirtualMembers
    {
        public new object Value { get; set; }

        public new event EventHandler Changed { add { } remove { } }

        public new void Run()
        {
        }
    }

    public class ClassWithPublicMembersOverridingBase : ClassWithPublicVirtualMembers
    {
        public override object Value { get; set; }

        public override event EventHandler Changed { add { } remove { } }

        public override void Run()
        {
        }
    }

    public class ClassWithPublicVirtualMembersHidingBase : ClassWithPublicVirtualMembers
    {
        public new virtual object Value { get; set; }

        public new virtual event EventHandler Changed { add { } remove { } }

        public new virtual void Run()
        {
        }
    }

    public struct StructWithPublicMembers
    {
        public object Value { get; set; }

        public event EventHandler Changed { add { } remove { } }

        public void Run()
        {
        }
    }

    public interface IInterfaceWithPublicMembers
    {
        object Value { get; set; }

        event EventHandler Changed;

        void Run();
    }

    public enum SimpleEnum
    {
        One,
        Two,
        Three,
    }
}

#endregion

#region Assembly Expectations

namespace Potter.ApiExtraction.Core.Tests
{
    public static partial class ExpectedTypes
    {
        public static ExpectationForAssembly SubsetSimpleClass { get; } = new ExpectationForAssembly
        {
            Assembly = typeof(Types.Subset.SimpleClass).Assembly,
            Configuration = createConfigurationForType(typeof(Types.Subset.SimpleClass)),
            SourceFileInfo =
            {
                new SourceFileInfoExpectation
                {
                    Name = "I" + nameof(Types.Subset.SimpleClass),
                    Group = "TestGroup",
                    CompilationUnit = new CompilationUnitExpectation
                    {
                        Usings =
                        {
                            "usingSystem;",
                        },
                        Namespaces =
                        {
                            new NamespaceExpectation
                            {
                                Namespace = typeof(Types.Subset.SimpleClass).Namespace,
                                Types =
                                {
                                    new TypeExpectation(TypeKind.Interface)
                                    {
                                        Declaration = $"publicinterfaceI{nameof(Types.Subset.SimpleClass)}",
                                        Members =
                                        {
                                            new MemberExpectation(MemberType.Method)
                                            {
                                                Declaration = "voidReset()",
                                            },
                                            new MemberExpectation(MemberType.Property)
                                            {
                                                Declaration = "intValue{get;set;}",
                                            },
                                            new MemberExpectation(MemberType.Indexer)
                                            {
                                                Declaration = "stringthis[intkey]{get;set;}",
                                            },
                                            new MemberExpectation(MemberType.Event)
                                            {
                                                Declaration = "eventEventHandlerChanged;",
                                            },
                                        },
                                    },
                                    new TypeExpectation(TypeKind.Interface)
                                    {
                                        Declaration = $"publicinterfaceI{nameof(Types.Subset.SimpleClass)}Factory",
                                        Members =
                                        {
                                            new MemberExpectation(MemberType.Method)
                                            {
                                                Declaration = $"I{nameof(Types.Subset.SimpleClass)}Create{nameof(Types.Subset.SimpleClass)}()",
                                            },
                                        },
                                    },
                                },
                            },
                        },
                    },
                },
            },
        };
    }
}

namespace Potter.ApiExtraction.Types.Subset
{
    public class SimpleClass
    {
        public int Value { get; set; }

        public string this[int key]
        {
            get { return null; }
            set { }
        }

        public event EventHandler Changed { add { } remove { } }

        public void Reset()
        {
        }
    }
}

#endregion
