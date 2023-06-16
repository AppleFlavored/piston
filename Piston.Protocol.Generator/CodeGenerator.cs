using System.Text;

namespace Piston.Protocol.Generator;

// FIXME: Rewrite this whole class...
public class CodeGenerator
{
    private StringBuilder _source = new();
    private int _indent = 0;

    public CodeGenerator BeginBlock()
    {
        Line("{");
        _indent++;
        return this;
    }

    public CodeGenerator EndBlock()
    {
        _indent--;
        Line("}");
        return this;
    }
    
    public CodeGenerator Using(string usingNamespace)
    {
        Line($"using {usingNamespace};");
        return this;
    }

    public CodeGenerator Namespace(string name)
    {
        Line($"namespace {name};");
        return this;
    }

    public CodeGenerator Class(string className)
    {
        Line($"public class {className}");
        BeginBlock();
        return this;
    }

    public CodeGenerator Line(string line)
    {
        _source
            .Append(new string(' ', _indent * 4))
            .AppendLine(line);
        return this;
    }

    public string GetSource()
    {
        return _source.ToString();
    }
}