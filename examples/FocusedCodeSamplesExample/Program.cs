using FocusedCodeSamplesExample;

const string sample = """
    the quick brown fox jumps over the lazy dog
    the fox was quick and the dog was lazy
    """;

Console.WriteLine("=== MonolithWordCounter ===");
Console.WriteLine(MonolithWordCounter.CountWords(sample, topN: 3));

Console.WriteLine("=== ModularWordCounter ===");
Console.WriteLine(ModularWordCounter.CountWords(sample, topN: 3));