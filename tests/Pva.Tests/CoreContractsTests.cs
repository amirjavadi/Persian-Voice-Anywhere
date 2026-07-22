using Pva.Core;

namespace Pva.Tests;

/// <summary>
/// تست‌های دود (smoke) برای قراردادهای Pva.Core. هدف در M0 فقط اطمینان از build و
/// اجرای سبز تست‌هاست؛ منطق واقعی هر ماژول در milestone خودش تست می‌شود.
/// </summary>
public class CoreContractsTests
{
    [Fact]
    public void PersianTextOptions_Defaults_AreTunedForPersian()
    {
        var options = new PersianTextOptions();

        Assert.True(options.NormalizeArabicLetters);
        Assert.True(options.ApplyZeroWidthNonJoiner);
        Assert.True(options.UsePersianDigits);
        Assert.True(options.FixPunctuationSpacing);
        Assert.True(options.PreserveMixedScript);
    }

    [Fact]
    public void TranscriptPart_TextFactory_CarriesTextOnly()
    {
        var part = TranscriptPart.OfText("سلام");

        Assert.Equal("سلام", part.Text);
        Assert.Null(part.Action);
    }

    [Fact]
    public void TranscriptPart_ActionFactory_CarriesActionOnly()
    {
        var part = TranscriptPart.OfAction(EditorAction.NewParagraph);

        Assert.Equal(EditorAction.NewParagraph, part.Action);
        Assert.Null(part.Text);
    }

    [Fact]
    public void SttOptions_DefaultLanguage_IsPersian()
    {
        var options = new SttOptions();

        Assert.Equal("fa", options.Language);
    }
}
