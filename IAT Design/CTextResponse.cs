namespace IATClient
{
    /// <summary>
    /// An abstract class that serves as a superclass for text responses
    /// </summary>
    abstract class CTextResponse : CResponse
    {
        /// <summary>
        /// Only called by derived class constructors
        /// </summary>
        /// <param name="type"></param>
        public CTextResponse(EResponseType type)
            : base(type)
        { }

        public CTextResponse(EResponseType t, CTextResponse r) : base(t, r) { }

        /// <summary>
        /// Overriden in derives classes.  Validates the text response's definition
        /// </summary>
        /// <returns>"true" if the object defines a valid response type, "false" otherwise</returns>
        public abstract override bool IsValid();

    }
}
