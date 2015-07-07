using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IonFar.SharePoint.Migration;

namespace IonFar.SharePoint.Synchronization.Preprocessors
{
    internal class StreamPreprocessor : IDisposable
    {
        private bool _disposedValue = false; // To detect redundant calls

        readonly System.IO.Stream _inputStream;
        IEnumerable<ITextFilePreprocessor> _preprocessors;
        readonly System.IO.Stream _outputStream;

        public StreamPreprocessor(IContextManager contextManager, IUpgradeLog logger, System.IO.Stream inputStream, IEnumerable<ITextFilePreprocessor> preprocessors)
        {
            _inputStream = inputStream;
            _preprocessors = preprocessors;

            if (_preprocessors.Any())
            {
                using (var reader = new System.IO.StreamReader(_inputStream, Encoding.UTF8))
                {
                    var content = reader.ReadToEnd();
                    foreach (var preprocessor in preprocessors)
                    {
                        content = preprocessor.Process(contextManager, logger, content);
                    }
                    _outputStream = new System.IO.MemoryStream(Encoding.UTF8.GetBytes(content));
                }
            }
        }

        public System.IO.Stream Stream
        {
            get
            {
                return _outputStream ?? _inputStream;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            // GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (_outputStream != null)
                    {
                        _outputStream.Dispose();
                    }
                }
                _disposedValue = true;
            }
        }
    }
}
