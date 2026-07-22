using Pva.Core;
using Pva.PersianText;

namespace Pva.Tests;

/// <summary>
/// تست‌های golden پس‌پردازش فارسی — قلب تمایز محصول. هر جفت (ورودی → خروجی) رفتار یک
/// قانون را تثبیت می‌کند.
/// </summary>
public class PersianTextProcessorTests
{
    private static readonly PersianTextProcessor Processor = new();
    private static readonly PersianTextOptions AllOn = new();

    private static string Run(string input) => Processor.Process(input, AllOn);

    [Fact]
    public void ReferenceSentence_MixedPersianEnglish_IsPreservedCleanly()
    {
        const string input = "امروز یک Pull Request روی GitHub زدم.";
        Assert.Equal("امروز یک Pull Request روی GitHub زدم.", Run(input));
    }

    [Theory]
    [InlineData("می روم", "می‌روم")]
    [InlineData("من می روم", "من می‌روم")]
    [InlineData("نمی خواهم", "نمی‌خواهم")]
    public void MiPrefix_GetsZwnj(string input, string expected)
        => Assert.Equal(expected, Run(input));

    [Theory]
    [InlineData("کتاب ها", "کتاب‌ها")]
    [InlineData("کتاب های من", "کتاب‌های من")]
    [InlineData("بزرگ تر", "بزرگ‌تر")]
    [InlineData("بزرگ ترین", "بزرگ‌ترین")]
    public void PluralAndComparativeSuffixes_GetZwnj(string input, string expected)
        => Assert.Equal(expected, Run(input));

    [Fact]
    public void Suffix_InsideSingleWord_IsNotSplit()
    {
        // «تنها» نباید به «تن‌ها» تبدیل شود (پسوند فقط وقتی توکن جدا با فاصله باشد اعمال می‌شود).
        Assert.Equal("تنها", Run("تنها"));
    }

    [Theory]
    [InlineData("سلام ، دنیا", "سلام، دنیا")]
    [InlineData("سلام, دنیا", "سلام، دنیا")]
    [InlineData("سلام،دنیا", "سلام، دنیا")]
    public void Comma_SpacingAndConversion(string input, string expected)
        => Assert.Equal(expected, Run(input));

    [Theory]
    [InlineData("خوبی ؟", "خوبی؟")]
    [InlineData("خوبی?", "خوبی؟")]
    public void QuestionMark_SpacingAndConversion(string input, string expected)
        => Assert.Equal(expected, Run(input));

    [Theory]
    [InlineData("من 2 کتاب دارم", "من ۲ کتاب دارم")]
    [InlineData("عدد 1400", "عدد ۱۴۰۰")]
    public void Digits_ConvertToPersian(string input, string expected)
        => Assert.Equal(expected, Run(input));

    [Fact]
    public void Digits_AttachedToLatin_ArePreserved()
    {
        // شناسه/نسخه‌ی فنی نباید فارسی شود.
        Assert.Equal("نسخه iOS16 است", Run("نسخه iOS16 است"));
    }

    [Theory]
    [InlineData("علي", "علی")] // ي عربی → ی فارسی
    [InlineData("كتاب", "کتاب")] // ك عربی → ک فارسی
    [InlineData("مدرسة", "مدرسه")] // ة → ه
    public void ArabicLetters_AreNormalized(string input, string expected)
        => Assert.Equal(expected, Run(input));

    [Fact]
    public void ExtraWhitespace_IsCollapsedAndTrimmed()
        => Assert.Equal("سلام دنیا", Run("   سلام    دنیا   "));

    [Fact]
    public void EmptyInput_ReturnsEmpty()
        => Assert.Equal(string.Empty, Run(string.Empty));

    [Fact]
    public void ProtectedTerms_AreReplaced()
    {
        var processor = new PersianTextProcessor(new Dictionary<string, string>
        {
            ["گیت هاب"] = "GitHub",
        });

        Assert.Equal("GitHub خوب است", processor.Process("گیت هاب خوب است", AllOn));
    }

    [Fact]
    public void DisablingOptions_SkipsThoseRules()
    {
        var options = new PersianTextOptions
        {
            ApplyZeroWidthNonJoiner = false,
            UsePersianDigits = false,
        };

        // بدون ZWNJ و بدون رقم فارسی، اما فاصله‌ها هنوز جمع می‌شوند.
        Assert.Equal("می روم 2", Processor.Process("می  روم  2", options));
    }
}
