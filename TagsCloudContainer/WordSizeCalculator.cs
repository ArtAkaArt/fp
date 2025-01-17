﻿using System.Drawing;
using TagsCloudContainer.Interfaces;
using ResultOfTask;

namespace TagsCloudContainer;

public class WordSizeCalculator : IWordSizeCalculator
{
    public Result<Dictionary<string, Font>> CalculateSize(Dictionary<string, int> input,
        ICustomOptions options)
    {
        var sizesDictionary = new Dictionary<string, Font>(input.Count);
        var max = input.First().Value;
        var min = input.Last().Value;

        var fontMax = options.MaxTagSize;
        var fontMin = options.MinTagSize;

        foreach (var pair in input)
        {
            var size = pair.Value == min
                ? fontMin
                : (pair.Value / (double)max) * (fontMax - fontMin) + fontMin;
            sizesDictionary.Add(pair.Key, new Font(options.Font, (int)size));
        }

        return sizesDictionary.AsResult();
    }
}