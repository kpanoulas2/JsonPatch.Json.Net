namespace JsonPatch.Json.Net.Model
{
    public sealed class JPatchFailReason
    {
        public string Description { get; private set; }
        public string PathOfFailure { get; private set; }
        public JPatchFailReason(string description, string pathOfFailure)
        {
            Description = description;
            PathOfFailure = pathOfFailure;
        }
    }
}
