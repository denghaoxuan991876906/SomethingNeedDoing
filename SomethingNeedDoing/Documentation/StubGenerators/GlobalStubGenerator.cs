using SomethingNeedDoing.Documentation.StubGenerators.Builders;

namespace SomethingNeedDoing.Documentation.StubGenerators;

internal sealed class GlobalStubGenerator : StubGenerator
{
    public override StubFile GenerateStub(IEnumerable<Type> _)
    {
        var file = new StubFile("global.lua");

        file.AddBuilder(new Builder().AddAlias("object", "any"));

        return file;
    }
}
