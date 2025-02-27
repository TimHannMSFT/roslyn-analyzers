﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Test.Utilities.CSharpSecurityCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Security.PotentialReferenceCycleInDeserializedObjectGraph,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.VisualBasicSecurityCodeFixVerifier<
    Microsoft.NetCore.Analyzers.Security.PotentialReferenceCycleInDeserializedObjectGraph,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    public class PotentialReferenceCycleInDeserializedObjectGraphTests
    {
        [Fact]
        public async Task TestSelfReferDirectlyDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
            using System;

            [Serializable()]
            class TestClass
            {
                private TestClass testClass;

                public void TestMethod()
                {
                }
            }",
            GetCSharpResultAt(7, 35, "testClass"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Namespace TestNamespace
    <Serializable()>
    Class TestClass
        Private testClass As TestClass
        
        Sub TestMethod()
        End Sub
    End Class
End Namespace",
            GetBasicResultAt(7, 17, "testClass"));
        }

        [Fact]
        public async Task TestParentChildCircleDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

[Serializable()]
class TestClassA
{
    private TestClassB testClassB;

    public void TestMethod()
    {
    }
}

[Serializable()]
class TestClassB
{
    private TestClassA testClassA;

    public void TestMethod()
    {
    }
}",
            GetCSharpResultAt(7, 24, "testClassB"),
            GetCSharpResultAt(17, 24, "testClassA"));
        }

        [Fact]
        public async Task TestParentGrandchildCircleDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

[Serializable()]
class TestClassA
{
    private TestClassB testClassBInA;

    private TestClassD testClassDInD;

    public void TestMethod()
    {
    }
}

[Serializable()]
class TestClassB
{
    private TestClassC testClassCInB;

    public void TestMethod()
    {
    }
}

[Serializable()]
class TestClassC
{
    private TestClassA testClassAInC;

    public void TestMethod()
    {
    }
}

[Serializable()]
class TestClassD
{
    public void TestMethod()
    {
    }
}",
            GetCSharpResultAt(7, 24, "testClassBInA"),
            GetCSharpResultAt(19, 24, "testClassCInB"),
            GetCSharpResultAt(29, 24, "testClassAInC"));
        }

        [Fact]
        public async Task TestChildCircleDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

[Serializable()]
class TestClassA
{
    private TestClassA testClassAInA;

    public void TestMethod()
    {
    }
}

[Serializable()]
class TestClassB
{
    private TestClassA testClassAInB;

    public void TestMethod()
    {
    }
}",
            GetCSharpResultAt(7, 24, "testClassAInA"));
        }

        [Fact]
        public async Task TestChildGrandchildCircleDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

[Serializable()]
class TestClassA
{
    private TestClassB testClassBInA;

    public void TestMethod()
    {
    }
}

[Serializable()]
class TestClassB
{
    private TestClassC testClassCInB;

    public void TestMethod()
    {
    }
}

[Serializable()]
class TestClassC
{
    private TestClassB testClassBInC;

    public void TestMethod()
    {
    }
}",
            GetCSharpResultAt(17, 24, "testClassCInB"),
            GetCSharpResultAt(27, 24, "testClassBInC"));
        }

        [Fact]
        public async Task TestClassReferedInTwoLoopsDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

[Serializable()]
class TestClassA
{
    private TestClassB testClassBInA;

    private TestClassD testClassDInA;

    private TestClassB2 testClassB2InA;

    public void TestMethod()
    {
    }
}

[Serializable()]
class TestClassB
{
    private TestClassC testClassCInB;

    public void TestMethod()
    {
    }
}

[Serializable()]
class TestClassC
{
    private TestClassA testClassAInC;

    public void TestMethod()
    {
    }
}

[Serializable()]
class TestClassD
{
    public void TestMethod()
    {
    }
}

[Serializable()]
class TestClassB2
{
    private TestClassC2 testClassC2InB2;

    public void TestMethod()
    {
    }
}

[Serializable()]
class TestClassC2
{
    private TestClassA testClassAInC2;

    public void TestMethod()
    {
    }
}",
            GetCSharpResultAt(7, 24, "testClassBInA"),
            GetCSharpResultAt(11, 25, "testClassB2InA"),
            GetCSharpResultAt(21, 24, "testClassCInB"),
            GetCSharpResultAt(31, 24, "testClassAInC"),
            GetCSharpResultAt(49, 25, "testClassC2InB2"),
            GetCSharpResultAt(59, 24, "testClassAInC2"));
        }

        [Fact]
        public async Task TestMultiFieldsWithSameTypeDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

[Serializable()]
class TestClassA
{
    private TestClassB testClassBInA;

    private TestClassB testClassB2InA;

    public void TestMethod()
    {
    }
}

[Serializable()]
class TestClassB
{
    private TestClassA testClassAInB;

    private TestClassA testClassA2InB;

    public void TestMethod()
    {
    }
}",
            GetCSharpResultAt(7, 24, "testClassBInA"),
            GetCSharpResultAt(9, 24, "testClassB2InA"),
            GetCSharpResultAt(19, 24, "testClassAInB"),
            GetCSharpResultAt(21, 24, "testClassA2InB"));
        }

        [Fact]
        public async Task TestChildCircleWithParentChildCircleDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

[Serializable()]
class TestClassA
{
    private TestClassB testClassBInA;

    public void TestMethod()
    {
    }
}

[Serializable()]
class TestClassB
{
    private TestClassA testClassAInB;

    private TestClassB testClassBInB;

    public void TestMethod()
    {
    }
}",
            GetCSharpResultAt(7, 24, "testClassBInA"),
            GetCSharpResultAt(17, 24, "testClassAInB"),
            GetCSharpResultAt(19, 24, "testClassBInB"));
        }

        [Fact]
        public async Task TestTwoIndependentParentChildCirclesDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

[Serializable()]
class TestClassA
{
    private TestClassB testClassB;

    public void TestMethod()
    {
    }
}

[Serializable()]
class TestClassB
{
    private TestClassA testClassA;

    public void TestMethod()
    {
    }
}

[Serializable()]
class TestClassA2
{
    private TestClassB2 testClassB2;

    public void TestMethod()
    {
    }
}

[Serializable()]
class TestClassB2
{
    private TestClassA2 testClassA2;

    public void TestMethod()
    {
    }
}",
            GetCSharpResultAt(7, 24, "testClassB"),
            GetCSharpResultAt(17, 24, "testClassA"),
            GetCSharpResultAt(27, 25, "testClassB2"),
            GetCSharpResultAt(37, 25, "testClassA2"));
        }

        [Fact]
        public async Task TestSelfReferDirectlyByPropertyDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

[Serializable()]
class TestClass
{
    public TestClass TestClassProperty { get; set; }

    public void TestMethod()
    {
    }
}",
            GetCSharpResultAt(7, 22, "TestClassProperty"));
        }

        [Fact]
        public async Task TestSelfReferDirectlyWithGenericTypeDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

[System.Serializable]
public class GenericClass<T>
{
}

[Serializable()]
class TestClass
{
    private GenericClass<TestClass> testClasses;

    public void TestMethod()
    {
    }
}",
            GetCSharpResultAt(12, 37, "testClasses"));
        }

        [Fact]
        public async Task TestSelfReferDirectlyWithArrayDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

[Serializable()]
class TestClass
{
    private TestClass[] testClasses;

    public void TestMethod()
    {
    }
}",
            GetCSharpResultAt(7, 25, "testClasses"));
        }

        [Fact]
        public async Task TestSelfReferDirectlyWithDoubleDimensionalArrayDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

[Serializable()]
class TestClass
{
    private TestClass[][] testClasses;

    public void TestMethod()
    {
    }
}",
            GetCSharpResultAt(7, 27, "testClasses"));
        }

        [Fact]
        public async Task TestSelfReferDirectlyWithListDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Collections.Generic;

[Serializable()]
class TestClass
{
    private List<TestClass> testClasses;

    public void TestMethod()
    {
    }
}",
            GetCSharpResultAt(8, 29, "testClasses"));
        }

        [Fact]
        public async Task TestSelfReferDirectlyWithListListDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Collections.Generic;

[Serializable()]
class TestClass
{
    private List<List<TestClass>> testClasses;

    public void TestMethod()
    {
    }
}",
            GetCSharpResultAt(8, 35, "testClasses"));
        }

        [Fact]
        public async Task TestSelfReferDirectlyWithListListListDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Collections.Generic;

[Serializable()]
class TestClass
{
    private List<List<List<TestClass>>> testClasses;

    public void TestMethod()
    {
    }
}",
            GetCSharpResultAt(8, 41, "testClasses"));
        }

        [Fact]
        public async Task TestGenericChildCircleDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Collections.Generic;

[Serializable()]
class TestClassA
{
    private List<TestClassA> testClassAInA;

    public void TestMethod()
    {
    }
}

[Serializable()]
class TestClassB
{
    private TestClassA testClassAInB;

    public void TestMethod()
    {
    }
}",
            GetCSharpResultAt(8, 30, "testClassAInA"));
        }

        [Fact]
        public async Task TestSelfReferDirectlyByPropertyWithArrayDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

[Serializable()]
class TestClass
{
    public TestClass[] TestClassProperty { get; set; }

    public void TestMethod()
    {
    }
}",
            GetCSharpResultAt(7, 24, "TestClassProperty"));
        }

        [Fact]
        public async Task TestSelfReferDirectlyWithinGenericTypeDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

[Serializable()]
class TestClass<T>
{
    private TestClass<T> testClass;

    public void TestMethod()
    {
    }
}",
            GetCSharpResultAt(7, 26, "testClass"));
        }

        [Fact]
        public async Task TestSelfReferDirectlyWithDictionaryDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Collections.Generic;

[Serializable()]
class NormalClass
{
}

[Serializable()]
class TestClass
{
    private Dictionary<TestClass, NormalClass> testClasses;

    public void TestMethod()
    {
    }
}",
            GetCSharpResultAt(13, 48, "testClasses"));
        }

        [Fact]
        public async Task TestParentClassSubclassCirlceDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

[Serializable()]
class TestClassA
{
    private TestClassB testClassB;

    public void TestMethod()
    {
    }
}

[Serializable()]
class TestClassB : TestClassA
{
    private int b;

    public void TestMethod()
    {
    }
}",
            GetCSharpResultAt(7, 24, "testClassB"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System

Namespace TestNamespace
    <Serializable()> _
    Class TestClassA
        Private testClassB As TestClassB
        
        Sub TestMethod()
        End Sub
    End Class

    <Serializable()> _
    Class TestClassB
        Inherits TestClassA

        Private b As Integer

        Sub TestMethod()
        End Sub
    End Class
End Namespace",
            GetBasicResultAt(7, 17, "testClassB"));
        }

        [Fact]
        public async Task TestParentClassIndirectSubclassCirlceDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

[Serializable()]
class TestClassA
{
    private TestClassB testClassB;

    public void TestMethod()
    {
    }
}

[Serializable()]
class TestClassB : TestClassA
{   
    private TestClassC testClassC;

    public void TestMethod()
    {
    }
}

[Serializable()]
class TestClassC : TestClassB
{   
    private TestClassA testClassA;

    public void TestMethod()
    {
    }
}",
            GetCSharpResultAt(7, 24, "testClassB"),
            GetCSharpResultAt(17, 24, "testClassC"),
            GetCSharpResultAt(27, 24, "testClassA"));
        }

        [Fact]
        public async Task TestWithoutSelfReferNoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

[Serializable()]
class TestClassA
{
    private int a;

    public void TestMethod()
    {
    }
}

[Serializable()]
class TestClassB
{
    private TestClassA testClassA;

    public void TestMethod()
    {
    }
}");
        }

        [Fact]
        public async Task TestStaticSelfReferNoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

[Serializable()]
class TestClass
{
    private static TestClass testClass;

    public void TestMethod()
    {
    }
}");
        }

        [Fact]
        public async Task TestStaticParentChildCircleNoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

[Serializable()]
class TestClassA
{
    private TestClassB testClassB;

    public void TestMethod()
    {
    }
}

[Serializable()]
class TestClassB
{
    private static TestClassA testClassA;

    public void TestMethod()
    {
    }
}");
        }

        [Fact]
        public async Task TestStaticParentGrandchildCircleNoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

[Serializable()]
class TestClassA
{
    private TestClassB testClassBInA;

    private TestClassD testClassDInD;

    public void TestMethod()
    {
    }
}

[Serializable()]
class TestClassB
{
    private TestClassC testClassCInB;

    public void TestMethod()
    {
    }
}

[Serializable()]
class TestClassC
{
    private static TestClassA testClassAInC;

    public void TestMethod()
    {
    }
}

[Serializable()]
class TestClassD
{
    public void TestMethod()
    {
    }
}");
        }

        [Fact]
        public async Task TestNonSerializedAttributeSelfReferNoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

[Serializable()]
class TestClass
{
    [NonSerialized]
    private TestClass testClass;

    public void TestMethod()
    {
    }
}");
        }

        [Fact]
        public async Task TestSelfReferDirectlyWithArrayNoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

[Serializable()]
class NormalClass
{
}

[Serializable()]
class TestClass
{
    private NormalClass[] normalClasses;

    public void TestMethod()
    {
    }
}");
        }

        [Fact]
        public async Task TestSelfReferDirectlyWithDoubleDimensionalArrayNoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

[Serializable()]
class NormalClass
{
}

[Serializable()]
class TestClass
{
    private NormalClass[][] normalClasses;

    public void TestMethod()
    {
    }
}");
        }

        [Fact]
        public async Task TestSelfReferDirectlyWithListNoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Collections.Generic;

[Serializable()]
class NormalClass
{
}

[Serializable()]
class TestClass
{
    private List<NormalClass> normalClasses;

    public void TestMethod()
    {
    }
}");
        }

        [Fact]
        public async Task TestSelfReferDirectlyWithListListNoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Collections.Generic;

[Serializable()]
class NormalClass
{
}

[Serializable()]
class TestClass
{
    private List<List<NormalClass>> normalClasses;

    public void TestMethod()
    {
    }
}");
        }

        private static DiagnosticResult GetCSharpResultAt(int line, int column, params string[] arguments)
#pragma warning disable RS0030 // Do not used banned APIs
            => VerifyCS.Diagnostic()
                .WithLocation(line, column)
#pragma warning restore RS0030 // Do not used banned APIs
                .WithArguments(arguments);

        private static DiagnosticResult GetBasicResultAt(int line, int column, params string[] arguments)
#pragma warning disable RS0030 // Do not used banned APIs
            => VerifyVB.Diagnostic()
                .WithLocation(line, column)
#pragma warning restore RS0030 // Do not used banned APIs
                .WithArguments(arguments);
    }
}
