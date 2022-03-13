﻿namespace ConsoleApp22
{
    public interface IHtmlRenderer
    {
        IElementHtmlRenderer StartElement(string elementName);

        void EndElement(string elementName);

        void Plain(string? value);

        void Content(string? value);

        void RenderChildren();

        void SetContext(string name, object? value);

        object? GetContext(string name);
    }

    public interface IElementHtmlRenderer
    {
        IElementHtmlRenderer Attr(string name, string? value);

        IElementHtmlRenderer Class(string? value);

        IElementHtmlRenderer Style(string name, string? value);

        IElementHtmlRenderer Done();
    }
}