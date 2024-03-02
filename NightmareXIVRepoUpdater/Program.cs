namespace NightmareXIVRepoUpdater;

internal class Program
{
    public static Updater Updater = null!;

    static void Main(string[] args)
    {
        for (int i = 10; i > 0; i--)
        {
            Console.WriteLine($"Counting down, {i}");
            Thread.Sleep(1000);
        }
        Updater = new(args);
        var task = Updater.Run();
        task.Wait();
    }
}
