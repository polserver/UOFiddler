using System;
using System.IO;

namespace Ultima
{
    public unsafe abstract class ProcessStream : Stream
    {
        private const int ProcessAllAccess = 0x1F0FFF;

        protected bool MOpen;
        protected ClientProcessHandle MProcess;

        protected int MPosition;

        public abstract ClientProcessHandle ProcessId { get; }

        public ProcessStream()
        {
        }

        public virtual bool BeginAccess()
        {
            if (MOpen)
                return false;

            MProcess = NativeMethods.OpenProcess(ProcessAllAccess, 0, ProcessId);
            MOpen = true;

            return true;
        }

        public virtual void EndAccess()
        {
            if (!MOpen)
                return;

            MProcess.Close();
            MOpen = false;
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            bool end = !BeginAccess();

            int res = 0;

            fixed (byte* p = buffer)
                NativeMethods.ReadProcessMemory(MProcess, MPosition, p + offset, count, ref res);

            MPosition += count;

            if (end)
                EndAccess();

            return res;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            bool end = !BeginAccess();

            fixed (byte* p = buffer)
                NativeMethods.WriteProcessMemory(MProcess, MPosition, p + offset, count, 0);

            MPosition += count;

            if (end)
                EndAccess();
        }

        public override bool CanRead => true;
        public override bool CanWrite => true;
        public override bool CanSeek => true;

        public override long Length => throw new NotSupportedException();
        public override long Position { get => MPosition;
            set => MPosition = (int)value;
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin: MPosition = (int)offset; break;
                case SeekOrigin.Current: MPosition += (int)offset; break;
                case SeekOrigin.End: throw new NotSupportedException();
            }

            return MPosition;
        }
    }
}