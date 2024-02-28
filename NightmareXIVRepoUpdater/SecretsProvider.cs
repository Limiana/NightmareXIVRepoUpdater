using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NightmareXIVRepoUpdater;
internal class SecretsProvider
{
    Dictionary<string, string> Data = [];
    public SecretsProvider()
    {
        Data["ReadOnlyKey"] = Environment.GetEnvironmentVariable("READONLYKEY");
        Data["NightmareXIVWriteKey"] = Environment.GetEnvironmentVariable("NIGHTMAREXIVWRITEKEY");
        Data["DynamicBridgeStandaloneWriteKey"] = Environment.GetEnvironmentVariable("DYNAMICBRIDGESTANDALONEWRITEKEY");
        Console.WriteLine($"Keys retrieved: {string.Join(", ", Data.Where(x => x.Value != null).Select(x => x.Key))}");
        /*
        try
        {
            Data = File.ReadAllText(@"").Split("\n").Where(x => x.Contains('=')).ToDictionary(x => x.Split("=")[0], x => x.Split("=")[1]);
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }*/
    }

    public string ReadOnlyKey => Data["ReadOnlyKey"];
    public string NightmareXIVWriteKey => Data["NightmareXIVWriteKey"];
    public string DynamicBridgeStandaloneWriteKey => Data["DynamicBridgeStandaloneWriteKey"];
}
