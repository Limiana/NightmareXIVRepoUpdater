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
        try
        {
            Data = File.ReadAllText(@"D:\NXRU.txt").Split("\n").Where(x => x.Contains('=')).ToDictionary(x => x.Split("=")[0], x => x.Split("=")[1]);
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    public string ReadOnlyKey => Data["ReadOnlyKey"];
    public string NightmareXIVWriteKey => Data["NightmareXIVWriteKey"];
    public string DynamicBridgeStandaloneWriteKey => Data["DynamicBridgeStandaloneWriteKey"];
}
