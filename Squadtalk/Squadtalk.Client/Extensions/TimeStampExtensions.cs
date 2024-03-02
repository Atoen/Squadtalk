namespace Squadtalk.Client.Extensions;

using static DateFormatMode;

public static class TimeStampExtensions
{
    private const string ShortFormat = "HH:mm";
    private const string DefaultFormat = "HH:mm:ss";
    private const string LongFormat = "dd-MM-yyyy HH:mm";
    private const string FullFormat = "dd-MM-yyyy HH:mm:ss";
    private const string WeekDayFormat = "dddd HH:mm";
    private const string MonthFormat = "m";
    private const string YearFormat = "d MMM yyyy";
    
    public static string ToStringFormat(this DateTimeOffset dateTimeOffset, DateFormatMode formatMode = Default)
    {
        var localTimestamp = dateTimeOffset.ToLocalTime();
        var date = localTimestamp.Date;
        
        
        return formatMode switch
        {
            Short => localTimestamp.ToString(ShortFormat),
            
            Default when date == DateTime.Today => $"Today {localTimestamp.ToString(ShortFormat)}",
            Default when date == DateTime.Today.AddDays(-1) => $"Yesterday {localTimestamp.ToString(ShortFormat)}",
            Default => localTimestamp.ToString(LongFormat),
            
            Long when date == DateTime.Today => $"Today {localTimestamp.ToString(DefaultFormat)}",
            Long when date == DateTime.Today.AddDays(-1) => $"Yesterday {localTimestamp.ToString(DefaultFormat)}",
            
            ChannelStatus when date == DateTime.Today => localTimestamp.ToString(ShortFormat),
            ChannelStatus when DateTime.Today - date < TimeSpan.FromDays(7) => localTimestamp.ToString(WeekDayFormat),
            ChannelStatus when date.Year == DateTime.Today.Year => localTimestamp.ToString(MonthFormat),
            ChannelStatus => localTimestamp.ToString(YearFormat),
            
            _ => localTimestamp.ToString(FullFormat)
        };
    }
}

public enum DateFormatMode
{
    Default,
    Short,
    Long,
    ChannelStatus
}