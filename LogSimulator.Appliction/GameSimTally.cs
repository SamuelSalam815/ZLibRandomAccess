using System.Net.NetworkInformation;

namespace LogSimulator.Appliction;

public class GameSimTally(int gamesPerTally, int talliesPerLine)
{
    private int? _gameSimCount;

    private int NewLineThreshold => gamesPerTally * talliesPerLine;

    private int ClumpSize => gamesPerTally * 5;

    private bool _isMarkSpecial;

    public void MarkGameSimulated()
    {
        if (_gameSimCount is null || _gameSimCount >= NewLineThreshold)
        {
            _gameSimCount = 0;
            Console.Write("\nGame Simulation Tally ({0} games per tally): ", gamesPerTally);
            return;
        }

        _gameSimCount++;

        if (_gameSimCount % gamesPerTally == 0)
        {
            Console.Write(_isMarkSpecial ? 'I' : '|');
            _isMarkSpecial = false;
        }

        if (_gameSimCount % ClumpSize == 0)
        {
            Console.Write(' ');
        }
    }

    public void MakeNextMarkSpecial()
    {
        _isMarkSpecial = true;
    }
}
