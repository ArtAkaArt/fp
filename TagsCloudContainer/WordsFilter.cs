﻿using System.Text.RegularExpressions;
using TagsCloudContainer.Interfaces;
using ResultOfTask;

namespace TagsCloudContainer;

public class WordsFilter : IWordsFilter
{
    public Result<List<string>> FilterWords(List<string> taggedWords, ICustomOptions options,
        HashSet<string>? boringWords = null)
    {
        //PoS - Part of Speech; grammemes - grammatical number etc, including PoS
        var excludedPoS =
            options.ExcludedParticals.Split(", ", StringSplitOptions.RemoveEmptyEntries);

        var jointPos = string.Join('|', excludedPoS
            .Select(x => $"={x}(,|=)")
            .ToArray());
        jointPos = jointPos.Length == 0 ? "= (,|=)" : jointPos;
        var regexString =
            $"^(\\w+){{((?!{jointPos}).)*$"; //.Insert(11, jointPos);
        // something like that ^(\w+){((?!=SPRO(,|=)|=PR(,|=)|=PART(,|=)|=CONJ(,|=)).)*$
        var regex = new Regex(regexString);

        var inputWords = taggedWords
            .Where(x => regex.IsMatch(x))
            .Select(x =>
            {
                var match = regex.Match(x);
                return match.Groups[1].Value;
            }).ToList();
        inputWords = boringWords is null ? inputWords : inputWords.Where(x => !boringWords.Contains(x)).ToList();
        return inputWords.AsResult();
    }
}