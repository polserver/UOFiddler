namespace Ultima
{
    public class WindowProcessStream : ProcessStream
    {
        private ClientWindowHandle _window;
        private ClientProcessHandle _processId;

        public ClientWindowHandle Window { get { return _window; } set { _window = value; } }

        public WindowProcessStream(ClientWindowHandle window)
        {
            _window = window;
            _processId = ClientProcessHandle.Invalid;
        }

        public override ClientProcessHandle ProcessID
        {
            get
            {
                if (NativeMethods.IsWindow(_window) != 0 && !_processId.IsInvalid)
                {
                    return _processId;
                }

                NativeMethods.GetWindowThreadProcessId(_window, ref _processId);

                return _processId;
            }
        }
    }
}