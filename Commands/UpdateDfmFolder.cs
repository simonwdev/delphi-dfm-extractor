using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using DfmExtractor.Delphi;
using DfmExtractor.Extensions;

namespace DfmExtractor.Commands
{
    public class UpdateDfmFolder
    {
        public UpdateDfmFolder()
        {
        }

        public void Execute()
        {
            var defaultEx = @"C:\CCare\Development\3rdPartyComponents;C:\CCare\Development\.git;C:\CCare\Development\Components;C:\CCare\Development\DotNet\bin";

            var inputDirectory = ConfigurationManager.AppSettings.GetSetting("InputPath", @"C:\CCare\Development");
            var outputDirectory = ConfigurationManager.AppSettings.GetSetting("OutputPath", @"C:\CCare\Development\Schema");
            var exclusions = ConfigurationManager.AppSettings.GetSetting("Exclude", defaultEx);

            var dfmPath = Path.Combine(outputDirectory, "Dfm");

            if (!Directory.Exists(dfmPath))
                Directory.CreateDirectory(dfmPath);

            var existingFileSet = Directory.GetFiles(dfmPath)
                .ToHashSet(a => a, StringComparer.CurrentCultureIgnoreCase);

            var existingDirectorySet = Directory.GetDirectories(dfmPath)
                .ToHashSet(a => a, StringComparer.CurrentCultureIgnoreCase);

            var exclusionSet = exclusions
                .Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries)
                .ToHashSet(a => a, StringComparer.InvariantCultureIgnoreCase);

            var fileNames = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

            foreach (var sourceFile in Directory.GetFiles(inputDirectory, "*.dfm", SearchOption.AllDirectories))
            {
                var ignore = false;
                foreach (var excludeDirectory in exclusionSet)
                {
                    if (sourceFile.StartsWith(excludeDirectory))
                    {
                        ignore = true;
                        break;
                    }
                }

                if (!ignore)
                {
                    //// NOTE: The symbolic link needs to be excluded at this level.
                    //var baseDirectory = Path.GetDirectoryName(sourceFile);
                    //if (baseDirectory == @"C:\CCare\Development\DotNet\bin")
                    //    continue;

                    var relativePath = Path.GetDirectoryName(sourceFile)?
                        .Replace(inputDirectory, string.Empty)
                        .TrimStart('\\').TrimStart('\\') ?? string.Empty;

                    var destinationPath = Path.Combine(dfmPath, relativePath);
                    var fileNoExtension = Path.GetFileNameWithoutExtension(sourceFile);
                    var fileName = Path.Combine(destinationPath, fileNoExtension + ".sql");

                    if (fileNames.ContainsKey(fileName))
                        throw new ApplicationException($"File '{sourceFile}' exists from source '{fileNames[fileName]}'.");

                    fileNames.Add(fileName, sourceFile);

                    var builder = new StringBuilder();
                    var lexer = new DfmLexer(File.OpenText(sourceFile));
                    var parser = new DfmParser(lexer);
                    var form = parser.ReadObject();

                    var objects = form.Children
                        .Flatten(a => a.Children)
                        .ToArray();

                    var firstFile = false;

                    foreach (var component in objects.OrderBy(a => a.ObjectName))
                    {
                        var firstComponent = false;

                        foreach (var property in component.Properties.OrderBy(a => a.Name))
                        {
                            if (property.Name.Contains("SQL") && !string.IsNullOrWhiteSpace(property.Value))
                            {
                                if (!firstFile)
                                {
                                    firstFile = true;

                                    existingFileSet.Remove(fileName);

                                    builder.AppendLine("/* Automatically generated - do not change */");
                                    builder.AppendLine($"/* Source File: {sourceFile} */");
                                    builder.AppendLine();
                                    builder.AppendLine();
                                }

                                if (!firstComponent)
                                {
                                    firstComponent = true;

                                    builder.AppendLine("/*");
                                    builder.AppendLine($"    Name: {fileNoExtension}.{component.ObjectName}: {component.ClassName}");
                                    builder.AppendLine("*/");
                                    builder.AppendLine();
                                }

                                builder.AppendLine($"/* Property: {property.Name} */");
                                builder.AppendLine(property.Value.Trim());
                                builder.AppendLine();
                            }
                        }
                    }

                    if (builder.Length > 0)
                    {
                        if (!Directory.Exists(destinationPath))
                            Directory.CreateDirectory(destinationPath);

                        File.WriteAllText(fileName, builder.ToString(), Encoding.UTF8);
                    }
                }
            }
            
            foreach (var file in existingFileSet)
            {
                File.Delete(file);
            }

            foreach (var directory in existingDirectorySet.OrderByDescending(a => a))
            {
                if (Directory.GetFiles(directory).Length == 0)
                    Directory.Delete(directory, true);
            }
        }
    }
}
