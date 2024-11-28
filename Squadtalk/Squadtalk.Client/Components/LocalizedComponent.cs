using Microsoft.AspNetCore.Components;
using Squadtalk.Client.Localization;

namespace Squadtalk.Client.Components;

public abstract class LocalizedComponent : ComponentBase
{
    [Inject]
    public LocalizedText LocalizedText { get; set; } = default!;

    protected LocalizedText.TextTable Table = default!;

    protected override void OnInitialized()
    {
        Table = LocalizedText.GetTextTable(this);
    }
}
