namespace NightmareXIVRepoUpdater;

internal class Program
{
    public static Updater Updater = null!;

    static void Main(string[] args)
    {
        Updater = new(args);
        var task = Updater.Run();
        task.Wait();
    }
}
