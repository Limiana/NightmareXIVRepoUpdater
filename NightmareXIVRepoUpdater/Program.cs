namespace NightmareXIVRepoUpdater;

internal class Program
{
    public static Updater Updater = null!;

    static void Main(string[] args)
    {
        try
        {
            Updater = new(args);
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
        Console.ReadLine();
    }
}
