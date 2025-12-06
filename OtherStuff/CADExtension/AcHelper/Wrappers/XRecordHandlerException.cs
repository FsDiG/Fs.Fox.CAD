using System;

namespace AcHelper.Wrappers
{
    [Serializable]
    public class XRecordHandlerException : Exception
    {
        
        private string _key;
        private string _dictionary_name;
        private ErrorCode _error_code;

        public string Key
        {
            get { return _key; }
        }
        public string DictionaryName
        {
            get { return _dictionary_name; }
        }
        public ErrorCode ErrorCode
        {
            get { return _error_code; }
        }

        public XRecordHandlerException(string message, ErrorCode errorCode)
            : base(message)
        {
            _error_code = errorCode;
        }
        public XRecordHandlerException(string dictionaryName, string xKey, string message, ErrorCode errorCode = Wrappers.ErrorCode.Error) : base(message)
        {
            _dictionary_name = dictionaryName;
            _key = xKey;
            _error_code = errorCode;
        }
        public XRecordHandlerException(string dictionaryName, string xKey, string message, System.Exception inner, ErrorCode errorCode = Wrappers.ErrorCode.Error)
            : base(message, inner)
        {
            _key = xKey;
            _dictionary_name = dictionaryName;
            _error_code = errorCode;
        }


        public XRecordHandlerException() { }
        public XRecordHandlerException(string message) : base(message) { }
        public XRecordHandlerException(string message, System.Exception inner) : base(message, inner) { }
        protected XRecordHandlerException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
