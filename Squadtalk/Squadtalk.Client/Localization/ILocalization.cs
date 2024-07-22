using Microsoft.AspNetCore.Components;

namespace Squadtalk.Client.Localization;

public interface ILocalization
{
    TextTable GetTextTable(ComponentBase component);

    TextTable TextTable { get; }
}
