using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatchRunner.Persistence.Test
{
    public class TempTestFile : IDisposable
    {
        private bool disposedValue;
        public string FilePath { get; init; }

        /// <summary>
        /// Creates a simple disposable temporary file wrapper.
        /// Upon construction deletes the filepath if it existed from before.
        /// Upon disposal deletes the filepath if it still exists.
        /// </summary>
        /// <param name="filePath"></param>
        public TempTestFile(string filePath)
        {
            File.Delete(filePath);
            FilePath = filePath;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                File.Delete(FilePath);
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~TempTestFile()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
