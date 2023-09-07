using Newtonsoft.Json.Linq;

namespace JsonPatch.Json.Net.Model
{
    public sealed class Operation
    {
        public OperationType Type { get; private set; }
        public IReadOnlyList<string> PathSegments { get; private set; }
        public IReadOnlyList<string> FromSegments { get; private set; }
        public JToken? Value { get; private set; }

        internal Operation(OperationType type, string[] pathSegments, string[] fromSegments, JToken? value)
        {
            Type = type;
            PathSegments = pathSegments;
            FromSegments = fromSegments;
            Value = value;
        }
    }
}
