using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StormLibSharp.Native
{
    [Flags]
    internal enum SFileOpenArchiveFlags : uint
    {
        None = 0,
        TypeIsFile = None,
        TypeIsMemoryMapped = 1,
        TypeIsHttp = 2,

        AccessReadOnly = 0x100,
        AccessReadWriteShare = 0x200,
        AccessUseBitmap = 0x400,

        DontOpenListfile = 0x10000,
        DontOpenAttributes = 0x20000,
        DontSearchHeader = 0x40000,
        ForceVersion1 = 0x80000,
        CheckSectorCRC = 0x100000,
    }

    [Flags]
    internal enum SFileCreateArchiveFlags : uint
    {
        MPQ_CREATE_LISTFILE       =  0x00100000,
        MPQ_CREATE_ATTRIBUTES     =  0x00200000, 
        MPQ_CREATE_SIGNATURE      =  0x00400000,
        MPQ_CREATE_ARCHIVE_V1     =  0x00000000,
        MPQ_CREATE_ARCHIVE_V2     =  0x01000000,
        MPQ_CREATE_ARCHIVE_V3     =  0x02000000,
        MPQ_CREATE_ARCHIVE_V4     =  0x03000000,
        MPQ_CREATE_ARCHIVE_VMASK  =  0x0F000000,
        TEST                      =  0x00000000
    }
}
