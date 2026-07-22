using Pva.Core;

namespace Pva.Tests;

/// <summary>تست نگاشت وضعیت دیکته به متن فارسی.</summary>
public class DictationStateTextTests
{
    [Theory]
    [InlineData(DictationState.Idle, "آماده")]
    [InlineData(DictationState.Listening, "در حال شنیدن…")]
    [InlineData(DictationState.Processing, "در حال پردازش…")]
    [InlineData(DictationState.Injecting, "در حال تایپ…")]
    public void ToPersian_MapsEveryState(DictationState state, string expected)
        => Assert.Equal(expected, DictationStateText.ToPersian(state));
}
