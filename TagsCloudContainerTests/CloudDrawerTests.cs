using CloudLayout;
using FluentAssertions;
using NUnit.Framework;
using TagsCloudContainer;
using TagsCloudContainer.Interfaces;
using Result;

namespace TagsCloudContainerTests;

public class CloudDrawer_Should
{
    private string firstPath;
    private string secondPath;
    private Result<ICustomOptions> defaultOptions;
    private CloudDrawer sut;


    [SetUp]
    public void SetUpDrawer()
    {
        var converter = new FileToDictionaryConverter(new WordsFilter(), new BudgetDocParser());
        var calculator = new WordSizeCalculator();
        var spiralDrawer = new SpiralDrawer();
        sut = new CloudDrawer(spiralDrawer, converter, calculator);

        defaultOptions = GetDeafultResultOptions();
        firstPath = Path.Combine(defaultOptions.Value.WorkingDir,
            (defaultOptions.Value.ImageName + "1." + defaultOptions.Value.ImageFormat));
        secondPath = Path.Combine(defaultOptions.Value.WorkingDir,
            (defaultOptions.Value.ImageName + "2." + defaultOptions.Value.ImageFormat));
    }

    [TearDown]
    public void CleanUp()
    {
        File.Delete(firstPath);
        File.Delete(secondPath);

        defaultOptions = GetDeafultResultOptions();
        firstPath = Path.Combine(defaultOptions.Value.WorkingDir,
            (defaultOptions.Value.ImageName + "1." + defaultOptions.Value.ImageFormat));
        secondPath = Path.Combine(defaultOptions.Value.WorkingDir,
            (defaultOptions.Value.ImageName + "2." + defaultOptions.Value.ImageFormat));
    }

    private Result<ICustomOptions> GetDeafultResultOptions()
    {
        return new Result<ICustomOptions>(new CustomOptions
        {
            WorkingDir = Path.Combine(Directory.GetCurrentDirectory(), "..\\..\\..\\WorkingDir"),
            WordsFileName = "FiveSingleWords.txt",
            BoringWordsName = "EmptyList.txt",
            Font = "Arial",
            PictureSize = 600,
            MinTagSize = 15,
            MaxTagSize = 35,
            BackgroundColor = "White",
            FontColor = "Blue",
            ExcludedParticals = "SPRO, PR, PART, CONJ",
            ImageFormat = "Png",
            ImageName = "Cloud1"
        });
    }
    [Test]
    public void DrawAPicture()
    {
        sut.DrawCloud(firstPath, defaultOptions);

        File.Exists(firstPath).Should().BeTrue();
    }

    [TestCase("png")]
    [TestCase("jpeg")]
    public void DrawAPNGPicture(string format)
    {
        defaultOptions.Value.ImageFormat = format;
        sut.DrawCloud(firstPath, defaultOptions);
        firstPath = Path.Combine(defaultOptions.Value.WorkingDir,
            (defaultOptions.Value.ImageName + "1." + defaultOptions.Value.ImageFormat));

        var result = new FileInfo(firstPath);

        result.Extension.Should().Be("." + format);
    }

    [Test]
    public void DrawSamePictureFromDocAndTxtWithSameWordsInThem()
    {
        var docOptions = new Result<ICustomOptions>((CustomOptions)((CustomOptions)defaultOptions.Value).Clone());
        docOptions.Value.WordsFileName = "SmallText.doc";
        defaultOptions.Value.WordsFileName = "SmallText.txt";

        sut.DrawCloud(firstPath, defaultOptions);
        sut.DrawCloud(secondPath, docOptions);

        var file1 = new FileInfo(firstPath);
        var file2 = new FileInfo(secondPath);
        file1.Length.Should().Be(file2.Length);
    }

    [Test]
    [Description("Sizes are relative to word count")]
    public void DrawSamePictureFromFiveSingleWordsAndFiveWordsPair()
    {
        var fivePairsOption = new Result<ICustomOptions>((CustomOptions)((CustomOptions)defaultOptions.Value).Clone());
        fivePairsOption.Value.WordsFileName = "FiveWordsPair.txt";
        defaultOptions.Value.WordsFileName = "FiveSingleWords.txt";

        sut.DrawCloud(firstPath, defaultOptions);
        sut.DrawCloud(secondPath, fivePairsOption);

        var file1 = new FileInfo(firstPath);
        var file2 = new FileInfo(secondPath);
        file1.Length.Should().Be(file2.Length);
    }

    [Test]
    public void DrawBiggerPictureWithoutBoringWordsList()
    {
        var boringWordsOption =
            new Result<ICustomOptions>((CustomOptions)((CustomOptions)defaultOptions.Value).Clone());
        boringWordsOption.Value.BoringWordsName = "SomeBoringWords.txt";

        sut.DrawCloud(firstPath, defaultOptions);
        sut.DrawCloud(secondPath, boringWordsOption);

        var file1 = new FileInfo(firstPath);
        var file2 = new FileInfo(secondPath);
        file1.Length.Should().BeGreaterThan(file2.Length);
    }
}