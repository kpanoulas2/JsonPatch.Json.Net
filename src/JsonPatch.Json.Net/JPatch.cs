using JsonPatch.Json.Net.Model;
using JsonPatch.Json.Net.Model.Exceptions;
using Newtonsoft.Json.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace JsonPatch.Json.Net
{
    public class JPatch
    {
        public static string Patch(string sourceObjectJson, string patchJson)
        {
            var sourceObject = JToken.Parse(sourceObjectJson);
            var patch = JPatchDocument.Load(patchJson);
            var successObject = Patch(sourceObject, patch);
            return successObject.ToString();
        }
        public static JToken Patch(JToken sourceObject, JPatchDocument patch)
        {
            if (sourceObject is null)
            {
                throw new ArgumentNullException(nameof(sourceObject));
            }

            if (patch is null)
            {
                throw new ArgumentNullException(nameof(sourceObject));
            }

            if (!TryPatch(sourceObject, patch, out var result, out var failReason))
            {
                throw new JPatchException(failReason);
            }

            return result;
        }

        public static bool TryPatch(JToken sourceObject, JPatchDocument patch, [NotNullWhen(true)] out JToken? result, [NotNullWhen(false)] out JPatchFailReason? patchFailReason)
        {
            if (sourceObject is null)
            {
                result = null;
                patchFailReason = new JPatchFailReason($"Argument {nameof(sourceObject)} is null", string.Empty);
                return false;
            }

            if (patch is null)
            {
                result = null;
                patchFailReason = new JPatchFailReason($"Argument {nameof(patch)} is null", string.Empty);
                return false;
            }

            foreach (var testOperation in patch.Operations.Where(t => t.Type == OperationType.Test))
            {
                if (!TryTest(testOperation, sourceObject, out patchFailReason))
                {
                    result = null;
                    return false;
                }
            }

            result = sourceObject.DeepClone();
            foreach (var operation in patch.Operations.Where(t => t.Type != OperationType.Test))
            {
                if (!TryExecuteOperation(operation, ref result, out patchFailReason))
                {
                    result = null;
                    return false;
                }
            }

            patchFailReason = null;
            return true;
        }

        private static bool TryTest(Operation operation, JToken sourceObject, [NotNullWhen(false)] out JPatchFailReason? patchFailReason)
        {
            if (operation.Type != OperationType.Test)
            {
                throw new InvalidOperationException("Only test operation is expected when executing this function.");
            }

            if (!TryGetTokenAtPath(sourceObject, operation.PathSegments, out var locatedValue, out patchFailReason))
            {
                return false;
            }

            var jTokenEqualityComparer = new JTokenEqualityComparer();

            if (!jTokenEqualityComparer.Equals(operation.Value, locatedValue))
            {
                patchFailReason = new JPatchFailReason("Value found at path, but does not match provided value.", string.Join('/', operation.PathSegments));
                return false;
            }
            return true;
        }

        private static bool TryExecuteOperation(Operation operation, ref JToken sourceObject, [NotNullWhen(false)] out JPatchFailReason? patchFailReason)
        {
            switch (operation.Type)
            {
                case OperationType.Test:
                    throw new InvalidOperationException("Test operations are executed outside of the main execution loop.");
                case OperationType.Add:
                    return TryExecuteAddOperation(operation, ref sourceObject, out patchFailReason);
                case OperationType.Remove:
                    return TryExecuteRemoveOperation(operation, ref sourceObject, out patchFailReason);
                case OperationType.Replace:
                    return TryExecuteReplaceOperation(operation, ref sourceObject, out patchFailReason);
                case OperationType.Move:
                    return TryExecuteMoveOperation(operation, ref sourceObject, out patchFailReason);
                case OperationType.Copy:
                    return TryExecuteCopyOperation(operation, ref sourceObject, out patchFailReason);
            }

            patchFailReason = new JPatchFailReason("Unknown operation type.", string.Empty);
            return false;
        }

        private static bool TryExecuteAddOperation(Operation operation, ref JToken sourceObject, [NotNullWhen(false)] out JPatchFailReason? patchFailReason)
        {
            if (operation is null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            if (operation.Type != OperationType.Add)
            {
                throw new InvalidOperationException("Expected an add operation");
            }

            if (operation.Value is null)
            {
                throw new InvalidOperationException("Operation add expects a value.");
            }

            if (!operation.PathSegments.Any())
            {
                patchFailReason = null;
                sourceObject = operation.Value;
                return true;
            }

            var parentPathSegments = operation.PathSegments.Take(operation.PathSegments.Count - 1).ToArray();

            if (!TryGetTokenAtPath(sourceObject, parentPathSegments, out var parentToken, out patchFailReason))
            {
                return false;
            }

            var lastSegment = operation.PathSegments.Last();

            return TryToAddTokenToToken(parentToken, lastSegment, operation.Value, out patchFailReason);

        }
        private static bool TryExecuteRemoveOperation(Operation operation, ref JToken sourceObject, [NotNullWhen(false)] out JPatchFailReason? patchFailReason)
        {
            if (operation is null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            if (operation.Type != OperationType.Remove)
            {
                throw new InvalidOperationException("Expected a remove operation");
            }

            var parentPathSegments = operation.PathSegments.Take(operation.PathSegments.Count - 1).ToArray();

            if (!TryGetTokenAtPath(sourceObject, parentPathSegments, out var parentToken, out patchFailReason))
            {
                return false;
            }

            var lastSegment = operation.PathSegments.Last();

            return TryRemoveTokenFromToken(parentToken, lastSegment, out patchFailReason);
        }
        private static bool TryExecuteReplaceOperation(Operation operation, ref JToken sourceObject, [NotNullWhen(false)] out JPatchFailReason? patchFailReason)
        {
            if (operation is null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            if (operation.Type != OperationType.Replace)
            {
                throw new InvalidOperationException("Expected a replace operation");
            }

            if (operation.Value is null)
            {
                throw new InvalidOperationException("Operation add expects a value.");
            }

            if (!operation.PathSegments.Any())
            {
                sourceObject = operation.Value;
                patchFailReason = null;
                return true;
            }

            var parentPathSegments = operation.PathSegments.Take(operation.PathSegments.Count - 1).ToArray();

            if (!TryGetTokenAtPath(sourceObject, parentPathSegments, out var parentToken, out patchFailReason))
            {
                return false;
            }

            var lastSegment = operation.PathSegments.Last();

            return TrySetTokenToToken(parentToken, lastSegment, operation.Value, out patchFailReason);
        }
        private static bool TryExecuteMoveOperation(Operation operation, ref JToken sourceObject, [NotNullWhen(false)] out JPatchFailReason? patchFailReason)
        {
            if (operation is null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            if (operation.Type != OperationType.Move)
            {
                throw new InvalidOperationException("Expected a move operation");
            }

            var parentFromSegments = operation.FromSegments.Take(operation.FromSegments.Count - 1).ToArray();
            var lastFromSegment = operation.FromSegments.Last();
            if (!TryGetTokenAtPath(sourceObject, parentFromSegments, out var fromParentToken, out patchFailReason))
            {
                return false;
            }

            if (!TryToGetTokenFromToken(fromParentToken, lastFromSegment, out var fromToken, out patchFailReason))
            {
                return false;
            }

            var parentPathSegments = operation.PathSegments.Take(operation.PathSegments.Count - 1).ToArray();
            if (!TryGetTokenAtPath(sourceObject, parentPathSegments, out var parentToken, out patchFailReason))
            {
                return false;
            }

            var lastSegment = operation.PathSegments.Last();

            if (!TryRemoveTokenFromToken(fromParentToken, lastFromSegment, out patchFailReason))
            {
                return false;
            }

            return TryToAddTokenToToken(parentToken, lastSegment, fromToken, out patchFailReason);
        }
        private static bool TryExecuteCopyOperation(Operation operation, ref JToken sourceObject, [NotNullWhen(false)] out JPatchFailReason? patchFailReason)
        {
            if (operation is null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            if (operation.Type != OperationType.Copy)
            {
                throw new InvalidOperationException("Expected a copy operation");
            }

            if (!TryGetTokenAtPath(sourceObject, operation.FromSegments, out var fromToken, out patchFailReason))
            {
                return false;
            }
            var parentPathSegments = operation.PathSegments.Take(operation.PathSegments.Count - 1).ToArray();

            if (!TryGetTokenAtPath(sourceObject, parentPathSegments, out var parentToken, out patchFailReason))
            {
                return false;
            }

            var lastSegment = operation.PathSegments.Last();
            return TryToAddTokenToToken(parentToken, lastSegment, fromToken, out patchFailReason);
        }

        private static bool TryGetTokenAtPath(JToken sourceObject, IReadOnlyList<string> pathSegments, [NotNullWhen(true)] out JToken? token, [NotNullWhen(false)] out JPatchFailReason? patchFailReason)
        {
            token = sourceObject;

            if (!pathSegments.Any())
            {
                patchFailReason = null;
                return true;
            }

            var effectivePath = new StringBuilder();

            for (var index = 0; index < pathSegments.Count; index++)
            {
                if (token is null)
                {
                    patchFailReason = new JPatchFailReason("Path not found", effectivePath.ToString());
                    return false;
                }

                var segment = pathSegments[index];
                if (token.Type == JTokenType.Object)
                {
                    var tokenAsObject = token as JObject;
                    if (tokenAsObject is null)
                    {
                        throw new InvalidOperationException("Expected non null since type is checked.");
                    }
                    effectivePath
                        .Append('/')
                        .Append(segment);
                    token = tokenAsObject[segment];
                    continue;
                }

                if (token.Type == JTokenType.Array)
                {
                    var tokenAsArray = token as JArray;
                    if (tokenAsArray is null)
                    {
                        throw new InvalidOperationException("Expected non null since type is checked.");
                    }
                    int arrayIndex;
                    if (string.Compare(segment, "-", StringComparison.Ordinal) == 0)
                    {
                        arrayIndex = tokenAsArray.Count - 1;
                    }
                    else if (!int.TryParse(segment, out arrayIndex))
                    {
                        patchFailReason = new JPatchFailReason($"Expected an array index at path segment but found '{segment}'.", effectivePath.ToString());
                        return false;
                    }
                    else if (string.Compare(arrayIndex.ToString(), segment, StringComparison.Ordinal) != 0)
                    {
                        patchFailReason = new JPatchFailReason($"Array indices with leading zeros are considered errors. Found '{segment}'.", string.Empty);
                        return false;
                    }

                    if (arrayIndex >= tokenAsArray.Count || arrayIndex < 0)
                    {
                        patchFailReason = new JPatchFailReason($"Index out of range. Provided {arrayIndex} when the minimum value is 0 and the maximum value is {tokenAsArray.Count - 1}.", string.Empty);
                        return false;
                    }


                    effectivePath.Append('/').Append(arrayIndex);
                    token = tokenAsArray[arrayIndex];
                    continue;
                }

                patchFailReason = new JPatchFailReason($"Unxpected token type '{token.Type}'.", effectivePath.ToString());
                return false;
            }
            if (token is null)
            {
                patchFailReason = new JPatchFailReason("Object not found at path.", effectivePath.ToString());
                return false;
            }

            patchFailReason = null;
            return true;

        }

        private static bool TryToAddTokenToToken(
            JToken sourceObject,
            string segment,
            JToken valueToAdd,
            [NotNullWhen(false)] out JPatchFailReason? patchFailReason)
        {
            patchFailReason = null;
            if (sourceObject.Type == JTokenType.Object)
            {
                sourceObject[segment] = valueToAdd;
                return true;
            }

            if (sourceObject.Type == JTokenType.Array)
            {
                var sourceObjectAsArray = sourceObject as JArray;
                if (sourceObjectAsArray is null)
                {
                    throw new InvalidOperationException("Expected non null since type is checked.");
                }

                if (string.Compare(segment, "-", StringComparison.Ordinal) == 0)
                {
                    sourceObjectAsArray.Add(valueToAdd);
                    return true;
                }

                if (!int.TryParse(segment, out var arrayIndex))
                {
                    patchFailReason = new JPatchFailReason($"Expected an array index at path segment but found '{segment}'.", string.Empty);
                    return false;
                }
                else if (string.Compare(arrayIndex.ToString(), segment, StringComparison.Ordinal) != 0)
                {
                    patchFailReason = new JPatchFailReason($"Array indices with leading zeros are considered errors. Found '{segment}'.", string.Empty);
                    return false;
                }

                if (arrayIndex > sourceObjectAsArray.Count || arrayIndex < 0)
                {
                    patchFailReason = new JPatchFailReason($"Index out of range. Provided {arrayIndex} when the minimum value is 0 and the maximum value is {sourceObjectAsArray.Count}.", string.Empty);
                    return false;
                }

                sourceObjectAsArray.Insert(arrayIndex, valueToAdd);
                patchFailReason = null;
                return true;
            }

            patchFailReason = new JPatchFailReason("Unexpected token type", string.Empty);
            return false;
        }


        private static bool TrySetTokenToToken(JToken sourceObject,
            string segment,
            JToken valueToAdd,
            [NotNullWhen(false)] out JPatchFailReason? patchFailReason)
        {
            patchFailReason = null;
            if (sourceObject.Type == JTokenType.Object)
            {
                sourceObject[segment] = valueToAdd;
                return true;
            }

            if (sourceObject.Type == JTokenType.Array)
            {
                var sourceObjectAsArray = sourceObject as JArray;
                if (sourceObjectAsArray is null)
                {
                    throw new InvalidOperationException("Expected non null since type is checked.");
                }
                int arrayIndex;
                if (string.Compare(segment, "-", StringComparison.Ordinal) == 0)
                {
                    arrayIndex = sourceObjectAsArray.Count - 1;
                }
                else if (!int.TryParse(segment, out arrayIndex))
                {
                    patchFailReason = new JPatchFailReason($"Expected an array index at path segment but found '{segment}'.", string.Empty);
                    return false;
                }
                else if (string.Compare(arrayIndex.ToString(), segment, StringComparison.Ordinal) != 0)
                {
                    patchFailReason = new JPatchFailReason($"Array indices with leading zeros are considered errors. Found '{segment}'.", string.Empty);
                    return false;
                }

                sourceObjectAsArray[arrayIndex] = valueToAdd;
                patchFailReason = null;
                return true;
            }

            patchFailReason = new JPatchFailReason("Unexpected token type", string.Empty);
            return false;
        }

        private static bool TryRemoveTokenFromToken(JToken sourceObject, string segment, [NotNullWhen(false)] out JPatchFailReason? patchFailReason)
        {
            patchFailReason = null;
            if (sourceObject.Type == JTokenType.Object)
            {
                var sourceObjectAsObject = sourceObject as JObject;
                if (sourceObjectAsObject is null)
                {
                    throw new InvalidOperationException("Expected object as type was checked");
                }

                if (!sourceObjectAsObject.Remove(segment))
                {
                    patchFailReason = new JPatchFailReason($"Property not found.", segment);
                    return false;
                }
                return true;
            }

            if (sourceObject.Type == JTokenType.Array)
            {
                var sourceObjectAsArray = sourceObject as JArray;
                if (sourceObjectAsArray is null)
                {
                    throw new InvalidOperationException("Expected non null since type is checked.");
                }
                int arrayIndex;
                if (string.Compare(segment, "-", StringComparison.Ordinal) == 0)
                {
                    arrayIndex = sourceObjectAsArray.Count;
                }
                else if (!int.TryParse(segment, out arrayIndex))
                {
                    patchFailReason = new JPatchFailReason($"Expected an array index at path segment but found '{segment}'.", string.Empty);
                    return false;
                }
                else if(string.Compare(arrayIndex.ToString(), segment, StringComparison.Ordinal) != 0)
                {
                    patchFailReason = new JPatchFailReason($"Array indices with leading zeros are considered errors. Found '{segment}'.", string.Empty);
                    return false;
                }

                if (arrayIndex >= sourceObjectAsArray.Count || arrayIndex < 0)
                {
                    patchFailReason = new JPatchFailReason($"Index out of range. Provided {arrayIndex} when the minimum value is 0 and the maximum value is {sourceObjectAsArray.Count - 1}.", string.Empty);
                    return false;
                }

                sourceObjectAsArray.RemoveAt(arrayIndex);
                return true;
            }

            patchFailReason = new JPatchFailReason("Unexpected token type", string.Empty);
            return false;
        }

        private static bool TryToGetTokenFromToken(
            JToken sourceObject,
            string segment,
            [NotNullWhen(true)] out JToken? result,
            [NotNullWhen(false)] out JPatchFailReason? patchFailReason)
        {
            patchFailReason = null;
            if (sourceObject.Type == JTokenType.Object)
            {
                var sourceObjectAsObject = sourceObject as JObject;
                if (sourceObjectAsObject is null)
                {
                    throw new InvalidOperationException("Expected object as type was checked");
                }
                if (!sourceObjectAsObject.ContainsKey(segment))
                {
                    patchFailReason = new JPatchFailReason("Property not found.", string.Empty);
                    result = null;
                    return false;
                }
                result = sourceObject[segment]!;
                return true;
            }

            if (sourceObject.Type == JTokenType.Array)
            {
                var sourceObjectAsArray = sourceObject as JArray;
                if (sourceObjectAsArray is null)
                {
                    throw new InvalidOperationException("Expected non null since type is checked.");
                }

                int arrayIndex;
                if (string.Compare(segment, "-", StringComparison.Ordinal) == 0)
                {
                    arrayIndex = sourceObjectAsArray.Count - 1;
                }
                else if (!int.TryParse(segment, out arrayIndex))
                {
                    patchFailReason = new JPatchFailReason($"Expected an array index at path segment but found '{segment}'.", string.Empty);
                    result = null;
                    return false;
                }
                else if (string.Compare(arrayIndex.ToString(), segment, StringComparison.Ordinal) != 0)
                {
                    patchFailReason = new JPatchFailReason($"Array indices with leading zeros are considered errors. Found '{segment}'.", string.Empty);
                    result = null;
                    return false;
                }
                result = sourceObjectAsArray[arrayIndex];
                patchFailReason = null;
                return true;
            }

            patchFailReason = new JPatchFailReason("Unexpected token type", string.Empty);
            result = null;
            return false;
        }

    }
}