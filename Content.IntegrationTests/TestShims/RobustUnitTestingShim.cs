// Minimal shims to decouple tests from Robust.UnitTesting engine library.
// Keep integration tests compiling without Robust.UnitTesting project.
using System;

namespace Robust.UnitTesting
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly)]
    public sealed class UnitTestProjectAttribute : Attribute
    {
    }

    public abstract class RobustUnitTest
    {
    }
}

namespace UnitTestProject { }
