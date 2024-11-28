namespace Squadtalk.Client.Extensions;

using static DateFormatMode;

public static class TimeStampExtensions
{
    private const string HourMinute = "HH:mm";
    private const string HourMinuteSecond = "HH:mm:ss";
    private const string DateHourMinute = "dd-MM-yyyy HH:mm";
    private const string DateHourMinuteSecond = "dd-MM-yyyy HH:mm:ss";
    private const string DayHourMinute = "ddd HH:mm";
    private const string Day = "m";
    private const string YearDay = "d MMMM yyyy";

    public static string ToStringFormat(
        this DateTimeOffset dateTimeOffset,
        // TextTable textTable,
        DateFormatMode formatMode = Default)
    {
        var localTimestamp = dateTimeOffset.ToLocalTime();
        var date = localTimestamp.Date;
        // var culture = textTable.CultureInfo;

        return "Time";

        // return formatMode switch
        // {
        //     Short => localTimestamp.ToString(HourMinute, culture),
        //     Default when date == DateTime.Today =>
        //         textTable.TodayTimeTemplate.TryFormat(localTimestamp.ToString(HourMinute), culture),
        //
        //     Default when date == DateTime.Today.AddDays(-1) =>
        //         textTable.YesterdayTimeTemplate.TryFormat(localTimestamp.ToString(HourMinute, culture)),
        //
        //     Default => localTimestamp.ToString(DateHourMinute),
        //
        //     Long when date == DateTime.Today =>
        //         textTable.TodayTimeTemplate.TryFormat(localTimestamp.ToString(HourMinuteSecond, culture)),
        //
        //     Long when date == DateTime.Today.AddDays(-1) =>
        //         textTable.YesterdayTimeTemplate.TryFormat(localTimestamp.ToString(HourMinuteSecond, culture)),
        //
        //     ChannelStatus when date == DateTime.Today =>
        //         localTimestamp.ToString(HourMinute, culture),
        //
        //     ChannelStatus when DateTime.Today - date < TimeSpan.FromDays(7) =>
        //         localTimestamp.ToString(DayHourMinute, culture),
        //
        //     ChannelStatus when date.Year == DateTime.Today.Year =>
        //         localTimestamp.ToString(Day, culture),
        //
        //     ChannelStatus => localTimestamp.ToString(YearDay, culture),
        //
        //     _ => localTimestamp.ToString(DateHourMinuteSecond, culture)
        // };
    }
}

public enum DateFormatMode
{
    Default,
    Short,
    Long,
    ChannelStatus
}
