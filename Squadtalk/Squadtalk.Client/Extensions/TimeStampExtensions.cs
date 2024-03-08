using System.Globalization;

namespace Squadtalk.Client.Extensions;

using static DateFormatMode;

public static class TimeStampExtensions
{
    private const string HourMinute = "HH:mm";
    private const string HourMinuteSecond = "HH:mm:ss";
    private const string DateHourMinute = "dd-MM-yyyy HH:mm";
    private const string DateHourMinuteSecond = "dd-MM-yyyy HH:mm:ss";
    private const string DayHourMinute = "dddd HH:mm";
    private const string Day = "m";
    private const string YearDay = "d MMMM yyyy";
    
    public static string ToStringFormat(this DateTimeOffset dateTimeOffset, DateFormatMode formatMode = Default)
    {
        var localTimestamp = dateTimeOffset.ToLocalTime();
        var date = localTimestamp.Date;
        
        return formatMode switch
        {
            Short => localTimestamp.ToString(HourMinute),
            
            Default when date == DateTime.Today => $"Today {localTimestamp.ToString(HourMinute)}",
            Default when date == DateTime.Today.AddDays(-1) => $"Yesterday {localTimestamp.ToString(HourMinute)}",
            Default => localTimestamp.ToString(DateHourMinute),
            
            Long when date == DateTime.Today => $"Today {localTimestamp.ToString(HourMinuteSecond)}",
            Long when date == DateTime.Today.AddDays(-1) => $"Yesterday {localTimestamp.ToString(HourMinuteSecond)}",
            
            ChannelStatus when date == DateTime.Today => localTimestamp.ToString(HourMinute),
            ChannelStatus when DateTime.Today - date < TimeSpan.FromDays(7) => localTimestamp.ToString(DayHourMinute, CultureInfo.InvariantCulture),
            ChannelStatus when date.Year == DateTime.Today.Year => localTimestamp.ToString(Day, CultureInfo.InvariantCulture),
            ChannelStatus => localTimestamp.ToString(YearDay, CultureInfo.InvariantCulture),
            
            _ => localTimestamp.ToString(DateHourMinuteSecond)
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