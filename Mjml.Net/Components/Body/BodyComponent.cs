﻿namespace Mjml.Net.Components.Body
{
    public sealed class BodyComponent : IComponent
    {
        public string ComponentName => "mj-body";

        public AllowedParents? AllowedParents { get; } =
            new AllowedParents
            {
                "mjml"
            };

        public void Render(IHtmlRenderer renderer, INode node)
        {
            renderer.BufferStart();
            renderer.RenderChildren();
            renderer.SetContext("body", renderer.BufferFlush());
        }
    }
}