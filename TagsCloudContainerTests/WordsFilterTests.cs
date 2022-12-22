﻿using FluentAssertions;
using NUnit.Framework;
using TagsCloudContainer;
using TagsCloudContainer.Interfaces;
using Result;

namespace TagsCloudContainerTests;

public class WordsFilterTests
{
    private WordsFilter sut;
    private Result<ICustomOptions> options;

    [SetUp]
    public void Setup()
    {
        sut = new WordsFilter();
        options = new Result<ICustomOptions>(new CustomOptions
        {
            ExcludedParticals = "SPRO, PR, PART, CONJ"
        });
    }

    [Test]
    public void FilterWords_AddNounTaggedWord_ShouldKeepWordInListAndRemoveTaggedInfo()
    {
        var taggedWords = new List<string> { "печь{печь=S,жен,неод=(вин,ед|им,ед)|печь=V,несов,пе=инф}" };
        var expectedResult = new List<string> { "печь" };

        var result = sut.FilterWords(taggedWords, options);

        result.Value.Should().BeEquivalentTo(expectedResult);
    }

    [Test]
    public void FilterWords_AddPRTaggedWord_ShouldRemoveItFromResult()
    {
        var taggedWords = new List<string> { "около{около=PR=|около=ADV=}" };
        var expectedResult = new List<string>();

        var result = sut.FilterWords(taggedWords, options);

        result.Value.Should().BeEquivalentTo(expectedResult);
    }

    [Test]
    public void FilterWords_AddTaggedPRInAnyPostitionWord_ShouldRemoveItFromResult()
    {
        var taggedWords = new List<string> { "около{около=ADV=|около=PR=}" };
        var expectedResult = new List<string>();

        var result = sut.FilterWords(taggedWords, options);

        result.Value.Should().BeEquivalentTo(expectedResult);
    }

    [Test]
    public void FilterWords_WithoutRestrictions_ShouldKeepAnyWord()
    {
        var taggedWords = new List<string>
        {
            "около{около=ADV=|около=PR=}",
            "печь{печь=S,жен,неод=(вин,ед|им,ед)|печь=V,несов,пе=инф}"
        };
        var expectedResult = new List<string>
        {
            "около",
            "печь"
        };

        var result = sut.FilterWords(taggedWords, new Result<ICustomOptions>(new CustomOptions()));

        result.Value.Should().BeEquivalentTo(expectedResult);
    }

    [Test]
    public void FilterWords_AddBoringWords_ShouldExcludeIt()
    {
        var taggedWords = new List<string> { "печь{печь=S,жен,неод=(вин,ед|им,ед)|печь=V,несов,пе=инф}" };
        var boringWords = new HashSet<string> { "печь" };

        var result = sut.FilterWords(taggedWords, options, boringWords);

        result.Value.Count.Should().Be(0);
    }
}