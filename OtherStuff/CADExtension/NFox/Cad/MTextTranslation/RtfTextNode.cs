namespace NFox.Cad
{
    internal class RtfTextNode : RtfNode
    {
        public string TextString
        { get; set; }

        public RtfTextNode(string textString)
        {
            _key = -1;
            TextString = textString;
        }

        public override string Contents => TextString;
    }
}