﻿namespace ConsoleApp22
{
    public interface INode
    {
        string? GetAttribute(string name, string? fallback = null);
    }
}