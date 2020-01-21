using System;

namespace FuseSharp {
    [Flags]
    public enum OperationsFlags : ulong {
        None = 0,

        GetPathStatus = 0x000000001,
        ReadSymbolicLink = 0x000000002,
        CreateSpecialFile = 0x000000004,
        CreateDirectory = 0x000000008,
        RemoveFile = 0x000000010,
        RemoveDirectory = 0x000000020,
        CreateSymbolicLink = 0x000000040,
        RenamePath = 0x000000080,
        CreateHardLink = 0x000000100,
        ChangePathPermissions = 0x000000200,
        ChangePathOwner = 0x000000400,
        TruncateFile = 0x000000800,
        ChangePathTimes = 0x000001000,
        OpenHandle = 0x000002000,
        ReadHandle = 0x000004000,
        WriteHandle = 0x000008000,
        GetFileSystemStatus = 0x000010000,
        FlushHandle = 0x000020000,
        ReleaseHandle = 0x000040000,
        SynchronizeHandle = 0x000080000,
        SetPathExtendedAttribute = 0x000100000,
        GetPathExtendedAttribute = 0x000200000,
        ListPathExtendedAttributes = 0x000400000,
        RemovePathExtendedAttribute = 0x000800000,
        OpenDirectory = 0x001000000,
        ReadDirectory = 0x002000000,
        ReleaseDirectory = 0x004000000,
        SynchronizeDirectory = 0x008000000,
        Init = 0x010000000,
        Destroy = 0x020000000,
        AccessPath = 0x040000000,
        CreateHandle = 0x080000000,
        TruncateHandle = 0x100000000,
        GetHandleStatus = 0x200000000,
        LockHandle = 0x400000000,
        MapPathLogicalToPhysicalIndex = 0x800000000,

        All = GetPathStatus | ReadSymbolicLink | CreateSpecialFile | CreateDirectory |
        RemoveFile | RemoveDirectory | CreateSymbolicLink | RenamePath | CreateHardLink |
        ChangePathPermissions | ChangePathOwner | TruncateFile | ChangePathTimes |
        OpenHandle | ReadHandle | WriteHandle | GetFileSystemStatus | FlushHandle |
        ReleaseHandle | SynchronizeHandle | SetPathExtendedAttribute | GetPathExtendedAttribute |
        ListPathExtendedAttributes | RemovePathExtendedAttribute | OpenDirectory | ReadDirectory |
        ReleaseDirectory | SynchronizeDirectory | Init | Destroy | AccessPath | CreateHandle |
        TruncateHandle | GetHandleStatus | LockHandle | MapPathLogicalToPhysicalIndex,
    }
}