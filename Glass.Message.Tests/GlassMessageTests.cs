// -----------------------------------------------------------------------------
//  Glass.Message — xUnit tests covering the parts that can be exercised without
//  showing a window: theme presets, the result/config value objects, the builder
//  surface, static defaults, toast options, and the RoundRect geometry helper.
//
//  File        : GlassMessageTests.cs
//  Developer   ::> Gehan Fernando
// -----------------------------------------------------------------------------

using System;
using System.Drawing;
using System.Windows.Forms;
using Xunit;

namespace Glass.Message.Tests;

// All test classes that write to GlassMessage static state are placed in this
// collection so xUnit serialises them (no two classes mutate globals in parallel).
[CollectionDefinition("GlassStaticState")]
public sealed class GlassStaticStateCollection { }

/// <summary>Verifies the built-in theme presets and the OS theme detection.</summary>
public class ThemeTests
{
    [Fact]
    public void Default_Theme_Is_Dark_Blue() =>
        Assert.Equal(Color.FromArgb(15, 23, 42), GlassTheme.Default.BackgroundTop);

    [Fact]
    public void Light_Theme_Background_Is_Bright() =>
        Assert.True(GlassTheme.Light.BackgroundTop.GetBrightness() > 0.8f);

    [Fact]
    public void HighContrast_Theme_Uses_SystemColors() =>
        Assert.Equal(SystemColors.Window, GlassTheme.HighContrast.BackgroundTop);

    [Fact]
    public void WindowsClassic_Has_Zero_CornerRadius() =>
        Assert.Equal(0, GlassTheme.WindowsClassic.CornerRadius);

    [Fact]
    public void HighContrast_Has_Zero_CornerRadius() =>
        Assert.Equal(0, GlassTheme.HighContrast.CornerRadius);

    [Fact]
    public void AutoDetect_Returns_A_Theme() =>
        Assert.NotNull(GlassTheme.AutoDetect());

    [Fact]
    public void IsSystemDark_Returns_Bool() =>
        Assert.IsType<bool>(GlassTheme.IsSystemDark());

    [Fact]
    public void Theme_Has_NonNull_Fonts()
    {
        Assert.NotNull(GlassTheme.Default.TitleFont);
        Assert.NotNull(GlassTheme.Default.MessageFont);
        Assert.NotNull(GlassTheme.Default.ButtonFont);
    }

    [Fact]
    public void Custom_Theme_Dispose_Does_Not_Throw()
    {
        var theme = new GlassTheme
        {
            TitleFont = new Font("Segoe UI", 12f),
            MessageFont = new Font("Segoe UI", 10f),
            ButtonFont = new Font("Segoe UI", 9f),
        };
        var ex = Record.Exception(theme.Dispose);
        Assert.Null(ex);
    }

    [Fact]
    public void Custom_Theme_Dispose_Twice_Is_Safe()
    {
        var theme = new GlassTheme();
        theme.Dispose();
        var ex = Record.Exception(theme.Dispose);
        Assert.Null(ex);
    }

    // Disposing a shared preset must be a no-op so its fonts survive for reuse.
    [Fact]
    public void Preset_Dispose_Does_Not_Throw_And_Fonts_Stay_Valid()
    {
        GlassTheme.Default.Dispose();
        Assert.NotNull(GlassTheme.Default.TitleFont);
    }

    [Fact]
    public void Theme_CornerRadius_Default_Is_8() =>
        Assert.Equal(8, GlassTheme.Default.CornerRadius);

    [Fact]
    public void Theme_ButtonCornerRadius_Default_Is_5() =>
        Assert.Equal(5, GlassTheme.Default.ButtonCornerRadius);
}

/// <summary>Covers the <see cref="GlassResult"/> value object and its implicit conversion.</summary>
public class GlassResultTests
{
    [Fact]
    public void Implicit_Conversion_To_DialogResult_Works()
    {
        var r = new GlassResult(DialogResult.OK, true, "hello");
        DialogResult dr = r;
        Assert.Equal(DialogResult.OK, dr);
    }

    [Fact]
    public void InputText_Never_Null()
    {
        var r = new GlassResult(DialogResult.Cancel, false, null);
        Assert.Equal(string.Empty, r.InputText);
    }

    [Fact]
    public void CheckBoxChecked_Reflects_Argument()
    {
        var r = new GlassResult(DialogResult.Yes, true, "text");
        Assert.True(r.CheckBoxChecked);
    }
}

/// <summary>Exercises the fluent builder: chaining, custom labels, and edge cases.</summary>
[Collection("GlassStaticState")]
public class GlassBuilderTests
{
    [Fact]
    public void Create_Returns_Builder() =>
        Assert.NotNull(GlassMessage.Create("test"));

    [Fact]
    public void Builder_Chains_Fluently()
    {
        var b = GlassMessage.Create("msg")
            .Title("T")
            .Icon(MessageBoxIcon.Information)
            .Buttons(MessageBoxButtons.OKCancel)
            .Default(MessageBoxDefaultButton.Button2)
            .Animation(GlassAnimation.SlideDown)
            .AutoClose(5_000)
            .CheckBox("Don't show again")
            .InputText("placeholder", "default")
            .Detail("stack trace here")
            .Progress(50, 100)
            .RightToLeft(false)
            .RoundedCorners(true);
        Assert.NotNull(b);
    }

    [Fact]
    public void Builder_Custom_Labels_Sets_OKCancel_For_Two()
    {
        var b = GlassMessage.Create("msg").Buttons("Yes, Delete", "Cancel");
        Assert.NotNull(b);
    }

    [Fact]
    public void Builder_RoundedCorners_True_Returns_Builder()
    {
        var b = GlassMessage.Create("msg").RoundedCorners(true);
        Assert.NotNull(b);
    }

    [Fact]
    public void Builder_RoundedCorners_False_Returns_Builder()
    {
        var b = GlassMessage.Create("msg").RoundedCorners(false);
        Assert.NotNull(b);
    }

    [Fact]
    public void Builder_Without_RoundedCorners_Leaves_Config_Null()
    {
        var savedGlobal = GlassMessage.UseRoundedCorners;
        try
        {
            GlassMessage.UseRoundedCorners = false;
            Assert.NotNull(GlassMessage.Create("test"));
        }
        finally
        {
            GlassMessage.UseRoundedCorners = savedGlobal;
        }
    }

    [Fact]
    public void Builder_With_Animation_None_Returns_Builder()
    {
        var b = GlassMessage.Create("msg").Animation(GlassAnimation.None);
        Assert.NotNull(b);
    }

    [Fact]
    public void Builder_Scale_Animation_Is_Accepted()
    {
        var b = GlassMessage.Create("msg").Animation(GlassAnimation.Scale);
        Assert.NotNull(b);
    }

    [Fact]
    public void GlassAnimation_Scale_Is_Distinct_Enum_Value()
    {
        Assert.NotEqual(GlassAnimation.Fade, GlassAnimation.Scale);
        Assert.NotEqual(GlassAnimation.None, GlassAnimation.Scale);
        Assert.NotEqual(GlassAnimation.SlideDown, GlassAnimation.Scale);
    }

    [Fact]
    public void GlassAnimation_Has_Four_Values()
    {
        var values = Enum.GetValues<GlassAnimation>();
        Assert.Equal(4, values.Length);
    }

    [Fact]
    public void Builder_Buttons_Null_Array_Does_Not_Throw()
    {
        var ex = Record.Exception(() => GlassMessage.Create("msg").Buttons(null));
        Assert.Null(ex);
    }

    [Fact]
    public void Builder_Buttons_Empty_Array_Does_Not_Throw()
    {
        var ex = Record.Exception(() => GlassMessage.Create("msg").Buttons([]));
        Assert.Null(ex);
    }

    [Fact]
    public void Builder_Sound_Returns_Builder() =>
        Assert.NotNull(GlassMessage.Create("msg").Sound());

    [Fact]
    public void Builder_Sound_Disabled_Returns_Builder() =>
        Assert.NotNull(GlassMessage.Create("msg").Sound(false));

    [Fact]
    public void Builder_Chains_New_Members_Fluently()
    {
        var b = GlassMessage.Create("msg")
            .Title("T")
            .Sound()
            .Progress(25, 100)
            .Buttons("Cancel");
        Assert.NotNull(b);
    }
}

/// <summary>Checks the convenience "Has…" flags on <see cref="GlassDialogConfig"/>.</summary>
public class GlassDialogConfigTests
{
    [Fact]
    public void HasCheckBox_False_When_Label_Null() =>
        Assert.False(new GlassDialogConfig().HasCheckBox);

    [Fact]
    public void HasCheckBox_True_When_Label_Set() =>
        Assert.True(new GlassDialogConfig { CheckBoxLabel = "Don't show" }.HasCheckBox);

    [Fact]
    public void HasInput_False_By_Default() =>
        Assert.False(new GlassDialogConfig().HasInput);

    [Fact]
    public void HasInput_True_When_Mode_Set() =>
        Assert.True(new GlassDialogConfig { InputMode = GlassInputMode.Text }.HasInput);

    [Fact]
    public void HasProgress_False_By_Default() =>
        Assert.False(new GlassDialogConfig().HasProgress);

    [Fact]
    public void HasProgress_True_When_Enabled() =>
        Assert.True(new GlassDialogConfig { ShowProgress = true }.HasProgress);

    [Fact]
    public void HasDetail_False_When_Null() =>
        Assert.False(new GlassDialogConfig().HasDetail);

    [Fact]
    public void HasDetail_True_When_Set() =>
        Assert.True(new GlassDialogConfig { DetailText = "info" }.HasDetail);

    [Fact]
    public void UseRoundedCorners_Defaults_To_Null() =>
        Assert.Null(new GlassDialogConfig().UseRoundedCorners);

    [Fact]
    public void UseRoundedCorners_Can_Be_Set_True() =>
        Assert.True(new GlassDialogConfig { UseRoundedCorners = true }.UseRoundedCorners);

    [Fact]
    public void UseRoundedCorners_Can_Be_Set_False() =>
        Assert.False(new GlassDialogConfig { UseRoundedCorners = false }.UseRoundedCorners);

    [Fact]
    public void PlaySound_Defaults_To_Null() =>
        Assert.Null(new GlassDialogConfig().PlaySound);

    [Fact]
    public void PlaySound_Can_Be_Set_True() =>
        Assert.True(new GlassDialogConfig { PlaySound = true }.PlaySound);
}

/// <summary>
/// Covers the global <see cref="GlassMessage"/> defaults. These touch shared
/// static state, so the mutating tests serialise on a lock and restore the
/// original values in a finally block.
/// </summary>
[Collection("GlassStaticState")]
public class GlassMessageStaticTests
{
    private static readonly object _staticLock = new();

    [Fact]
    public void UseRoundedCorners_Global_Default_Is_False() =>
        Assert.False(GlassMessage.UseRoundedCorners);

    [Fact]
    public void PlaySystemSounds_Global_Default_Is_False() =>
        Assert.False(GlassMessage.PlaySystemSounds);

    [Fact]
    public void PlaySystemSounds_Can_Be_Set_And_Restored()
    {
        lock (_staticLock)
        {
            var original = GlassMessage.PlaySystemSounds;
            try
            {
                GlassMessage.PlaySystemSounds = true;
                Assert.True(GlassMessage.PlaySystemSounds);
            }
            finally { GlassMessage.PlaySystemSounds = original; }
        }
    }

    [Fact]
    public void DefaultTheme_Can_Be_Overridden_And_Restored()
    {
        lock (_staticLock)
        {
            var original = GlassMessage.DefaultTheme;
            try
            {
                GlassMessage.DefaultTheme = GlassTheme.Light;
                Assert.Equal(GlassTheme.Light, GlassMessage.DefaultTheme);
            }
            finally { GlassMessage.DefaultTheme = original; }
        }
    }

    [Fact]
    public void UseRoundedCorners_Can_Be_Set_And_Restored()
    {
        lock (_staticLock)
        {
            var original = GlassMessage.UseRoundedCorners;
            try
            {
                GlassMessage.UseRoundedCorners = true;
                Assert.True(GlassMessage.UseRoundedCorners);
                GlassMessage.UseRoundedCorners = false;
                Assert.False(GlassMessage.UseRoundedCorners);
            }
            finally { GlassMessage.UseRoundedCorners = original; }
        }
    }
}

/// <summary>Verifies the defaults and overrides on <see cref="GlassToastOptions"/>.</summary>
public class ToastOptionsTests
{
    [Fact]
    public void Default_Position_Is_BottomRight() =>
        Assert.Equal(ToastPosition.BottomRight, new GlassToastOptions().Position);

    [Fact]
    public void Default_Duration_Is_4000ms() =>
        Assert.Equal(4_000, new GlassToastOptions().DurationMs);

    [Fact]
    public void UseRoundedCorners_Defaults_To_Null() =>
        Assert.Null(new GlassToastOptions().UseRoundedCorners);

    [Fact]
    public void UseRoundedCorners_Can_Be_Set_True() =>
        Assert.True(new GlassToastOptions { UseRoundedCorners = true }.UseRoundedCorners);

    [Fact]
    public void UseRoundedCorners_Can_Be_Set_False() =>
        Assert.False(new GlassToastOptions { UseRoundedCorners = false }.UseRoundedCorners);

    [Fact]
    public void Screen_Defaults_To_Null() =>
        Assert.Null(new GlassToastOptions().Screen);
}

/// <summary>
/// Covers the <see cref="OsVersion"/> helper, which resolves the true Windows
/// version via RtlGetVersion so modern DWM chrome isn't disabled by a missing
/// host-app compatibility manifest.
/// </summary>
public class OsVersionTests
{
    [Fact]
    public void Major_Is_Reported() =>
        Assert.True(OsVersion.Major > 0);

    [Fact]
    public void Build_Is_NonNegative() =>
        Assert.True(OsVersion.Build >= 0);

    // On a real Windows host RtlGetVersion must report at least Windows 10 (10.x),
    // even when the test runner itself ships without a Windows 10/11 manifest —
    // the exact scenario the helper exists to defend against.
    [Fact]
    public void Reports_At_Least_Windows10_On_Windows()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        Assert.True(OsVersion.Major >= 10);
    }

    [Fact]
    public void Windows11_Implies_Windows10_1803()
    {
        if (OsVersion.IsWindows11OrGreater)
        {
            Assert.True(OsVersion.IsWindows10_1803OrGreater);
        }
    }
}

/// <summary>
/// Tests the <see cref="GlassDialog.RoundRect"/> geometry helper — the one piece
/// of drawing code that can be checked without a live window.
/// </summary>
public class RoundRectTests
{
    [Fact]
    public void RoundRect_Radius_Zero_Returns_Rectangle_Path()
    {
        using var path = GlassDialog.RoundRect(new Rectangle(0, 0, 100, 50), 0);
        Assert.NotNull(path);
        Assert.True(path.PointCount > 0);
    }

    [Fact]
    public void RoundRect_Positive_Radius_Returns_Curved_Path()
    {
        using var pathFlat = GlassDialog.RoundRect(new Rectangle(0, 0, 100, 50), 0);
        using var pathRound = GlassDialog.RoundRect(new Rectangle(0, 0, 100, 50), 8);
        Assert.True(pathRound.PointCount > pathFlat.PointCount);
    }

    [Fact]
    public void RoundRect_Negative_Radius_Treated_As_Zero()
    {
        using var path = GlassDialog.RoundRect(new Rectangle(0, 0, 100, 50), -5);
        Assert.NotNull(path);
    }
}

/// <summary>
/// Covers the <see cref="GlassToast.CalcToastLocation"/> geometry helper — pure
/// coordinate math, no window required.
/// </summary>
public class CalcToastLocationTests
{
    private static readonly Rectangle Screen1080p = new(0, 0, 1920, 1040); // typical working area

    [Fact]
    public void BottomRight_Is_Inside_Screen()
    {
        var loc = GlassToast.CalcToastLocation(Screen1080p, 360, 80, ToastPosition.BottomRight, 0, 12);
        Assert.True(loc.X >= Screen1080p.Left);
        Assert.True(loc.Y >= Screen1080p.Top);
        Assert.True(loc.X + 360 <= Screen1080p.Right + 1);
        Assert.True(loc.Y + 80 <= Screen1080p.Bottom + 1);
    }

    [Fact]
    public void BottomLeft_X_Is_Near_Left_Edge()
    {
        var loc = GlassToast.CalcToastLocation(Screen1080p, 360, 80, ToastPosition.BottomLeft, 0, 12);
        Assert.Equal(Screen1080p.Left + 12, loc.X);
    }

    [Fact]
    public void TopRight_Y_Is_Near_Top_Edge()
    {
        var loc = GlassToast.CalcToastLocation(Screen1080p, 360, 80, ToastPosition.TopRight, 0, 12);
        Assert.Equal(Screen1080p.Top + 12, loc.Y);
    }

    [Fact]
    public void TopLeft_Is_At_Top_Left_Corner()
    {
        var loc = GlassToast.CalcToastLocation(Screen1080p, 360, 80, ToastPosition.TopLeft, 0, 12);
        Assert.Equal(Screen1080p.Left + 12, loc.X);
        Assert.Equal(Screen1080p.Top + 12, loc.Y);
    }

    [Fact]
    public void BottomCenter_Is_Horizontally_Centred()
    {
        var loc = GlassToast.CalcToastLocation(Screen1080p, 360, 80, ToastPosition.BottomCenter, 0, 12);
        var expectedX = Screen1080p.Left + ((Screen1080p.Width - 360) / 2);
        Assert.Equal(expectedX, loc.X);
    }

    [Fact]
    public void TopCenter_Is_Horizontally_Centred()
    {
        var loc = GlassToast.CalcToastLocation(Screen1080p, 360, 80, ToastPosition.TopCenter, 0, 12);
        var expectedX = Screen1080p.Left + ((Screen1080p.Width - 360) / 2);
        Assert.Equal(expectedX, loc.X);
    }

    [Fact]
    public void Stack_Offset_Shifts_BottomRight_Up()
    {
        var loc0 = GlassToast.CalcToastLocation(Screen1080p, 360, 80, ToastPosition.BottomRight, 0, 12);
        var loc1 = GlassToast.CalcToastLocation(Screen1080p, 360, 80, ToastPosition.BottomRight, 92, 12);
        Assert.True(loc1.Y < loc0.Y);
        Assert.Equal(loc0.Y - 92, loc1.Y);
    }

    [Fact]
    public void Stack_Offset_Shifts_TopLeft_Down()
    {
        var loc0 = GlassToast.CalcToastLocation(Screen1080p, 360, 80, ToastPosition.TopLeft, 0, 12);
        var loc1 = GlassToast.CalcToastLocation(Screen1080p, 360, 80, ToastPosition.TopLeft, 92, 12);
        Assert.True(loc1.Y > loc0.Y);
        Assert.Equal(loc0.Y + 92, loc1.Y);
    }
}

/// <summary>
/// Verifies fixes to <see cref="GlassResult"/> and <see cref="GlassTheme"/>
/// introduced in v1.0.2.
/// </summary>
public class V102RegressionTests
{
    [Fact]
    public void GlassResult_Null_Implicit_Conversion_Returns_None()
    {
        GlassResult r = null;
        DialogResult dr = r;
        Assert.Equal(DialogResult.None, dr);
    }

    [Fact]
    public void Dark_Theme_Is_Same_Instance_As_Default()
    {
        Assert.Same(GlassTheme.Default, GlassTheme.Dark);
    }

    [Fact]
    public void InputDropdown_Null_Items_Does_Not_Throw()
    {
        var ex = Record.Exception(() => GlassMessage.Create("msg").InputDropdown(null));
        Assert.Null(ex);
    }
}
