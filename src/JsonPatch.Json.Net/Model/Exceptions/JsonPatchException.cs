namespace JsonPatch.Json.Net.Model.Exceptions
{
    public class JPatchException : Exception
    {
        public JPatchFailReason FailReason { get; private set; } 
        public JPatchException(JPatchFailReason patchFailReason) : base(patchFailReason.Description)
        {
            FailReason = patchFailReason;
        }
    }
}
