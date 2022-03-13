﻿using System.Xml;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace Mjml.Net
{
    public sealed partial class MjmlRenderContext : INode
    {
        private static readonly char[] TrimChars = { ' ', '\n', '\r' };
        private readonly GlobalData globalData = new GlobalData();
        private readonly Dictionary<string, Dictionary<string, string>> attributesByName = new Dictionary<string, Dictionary<string, string>>(10);
        private readonly Dictionary<string, Dictionary<string, string>> attributesByClass = new Dictionary<string, Dictionary<string, string>>(10);
        private readonly Dictionary<string, string> currentAttributes = new Dictionary<string, string>(10);
        private readonly Dictionary<string, object?> context = new Dictionary<string, object?>(10);
        private IComponent? currentComponent;
        private MjmlOptions options;
        private MjmlRenderer renderer;
        private XmlReader reader;
        private string? currentText;
        private string? currentElement;
        private string[]? currentClasses;

        public MjmlRenderContext()
        {
        }

        public MjmlRenderContext(MjmlRenderer renderer, XmlReader reader, MjmlOptions options = default)
        {
            Setup(renderer, reader, options);
        }

        public void Setup(MjmlRenderer renderer, XmlReader reader, MjmlOptions options = default)
        {
            this.renderer = renderer;
            this.reader = reader;
            this.options = options;
        }

        internal void Clear()
        {
            context.Clear();
            currentAttributes.Clear();
            currentClasses = null;
            currentComponent = null;
            currentElement = null;
            currentText = null;
            attributesByName.Clear();
            globalData.Clear();
            reader = null!;
            renderer = null!;

            ClearRenderData();
        }

        public void Read()
        {
            do
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        ReadElement(reader.Name);
                        break;
                }
            }
            while (reader.Read());
        }

        private void ReadElement(string name)
        {
            currentElement = name;
            currentClasses = null;
            currentText = null;
            currentAttributes.Clear();

            for (var i = 0; i < reader.AttributeCount; i++)
            {
                reader.MoveToAttribute(i);

                currentAttributes[reader.Name] = reader.Value;
            }

            while (reader.Read())
            {
                var type = reader.NodeType;

                if (type == XmlNodeType.Text)
                {
                    currentText = reader.Value.Trim(TrimChars);
                    break;
                }
                else if (type == XmlNodeType.Element || type == XmlNodeType.EndElement)
                {
                    break;
                }
            }

            currentComponent = renderer.GetComponent(currentElement);

            if (currentComponent == null)
            {
                throw new InvalidOperationException($"Invalid element '{currentElement}'.");
            }

            var childRenderer = childOptions.Current.Renderer;

            if (childRenderer != null)
            {
                childRenderer(this);
            }
            else
            {
                currentComponent.Render(this, this);
            }
        }

        public string? GetAttribute(string name, string? fallback = null)
        {
            if (currentAttributes.TryGetValue(name, out var attribute))
            {
                return attribute;
            }

            if (attributesByName.TryGetValue(name, out var byType))
            {
                if (byType.TryGetValue(currentElement!, out attribute))
                {
                    return attribute;
                }

                if (byType.TryGetValue(Constants.All, out attribute))
                {
                    return attribute;
                }
            }

            if (attributesByClass.Count > 0)
            {
                if (currentClasses == null)
                {
                    currentClasses = currentAttributes.GetValueOrDefault("mj-class")?.Split() ?? Array.Empty<string>();
                }

                foreach (var className in currentClasses)
                {
                    if (attributesByClass.TryGetValue(className, out var byName))
                    {
                        if (byName.TryGetValue(name, out attribute))
                        {
                            return attribute;
                        }
                    }
                }
            }

            var defaultAttributes = currentComponent?.DefaultAttributes;

            if (defaultAttributes != null && defaultAttributes.TryGetValue(name, out attribute))
            {
                return attribute;
            }

            return fallback;
        }

        public void SetContext(string name, object? value)
        {
            context[name] = value;
        }

        public object? GetContext(string name)
        {
            return context.GetValueOrDefault(name);
        }

        public string? GetContent()
        {
            return currentText;
        }

        public void SetGlobalData(string name, object value, bool skipIfAdded = true)
        {
            var type = value.GetType();

            if (skipIfAdded && globalData.ContainsKey((type, name)))
            {
                return;
            }

            globalData[(type, name)] = value;
        }

        public void SetTypeAttribute(string name, string type, string value)
        {
            if (!attributesByName.TryGetValue(name, out var attributes))
            {
                attributes = new Dictionary<string, string>();

                attributesByName[name] = attributes;
            }

            attributes[type] = value;
        }

        public void SetClassAttribute(string name, string className, string value)
        {
            if (!attributesByClass.TryGetValue(className, out var attributes))
            {
                attributes = new Dictionary<string, string>();

                attributesByClass[className] = attributes;
            }

            attributes[name] = value;
        }
    }
}