﻿using ResultOfTask;

namespace TagsCloudContainer.Interfaces;

public interface IDocParser
{
    public Result<List<string>> ParseDoc(string filePath);
}