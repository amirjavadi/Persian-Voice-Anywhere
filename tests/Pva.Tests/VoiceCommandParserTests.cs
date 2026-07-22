using Pva.Commands;
using Pva.Core;

namespace Pva.Tests;

/// <summary>تست تفسیر دستورهای صوتی (منطق خالص).</summary>
public class VoiceCommandParserTests
{
    private static readonly VoiceCommandParser Parser = new();
    private static readonly CommandOptions On = new();

    private static ParsedTranscript Parse(string text) => Parser.Parse(text, On);

    [Fact]
    public void NewLineCommand_SplitsTextWithAction()
    {
        var parts = Parse("سلام خط بعد خداحافظ").Parts;

        Assert.Equal(3, parts.Count);
        Assert.Equal("سلام", parts[0].Text);
        Assert.Equal(EditorAction.NewLine, parts[1].Action);
        Assert.Equal("خداحافظ", parts[2].Text);
    }

    [Fact]
    public void PunctuationCommand_IsInlinedIntoText()
    {
        var parts = Parse("این ویرگول یک تست").Parts;

        var part = Assert.Single(parts);
        Assert.Equal("این، یک تست", part.Text);
    }

    [Fact]
    public void Parentheses_ProduceCleanSpacing()
    {
        var part = Assert.Single(Parse("پرانتز باز متن پرانتز بسته").Parts);
        Assert.Equal("(متن)", part.Text);
    }

    [Fact]
    public void QuestionMark_AttachesToWord()
    {
        var part = Assert.Single(Parse("سلام علامت سوال").Parts);
        Assert.Equal("سلام؟", part.Text);
    }

    [Fact]
    public void ThreeWordCommand_DeleteWord_IsRecognized()
    {
        var part = Assert.Single(Parse("حذف کلمه قبل").Parts);
        Assert.Equal(EditorAction.DeleteWord, part.Action);
    }

    [Fact]
    public void LatinSynonym_Undo_Works()
    {
        var part = Assert.Single(Parse("undo").Parts);
        Assert.Equal(EditorAction.Undo, part.Action);
    }

    [Fact]
    public void CommandModeDisabled_KeepsEverythingLiteral()
    {
        var parts = Parser.Parse("سلام خط بعد", new CommandOptions { CommandModeEnabled = false }).Parts;

        var part = Assert.Single(parts);
        Assert.Equal("سلام خط بعد", part.Text);
        Assert.Null(part.Action);
    }

    [Fact]
    public void EmptyInput_ProducesNoParts()
        => Assert.Empty(Parse("   ").Parts);

    [Fact]
    public void ArabicYehVariant_StillMatches()
    {
        // «سوال» با ي عربی هم باید به علامت سؤال نگاشت شود (نرمال‌سازی داخلی).
        var part = Assert.Single(Parse("علامت سوال").Parts);
        Assert.Equal("؟", part.Text);
    }
}
