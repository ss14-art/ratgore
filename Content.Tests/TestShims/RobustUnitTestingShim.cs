// Minimal shims to decouple tests from Robust.UnitTesting engine library.
// Keep tests compiling without direct ProjectReference to Robust.UnitTesting.
using System;

namespace Robust.UnitTesting
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly)]
    public sealed class UnitTestProjectAttribute : Attribute
    {
    }

    // If your tests inherited from RobustUnitTest, this empty base keeps signatures intact.
    public abstract class RobustUnitTest
    {
        // Add helpers here only if a particular test needs them.
    }
}

// Some older tests may have `using UnitTestProject;`
// Provide an empty namespace to satisfy the using without changing those files.
namespace UnitTestProject { }
