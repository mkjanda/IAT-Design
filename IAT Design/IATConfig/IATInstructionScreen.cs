using System.Text;

namespace IATClient.IATConfig
{

    abstract class IATInstructionScreen : IATEvent
    {
        public ConfigFile ConfigFile { get; set; }
        int _ContinueInstructionsDisplayID = -1;
        public int ContinueInstructionsDisplayID
        {
            get
            {
                if (_ContinueInstructionsDisplayID != -1)
                    return _ContinueInstructionsDisplayID;
                _ContinueInstructionsDisplayID = ConfigFile.GetIATImage(InstructionScreen.ContinueInstructionsUri).Id;
                return _ContinueInstructionsDisplayID;
            }
            set
            {
                _ContinueInstructionsDisplayID = value;
            }
        }
        public CInstructionScreen InstructionScreen { get; set; } = null;

        private int _ContinueASCIIKeyCode = -1;
        public int ContinueASCIIKeyCode
        {
            get
            {
                if (_ContinueASCIIKeyCode != -1)
                    return _ContinueASCIIKeyCode;
                _ContinueASCIIKeyCode = Encoding.ASCII.GetBytes(InstructionScreen.ContinueKey)[0];
                return _ContinueASCIIKeyCode;
            }
            set
            {
                _ContinueASCIIKeyCode = value;
            }
        }

        public IATInstructionScreen(EEventType type)
            : base(type)
        {
        }
    }
}
