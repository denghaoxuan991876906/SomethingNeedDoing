using SomethingNeedDoing.Documentation.StubGenerators.Builders;

namespace SomethingNeedDoing.Documentation.StubGenerators;

internal sealed class EnumStubGenerator<T> : StubGenerator
    where T : Enum
{
    public override StubFile GenerateStub(IEnumerable<Type> _)
    {
        var filename = ToSnakeCase(typeof(T).Name);
        var file = new StubFile("enums", $"{filename}.lua");

        file.AddBuilder(new Builder().AddAlias<T>());

        return file;
    }
}
