﻿using System.Xml;
using Mjml.Net.Components;
using Mjml.Net.Components.Body;
using Mjml.Net.Components.Head;
using Mjml.Net.Helpers;

namespace Mjml.Net
{
    public sealed class MjmlRenderer : IMjmlRenderer
    {
        private readonly Dictionary<string, IComponent> components = new Dictionary<string, IComponent>();
        private readonly List<IHelper> helpers = new List<IHelper>();

        public IEnumerable<IHelper> Helpers => helpers;

        public MjmlRenderer()
        {
            Add(new AttributesComponent());
            Add(new BodyComponent());
            Add(new BreakpointComponent());
            Add(new FontComponent());
            Add(new HeadComponent());
            Add(new PreviewComponent());
            Add(new RawComponent());
            Add(new RootComponent());
            Add(new SpacerComponent());
            Add(new StyleComponent());
            Add(new TitleComponent());

            Add(new BreakpointHelper());
            Add(new FontHelper());
            Add(new PreviewHelper());
            Add(new StyleHelper());
            Add(new TitleHelper());
        }

        public void Add(IComponent component)
        {
            components[component.ComponentName] = component;
        }

        public void Add(IHelper helper)
        {
            helpers.Add(helper);
        }

        internal IComponent? GetComponent(string previousElement)
        {
            return components.GetValueOrDefault(previousElement);
        }

        public string Render(string mjml, MjmlOptions options = default)
        {
            var xml = XmlReader.Create(new StringReader(mjml));

            return Render(xml, options);
        }

        public string Render(Stream mjml, MjmlOptions options = default)
        {
            var xml = XmlReader.Create(mjml);

            return Render(xml, options);
        }

        public string Render(TextReader mjml, MjmlOptions options = default)
        {
            var xml = XmlReader.Create(mjml);

            return Render(xml, options);
        }

        private string Render(XmlReader xml, MjmlOptions options)
        {
            var context = ObjectPools.Contexts.Get();
            try
            {
                context.Setup(this, xml, options);
                context.BufferStart();
                context.Read();

                return context.BufferFlush()!;
            }
            finally
            {
                ObjectPools.Contexts.Return(context);
            }
        }
    }
}