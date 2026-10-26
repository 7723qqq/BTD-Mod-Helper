using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Tests;

/// <summary>
/// Guards the localization files against the two failure modes that made Mod Helper silently
/// fall back to English: file names that are not game <c>Language</c> enum members, and language
/// files drifting apart from each other.
/// </summary>
public class LocalizationTests
{
    private const string KeyPrefix = "BloonsTD6 Mod Helper-";
    private const int MinimumEntries = 82;

    private static string LocalizationDirectory =>
        Path.Combine(ModSourcesDiscovery.SolutionRoot, "BloonsTD6 Mod Helper", "Localization");

    private static Dictionary<string, JObject> LoadAll() =>
        Directory.GetFiles(LocalizationDirectory, "*.json")
            .Select(file => (
                Name: Path.GetFileNameWithoutExtension(file) ?? file,
                Table: JObject.Parse(File.ReadAllText(file))))
            .ToDictionary(entry => entry.Name, entry => entry.Table, StringComparer.Ordinal);

    [Fact]
    public void AllFilesAreWellFormed()
    {
        var all = LoadAll();

        Assert.NotEmpty(all);
        foreach (var (language, table) in all)
        {
            Assert.True(table.Count >= MinimumEntries,
                $"{language}.json only has {table.Count} entries, expected at least {MinimumEntries}");

            foreach (var property in table.Properties())
            {
                Assert.StartsWith(KeyPrefix, property.Name);
                Assert.False(string.IsNullOrWhiteSpace(property.Value.Value<string>()),
                    $"{language}.json: '{property.Name}' is empty");
            }
        }
    }

    [SkippableFact]
    public void FileNamesMatchGameLanguageEnum()
    {
        // Mod Helper creates one directory per supported Language value on startup, so those
        // directory names are the enum members without having to load the Il2Cpp assembly.
        var languageRoot = Path.Combine(ModSourcesDiscovery.Btd6Path, "Btd6ModHelper", "Localization");
        Skip.IfNot(Directory.Exists(languageRoot),
            $"'{languageRoot}' does not exist yet - run the game once with Mod Helper installed");

        var supported = Directory.GetDirectories(languageRoot)
            .Select(Path.GetFileName)
            .OfType<string>()
            .ToHashSet(StringComparer.Ordinal);
        Skip.If(supported.Count == 0, "No language directories were found");

        var mismatched = Directory.GetFiles(LocalizationDirectory, "*.json")
            .Select(Path.GetFileNameWithoutExtension)
            .OfType<string>()
            .Where(file => !supported.Contains(file))
            .OrderBy(file => file)
            .ToArray();

        Assert.True(mismatched.Length == 0,
            $"These localization files never load because the name is not a Language enum member: " +
            $"{string.Join(", ", mismatched)}. Supported names: {string.Join(", ", supported.Order())}");
    }

    [Fact]
    public void ReviewedLanguagesShareTheSameKeys()
    {
        var all = LoadAll();
        Assert.True(all.ContainsKey("ChineseSimplified"), "ChineseSimplified.json is the key-set baseline");

        var canonical = all["ChineseSimplified"].Properties().Select(property => property.Name)
            .ToHashSet(StringComparer.Ordinal);
        Assert.True(canonical.Count >= 130, $"Baseline only has {canonical.Count} keys, expected at least 130");

        string[] reviewed = ["ChineseSimplified", "ChineseTraditional", "Japanese", "Russian", "German"];
        foreach (var language in reviewed)
        {
            Assert.True(all.ContainsKey(language), $"{language}.json is missing");

            var keys = all[language].Properties().Select(property => property.Name)
                .ToHashSet(StringComparer.Ordinal);
            var missing = canonical.Except(keys).OrderBy(key => key).ToArray();
            var extra = keys.Except(canonical).OrderBy(key => key).ToArray();

            Assert.True(missing.Length == 0 && extra.Length == 0,
                $"{language}.json key mismatch. missing=[{string.Join(", ", missing)}] extra=[{string.Join(", ", extra)}]");
        }
    }
}
