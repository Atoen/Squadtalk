namespace Shared.Data.TypedIds;

/// <summary>
/// Enum representing different string ID formats.
/// </summary>
public enum StringIdValueFormat
{
    /// <summary>
    /// 32 digits without hyphens.
    /// Example: 00000000000000000000000000000000
    /// </summary>
    GuidN,

    /// <summary>
    /// 32 digits separated by hyphens.
    /// Example: 00000000-0000-0000-0000-000000000000
    /// </summary>
    GuidD,

    /// <summary>
    /// 32 digits separated by hyphens, enclosed in braces.
    /// Example: {00000000-0000-0000-0000-000000000000}
    /// </summary>
    GuidB,

    /// <summary>
    /// 32 digits separated by hyphens, enclosed in parentheses.
    /// Example: (00000000-0000-0000-0000-000000000000)
    /// </summary>
    GuidP,

    /// <summary>
    /// Four hexadecimal values enclosed in braces, where the fourth value is a subset of eight hexadecimal values that is also enclosed in braces.
    /// Example: {0x00000000,0x0000,0x0000,{0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00}}
    /// </summary>
    GuidX,

    /// <summary>
    /// Base64 encoded string.
    /// Example: QUJDREVGR0hJSktMTU5PUFFSU1RVVldYWVo=
    /// </summary>
    Base64,

    /// <summary>
    /// URL-friendly string.
    /// Example: YWJjZGVmZ2hpamtsbW5vcA
    /// </summary>
    UrlFriendly,
}
