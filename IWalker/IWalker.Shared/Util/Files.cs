using System;
using System.Collections.Generic;
using System.Text;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;

namespace IWalker.Util
{
    /// <summary>
    /// Help with all things having to do with files.
    /// </summary>
    public static class Files
    {
        /// <summary>
        /// Configure a file picker for certificate files we can understand.
        /// </summary>
        /// <param name="op">The file picker</param>
        /// <returns>The same file picker with various options configured for opening a file.</returns>
        public static FileOpenPicker ForCert(this FileOpenPicker op)
        {
            op.CommitButtonText = "Use Certificate";
            op.SettingsIdentifier = "OpenCert";
            op.FileTypeFilter.Add(".p12");
            op.FileTypeFilter.Add(".pfx");
            op.ViewMode = PickerViewMode.List;

            return op;
        }

        /// <summary>
        /// Return the byte array as a read only buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static IRandomAccessStream AsRORAByteStream (this byte[] buffer)
        {
            return new InMemoryRORAByteArrayStream(buffer);
        }

        class InMemoryRORAByteArrayStream : IRandomAccessStream
        {
            private  byte[] _buffer;
            private ulong _position;

            public InMemoryRORAByteArrayStream(byte[] buffer, ulong position = 0)
            {
                _buffer = buffer;
                _position = position;
                CheckPosition();
            }

            /// <summary>
            /// Make sure the current EOF position is ok.
            /// </summary>
            private void CheckPosition()
            {
                if (_position > (ulong)_buffer.Length)
                {
                    _position = (ulong) _buffer.Length;
                }
            }

            private ulong DistTillEnd()
            {
                return ((ulong) _buffer.Length) - _position;
            }

            public bool CanRead
            {
                get { return true; }
            }

            public bool CanWrite
            {
                get { return false; }
            }

            public IRandomAccessStream CloneStream()
            {
                return new InMemoryRORAByteArrayStream(_buffer, _position);
            }

            public IInputStream GetInputStreamAt(ulong position)
            {
                return new InMemoryRORAByteArrayStream(_buffer, position);
            }

            /// <summary>
            /// Fail because we don't do writing.
            /// </summary>
            /// <param name="position"></param>
            /// <returns></returns>
            public IOutputStream GetOutputStreamAt(ulong position)
            {
                throw new NotImplementedException();
            }

            public ulong Position
            {
                get { return _position; }
            }

            public void Seek(ulong position)
            {
                _position = position;
                CheckPosition();
            }

            public ulong Size
            {
                get
                {
                    return (ulong) _buffer.Length;
                }
                set
                {
                    throw new NotImplementedException();
                }
            }

            public void Dispose()
            {
            }

            /// <summary>
            /// Read back some number of bytes from our byte array into the buffer.
            /// </summary>
            /// <param name="buffer"></param>
            /// <param name="count"></param>
            /// <param name="options"></param>
            /// <returns></returns>
            public Windows.Foundation.IAsyncOperationWithProgress<IBuffer, uint> ReadAsync(IBuffer buffer, uint count, InputStreamOptions options)
            {
                return AsyncInfo.Run<IBuffer, uint>(async (token, progress) =>
                {
                    return await Task.Factory.StartNew(() =>
                    {
                        lock (this)
                        {
                            var goodCount = (int)Math.Min(DistTillEnd(), (ulong)count);
                            if (goodCount == 0)
                                return null;
                            using (var dw = new DataWriter())
                            {
                                var b = _buffer.AsBuffer((int)_position, goodCount);
                                dw.WriteBuffer(b);
                                var result = dw.DetachBuffer();
                                _position += (ulong)goodCount;
                                return result;
                            }
                        }
                    });
                });
            }

            /// <summary>
            /// Don't support writing, so don't support this guy!
            /// </summary>
            /// <returns></returns>
            public Windows.Foundation.IAsyncOperation<bool> FlushAsync()
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// We don't support writing.
            /// </summary>
            /// <param name="buffer"></param>
            /// <returns></returns>
            public Windows.Foundation.IAsyncOperationWithProgress<uint, uint> WriteAsync(IBuffer buffer)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Clean up a string to make into a legal filename.
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        public static string CleanFilename(this string original)
        {
            return original.Replace(":", "_")
                .Replace("/", "_")
                .Replace("\\", "_")
                .Replace("+", "_")
                .Replace("?", "")
                .Replace("*", "")
                .Replace(">", "_")
                .Replace("<", "_");
        }

    }
}
