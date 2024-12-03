namespace Fpi.Communication.Commands
{
    public class CommandExtendId
    {
        public const int Read = 0x55;
        public const int Write = 0x66;
        public const int ReadResponse = 0xaa;
        public const int WriteResponse = 0x99;
        public const int ReadWrite = 0xff;

        public const int ERROR_CODE = 0xEE;
        public const int NOT_SUPPORTED_EXT_CODE = 0x77;
    }
}