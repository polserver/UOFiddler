namespace Ultima
{
    public class WindowProcessStream : ProcessStream
    {
        private ClientWindowHandle _mWindow;
        private ClientProcessHandle _mProcessId;

        public ClientWindowHandle Window { get => _mWindow;
            set => _mWindow = value;
        }

        public WindowProcessStream(ClientWindowHandle window)
        {
            _mWindow = window;
            _mProcessId = ClientProcessHandle.Invalid;
        }

        public override ClientProcessHandle ProcessId
        {
            get
            {
                if (NativeMethods.IsWindow(_mWindow) != 0 && !_mProcessId.IsInvalid)
                    return _mProcessId;

                NativeMethods.GetWindowThreadProcessId(_mWindow, ref _mProcessId);

                return _mProcessId;
            }
        }
    }
}