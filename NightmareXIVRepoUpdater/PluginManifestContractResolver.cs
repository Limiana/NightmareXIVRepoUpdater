using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NightmareXIVRepoUpdater;
internal class PluginManifestContractResolver : DefaultContractResolver
{
    static string[] ForceInclude = ["IsHide", "IsTestingExclusive"];
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        var property = base.CreateProperty(member, memberSerialization);
        if(ForceInclude.Contains(property.PropertyName)) property.DefaultValueHandling = DefaultValueHandling.Populate;

        return property;
    }
}
