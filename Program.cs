using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        var matrix = new[]
        {
            "ABCDC",
            "FGWIO",
            "CHILL",
            "PQNSD",
            "UVDXY"
        };

        var wf = new WordFinder(matrix);
        var stream = new[] { "cold", "wind", "snow", "chill"};

        var result = wf.Find(stream);

        Console.WriteLine("Words found:");
        foreach (var w in result)
            Console.WriteLine(w);
    }
}