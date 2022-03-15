using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading.Tasks;
using VerifyCS = Analyzer.Test.CSharpCodeFixVerifier<
    Analyzer.AnalyzerAnalyzer,
    Analyzer.AnalyzerCodeFixProvider>;

namespace Analyzer.Test
{
    [TestClass]
    public class AnalyzerUnitTest
    {
        //No diagnostics expected to show up
        [TestMethod]
        public async Task Diagnostic_Not_Triggered()
        {
            var test = @"
using MockAnalyzer.Test.Registration.Step1;

namespace MockAnalyzer.Test.Registration
{
    public class RegistrationOrchestrator
    {
        public void GoThroughFlow()
        {
            //
        }
    }
}

namespace MockAnalyzer.Test.Registration.Step1
{
    public class Thing{}
}
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task Diagnostic_Triggered()
        {
            var test = @"

using MockAnalyzer.Test.Login;

namespace MockAnalyzer.Test.Registration
{
    public class RegistrationOrchestrator
    {
        public void GoThroughFlow()
        {
            //
        }
    }
}

namespace MockAnalyzer.Test.Login
{
    public class Thing{ }
}
";
            var diagnostic = VerifyCS.Diagnostic("Analyzer").WithLocation(5, 1).WithArguments("Registration", 2, 1);
            await VerifyCS.VerifyAnalyzerAsync(test, diagnostic);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public async Task TestMethod2()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class {|#0:TypeName|}
        {   
        }
    }";

            var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TYPENAME
        {   
        }
    }";

            var expected = VerifyCS.Diagnostic("Analyzer").WithLocation(0).WithArguments("TypeName");
            await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
        }
    }
}
