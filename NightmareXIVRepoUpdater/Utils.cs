using Newtonsoft.Json;
using Octokit;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NightmareXIVRepoUpdater;
internal static class Utils
{
    public static bool AreChangesSignificant(this PluginManifest a, PluginManifest b)
    {
        var insignificant =
            a.AssemblyVersion == b.AssemblyVersion
                && a.TestingAssemblyVersion == b.TestingAssemblyVersion
                && a.IconUrl == b.IconUrl
                && a.DownloadLinkInstall == b.DownloadLinkInstall
                && a.DownloadLinkTesting == b.DownloadLinkTesting
                && a.DownloadLinkUpdate == b.DownloadLinkUpdate
                && a.LastUpdate == b.LastUpdate
                && a.ApplicableVersion == b.ApplicableVersion
                && a.DalamudApiLevel == b.DalamudApiLevel
                && a.TestingAssemblyVersion == b.TestingAssemblyVersion
                && a.Description == b.Description
                && a.Punchline == b.Punchline
                && a.Changelog == b.Changelog
                && a.SupportsProfiles == b.SupportsProfiles
                && a.ImageUrls.SequenceEqualsNullSafe(b.ImageUrls)
                && a.IsTestingExclusive == b.IsTestingExclusive
                && a.Name == b.Name
                && Math.Abs(a.DownloadCount - b.DownloadCount) < 1000;
        return !insignificant;
    }

    public static bool SequenceEqualsNullSafe<T>(this IEnumerable<T>? a, IEnumerable<T>? b)
    {
        if (a == null)
        {
            return b == null;
        }
        if(b == null)
        {
            return a == null;
        }
        return a.SequenceEqual(b);
    }

    public static ReleaseAsset? FindSuitableAsset(this IEnumerable<ReleaseAsset> assets)
    {
        foreach (var x in assets)
        {
            if (x.Name.Equals("latest.zip", StringComparison.OrdinalIgnoreCase)) return x;
        }
        foreach (var x in assets)
        {
            if (x.Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)) return x;
        }
        return null;
    }

    public static IEnumerable<ReleaseAsset> FindSuitableAssets(this IEnumerable<ReleaseAsset> assets)
    {
        foreach (var x in assets)
        {
            if (x.Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)) yield return x;
        }
    }
    public static PluginManifest? GetMainfest(Stream stream)
    {
        using ZipArchive zip = new ZipArchive(stream);
        foreach (ZipArchiveEntry entry in zip.Entries)
        {
            if (entry.FullName.EndsWith(".dll"))
            {
                try
                {
                    Console.WriteLine($"Inspecting {entry.FullName}");
                    using var sr = new MemoryStream(ReadStream(entry.Open()));
                    using var portableExecutableReader = new PEReader(sr);
                    var metadataReader = portableExecutableReader.GetMetadataReader();

                    foreach (var typeDefHandle in metadataReader.TypeDefinitions)
                    {
                        var typeDef = metadataReader.GetTypeDefinition(typeDefHandle);
                        var name = metadataReader.GetString(typeDef.Name);
                        //Console.WriteLine($"Checking {name}");
                        var interfaceHandles = typeDef.GetInterfaceImplementations();

                        foreach (var interfaceHandle in interfaceHandles)
                        {
                            var interfaceImplementation = metadataReader.GetInterfaceImplementation(interfaceHandle);
                            var interfaceType = interfaceImplementation.Interface;

                            if (interfaceType.Kind == HandleKind.TypeReference)
                            {
                                var typeReferenceHandle = (TypeReferenceHandle)interfaceType;
                                var typeReference = metadataReader.GetTypeReference(typeReferenceHandle);
                                var namespaceName = metadataReader.GetString(typeReference.Namespace);
                                var interfaceName = metadataReader.GetString(typeReference.Name);
                                if ($"{namespaceName}.{interfaceName}" == "Dalamud.Plugin.IDalamudPlugin")
                                {
                                    var jsonF = $"{entry.FullName[..^4]}.json";
                                    Console.WriteLine($"Found plugin: {entry.FullName}, loading {jsonF}");
                                    using var reader = new StreamReader(zip.Entries.First(x => x.FullName == jsonF).Open());
                                    var json = JsonConvert.DeserializeObject<PluginManifest>(reader.ReadToEnd());
                                    json.LastUpdate = entry.LastWriteTime.ToUnixTimeSeconds();
                                    return json;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Can't load {entry.FullName}: {ex.Message}");
                }
            }
        }
        return null;
    }

    public static byte[] ReadStream(Stream sourceStream)
    {
        using (var memoryStream = new MemoryStream())
        {
            sourceStream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }
    }
}
