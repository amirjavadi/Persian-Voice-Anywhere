using Pva.Core;
using Pva.Injection;

namespace Pva.Tests;

/// <summary>تست نگاشت کنش ویرایشی به کلید مجازی (منطق خالص، بدون SendInput).</summary>
public class EditorActionMapperTests
{
    [Fact]
    public void NewLine_MapsToSingleEnter()
    {
        var chords = EditorActionMapper.Map(EditorAction.NewLine);
        Assert.Single(chords);
        Assert.Equal(EditorActionMapper.VkReturn, chords[0].VirtualKey);
        Assert.False(chords[0].Ctrl);
    }

    [Fact]
    public void NewParagraph_MapsToTwoEnters()
    {
        var chords = EditorActionMapper.Map(EditorAction.NewParagraph);
        Assert.Equal(2, chords.Count);
        Assert.All(chords, c => Assert.Equal(EditorActionMapper.VkReturn, c.VirtualKey));
    }

    [Fact]
    public void DeleteWord_IsCtrlBackspace()
    {
        var chord = Assert.Single(EditorActionMapper.Map(EditorAction.DeleteWord));
        Assert.Equal(EditorActionMapper.VkBack, chord.VirtualKey);
        Assert.True(chord.Ctrl);
    }

    [Fact]
    public void Undo_IsCtrlZ()
    {
        var chord = Assert.Single(EditorActionMapper.Map(EditorAction.Undo));
        Assert.Equal(EditorActionMapper.VkZ, chord.VirtualKey);
        Assert.True(chord.Ctrl);
    }

    [Fact]
    public void Redo_IsCtrlY()
    {
        var chord = Assert.Single(EditorActionMapper.Map(EditorAction.Redo));
        Assert.Equal(EditorActionMapper.VkY, chord.VirtualKey);
        Assert.True(chord.Ctrl);
    }

    [Theory]
    [InlineData(EditorAction.Backspace)]
    [InlineData(EditorAction.Space)]
    [InlineData(EditorAction.NewLine)]
    public void EverySimpleAction_ProducesAtLeastOneChord(EditorAction action)
        => Assert.NotEmpty(EditorActionMapper.Map(action));
}
