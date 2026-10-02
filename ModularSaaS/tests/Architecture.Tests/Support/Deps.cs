using System.Collections.Concurrent;
using System.Reflection;
using Mono.Cecil;

namespace ModularSaaS.Architecture.Tests.Support;

/// <summary>A dependency edge: top-level type From uses top-level type To.</summary>
internal sealed class Ref(TypeDefinition from, TypeReference to)
{
    public TypeDefinition From { get; } = from;
    public TypeReference To { get; } = to;
    public string ToNamespace => To.Namespace;
    public string ToAssembly => Deps.AssemblyName(To);
    public bool FromIsProgram => From.Name is "Program" or "<Program>$";
    public override string ToString() => $"{From.FullName} -> {To.FullName}";
}

/// <summary>Scans compiled assemblies with Mono.Cecil: signatures, bodies, locals, attributes, generics, lambdas.</summary>
internal static class Deps
{
    private static readonly ConcurrentDictionary<string, ModuleDefinition> Modules = new();
    private static readonly ConcurrentDictionary<string, IReadOnlyList<Ref>> RefCache = new();

    public static ModuleDefinition Load(string assemblyName) =>
        Modules.GetOrAdd(assemblyName,
            n => ModuleDefinition.ReadModule(Assembly.Load(new AssemblyName(n)).Location));

    public static IReadOnlyList<Ref> Refs(IEnumerable<string> assemblies) =>
        assemblies.SelectMany(a => RefCache.GetOrAdd(a, _ => Scan(Load(a)).ToList())).ToList();

    /// <summary>Top-level, non compiler-generated types.</summary>
    public static IReadOnlyList<TypeDefinition> Types(IEnumerable<string> assemblies) =>
        assemblies.SelectMany(a => Load(a).Types)
            .Where(t => t.Name != "<Module>" && !t.Name.StartsWith('<') &&
                        !t.CustomAttributes.Any(c => c.AttributeType.FullName == "System.Runtime.CompilerServices.CompilerGeneratedAttribute"))
            .ToList();

    public static string AssemblyName(TypeReference t) => t.Scope switch
    {
        AssemblyNameReference a => a.Name,
        ModuleDefinition m => m.Assembly.Name.Name,
        ModuleReference r => r.Name,
        _ => string.Empty
    };

    /// <summary>Types referenced by the PUBLIC/PROTECTED signature of a type (not method bodies).</summary>
    public static IEnumerable<TypeReference> SignatureRefs(TypeDefinition t)
    {
        var raw = new List<TypeReference>();
        if (t.BaseType != null)
        {
            raw.Add(t.BaseType);
        }

        raw.AddRange(t.Interfaces.Select(i => i.InterfaceType));
        foreach (var f in t.Fields.Where(f => f.IsPublic || f.IsFamily))
        {
            raw.Add(f.FieldType);
        }

        foreach (var p in t.Properties)
        {
            var acc = p.GetMethod ?? p.SetMethod;
            if (acc != null && (acc.IsPublic || acc.IsFamily))
            {
                raw.Add(p.PropertyType);
            }
        }

        foreach (var m in t.Methods.Where(m => m.IsPublic || m.IsFamily))
        {
            raw.Add(m.ReturnType);
            raw.AddRange(m.Parameters.Select(p => p.ParameterType));
        }

        return raw.SelectMany(Flatten).Select(Top);
    }

    // ------------------------------------------------------------------ internals

    private static IEnumerable<Ref> Scan(ModuleDefinition module)
    {
        foreach (var type in module.GetTypes())
        {
            var owner = TopDef(type);
            if (owner.Name == "<Module>" || owner.Name.StartsWith("<PrivateImplementationDetails>", StringComparison.Ordinal))
            {
                continue;
            }

            foreach (var raw in RawRefs(type))
            {
                foreach (var flat in Flatten(raw))
                {
                    var to = Top(flat);
                    if (ReferenceEquals(to, owner) || to.FullName == owner.FullName)
                    {
                        continue;
                    }

                    yield return new Ref(owner, to);
                }
            }
        }
    }

    private static IEnumerable<TypeReference> RawRefs(TypeDefinition t)
    {
        if (t.BaseType != null)
        {
            yield return t.BaseType;
        }

        foreach (var i in t.Interfaces)
        {
            yield return i.InterfaceType;
        }

        foreach (var a in t.CustomAttributes)
        {
            yield return a.AttributeType;
        }

        foreach (var f in t.Fields)
        {
            yield return f.FieldType;
        }

        foreach (var p in t.Properties)
        {
            yield return p.PropertyType;
        }

        foreach (var m in t.Methods)
        {
            yield return m.ReturnType;
            foreach (var p in m.Parameters)
            {
                yield return p.ParameterType;
            }

            if (!m.HasBody)
            {
                continue;
            }

            foreach (var v in m.Body.Variables)
            {
                yield return v.VariableType;
            }

            foreach (var ins in m.Body.Instructions)
            {
                switch (ins.Operand)
                {
                    case TypeReference tr:
                        yield return tr;
                        break;
                    case MethodReference mr:
                        yield return mr.DeclaringType;
                        yield return mr.ReturnType;
                        foreach (var p in mr.Parameters)
                        {
                            yield return p.ParameterType;
                        }

                        if (mr is GenericInstanceMethod gim)
                        {
                            foreach (var ga in gim.GenericArguments)
                            {
                                yield return ga;
                            }
                        }

                        break;
                    case FieldReference fr:
                        yield return fr.DeclaringType;
                        yield return fr.FieldType;
                        break;
                }
            }
        }
    }

    private static IEnumerable<TypeReference> Flatten(TypeReference? tr)
    {
        switch (tr)
        {
            case null:
            case GenericParameter:
            case FunctionPointerType:
                yield break;
            case GenericInstanceType g:
                foreach (var a in g.GenericArguments)
                {
                    foreach (var x in Flatten(a))
                    {
                        yield return x;
                    }
                }

                foreach (var x in Flatten(g.ElementType))
                {
                    yield return x;
                }

                yield break;
            case TypeSpecification s:
                foreach (var x in Flatten(s.ElementType))
                {
                    yield return x;
                }

                yield break;
            default:
                yield return tr;
                yield break;
        }
    }

    private static TypeReference Top(TypeReference t)
    {
        while (t.DeclaringType != null)
        {
            t = t.DeclaringType;
        }

        return t;
    }

    private static TypeDefinition TopDef(TypeDefinition t)
    {
        while (t.DeclaringType != null)
        {
            t = t.DeclaringType;
        }

        return t;
    }
}
