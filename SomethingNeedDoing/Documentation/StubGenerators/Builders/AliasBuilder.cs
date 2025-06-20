namespace SomethingNeedDoing.Documentation.StubGenerators.Builders;

public static class AliasBuilder
{
    public static Builder AddAlias(this Builder builder, string original, string alias)
    {
        return builder.AddLine("alias", original, alias);
    }

    public static Builder AddAlias<T>(this Builder builder)
        where T : Enum
    {
        var enumType = typeof(T);

        var names = Enum.GetNames(enumType);
        var values = Enum.GetValues(enumType);

        // Helper docs
        builder.AddLine($"-- Usage:");
        builder.AddLine($"-- luanet.load_assembly('{enumType.Assembly.GetName().Name ?? "UnknownAssembly"}')");
        builder.AddLine($"-- {enumType.Name} = luanet.import_type('{enumType.FullName ?? enumType.Name}')");

        var firstName = names.Length > 0 ? names[0] : string.Empty;
        if (!string.IsNullOrEmpty(firstName))
        {
            builder.AddLine($"-- {enumType.Name}.{firstName}");
        }

        builder.AddLine("");

        // Alias for numeric values
        builder.AddLine($"--- @alias {enumType.Name}Value");
        for (int i = 0; i < names.Length; i++)
        {
            var numericValue = Convert.ChangeType(values.GetValue(i), Enum.GetUnderlyingType(enumType));
            builder.AddLine($"---| {numericValue} # {names[i]}");
        }
        builder.AddLine("");

        // Generate the table-like class definition for enum members
        builder.AddLine($"--- @class {enumType.Name}");
        foreach (var name in names)
        {
            builder.AddLine($"--- @field {name} {enumType.Name}Value");
        }

        return builder;
    }
}
