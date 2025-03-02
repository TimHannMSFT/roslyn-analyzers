﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeRefactoringVerifier<
    Roslyn.Diagnostics.CSharp.Analyzers.CSharpApplyTraitToClass>;
using VerifyVB = Test.Utilities.VisualBasicCodeRefactoringVerifier<
    Roslyn.Diagnostics.VisualBasic.Analyzers.VisualBasicApplyTraitToClass>;

namespace Roslyn.Diagnostics.Analyzers.UnitTests
{
    public class ApplyTraitToClassTests
    {
        [Theory]
        [InlineData("A", "")]
        [InlineData("", "A")]
        [InlineData("A", "A")]
        public async Task MoveTraitToType_MovesSecond_CSharp(string name, string value)
        {
            var source = $@"
using Xunit;

class C
{{
    [$$Trait(""{name}"", ""{value}"")]
    public void Method() {{ }}

    [Fact, Trait(""{name}"", ""{value}"")]
    public void Method2() {{ }}
}}
";
            var fixedSource = $@"
using Xunit;

[Trait(""{name}"", ""{value}"")]
class C
{{
    public void Method() {{ }}

    [Fact]
    public void Method2() {{ }}
}}
";

            await new VerifyCS.Test
            {
                ReferenceAssemblies = AdditionalMetadataReferences.DefaultWithXUnit,
                TestCode = source,
                FixedCode = fixedSource,
            }.RunAsync();
        }

        [Theory]
        [InlineData("A", "")]
        [InlineData("", "A")]
        [InlineData("A", "A")]
        public async Task MoveTraitToType_MovesSecond_VisualBasic(string name, string value)
        {
            var source = $@"
Imports Xunit

Class C
    <$$Trait(""{name}"", ""{value}"")>
    Public Sub Method()
    End Sub

    <Fact, Trait(""{name}"", ""{value}"")>
    Public Sub Method2()
    End Sub
End Class
";
            var fixedSource = $@"
Imports Xunit

<Trait(""{name}"", ""{value}"")>
Class C
    Public Sub Method()
    End Sub

    <Fact>
    Public Sub Method2()
    End Sub
End Class
";

            await new VerifyVB.Test
            {
                ReferenceAssemblies = AdditionalMetadataReferences.DefaultWithXUnit,
                TestCode = source,
                FixedCode = fixedSource,
            }.RunAsync();
        }

        [Theory]
        [InlineData("A", "")]
        [InlineData("", "A")]
        [InlineData("A", "A")]
        public async Task MoveTraitToType_MovesOnlyFirst_CSharp(string name, string value)
        {
            var source = $@"
using Xunit;

class C
{{
    [$$Trait("""", """")]
    public void Method() {{ }}

    [Fact, Trait(""{name}"", ""{value}"")]
    public void Method2() {{ }}
}}
";
            var fixedSource = $@"
using Xunit;

[Trait("""", """")]
class C
{{
    public void Method() {{ }}

    [Fact, Trait(""{name}"", ""{value}"")]
    public void Method2() {{ }}
}}
";

            await new VerifyCS.Test
            {
                ReferenceAssemblies = AdditionalMetadataReferences.DefaultWithXUnit,
                TestCode = source,
                FixedCode = fixedSource,
            }.RunAsync();
        }

        [Theory]
        [InlineData("A", "")]
        [InlineData("", "A")]
        [InlineData("A", "A")]
        public async Task MoveTraitToType_MovesOnlyFirst_VisualBasic(string name, string value)
        {
            var source = $@"
Imports Xunit

Class C
    <$$Trait("""", """")>
    Public Sub Method()
    End Sub

    <Fact, Trait(""{name}"", ""{value}"")>
    Public Sub Method2()
    End Sub
End Class
";
            var fixedSource = $@"
Imports Xunit

<Trait("""", """")>
Class C
    Public Sub Method()
    End Sub

    <Fact, Trait(""{name}"", ""{value}"")>
    Public Sub Method2()
    End Sub
End Class
";

            await new VerifyVB.Test
            {
                ReferenceAssemblies = AdditionalMetadataReferences.DefaultWithXUnit,
                TestCode = source,
                FixedCode = fixedSource,
            }.RunAsync();
        }
    }
}
