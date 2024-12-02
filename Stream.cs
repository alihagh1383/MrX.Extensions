using System.Text;

namespace MrX.Extensions;

public static class Stream
{
    public static void CopyTo(this System.IO.Stream input, string initialData, System.IO.Stream output, int bufferSize)
    {
        var bytes = Encoding.ASCII.GetBytes(initialData);
        output.Write(bytes, 0, bytes.Length);
        CopyTo(input, output, bufferSize);
    }

    public static void CopyTo(this System.IO.Stream input, System.IO.Stream output, int bufferSize = 8192)
    {
        try
        {
            if (!input.CanRead) throw new InvalidOperationException("input must be open for reading");
            if (!output.CanWrite) throw new InvalidOperationException("output must be open for writing");

            byte[][] buf = { new byte[bufferSize], new byte[bufferSize] };
            int[] bufl = { 0, 0 };
            var bufno = 0;
            var read = input.BeginRead(buf[bufno], 0, buf[bufno].Length, null, null);
            IAsyncResult write = null;

            while (true)
            {
                // wait for the read operation to complete
                read.AsyncWaitHandle.WaitOne();
                bufl[bufno] = input.EndRead(read);

                // if zero bytes read, the copy is complete
                if (bufl[bufno] == 0) break;

                // wait for the in-flight write operation, if one exists, to complete
                // the only time one won't exist is after the very first read operation completes
                if (write != null)
                {
                    write.AsyncWaitHandle.WaitOne();
                    output.EndWrite(write);
                }

                // start the new write operation
                write = output.BeginWrite(buf[bufno], 0, bufl[bufno], null, null);

                // toggle the current, in-use buffer
                // and start the read operation on the new buffer.
                //
                // Changed to use XOR to toggle between 0 and 1.
                // A little speedier than using a ternary expression.
                bufno ^= 1; // bufno = ( bufno == 0 ? 1 : 0 ) ;
                read = input.BeginRead(buf[bufno], 0, buf[bufno].Length, null, null);
            }

            // wait for the final in-flight write operation, if one exists, to complete
            // the only time one won't exist is if the input stream is empty.
            if (write != null)
            {
                write.AsyncWaitHandle.WaitOne();
                output.EndWrite(write);
            }

            output.Flush();
        }
        catch
        {
        }
    }
}