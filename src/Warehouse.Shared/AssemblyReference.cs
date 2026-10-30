using System.Reflection;

namespace Warehouse.Shared;

public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
