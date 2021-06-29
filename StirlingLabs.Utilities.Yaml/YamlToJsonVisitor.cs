using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using YamlDotNet.RepresentationModel;

namespace StirlingLabs.Utilities.Yaml
{
    [PublicAPI]
    public class YamlToJsonVisitor : IYamlVisitor
    {
        private const string ValidNumberPattern = @"^\s*[\+-]?((\d,*)+\.\d*|(\d,*)*\.\d+|(\d,*)+)([eE][\+-]?\d+)?\s*$";
        private readonly Regex _validNumberRx = new(ValidNumberPattern, RegexOptions.Compiled);
        private readonly StringBuilder _buf = new();

        public override string ToString() => _buf.ToString();

        public void Visit(YamlStream stream)
        {
            _buf.Clear();
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
        {
            _buf.Append('{');
            document.RootNode?.Accept(this);
            _buf.Append('}');
        }

        public void Visit(YamlScalarNode scalar)
        {
            if (scalar.Value == null || scalar.Value == "~" || scalar.Value == "null")
                _buf.Append("null");
            else if (_validNumberRx.IsMatch(scalar.Value!)) // if valid number
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
