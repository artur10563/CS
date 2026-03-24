
using Lab2_ConsoleApp;

string input = string.Empty;
while (true)
{
    Console.Write("Text to predict. 'q' to quit => ");
    input = Console.ReadLine() ?? string.Empty;

    if (input == "q") break;

    Sad_Text_Model.ModelInput sampleData = new Sad_Text_Model.ModelInput() { Col0 = input };

    Sad_Text_Model_v2.ModelInput v2 = new Sad_Text_Model_v2.ModelInput() { Col0 = input };


    var sortedScoresWithLabel = Sad_Text_Model.PredictAllLabels(sampleData).Select(x => new KeyValuePair<string, float>(x.Key == "0" ? "Sad" : "Positive", x.Value));
    var sortedScoresWithLabelv2 = Sad_Text_Model_v2.PredictAllLabels(v2).Select(x => new KeyValuePair<string, float>(x.Key == "0" ? "Sad" : "Positive", x.Value));


    Console.WriteLine($"{"Class",-40}{"Score",-20}");
    Console.WriteLine($"{"-----",-40}{"-----",-20}");

    foreach (var score in sortedScoresWithLabel)
    {
        Console.ForegroundColor = score.Key == "Sad" ? ConsoleColor.Red : ConsoleColor.Green;
        Console.WriteLine($"{score.Key,-40}{score.Value,-20}");
        Console.ForegroundColor = ConsoleColor.White;
    }

    Console.WriteLine($"{"-----",-40}");

    foreach (var score in sortedScoresWithLabelv2)
    {
        Console.ForegroundColor = score.Key == "Sad" ? ConsoleColor.Red : ConsoleColor.Green;
        Console.WriteLine($"{score.Key,-40}{score.Value,-20}");
        Console.ForegroundColor = ConsoleColor.White;
    }



}

Console.WriteLine("=============== End of process, hit any key to finish ===============");
Console.ReadKey();

