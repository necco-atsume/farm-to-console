using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;

using System.Text.Json;


var sourceOption = new Option<FileInfo>("--source", description: "the source MessageData.bin to extract text from.") { IsRequired = true };
var outputOption = new Option<FileInfo>("--output", getDefaultValue: () => new FileInfo("MessageData.bin.json"), description: "the filename of the JSON file contianing the extracted text") { IsRequired = true };
var overwriteOption = new Option<bool>("--overwrite", getDefaultValue: () => false, description: "whether to overwrite the output file if it exists.");

var extractText = new Command("extract-text", "extract text from a Harvest Moon DS MessageData.bin file into a JSON format.")
{
};

var root = new RootCommand("farm-to-console: A command-line Harvest Moon DS data editing tool.")
{
    extractText
};

extractText.SetHandler(async (context) => {
    var source = context.ParseResult.GetValueForOption(sourceOption);
    var output = context.ParseResult.GetValueForOption(outputOption);
    bool overwrite = context.ParseResult.GetValueForOption(overwriteOption);

    if (source == null || output == null) throw new InvalidOperationException("Please specify both source and output.");

    if (!overwrite && output.Exists) {
        throw new InvalidOperationException($"File {output} already exists. (Did you mean to pass --overwrite?)");
    }

    if (source.FullName.Equals(output.FullName, StringComparison.OrdinalIgnoreCase)) {
        throw new InvalidOperationException($"Source ({source}) and Output ({output}) are the same.");
    }

    var table = TextTableReader.ReadFrom(source.FullName);
    var serialized = JsonSerializer.Serialize(table);

    await File.WriteAllTextAsync(output.FullName, serialized);
});

return await root.InvokeAsync(args);
