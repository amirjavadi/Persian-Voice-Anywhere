using Pva.Hotkeys;

namespace Pva.Tests;

/// <summary>تست parse ژست کلید میانبر (منطق خالص).</summary>
public class HotkeyGestureTests
{
    [Fact]
    public void CtrlSpace_IsComboWithCtrl()
    {
        var g = HotkeyGesture.Parse("Ctrl+Space");
        Assert.Equal(HotkeyGestureKind.Combo, g.Kind);
        Assert.True(g.Ctrl);
        Assert.False(g.Shift);
        Assert.Equal(HotkeyGesture.VkSpace, g.Key);
    }

    [Fact]
    public void CtrlShiftSpace_HasBothModifiers()
    {
        var g = HotkeyGesture.Parse("Ctrl+Shift+Space");
        Assert.True(g.Ctrl);
        Assert.True(g.Shift);
        Assert.Equal(HotkeyGesture.VkSpace, g.Key);
    }

    [Fact]
    public void DoubleCtrl_IsDoubleTap()
    {
        var g = HotkeyGesture.Parse("DoubleCtrl");
        Assert.Equal(HotkeyGestureKind.DoubleTap, g.Kind);
        Assert.Equal(HotkeyGesture.VkControl, g.Key);
    }

    [Fact]
    public void CapsLock_IsSingleKey()
    {
        var g = HotkeyGesture.Parse("CapsLock");
        Assert.Equal(HotkeyGestureKind.SingleKey, g.Kind);
        Assert.Equal(HotkeyGesture.VkCapsLock, g.Key);
    }

    [Fact]
    public void SingleLetter_MapsToVirtualKey()
    {
        var g = HotkeyGesture.Parse("A");
        Assert.Equal((ushort)'A', g.Key);
        Assert.Equal(HotkeyGestureKind.SingleKey, g.Kind);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptyGesture_Throws(string gesture)
        => Assert.Throws<ArgumentException>(() => HotkeyGesture.Parse(gesture));

    [Fact]
    public void UnknownKey_Throws()
        => Assert.Throws<FormatException>(() => HotkeyGesture.Parse("Ctrl+"));
}
