using JsonPatch.Json.Net.Model.Exceptions;
using JsonPatch.Json.Net.Tests.Helpers;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;

namespace JsonPatch.Json.Net.Tests
{
    public sealed class JPatchTests
    {
        [TestCase("JsonPatch.Json.Net.Tests.RFC6902Tests.TestA01.json")]
        [TestCase("JsonPatch.Json.Net.Tests.RFC6902Tests.TestA02.json")]
        [TestCase("JsonPatch.Json.Net.Tests.RFC6902Tests.TestA03.json")]
        [TestCase("JsonPatch.Json.Net.Tests.RFC6902Tests.TestA04.json")]
        [TestCase("JsonPatch.Json.Net.Tests.RFC6902Tests.TestA05.json")]
        [TestCase("JsonPatch.Json.Net.Tests.RFC6902Tests.TestA06.json")]
        [TestCase("JsonPatch.Json.Net.Tests.RFC6902Tests.TestA07.json")]
        [TestCase("JsonPatch.Json.Net.Tests.RFC6902Tests.TestA08.json")]
        [TestCase("JsonPatch.Json.Net.Tests.RFC6902Tests.TestA09.json")]
        [TestCase("JsonPatch.Json.Net.Tests.RFC6902Tests.TestA10.json")]
        [TestCase("JsonPatch.Json.Net.Tests.RFC6902Tests.TestA11.json")]
        [TestCase("JsonPatch.Json.Net.Tests.RFC6902Tests.TestA12.json")]
        [TestCase("JsonPatch.Json.Net.Tests.RFC6902Tests.TestA14.json")]
        [TestCase("JsonPatch.Json.Net.Tests.RFC6902Tests.TestA15.json")]
        [TestCase("JsonPatch.Json.Net.Tests.RFC6902Tests.TestA16.json")]


        [TestCase("JsonPatch.Json.Net.Tests.OfficialTests.Test01.json")]
        [TestCase("JsonPatch.Json.Net.Tests.OfficialTests.Test02.json")]
        [TestCase("JsonPatch.Json.Net.Tests.OfficialTests.Test03.json")]
        [TestCase("JsonPatch.Json.Net.Tests.OfficialTests.Test04.json")]
        [TestCase("JsonPatch.Json.Net.Tests.OfficialTests.Test05.json")]
        [TestCase("JsonPatch.Json.Net.Tests.OfficialTests.Test06.json")]
        [TestCase("JsonPatch.Json.Net.Tests.OfficialTests.Test07.json")]
        [TestCase("JsonPatch.Json.Net.Tests.OfficialTests.Test08.json")]
        [TestCase("JsonPatch.Json.Net.Tests.OfficialTests.Test09.json")]
        [TestCase("JsonPatch.Json.Net.Tests.OfficialTests.Test10.json")]
        [TestCase("JsonPatch.Json.Net.Tests.OfficialTests.Test13.json")]
        [TestCase("JsonPatch.Json.Net.Tests.OfficialTests.Test15.json")]
        [TestCase("JsonPatch.Json.Net.Tests.OfficialTests.Test16.json")]
        [TestCase("JsonPatch.Json.Net.Tests.OfficialTests.Test17.json")]
        [TestCase("JsonPatch.Json.Net.Tests.OfficialTests.Test18.json")]
        [TestCase("JsonPatch.Json.Net.Tests.OfficialTests.Test19.json")]
        [TestCase("JsonPatch.Json.Net.Tests.OfficialTests.Test20.json")]
        [TestCase("JsonPatch.Json.Net.Tests.OfficialTests.Test21.json")]
        [TestCase("JsonPatch.Json.Net.Tests.OfficialTests.Test22.json")]
        [TestCase("JsonPatch.Json.Net.Tests.OfficialTests.Test23.json")]
        [TestCase("JsonPatch.Json.Net.Tests.OfficialTests.Test24.json")]
        [TestCase("JsonPatch.Json.Net.Tests.OfficialTests.Test25.json")]
        [TestCase("JsonPatch.Json.Net.Tests.OfficialTests.Test26.json")]
        [TestCase("JsonPatch.Json.Net.Tests.OfficialTests.Test27.json")]
        [TestCase("JsonPatch.Json.Net.Tests.OfficialTests.Test28.json")]
        [TestCase("JsonPatch.Json.Net.Tests.OfficialTests.Test29.json")]
        [TestCase("JsonPatch.Json.Net.Tests.OfficialTests.Test30.json")]
        [TestCase("JsonPatch.Json.Net.Tests.OfficialTests.Test31.json")]
        [TestCase("JsonPatch.Json.Net.Tests.OfficialTests.Test32.json")]
        [TestCase("JsonPatch.Json.Net.Tests.OfficialTests.Test33.json")]
        [TestCase("JsonPatch.Json.Net.Tests.OfficialTests.Test34.json")]
        [TestCase("JsonPatch.Json.Net.Tests.OfficialTests.Test35.json")]
        [TestCase("JsonPatch.Json.Net.Tests.OfficialTests.Test36.json")]
        public void GivenASourceDocumentAPatchDocumentAndAnExpectedResult_WhenPatchIsExecutedOnTheSourceDocument_ThenTheExpectedResultIsReturned(
            string testFilename)
        {
            var test = JToken.Parse(ResourceLoader.LoadJson(testFilename));

            var sourceObject = test["doc"];
            var patchObject = test["patch"];
            var expectedResult = test["expected"];
            var expectedError = test["error"];

            if (sourceObject is null || patchObject is null)
            {
                throw new InvalidOperationException("Test does not have the right structure");
            }


            if(expectedResult is null && expectedError is null)
            {
                throw new InvalidOperationException("Test does not have the right structure");
            }

            var patchDocument = JPatchDocument.Load(patchObject.ToString());

            if(expectedResult is not null)
            {
                var actualResult = JPatch.Patch(sourceObject, patchDocument);
                var jTokenEqualityComparer = new JTokenEqualityComparer();
                Assert.That(jTokenEqualityComparer.Equals(expectedResult, actualResult), Is.True);
                return;
            }

            Assert.Throws<JPatchException>(() => JPatch.Patch(sourceObject, patchDocument));

        }
    }
}
