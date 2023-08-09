using Tomlyn;
using Tomlyn.Model;

namespace Piston;

public sealed class TomlConfig
{
    public TomlTable Table { get; init; }

    /// <summary>
    /// The main constructor, if file does not exist we create one based on the table we pass in.
    /// if file does exist we read it and save it.
    /// </summary>
    /// <param name="table">Takes in default defined config from TomlConfig folder</param>
    /// <param name="fileName">Name of configuration file, works as a prefix [fileName]_config.toml</param>
    public TomlConfig(TomlTable table, string fileName)
    {
        // Create "configs" folder, it holds all of the configs
        DirectoryInfo di = Directory.CreateDirectory("configs");
        // Path to the config file
        string realFileName = $"{di.Name}/{fileName}_config.toml";

        if (File.Exists(realFileName))
        {
            string text = File.ReadAllText(realFileName);
            Table = Toml.ToModel(text);
        } 
        else
        {
            string text = Toml.FromModel(table);
            File.WriteAllText(realFileName, text);
            Table = table;
        }
    }

    /// <summary>
    /// Makes it really comfortable to interface with the class.
    /// Example of usage: Instance["sub-table"]["one-key"]
    /// </summary>
    /// <param name="subTableName">The name of desired subtable</param>
    /// <param name="key">The name of desired key</param>
    /// <returns></returns>
    public object this[string subTableName, string key]
    {
        get => ((TomlTable)Table[subTableName]!)[key];
    }
    public object this[string key]
    {
        get => Table[key];
    }
}
