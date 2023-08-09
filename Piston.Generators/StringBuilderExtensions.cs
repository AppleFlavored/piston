using System.Text;

namespace Piston.Generators;

public static class StringBuilderExtensions
{
    public static StringBuilder Indent(this StringBuilder builder, int indentSize)
    {
        builder.Append(new string(' ', indentSize));
        return builder;
    }
}