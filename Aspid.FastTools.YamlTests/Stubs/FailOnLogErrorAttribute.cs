using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using Debug = UnityEngine.Debug;

[assembly: Aspid.FastTools.SerializeReferences.Editors.Tests.FailOnLogError]

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // Unity's Test Runner fails a test that logs an error it did not expect; the tests rely on that, so it is
    // reproduced here for every test in the assembly.
    [AttributeUsage(AttributeTargets.Assembly)]
    internal sealed class FailOnLogErrorAttribute : Attribute, ITestAction
    {
        public ActionTargets Targets => ActionTargets.Test;

        public void BeforeTest(ITest test) => Debug.TakeErrors();

        public void AfterTest(ITest test)
        {
            var errors = Debug.TakeErrors();
            if (errors.Length > 0)
                Assert.Fail("Unhandled log message: [Error] " + string.Join(Environment.NewLine, errors));
        }
    }
}
