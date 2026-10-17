using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using CsvHelper.TypeConversion;

namespace Benchmarking.TextSearch;

public class BenchmarkResult
{
    public string Method {get; set;} = string.Empty;

    public long UncompressedLogSizeInMegabytes {get; set;}

    [TypeConverter(typeof(TimeSpanConverter))]
    public TimeSpan Mean {get; set;}

    [TypeConverter(typeof(TimeSpanConverter))]
    public TimeSpan Error {get; set;}
}

public class TimeSpanConverter : TypeConverter<TimeSpan>{

    public override TimeSpan ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        if(text is null){
            throw new ArgumentException("Text was unexpectedly null!", nameof(text));
        }

        if (text.Split(" ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) is not
            [{ } numberString, { } unitString])
        {
            throw new ArgumentException("Text was in an unexpected format for a TimeSpan", nameof(text));
        }

        var number = double.Parse(numberString, CultureInfo.InvariantCulture);

        return unitString switch
        {
            "ns" =>  TimeSpan.FromMicroseconds(number * 1000),
            "us" =>  TimeSpan.FromMicroseconds(number),
            "ms" =>  TimeSpan.FromMilliseconds(number),
            "s" => TimeSpan.FromSeconds(number),
            _ => throw new ArgumentException($"Unrecognized unit of time '{unitString}'!", nameof(text))
        };
    }

    public override string? ConvertToString(TimeSpan value, IWriterRow row, MemberMapData memberMapData)
    {
        throw new NotImplementedException();
    }
}


public class GraphGenerator
{
    private const string CsvPath = @"2026-09-07results\Benchmarking.TextSearch.BenchmarkingRegexInGzipStream-report.csv";

    public static void Run()
    {
        var records =
            LoadRecords()
                .ToList();

        ;
    }

    private static List<BenchmarkResult> LoadRecords()
    {
        using var reader = new StreamReader(CsvPath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        return csv.GetRecords<BenchmarkResult>().ToList();
    }
}
