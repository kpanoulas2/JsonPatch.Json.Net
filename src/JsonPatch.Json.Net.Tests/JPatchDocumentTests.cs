using JsonPatch.Json.Net.Model;
using JsonPatch.Json.Net.Model.Exceptions;
using JsonPatch.Json.Net.Tests.Helpers;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.Linq;

namespace JsonPatch.Json.Net.Tests
{
    public sealed class JPatchDocumentTests
    {
        [TestCase("JsonPatch.Json.Net.Tests.JsonPatchDocuments.SimpleAddOperation.json")]
        [TestCase("JsonPatch.Json.Net.Tests.JsonPatchDocuments.SimpleRemoveOperation.json")]
        [TestCase("JsonPatch.Json.Net.Tests.JsonPatchDocuments.RemoveOperationFromObjectOrArray.json")]
        [TestCase("JsonPatch.Json.Net.Tests.JsonPatchDocuments.SimpleReplaceOperation.json")]
        [TestCase("JsonPatch.Json.Net.Tests.JsonPatchDocuments.SimpleCopyOperation.json")]
        [TestCase("JsonPatch.Json.Net.Tests.JsonPatchDocuments.SimpleMoveOperation.json")]
        [TestCase("JsonPatch.Json.Net.Tests.JsonPatchDocuments.SimpleTestOperation.json")]
        [TestCase("JsonPatch.Json.Net.Tests.JsonPatchDocuments.MultipleOperations.json")]
        public void GivenAJsonStringThatContainsAJsonPatchDocument_WhenTheParseFunctionIsExecuted_ThenAJsonPatchObjectIsCreated(
            string resourceFilename)
        { 
            Assert.DoesNotThrow(() => JPatchDocument.Load(ResourceLoader.LoadJson(resourceFilename)));

        }

        [Test]
        public void GivenAJsonStringThatContainsAJsonPatchDocumentWithAnAddOperation_WhenTheParseFunctionIsExecuted_ThenAllTheFieldsAreTransferredCorrectly()
        {
            var jsonPatchDocument = JPatchDocument.Load(ResourceLoader.LoadJson("JsonPatch.Json.Net.Tests.JsonPatchDocuments.SimpleAddOperation.json"));

            Assert.That(jsonPatchDocument.Operations.Count, Is.EqualTo(1));
            var addOperation = jsonPatchDocument.Operations[0];
            Assert.That(addOperation.Type, Is.EqualTo(OperationType.Add));
            Assert.That(addOperation.PathSegments.Count, Is.EqualTo(2));
            Assert.That(addOperation.FromSegments, Is.Empty);
            Assert.That(addOperation.Value, Is.TypeOf<JObject>());
            Assert.That(addOperation.Value.Value<string>("name"), Is.EqualTo("Ginger Nut"));
        }

        [Test]
        public void GivenAJsonStringThatContainsAJsonPatchDocumentWithACopyOperation_WhenTheParseFunctionIsExecuted_ThenAllTheFieldsAreTransferredCorrectly()
        {
            var jsonPatchDocument = JPatchDocument.Load(ResourceLoader.LoadJson("JsonPatch.Json.Net.Tests.JsonPatchDocuments.SimpleCopyOperation.json"));

            Assert.That(jsonPatchDocument.Operations.Count, Is.EqualTo(1));
            var addOperation = jsonPatchDocument.Operations[0];
            Assert.That(addOperation.Type, Is.EqualTo(OperationType.Copy));
            Assert.That(addOperation.PathSegments.Count, Is.EqualTo(1));
            Assert.That(addOperation.FromSegments.Count, Is.EqualTo(2));
            Assert.That(addOperation.Value, Is.Null);
        }

        [Test]
        public void GivenAJsonStringThatContainsAJsonPatchDocumentWithAMoveOperation_WhenTheParseFunctionIsExecuted_ThenAllTheFieldsAreTransferredCorrectly()
        {
            var jsonPatchDocument = JPatchDocument.Load(ResourceLoader.LoadJson("JsonPatch.Json.Net.Tests.JsonPatchDocuments.SimpleMoveOperation.json"));

            Assert.That(jsonPatchDocument.Operations.Count, Is.EqualTo(1));
            var addOperation = jsonPatchDocument.Operations[0];
            Assert.That(addOperation.Type, Is.EqualTo(OperationType.Move));
            Assert.That(addOperation.PathSegments.Count, Is.EqualTo(1));
            Assert.That(addOperation.FromSegments.Count, Is.EqualTo(1));
            Assert.That(addOperation.Value, Is.Null);
        }

        [Test]
        public void GivenAJsonStringThatContainsAJsonPatchDocumentWithARemoveOperation_WhenTheParseFunctionIsExecuted_ThenAllTheFieldsAreTransferredCorrectly()
        {
            var jsonPatchDocument = JPatchDocument.Load(ResourceLoader.LoadJson("JsonPatch.Json.Net.Tests.JsonPatchDocuments.SimpleRemoveOperation.json"));

            Assert.That(jsonPatchDocument.Operations.Count, Is.EqualTo(1));
            var addOperation = jsonPatchDocument.Operations[0];
            Assert.That(addOperation.Type, Is.EqualTo(OperationType.Remove));
            Assert.That(addOperation.PathSegments.Count, Is.EqualTo(1));
            Assert.That(addOperation.FromSegments, Is.Empty);
            Assert.That(addOperation.Value, Is.Null);
        }

        [Test]
        public void GivenAJsonStringThatContainsAJsonPatchDocumentWithAReplaceOperation_WhenTheParseFunctionIsExecuted_ThenAllTheFieldsAreTransferredCorrectly()
        {
            var jsonPatchDocument = JPatchDocument.Load(ResourceLoader.LoadJson("JsonPatch.Json.Net.Tests.JsonPatchDocuments.SimpleReplaceOperation.json"));

            Assert.That(jsonPatchDocument.Operations.Count, Is.EqualTo(1));
            var addOperation = jsonPatchDocument.Operations[0];
            Assert.That(addOperation.Type, Is.EqualTo(OperationType.Replace));
            Assert.That(addOperation.PathSegments.Count, Is.EqualTo(3));
            Assert.That(addOperation.FromSegments, Is.Empty);
            Assert.That(addOperation.Value, Is.TypeOf<JValue>());
        }

        [Test]
        public void GivenAJsonStringThatContainsAJsonPatchDocumentWithATestOperation_WhenTheParseFunctionIsExecuted_ThenAllTheFieldsAreTransferredCorrectly()
        {
            var jsonPatchDocument = JPatchDocument.Load(ResourceLoader.LoadJson("JsonPatch.Json.Net.Tests.JsonPatchDocuments.SimpleTestOperation.json"));

            Assert.That(jsonPatchDocument.Operations.Count, Is.EqualTo(1));
            var addOperation = jsonPatchDocument.Operations[0];
            Assert.That(addOperation.Type, Is.EqualTo(OperationType.Test));
            Assert.That(addOperation.PathSegments.Count, Is.EqualTo(2));
            Assert.That(addOperation.FromSegments, Is.Empty);
            Assert.That(addOperation.Value, Is.TypeOf<JValue>());
        }


        [TestCase("JsonPatch.Json.Net.Tests.InvalidJsonPatchDocuments.InvalidSimpleAddOperation_MissingPath.json")]
        [TestCase("JsonPatch.Json.Net.Tests.InvalidJsonPatchDocuments.InvalidSimpleAddOperation_MissingValue.json")]
        [TestCase("JsonPatch.Json.Net.Tests.InvalidJsonPatchDocuments.InvalidSimpleCopyOperation_MissingFrom.json")]
        [TestCase("JsonPatch.Json.Net.Tests.InvalidJsonPatchDocuments.InvalidSimpleCopyOperation_MissingPath.json")]
        [TestCase("JsonPatch.Json.Net.Tests.InvalidJsonPatchDocuments.InvalidSimpleMoveOperation_MissingFrom.json")]
        [TestCase("JsonPatch.Json.Net.Tests.InvalidJsonPatchDocuments.InvalidSimpleMoveOperation_MissingPath.json")]
        [TestCase("JsonPatch.Json.Net.Tests.InvalidJsonPatchDocuments.InvalidSimpleRemoveOperation_MissingPath.json")]
        [TestCase("JsonPatch.Json.Net.Tests.InvalidJsonPatchDocuments.InvalidSimpleReplaceOperation_MissingPath.json")]
        [TestCase("JsonPatch.Json.Net.Tests.InvalidJsonPatchDocuments.InvalidSimpleReplaceOperation_MissingValue.json")]
        [TestCase("JsonPatch.Json.Net.Tests.InvalidJsonPatchDocuments.InvalidSimpleTestOperation_MissingPath.json")]
        [TestCase("JsonPatch.Json.Net.Tests.InvalidJsonPatchDocuments.InvalidSimpleTestOperation_MissingValue.json")]
        public void GivenAJsonStringThatContainsAnInvalidJsonPatchDocument_WhenTheParseFunctionIsExecuted_ThenAnExceptionIsThrown(
    string resourceFilename)
        {
            Assert.Throws<JPatchException>(() => JPatchDocument.Load(ResourceLoader.LoadJson(resourceFilename)));
        }

        [Test]
        public void GivenAJsonStringThatContainsAJsonPatchDocumentWithASlashIdentifier_WhenTheParseFunctionIsExecuted_ThenThePathSegmentIsRestored()
        {
            var resourceFilename = "JsonPatch.Json.Net.Tests.JsonPatchDocuments_SpecialPaths.Operation_PathWithSlashIdentifier.json";
            var jPatchDocument = JPatchDocument.Load(ResourceLoader.LoadJson(resourceFilename));
            Assert.That(jPatchDocument.Operations.First().PathSegments[1], Is.EqualTo("fieldA/B"));
        }

        [Test]
        public void GivenAJsonStringThatContainsAJsonPatchDocumentWithATildeIdentifier_WhenTheParseFunctionIsExecuted_ThenThePathSegmentIsRestored()
        {
            var resourceFilename = "JsonPatch.Json.Net.Tests.JsonPatchDocuments_SpecialPaths.Operation_PathWithTildeIdentifier.json";
            var jPatchDocument = JPatchDocument.Load(ResourceLoader.LoadJson(resourceFilename));
            Assert.That(jPatchDocument.Operations.First().PathSegments[1], Is.EqualTo("fieldA~B"));
        }

        [Test]
        public void GivenAJsonStringThatContainsAJsonPatchDocumentWithALastIndexOfArrayIdentifier_WhenTheParseFunctionIsExecuted_ThenThePathSegmentIsRestored()
        {
            var resourceFilename = "JsonPatch.Json.Net.Tests.JsonPatchDocuments_SpecialPaths.Operation_PathWithLastIndexOfArrayIdentifier.json";
            var jPatchDocument = JPatchDocument.Load(ResourceLoader.LoadJson(resourceFilename));
            Assert.That(jPatchDocument.Operations.First().PathSegments[1], Is.EqualTo("-"));
        }

    }
}