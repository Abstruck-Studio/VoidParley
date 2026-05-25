using System;
using Godot;

namespace VoidParley.core;

public partial class ResourceLocation(string ns, string id) : Node
{ 
    public string Namespace { get; init; } = ns;
    public string Identifier { get; init; } = id;

    public static ResourceLocation Parse(string s)
    {
        var parts = s.Split(':');
        if (parts.Length != 2 || string.IsNullOrEmpty(parts[0]) || string.IsNullOrEmpty(parts[1]))
        {
            throw new ArgumentException($"Invalid ResourceLocation format: {s}. Expected format: 'namespace:identifier'");
        }
        return new ResourceLocation(parts[0], parts[1]);
    }
    public override string ToString() => $"{Namespace}:{Identifier}";
}