namespace Squadtalk.Client.Extensions;

public static class TimeStampExtensions
{
    private const string ShortFormat = "HH:mm";
    private const string DefaultFormat = "HH:mm:ss";
    private const string LongFormat = "dd-MM-yyyy HH:mm";
    private const string FullFormat = "dd-MM-yyyy HH:mm:ss";
    private const string WeekDayFormat = "dddd HH:mm";
    private const string MonthDayFormat = "m";
    
    public static string ToStringFormat(this DateTimeOffset dateTimeOffset, DateFormatMode formatMode = DateFormatMode.Default)
    {
        // var localTimestamp = dateTimeOffset.ToLocalTime();
        //
        // return formatMode switch
        // {
        //     DateFormatMode.Short => localTimestamp.ToString(ShortFormat),
        //     
        //     DateFormatMode.Default or DateFormatMode.DefaultWithSeconds when localTimestamp.Date == DateTime.Today =>
        //         localTimestamp.ToString(DefaultFormat),
        //     
        //     DateFormatMode.Default or DateFormatMode.DefaultWithSeconds when localTimestamp.Date == DateTime.Today.AddDays(-1) =>
        //         $"Yesterday {localTimestamp.ToString(DefaultFormat)}",
        //     
        //     DateFormatMode.Default => localTimestamp.ToString(LongFormat),
        //     
        //     DateFormatMode.DefaultWithSeconds when localTimestamp.Date == DateTime.Today.AddDays(-1)
        //         => $"Yesterday {localTimestamp.ToString(DefaultFormat)}",
        //     
        //     DateFormatMode.DefaultWithSeconds when localTimestamp.Date == DateTime.Today
        //         => localTimestamp.ToString(DefaultFormat),
        //     
        //     DateFormatMode.Long => localTimestamp.ToString(LongFormat),
        //     DateFormatMode.Day when DateTime.Now - localTimestamp < TimeSpan.FromDays(7) =>
        //         localTimestamp.ToString(WeekDayFormat),
        //     
        //     DateFormatMode.Day => localTimestamp.ToString(MonthDayFormat),
        //     _ => throw new ArgumentOutOfRangeException(nameof(formatMode), formatMode, null)
        // };

        var format = formatMode == DateFormatMode.Long ? "HH:mm:ss" : "HH:mm";
        var localTime = dateTimeOffset.ToLocalTime();
        
        if (formatMode == DateFormatMode.Short)
        {
            return localTime.ToString(format);
        }
        
        if (localTime.Date == DateTime.Today)
        {
            return $"Today {localTime.ToString(format)}";
        }
        
        if (localTime.Date == DateTime.Today.AddDays(-1))
        {
            return $"Yesterday {localTime.ToString(format)}";
        }
        
        var fullFormat = formatMode == DateFormatMode.Long ? "dd-MM-yyyy HH:mm:ss" : "dd-MM-yyyy HH:mm";
        return localTime.ToString(fullFormat);
    }
}

public enum DateFormatMode
{
    Default,
    DefaultWithSeconds,
    Short,
    Long,
    Day
}