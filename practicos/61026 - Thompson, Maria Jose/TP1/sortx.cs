using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

try
{
    var config = ParseArgs(args);
    var text = ReadInput(config);

    var rows = ParseDelimited(text, config);
    var sorted = SortRows(rows, config);
    var outputText = Serialize(sorted, config);

    WriteOutput(outputText, config);
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    Environment.Exit(1);
}

AppConfig ParseArgs(string[] args)
{
    string? inputFile = null;
    string? outputFile = null;
    string delimiter = ",";
    bool noHeader = false;
    var sortFields = new List<SortField>();

    for (int i = 0; i < args.Length; i++)
    {
        switch (args[i])
        {
            case "-i":
            case "--input":
                inputFile = args[++i];
                break;
            case "-o":
            case "--output":
                outputFile = args[++i];
                break;
            case "-d":
            case "--delimiter":
                delimiter = args[++i].Replace("\\t", "\t");
                break;
            case "-nh":
            case "--no-header":
                noHeader = true;
                break;
            case "-b":
            case "--by":
                var parts = args[++i].Split(':');
                string name = parts[0];
                bool numeric = parts.Length > 1 && parts[1] == "num";
                bool desc = parts.Length > 2 && parts[2] == "desc";
                sortFields.Add(new SortField(name, numeric, desc));
                break;
            default:
                if (inputFile == null) inputFile = args[i];
                else if (outputFile == null) outputFile = args[i];
                break;
        }
    }

    return new AppConfig(inputFile, outputFile, delimiter, noHeader, sortFields);
}

string ReadInput(AppConfig cfg)
{
    if (cfg.InputFile is null)
        return Console.In.ReadToEnd();
    else
        return File.ReadAllText(cfg.InputFile);
}

List<Dictionary<string, string>> ParseDelimited(string text, AppConfig cfg)
{
    var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
    var rows = new List<Dictionary<string, string>>();

    string[] headers;
    int start = 0;

    if (!cfg.NoHeader)
    {
        headers = lines[0].Trim().Split(cfg.Delimiter);
        start = 1;
    }
    else
    {
        headers = lines[0].Trim().Split(cfg.Delimiter)
            .Select((_, idx) => idx.ToString()).ToArray();
    }

    for (int i = start; i < lines.Length; i++)
    {
        var values = lines[i].Trim().Split(cfg.Delimiter);
        var dict = new Dictionary<string, string>();
        for (int j = 0; j < headers.Length && j < values.Length; j++)
        {
            dict[headers[j]] = values[j];
        }
        rows.Add(dict);
    }

    return rows;
}

List<Dictionary<string, string>> SortRows(List<Dictionary<string, string>> rows, AppConfig cfg)
{
    IOrderedEnumerable<Dictionary<string, string>>? ordered = null;

    foreach (var field in cfg.SortFields)
    {
        Func<Dictionary<string, string>, object> keySelector = row =>
        {
            if (!row.ContainsKey(field.Name))
                throw new Exception($"Campo inexistente: {field.Name}");

            var val = row[field.Name];
            if (field.Numeric && double.TryParse(val, out var num))
                return num;
            return val;
        };

        if (ordered == null)
        {
            ordered = field.Descending
                ? rows.OrderByDescending(keySelector)
                : rows.OrderBy(keySelector);
        }
        else
        {
            ordered = field.Descending
                ? ordered.ThenByDescending(keySelector)
                : ordered.ThenBy(keySelector);
        }
    }
