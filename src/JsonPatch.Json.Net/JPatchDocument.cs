using JsonPatch.Json.Net.Model;
using JsonPatch.Json.Net.Model.Exceptions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace JsonPatch.Json.Net
{
    public sealed class JPatchDocument
    {
        public IReadOnlyList<Operation> Operations { get; private set; }

        internal JPatchDocument(Operation[] operations)
        {
            Operations = operations;
        }

        public static JPatchDocument Load(string json)
        {
            var token = JToken.Parse(json);
            return Load(token);
        }

        public static JPatchDocument Load(JToken fromToken)
        {
            var operationsList = new List<Operation>();

            var lala = JPatchSchema.Instance.ToString();
            if (!fromToken.IsValid(JPatchSchema.Instance, out IList<string> errorMessages))
            {
                throw new JPatchException(new JPatchFailReason("Document does not contain a valid JsonPatch schema.", ""));
            }

            var operations = fromToken.ToArray();

            foreach (var operation in operations)
            {
                operationsList.Add(LoadOperation(operation));
            }

            return new JPatchDocument(operationsList.ToArray());
        }


        private static Operation LoadOperation(JToken fromToken)
        {
            var operationTypeString = fromToken.Value<string>("op");
            var path = fromToken.Value<string>("path");
            if (path is null)
            {
                throw new JPatchException(new JPatchFailReason("Path value in operation cannot be null.", ""));
            }

            var pathSegments = ParsePath(path);
            var fromSegments = ParseFrom(fromToken.Value<string>("from"));

            switch (operationTypeString)
            {
                case "add":
                    return new Operation(OperationType.Add, pathSegments, fromSegments, fromToken["value"]);
                case "remove":
                    return new Operation(OperationType.Remove, pathSegments, fromSegments, null);
                case "replace":
                    return new Operation(OperationType.Replace, pathSegments, fromSegments, fromToken["value"]);
                case "move":
                    return new Operation(OperationType.Move, pathSegments, fromSegments, null);
                case "copy":
                    return new Operation(OperationType.Copy, pathSegments, fromSegments, null);
                case "test":
                    return new Operation(OperationType.Test, pathSegments, fromSegments, fromToken["value"]);
                default:
                    throw new InvalidOperationException("Since the schema was validated, no unknown operation expected.");
            }
        }



        private static string[] ParsePath(string path)
        {
            if (string.Compare(path, string.Empty, StringComparison.Ordinal) == 0)
            {
                return new string[0];
            }

            path = ValidateAndNormalizePath(path);
            var parts = path.Split('/');

            for (var index = 0; index < parts.Length; index++)
            {
                parts[index] = ParseSegment(parts[index]);
            }

            return parts;
        }

        private static string[] ParseFrom(string? from)
        {
            if (from is null)
            {
                return new string[0];
            }
            return ParsePath(from);
        }


        private static string ParseSegment(string segment)
        {
            segment = segment.Replace("~0", "~");
            segment = segment.Replace("~1", "/");
            return segment;
        }

        private static string ValidateAndNormalizePath(string path)
        {
            if (path.Contains("//"))
            {
                throw new JPatchException(new JPatchFailReason("A path cannot contain an empty segment.", ""));
            }

            if (path.StartsWith("/"))
            {
                path = path.Substring(1);
            }

            return path;

        }
    }
}
