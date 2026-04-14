using System;
using System.Reflection;

namespace Robust.UnitTesting
{
    public enum UnitTestProject
    {
        Shared = 0,
        Client = 1,
        Server = 2,
        Tools  = 3
    }

    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class)]
    public sealed class UnitTestProjectAttribute : Attribute
    {
        public UnitTestProjectAttribute() { }
        public UnitTestProjectAttribute(UnitTestProject project) => Project = project;
        public UnitTestProject Project { get; } = UnitTestProject.Shared;
    }
    public abstract class RobustUnitTest
    {
        public virtual UnitTestProject Project => UnitTestProject.Shared;
        protected virtual void OverrideIoC() { }
        protected virtual Assembly[] GetContentAssemblies() => Array.Empty<Assembly>();
    }
}
