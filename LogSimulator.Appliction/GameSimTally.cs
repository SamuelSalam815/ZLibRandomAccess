using System.Net.NetworkInformation;

namespace LogSimulator.Appliction;

public class GameSimTally(int gamesPerTally, int talliesPerLine)
{
    private const char SpecialTally = 'I';

    private const char Tally = '|';

    private const char GroupSeparator = ' ';

    private int? _gameSimCount;

    private int NewLineThreshold => gamesPerTally * talliesPerLine;

    private int GamesPerClump => gamesPerTally * TalliesPerClump;

    private int GamesPerLine => gamesPerTally * talliesPerLine;

    private int TalliesPerClump => 5;

    private bool _isMarkSpecial;

    public string TallyLegend => $"""
                                 {gamesPerTally} games are represented by '{Tally}'
                                 Tallies marked as special are written as '{SpecialTally}'
                                 Groups of {TalliesPerClump} tallies ({GamesPerClump} games) are separated by '{GroupSeparator}'
                                 A completed line contains {talliesPerLine} tallies ({GamesPerLine} games)
                                 """;

    public void MarkGameSimulated()
    {
        if (_gameSimCount is null || _gameSimCount >= NewLineThreshold)
        {
            _gameSimCount = 0;
            Console.WriteLine();
            return;
        }

        _gameSimCount++;

        if (_gameSimCount % gamesPerTally == 0)
        {
            Console.Write(_isMarkSpecial ? SpecialTally : Tally);
            _isMarkSpecial = false;
        }

        if (_gameSimCount % GamesPerClump == 0)
        {
            Console.Write(GroupSeparator);
        }
    }

    public void MakeNextMarkSpecial()
    {
        _isMarkSpecial = true;
    }
}
