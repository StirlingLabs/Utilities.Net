using System;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using YamlDotNet.RepresentationModel;

namespace StirlingLabs.Utilities.Yaml
{
    [PublicAPI]
    public class YamlToJsonVisitor : IYamlVisitor
    {
        private const string JsonSpecNumberPattern = @"^(?=[1-9]|0(?![0-9]))[0-9]+(\.[0-9]+)?([eE][+-]?[0-9]+)?$";
        private static readonly Regex ValidNumberRx = new(JsonSpecNumberPattern,
            RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant);
        private readonly StringBuilder _buf;

        public YamlToJsonVisitor()
            => _buf = new();

        public YamlToJsonVisitor(int bufferSize) : this()
            => _buf.EnsureCapacity(bufferSize);

        public YamlToJsonVisitor(StringBuilder buffer)
            => _buf = buffer;

        public override string ToString()
            => _buf.ToString();

        public void Visit(YamlStream stream)
        {
            switch (stream.Documents.Count)
            {
                case 0: return;
                case 1:
                    stream.Documents[0].Accept(this);
                    return;
            }
            _buf.Append('[');
            foreach (var document in stream.Documents)
            {
                document.Accept(this);
                _buf.Append(',');
            }
            _buf[^1] = ']';
        }

        public void Visit(YamlDocument document)
            => document.RootNode.Accept(this);

        public void Visit(YamlScalarNode scalar)
        {
            if (scalar.Value == null || scalar.Value == "~" || scalar.Value == "null")
                _buf.Append("null");
            else if (ValidNumberRx.IsMatch(scalar.Value!)) // if valid number
                _buf.Append(scalar.Value);
            else
                _buf.Append('"').Append(scalar.Value).Append('"');
        }

        public void Visit(YamlSequenceNode sequence)
        {
            _buf.Append('[');
            if (sequence.Children.Count == 0)
            {
                _buf.Append(']');
                return;
            }
            foreach (var node in sequence)
            {
                node.Accept(this);
                _buf.Append(',');
            }
            _buf[^1] = ']';
        }

        public void Visit(YamlMappingNode mapping)
        {
            _buf.Append('{');
            if (mapping.Children.Count == 0)
            {
                _buf.Append('}');
                return;
            }
            foreach (var (key, value) in mapping.Children)
            {
                key.Accept(this);
                _buf.Append(':');
                value.Accept(this);
                _buf.Append(',');
            }
            _buf[^1] = '}';
        }
    }
}
