﻿using FluentAssertions;
using NUnit.Framework;
using TagsCloudContainer;

namespace TagsCloudContainerTests;

public class CustomOptionsValidatorTests
{
    private CustomOptions options;
    private CustomOptionsValidator sut;
    private ImageFormatProvider formatProvider;


    [SetUp]
    public void Setup()
    {
        options = new CustomOptions
        {
            WorkingDirectory = $"{Path.Combine(Directory.GetCurrentDirectory(), "WorkingDir")}",
            WordsFileName = "SmallText.txt",
            BoringWordsName = "SmallText.txt",
            Font = "Arial",
            PictureSize = 600,
            MinTagSize = 15,
            MaxTagSize = 30,
            BackgroundColor = "White",
            FontColor = "Blue",
            ImageFormat = "png"
        };
        formatProvider = new ImageFormatProvider();
        sut = new CustomOptionsValidator(formatProvider);
    }

    [Test]
    public void ValidateConfig_AddPreSetOptions_ShouldReturnValidResult()
    {
        var result = sut.ValidateOptions(options);

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public void ValidateConfig_AddLowerCaseColor_ShouldReturnValidResult()
    {
        options.FontColor = "white";

        var result = sut.ValidateOptions(options);

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public void ValidateConfig_AddEmptyTextsPath_ShouldReturnInvalidResult()
    {
        options.WorkingDirectory = "";

        var result = sut.ValidateOptions(options);

        result.Error.Should().Be($"{options.WorkingDirectory} folder should exist.");
        result.IsSuccess.Should().BeFalse();
    }

    [Test]
    public void ValidateConfig_AddEmptyWordsFileName_ShouldReturnInvalidResult()
    {
        options.WordsFileName = "";

        var result = sut.ValidateOptions(options);

        result.Error.Should().Be($"{options.WorkingDirectory} should contain your file with words to draw.");
        result.IsSuccess.Should().BeFalse();
    }

    [Test]
    public void ValidateConfig_AddEmptyBoringWordsFileName_ShouldReturnInvalidResult()
    {
        options.BoringWordsName = "";

        var result = sut.ValidateOptions(options);

        result.Error.Should()
            .Be($"{options.WorkingDirectory} should contain your text (*.txt) file with excluded words.");
        result.IsSuccess.Should().BeFalse();
    }

    [TestCase("")]
    [TestCase("NonExistingFont")]
    public void ValidateConfig_AddIncorectFontName_ShouldReturnInvalidResult(string font)
    {
        options.Font = font;

        var result = sut.ValidateOptions(options);

        result.Error.Should()
            .Be($"Font \"{options.Font}\" can't be found");
        result.IsSuccess.Should().BeFalse();
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void ValidateConfig_AddPictureSizeLessThanOne_ShouldReturnInvalidResult(int size)
    {
        options.PictureSize = size;

        var result = sut.ValidateOptions(options);

        result.Error.Should().Be("Picture size should be above 0");
        result.IsSuccess.Should().BeFalse();
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void ValidateConfig_AddMaxFontMoreThamPictureSize_ShouldReturnInvalidResult(int size)
    {
        options.MinTagSize = size;

        var result = sut.ValidateOptions(options);

        result.Error.Should().Be("Font size should be above 0");
        result.IsSuccess.Should().BeFalse();
    }

    [TestCase("")]
    [TestCase("NonExistingColor")]
    public void ValidateConfig_AddIncorrectFontColorName_ShouldReturnInvalidResult(string font)
    {
        options.FontColor = font;

        var result = sut.ValidateOptions(options);

        result.Error.Should().Be("Unknown font color.\r\nSupported colors listed here" +
                                 " https://learn.microsoft.com/en-us/dotnet/api/system.drawing.color?view=net-7.0#properties");
        result.IsSuccess.Should().BeFalse();
    }

    [TestCase("")]
    [TestCase("NonExistingColor")]
    public void ValidateConfig_AddIncorrectBackgroundColorName_ShouldReturnInvalidResult(string font)
    {
        options.BackgroundColor = font;

        var result = sut.ValidateOptions(options);

        result.Error.Should().Be($"Unknown background color.\r\nSupported colors listed here" +
                                 $" https://learn.microsoft.com/en-us/dotnet/api/system.drawing.color?view=net-7.0#properties");
        result.IsSuccess.Should().BeFalse();
    }

    [TestCase(600)]
    [TestCase(601)]
    public void ValidateConfig_AddFontSizeMoreOrEqualThanPictureSize_ShouldReturnInvalidResult(
        int size)
    {
        options.MaxTagSize = size;

        var result = sut.ValidateOptions(options);

        result.Error.Should().Be("Font size should be less than picture size");
        result.IsSuccess.Should().BeFalse();
    }

    [TestCase("png")]
    [TestCase("PNG")]
    public void ValidateConfig_AddSupportedFormat_ShouldReturnValidResult(string format)
    {
        options.ImageFormat = format;

        var result = sut.ValidateOptions(options);

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public void ValidateConfig_AddUsupportedFormat_ShouldReturnInvalidResult()
    {
        options.ImageFormat = "ping";

        var result = sut.ValidateOptions(options);

        result.Error.Should().Be($"Unsupported image format. Supported formats are:" +
                                 $" {formatProvider.GetSupportedFormats()}");
        result.IsSuccess.Should().BeFalse();
    }

    [Test]
    public void ValidateConfig_AddDirectoryWithoutMystem_ShouldReturnInvalidResult()
    {
        options.WorkingDirectory = "c:\\Windows\\System32";

        var result = sut.ValidateOptions(options);

        result.Error.Should()
            .Be(
                $"Mystem.exe should be in {options.WorkingDirectory} folder. You can download it from https://yandex.ru/dev/mystem/.");
        result.IsSuccess.Should().BeFalse();
    }
}