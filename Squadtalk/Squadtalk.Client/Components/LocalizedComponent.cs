using Microsoft.AspNetCore.Components;
using Squadtalk.Client.Localization;

namespace Squadtalk.Client.Components;

public abstract class LocalizedComponent : ComponentBase
{
    [Inject]
    public ILocalization Localization { get; set; } = default!;

    protected TextTable Table = default!;

    protected override void OnInitialized()
    {
        Table = Localization.GetTextTable(this);
    }
}
