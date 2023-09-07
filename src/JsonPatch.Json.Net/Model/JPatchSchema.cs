using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace JsonPatch.Json.Net.Model
{
    public static class JPatchSchema
    {
        private static readonly JSchema instance;

        static JPatchSchema()
        {
             instance = new JSchema()
            {
                Title = "JSON Patch",
                Description = "The schema describing a JSON Patch object",
                Type = JSchemaType.Array,
            };
            instance.Items.Add(BuildItemTypeSchema());
        }

        private static JSchema BuildItemTypeSchema()
        {
            var result = new JSchema()
            {
                Description = "The schema describing a single JSON Patch operation",
            };
            result.OneOf.Add(BuildAddOperationSchema());
            result.OneOf.Add(BuildMoveOperationSchema());
            result.OneOf.Add(BuildReplaceOperationSchema());
            result.OneOf.Add(BuildRemoveOperationSchema());
            result.OneOf.Add(BuildCopyOperationSchema());
            result.OneOf.Add(BuildTestOperationSchema());

            return result;
        }

        private static JSchema BuildAddOperationSchema()
        {
            var result = new JSchema()
            {
                Description = "Add operation",
            };

            result.Type = JSchemaType.Object;
            result.Properties.Add("op", BuildOpPropertySchema("add"));
            result.Properties.Add("path", BuildJsonPathPropertySchema("The path that the object will be added to."));
            result.Required.Add("op");
            result.Required.Add("path");
            result.Required.Add("value");

            return result;
        }

        private static JSchema BuildMoveOperationSchema()
        {
            var result = new JSchema()
            {
                Description = "Move operation",
            };

            result.Type = JSchemaType.Object;
            result.Properties.Add("op", BuildOpPropertySchema("move"));
            result.Properties.Add("path", BuildJsonPathPropertySchema("The path that the object will be moved to."));
            result.Properties.Add("from", BuildJsonPathPropertySchema("The path that the object will be moved from."));

            result.Required.Add("op");
            result.Required.Add("path");
            result.Required.Add("from");

            return result;
        }

        private static JSchema BuildReplaceOperationSchema()
        {
            var result = new JSchema()
            {
                Description = "Replace operation",
            };

            result.Type = JSchemaType.Object;
            result.Properties.Add("op", BuildOpPropertySchema("replace"));
            result.Properties.Add("path", BuildJsonPathPropertySchema("The path of the object that the provided object will be replacing."));
            result.Required.Add("op");
            result.Required.Add("path");
            result.Required.Add("value");

            return result;
        }

        private static JSchema BuildRemoveOperationSchema()
        { 
            var result = new JSchema()
            {
                Description = "Remove operation",
            };

            result.Type = JSchemaType.Object;
            result.Properties.Add("op", BuildOpPropertySchema("remove"));
            result.Properties.Add("path", BuildJsonPathPropertySchema("The path of the object that will be removed."));
            result.Required.Add("op");
            result.Required.Add("path");

            return result;
        }

        private static JSchema BuildCopyOperationSchema()
        {
            var result = new JSchema()
            {
                Description = "Copy operation",
            };

            result.Type = JSchemaType.Object;
            result.Properties.Add("op", BuildOpPropertySchema("copy"));
            result.Properties.Add("path", BuildJsonPathPropertySchema("The path of the object that will copied to."));
            result.Properties.Add("from", BuildJsonPathPropertySchema("The path that the object will be copied from."));
            result.Required.Add("op");
            result.Required.Add("path");
            result.Required.Add("from");

            return result;
        }

        private static JSchema BuildTestOperationSchema()
        {
            var result = new JSchema()
            {
                Description = "Test operation",
            };

            result.Type = JSchemaType.Object;
            result.Properties.Add("op", BuildOpPropertySchema("test"));
            result.Properties.Add("path", BuildJsonPathPropertySchema("The path of the object will be tested for the existence of the value."));
            result.Required.Add("op");
            result.Required.Add("path");
            result.Required.Add("value");

            return result;
        }

        private static JSchema BuildOpPropertySchema(string operationName)
        {
            var result = new JSchema();
            result.Enum.Add(JToken.FromObject(operationName));
            return result;
        }

        private static JSchema BuildJsonPathPropertySchema(string description)
        {
            var result = new JSchema();
            result.Description = description;
            result.Type = JSchemaType.String;
            result.Pattern = "^(/[^/~]*(~[01][^/~]*)*)*$";
            return result;
        }

        public static JSchema Instance => instance;

    }
}
