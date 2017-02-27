using System;

public enum Opcodes : byte
{
    CL_CHECK = 1,
    LCR_CHECK,
    CL_START,
    LCR_START,
    CL_CREATE,
    LCR_CREATE,
    CL_INFO,
    LCR_INFO
}

public enum CheckResult : byte
{
    LAUNCHER_OK,
    LAUNCHER_VERSION,
    LAUNCHER_FILE
}