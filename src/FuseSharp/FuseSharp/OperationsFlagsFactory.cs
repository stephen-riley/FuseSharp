using System;
using System.Collections.Generic;
using System.Linq;
using static System.Reflection.BindingFlags;

namespace FuseSharp
{
    using static OperationsFlags;

    public class OperationsFlagsFactory
    {
        public static OperationsFlags GetOperationsFlags(Type type)
        {
            var methodNames = new HashSet<string>(type.GetMethods(DeclaredOnly | Public | Instance).Select(mi => mi.Name));
            OperationsFlags flags = None;

            if (methodNames.Contains("OnGetPathStatus")) { flags |= GetPathStatus; }
            if (methodNames.Contains("OnReadSymbolicLink")) { flags |= ReadSymbolicLink; }
            if (methodNames.Contains("OnCreateSpecialFile")) { flags |= CreateSpecialFile; }
            if (methodNames.Contains("OnCreateDirectory")) { flags |= CreateDirectory; }
            if (methodNames.Contains("OnRemoveFile")) { flags |= RemoveFile; }
            if (methodNames.Contains("OnRemoveDirectory")) { flags |= RemoveDirectory; }
            if (methodNames.Contains("OnCreateSymbolicLink")) { flags |= CreateSymbolicLink; }
            if (methodNames.Contains("OnRenamePath")) { flags |= RenamePath; }
            if (methodNames.Contains("OnCreateHardLink")) { flags |= CreateHardLink; }
            if (methodNames.Contains("OnChangePathPermissions")) { flags |= ChangePathPermissions; }
            if (methodNames.Contains("OnChangePathOwner")) { flags |= ChangePathOwner; }
            if (methodNames.Contains("OnTruncateFile")) { flags |= TruncateFile; }
            if (methodNames.Contains("OnChangePathTimes")) { flags |= ChangePathTimes; }
            if (methodNames.Contains("OnOpenHandle")) { flags |= OpenHandle; }
            if (methodNames.Contains("OnReadHandle")) { flags |= ReadHandle; }
            if (methodNames.Contains("OnWriteHandle")) { flags |= WriteHandle; }
            if (methodNames.Contains("OnGetFileSystemStatus")) { flags |= GetFileSystemStatus; }
            if (methodNames.Contains("OnFlushHandle")) { flags |= FlushHandle; }
            if (methodNames.Contains("OnReleaseHandle")) { flags |= ReleaseHandle; }
            if (methodNames.Contains("OnSynchronizeHandle")) { flags |= SynchronizeHandle; }
            if (methodNames.Contains("OnSetPathExtendedAttribute")) { flags |= SetPathExtendedAttribute; }
            if (methodNames.Contains("OnGetPathExtendedAttribute")) { flags |= GetPathExtendedAttribute; }
            if (methodNames.Contains("OnListPathExtendedAttributes")) { flags |= ListPathExtendedAttributes; }
            if (methodNames.Contains("OnRemovePathExtendedAttribute")) { flags |= RemovePathExtendedAttribute; }
            if (methodNames.Contains("OnOpenDirectory")) { flags |= OpenDirectory; }
            if (methodNames.Contains("OnReadDirectory")) { flags |= ReadDirectory; }
            if (methodNames.Contains("OnReleaseDirectory")) { flags |= ReleaseDirectory; }
            if (methodNames.Contains("OnSynchronizeDirectory")) { flags |= SynchronizeDirectory; }
            if (methodNames.Contains("OnInit")) { flags |= Init; }
            if (methodNames.Contains("OnDestroy")) { flags |= Destroy; }
            if (methodNames.Contains("OnAccessPath")) { flags |= AccessPath; }
            if (methodNames.Contains("OnCreateHandle")) { flags |= CreateHandle; }
            if (methodNames.Contains("OnTruncateHandle")) { flags |= TruncateHandle; }
            if (methodNames.Contains("OnGetHandleStatus")) { flags |= GetHandleStatus; }
            if (methodNames.Contains("OnLockHandle")) { flags |= LockHandle; }
            if (methodNames.Contains("OnMapPathLogicalToPhysicalIndex")) { flags |= MapPathLogicalToPhysicalIndex; }

            return flags;
        }
    }
}