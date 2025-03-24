using HackAssembler.Core;

namespace HackAssembler;

public class Program
{
    public const string fileName = "Max";

    static void Main(string[] args)
    {
        var app = new App(fileName);
        app.Run();
    }
}
