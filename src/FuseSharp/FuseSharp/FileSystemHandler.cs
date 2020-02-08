using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Mono.Unix.Native;

namespace FuseSharp
{
    using static OperationsFlags;

    public class FileSystemHandler : IDisposable
    {
        FileSystem _filesystem;
        private Operations _operations;
        private IntPtr _opPtr;
        private IntPtr _argsPtr;
        private int _argsCount;
        private string _mountPoint;

        public FileSystemHandler(FileSystem filesystem, String[] args, OperationsFlags specifiedFlags = None)
        {
            _filesystem = filesystem;

            var flags = specifiedFlags;
            if (flags == None)
            {
                flags = OperationsFlagsFactory.GetOperationsFlags(filesystem.GetType());
                flags = flags | Init;
            }
            InitOperations(flags);
            _opPtr = Operations.Allocate(_operations);

            _argsCount = args.Length;
            _argsPtr = Arguments.Allocate(args);

            _mountPoint = args[_argsCount - 1];
        }

        private void InitOperations(OperationsFlags opsFlags)
        {
            _operations = new Operations();
            {
                if ((opsFlags & GetPathStatus) != 0) { _operations.getattr = _OnGetPathStatus; } else { _operations.getattr = null; }
                if ((opsFlags & ReadSymbolicLink) != 0) { _operations.readlink = _OnReadSymbolicLink; } else { _operations.readlink = null; }
                if ((opsFlags & CreateSpecialFile) != 0) { _operations.mknod = _OnCreateSpecialFile; } else { _operations.mknod = null; }
                if ((opsFlags & CreateDirectory) != 0) { _operations.mkdir = _OnCreateDirectory; } else { _operations.mkdir = null; }
                if ((opsFlags & RemoveFile) != 0) { _operations.unlink = _OnRemoveFile; } else { _operations.unlink = null; }
                if ((opsFlags & RemoveDirectory) != 0) { _operations.rmdir = _OnRemoveDirectory; } else { _operations.rmdir = null; }
                if ((opsFlags & CreateSymbolicLink) != 0) { _operations.symlink = _OnCreateSymbolicLink; } else { _operations.symlink = null; }
                if ((opsFlags & RenamePath) != 0) { _operations.rename = _OnRenamePath; } else { _operations.rename = null; }
                if ((opsFlags & CreateHardLink) != 0) { _operations.link = _OnCreateHardLink; } else { _operations.link = null; }
                if ((opsFlags & ChangePathPermissions) != 0) { _operations.chmod = _OnChangePathPermissions; } else { _operations.chmod = null; }
                if ((opsFlags & ChangePathOwner) != 0) { _operations.chown = _OnChangePathOwner; } else { _operations.chown = null; }
                if ((opsFlags & TruncateFile) != 0) { _operations.truncate = _OnTruncateFile; } else { _operations.truncate = null; }
                if ((opsFlags & ChangePathTimes) != 0) { _operations.utime = _OnChangePathTimes; } else { _operations.utime = null; }
                if ((opsFlags & OpenHandle) != 0) { _operations.open = _OnOpenHandle; } else { _operations.open = null; }
                if ((opsFlags & ReadHandle) != 0) { _operations.read = _OnReadHandle; } else { _operations.read = null; }
                if ((opsFlags & WriteHandle) != 0) { _operations.write = _OnWriteHandle; } else { _operations.write = null; }
                if ((opsFlags & GetFileSystemStatus) != 0) { _operations.statfs = _OnGetFileSystemStatus; } else { _operations.statfs = null; }
                if ((opsFlags & FlushHandle) != 0) { _operations.flush = _OnFlushHandle; } else { _operations.flush = null; }
                if ((opsFlags & ReleaseHandle) != 0) { _operations.release = _OnReleaseHandle; } else { _operations.release = null; }
                if ((opsFlags & SynchronizeHandle) != 0) { _operations.fsync = _OnSynchronizeHandle; } else { _operations.fsync = null; }
                if ((opsFlags & SetPathExtendedAttribute) != 0) { _operations.setxattr = _OnSetPathExtendedAttribute; } else { _operations.setxattr = null; }
                if ((opsFlags & GetPathExtendedAttribute) != 0) { _operations.getxattr = _OnGetPathExtendedAttribute; } else { _operations.getxattr = null; }
                if ((opsFlags & ListPathExtendedAttributes) != 0) { _operations.listxattr = _OnListPathExtendedAttributes; } else { _operations.listxattr = null; }
                if ((opsFlags & RemovePathExtendedAttribute) != 0) { _operations.removexattr = _OnRemovePathExtendedAttribute; } else { _operations.removexattr = null; }
                if ((opsFlags & OpenDirectory) != 0) { _operations.opendir = _OnOpenDirectory; } else { _operations.opendir = null; }
                if ((opsFlags & ReadDirectory) != 0) { _operations.readdir = _OnReadDirectory; } else { _operations.readdir = null; }
                if ((opsFlags & ReleaseDirectory) != 0) { _operations.releasedir = _OnReleaseDirectory; } else { _operations.releasedir = null; }
                if ((opsFlags & SynchronizeDirectory) != 0) { _operations.fsyncdir = _OnSynchronizeDirectory; } else { _operations.fsyncdir = null; }
                if ((opsFlags & Init) != 0) { _operations.init = _OnInit; } else { _operations.init = null; }
                if ((opsFlags & Destroy) != 0) { _operations.destroy = _OnDestroy; } else { _operations.destroy = null; }
                if ((opsFlags & AccessPath) != 0) { _operations.access = _OnAccessPath; } else { _operations.access = null; }
                if ((opsFlags & CreateHandle) != 0) { _operations.create = _OnCreateHandle; } else { _operations.create = null; }
                if ((opsFlags & TruncateHandle) != 0) { _operations.ftruncate = _OnTruncateHandle; } else { _operations.ftruncate = null; }
                if ((opsFlags & GetHandleStatus) != 0) { _operations.fgetattr = _OnGetHandleStatus; } else { _operations.fgetattr = null; }
                if ((opsFlags & LockHandle) != 0) { _operations.@lock = _OnLockHandle; } else { _operations.@lock = null; }
                if ((opsFlags & MapPathLogicalToPhysicalIndex) != 0) { _operations.bmap = _OnMapPathLogicalToPhysicalIndex; } else { _operations.bmap = null; }
            };
        }

        public int Start()
        {
            return Process.Start(
                _argsPtr, _argsCount, _opPtr, Operations.SizeOf(_operations));
        }

        public void Dispose()
        {
            Process.Stop();
            Arguments.Free(_argsCount, _argsPtr);
            Operations.Free(_opPtr);
            _operations = null;
            GC.SuppressFinalize(this);
        }

        public string MountPoint
        {
            get { return _mountPoint; }
            set { _mountPoint = value; }
        }

        protected Context GetContext()
        {
            return Process.GetContext();
        }

        public static void ShowFuseHelp(string appname)
        {
            //mfh_show_fuse_help(appname);
        }

        private int _OnGetPathStatus(string path, IntPtr stat)
        {
            Trace.WriteLine($"_OnGetPathStatus {path} {stat}");

            Errno errno;
            try
            {
                Stat buf;
                Interop.CopyStat(stat, out buf);
                errno = _filesystem.OnGetPathStatus(path, out buf);
                if (errno == 0)
                    Interop.CopyStat(ref buf, stat);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
                errno = Errno.EIO;
            }
            return Interop.ConvertErrno(errno);
        }

        private int _OnReadSymbolicLink(string path, IntPtr buf, ulong bufsize)
        {
            Trace.WriteLine("_OnReadSymbolicLink");

            Errno errno;
            try
            {
                if (bufsize <= 1)
                    return Interop.ConvertErrno(Errno.EINVAL);
                string target;
                errno = _filesystem.OnReadSymbolicLink(path, out target);
                if (errno == 0 && target != null)
                {
                    byte[] b = Encoding.UTF8.GetBytes(target);
                    if ((bufsize - 1) < (ulong)b.Length)
                    {
                        errno = Errno.EINVAL;
                    }
                    else
                    {
                        Marshal.Copy(b, 0, buf, b.Length);
                        Marshal.WriteByte(buf, b.Length, (byte)0);
                    }
                }
                else if (errno == 0 && target == null)
                {
                    Trace.WriteLine("OnReadSymbolicLink: error: 0 return value but target is `null'");
                    errno = Errno.EIO;
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
                errno = Errno.EIO;
            }
            return Interop.ConvertErrno(errno);
        }

        private int _OnCreateSpecialFile(string path, uint perms, ulong dev)
        {
            Trace.WriteLine("_OnCreateSpecialFile");

            Errno errno;
            try
            {
                FilePermissions _perms = NativeConvert.ToFilePermissions(perms);
                errno = _filesystem.OnCreateSpecialFile(path, _perms, dev);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
                errno = Errno.EIO;
            }
            return Interop.ConvertErrno(errno);
        }

        private int _OnCreateDirectory(string path, uint mode)
        {
            Trace.WriteLine("_OnCreateDirectory");

            Errno errno;
            try
            {
                FilePermissions _mode = NativeConvert.ToFilePermissions(mode);
                errno = _filesystem.OnCreateDirectory(path, _mode);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
                errno = Errno.EIO;
            }
            return Interop.ConvertErrno(errno);
        }

        private int _OnRemoveFile(string path)
        {
            Trace.WriteLine("_OnRemoveFile");

            Errno errno;
            try
            {
                errno = _filesystem.OnRemoveFile(path);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
                errno = Errno.EIO;
            }
            return Interop.ConvertErrno(errno);
        }

        private int _OnRemoveDirectory(string path)
        {
            Trace.WriteLine("_OnRemoveDirectory");

            Errno errno;
            try
            {
                errno = _filesystem.OnRemoveDirectory(path);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
                errno = Errno.EIO;
            }
            return Interop.ConvertErrno(errno);
        }

        private int _OnCreateSymbolicLink(string oldpath, string newpath)
        {
            Trace.WriteLine("_OnCreateSymbolicLink");

            Errno errno;
            try
            {
                errno = _filesystem.OnCreateSymbolicLink(oldpath, newpath);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
                errno = Errno.EIO;
            }
            return Interop.ConvertErrno(errno);
        }

        private int _OnRenamePath(string oldpath, string newpath)
        {
            Trace.WriteLine("_OnRenamePath");

            Errno errno;
            try
            {
                errno = _filesystem.OnRenamePath(oldpath, newpath);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
                errno = Errno.EIO;
            }
            return Interop.ConvertErrno(errno);
        }

        private int _OnCreateHardLink(string oldpath, string newpath)
        {
            Trace.WriteLine("_OnCreateHardLink");

            Errno errno;
            try
            {
                errno = _filesystem.OnCreateHardLink(oldpath, newpath);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
                errno = Errno.EIO;
            }
            return Interop.ConvertErrno(errno);
        }

        private int _OnChangePathPermissions(string path, uint mode)
        {
            Trace.WriteLine("_OnChangePathPermissions");

            Errno errno;
            try
            {
                FilePermissions _mode = NativeConvert.ToFilePermissions(mode);
                errno = _filesystem.OnChangePathPermissions(path, _mode);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
                errno = Errno.EIO;
            }
            return Interop.ConvertErrno(errno);
        }

        private int _OnChangePathOwner(string path, long owner, long group)
        {
            Trace.WriteLine("_OnChangePathOwner");

            Errno errno;
            try
            {
                errno = _filesystem.OnChangePathOwner(path, owner, group);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
                errno = Errno.EIO;
            }
            return Interop.ConvertErrno(errno);
        }

        private int _OnTruncateFile(string path, long length)
        {
            Trace.WriteLine("_OnTruncateFile");

            Errno errno;
            try
            {
                errno = _filesystem.OnTruncateFile(path, length);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
                errno = Errno.EIO;
            }
            return Interop.ConvertErrno(errno);
        }

        private int _OnChangePathTimes(string path, IntPtr buf)
        {
            Trace.WriteLine("_OnChangePathTimes");

            Errno errno;
            try
            {
                Utimbuf b;
                Interop.CopyUtimbuf(buf, out b);
                errno = _filesystem.OnChangePathTimes(path, ref b);
                if (errno == 0)
                    Interop.CopyUtimbuf(ref b, buf);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
                errno = Errno.EIO;
            }
            return Interop.ConvertErrno(errno);
        }

        private int _OnOpenHandle(string path, IntPtr fi)
        {
            Trace.WriteLine("_OnOpenHandle");

            Errno errno;
            try
            {
                PathInfo info = new PathInfo();
                PathInfo.CopyFromPtr(fi, info);
                errno = _filesystem.OnOpenHandle(path, info);
                if (errno == 0)
                    PathInfo.CopyToPtr(info, fi);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
                errno = Errno.EIO;
            }
            return Interop.ConvertErrno(errno);
        }

        private int _OnReadHandle(
            string path,
            byte[] buf,
            ulong size,
            long offset,
            IntPtr fi,
            out int bytesWritten)
        {
            Errno errno;
            try
            {
                PathInfo info = new PathInfo();
                PathInfo.CopyFromPtr(fi, info);
                errno = _filesystem.OnReadHandle(path, info, buf, offset, out bytesWritten);
                if (errno == 0)
                    PathInfo.CopyToPtr(info, fi);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
                bytesWritten = 0;
                errno = Errno.EIO;
            }
            return Interop.ConvertErrno(errno);
        }

        private int _OnWriteHandle(
            string path,
            byte[] buf,
            ulong size,
            long offset,
            IntPtr fi,
            out int bytesRead)
        {
            Errno errno;
            try
            {
                PathInfo info = new PathInfo();
                PathInfo.CopyFromPtr(fi, info);
                errno = _filesystem.OnWriteHandle(path, info, buf, offset, out bytesRead);

                if (errno == 0)
                    PathInfo.CopyToPtr(info, fi);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
                bytesRead = 0;
                errno = Errno.EIO;
            }
            return Interop.ConvertErrno(errno);
        }

        private int _OnGetFileSystemStatus(string path, IntPtr buf)
        {
            Trace.WriteLine("_OnGetFileSystemStatus");

            Errno errno;
            try
            {
                Statvfs b;
                Interop.CopyStatvfs(buf, out b);
                errno = _filesystem.OnGetFileSystemStatus(path, out b);
                if (errno == 0)
                    Interop.CopyStatvfs(ref b, buf);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
                errno = Errno.EIO;
            }
            return Interop.ConvertErrno(errno);
        }

        private int _OnFlushHandle(string path, IntPtr fi)
        {
            Trace.WriteLine("_OnFlushHandle");

            Errno errno;
            try
            {
                PathInfo info = new PathInfo();
                PathInfo.CopyFromPtr(fi, info);
                errno = _filesystem.OnFlushHandle(path, info);
                PathInfo.CopyToPtr(info, fi);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
                errno = Errno.EIO;
            }
            return Interop.ConvertErrno(errno);
        }

        private int _OnReleaseHandle(string path, IntPtr fi)
        {
            Trace.WriteLine("_OnReleaseHandle");

            Errno errno;
            try
            {
                PathInfo info = new PathInfo();
                PathInfo.CopyFromPtr(fi, info);
                errno = _filesystem.OnReleaseHandle(path, info);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
                errno = Errno.EIO;
            }
            return Interop.ConvertErrno(errno);
        }

        private int _OnSynchronizeHandle(
            string path, bool onlyUserData, IntPtr fi)
        {
            Errno errno;
            try
            {
                PathInfo info = new PathInfo();
                PathInfo.CopyFromPtr(fi, info);
                errno = _filesystem.OnSynchronizeHandle(path, info, onlyUserData);
                if (errno == 0)
                    PathInfo.CopyToPtr(info, fi);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
                errno = Errno.EIO;
            }
            return Interop.ConvertErrno(errno);
        }

        private int _OnSetPathExtendedAttribute(
            string path, string name, byte[] value, ulong size, int flags)
        {
            Errno errno;
            try
            {
                XattrFlags f = NativeConvert.ToXattrFlags(flags);
                errno = _filesystem.OnSetPathExtendedAttribute(path, name, value, f);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
                errno = Errno.EIO;
            }
            return Interop.ConvertErrno(errno);
        }

        private int _OnGetPathExtendedAttribute(
            string path,
            string name,
            byte[] value,
            ulong size,
            out int bytesWritten)
        {
            Errno errno;
            try
            {
                errno = _filesystem.OnGetPathExtendedAttribute(
                    path, name, value, out bytesWritten);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
                bytesWritten = 0;
                errno = Errno.EIO;
            }
            return Interop.ConvertErrno(errno);
        }

        private int _OnListPathExtendedAttributes(
            string path, byte[] list, ulong size, out int bytesWritten)
        {
            Errno errno;
            try
            {
                bytesWritten = 0;
                string[] names;
                errno = _filesystem.OnListPathExtendedAttributes(path, out names);
                if (errno == 0 && names != null)
                {
                    int bytesNeeded = 0;
                    for (int i = 0; i < names.Length; ++i)
                    {
                        bytesNeeded += Encoding.UTF8.GetByteCount(names[i]) + 1;
                    }
                    if (size == 0)
                        bytesWritten = bytesNeeded;
                    if (size < (ulong)bytesNeeded)
                    {
                        errno = Errno.ERANGE;
                    }
                    else
                    {
                        int dest = 0;
                        for (int i = 0; i < names.Length; ++i)
                        {
                            int b = Encoding.UTF8.GetBytes(names[i], 0, names[i].Length,
                                list, dest);
                            list[dest + b] = (byte)
                            '\0';
                            dest += (b + 1);
                        }
                        bytesWritten = dest;
                    }
                }
                else
                    bytesWritten = 0;
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
                bytesWritten = 0;
                errno = Errno.EIO;
            }
            return Interop.ConvertErrno(errno);
        }

        private int _OnRemovePathExtendedAttribute(string path, string name)
        {
            Trace.WriteLine("_OnRemovePathExtendedAttribute");

            Errno errno;
            try
            {
                errno = _filesystem.OnRemovePathExtendedAttribute(path, name);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
                errno = Errno.EIO;
            }
            return Interop.ConvertErrno(errno);
        }

        private int _OnOpenDirectory(string path, IntPtr fi)
        {
            Trace.WriteLine("_OnOpenDirectory");

            Errno errno;
            try
            {
                PathInfo info = new PathInfo();
                PathInfo.CopyFromPtr(fi, info);
                errno = _filesystem.OnOpenDirectory(path, info);
                if (errno == 0)
                    PathInfo.CopyToPtr(info, fi);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
                errno = Errno.EIO;
            }
            return Interop.ConvertErrno(errno);
        }

        private object directoryLock = new object();

        private Dictionary<string, EntryEnumerator> directoryReaders =
            new Dictionary<string, EntryEnumerator>();

        private Random directoryKeys = new Random();

        private int _OnReadDirectory(string path, IntPtr buf, IntPtr filler,
            long offset, IntPtr fi, IntPtr stbuf)
        {
            Trace.WriteLine($"_OnReadDirectory {path}");

            Errno errno = 0;
            try
            {
                if (offset == 0)
                    GetDirectoryEnumerator(path, fi, out offset, out errno);
                if (errno != 0)
                    return Interop.ConvertErrno(errno);

                EntryEnumerator entries = null;
                lock (directoryLock)
                {
                    string key = offset.ToString();
                    if (directoryReaders.ContainsKey(key))
                        entries = directoryReaders[key];
                }

                // FUSE will invoke _OnReadDirectory at least twice, but if
                // there were very few entries then the enumerator will get
                // cleaned up during the first call, so this is (1) expected,
                // and (2) ignorable.
                if (entries == null)
                {
                    return 0;
                }

                bool cleanup = FillEntries(filler, buf, stbuf, offset, entries);

                if (cleanup)
                {
                    entries.Dispose();
                    lock (directoryLock)
                    {
                        directoryReaders.Remove(offset.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
                errno = Errno.EIO;
            }
            return Interop.ConvertErrno(errno);
        }

        private void GetDirectoryEnumerator(
            string path, IntPtr fi, out long offset, out Errno errno)
        {
            PathInfo info = new PathInfo();
            PathInfo.CopyFromPtr(fi, info);

            offset = -1;

            IEnumerable<DirectoryEntry> paths;
            errno = _filesystem.OnReadDirectory(path, info, out paths);
            if (errno != 0)
                return;
            if (paths == null)
            {
                Trace.WriteLine("OnReadDirectory: errno = 0 but paths is null!");
                errno = Errno.EIO;
                return;
            }
            IEnumerator<DirectoryEntry> e = paths.GetEnumerator();
            if (e == null)
            {
                Trace.WriteLine("OnReadDirectory: errno = 0 but enumerator is null!");
                errno = Errno.EIO;
                return;
            }
            int key;
            lock (directoryLock)
            {
                do
                {
                    key = directoryKeys.Next(1, int.MaxValue);
                } while (directoryReaders.ContainsKey(key.ToString()));
                directoryReaders[key.ToString()] = new EntryEnumerator(e);
            }

            PathInfo.CopyToPtr(info, fi);

            offset = key;
            errno = 0;
        }

        class EntryEnumerator : IEnumerator<DirectoryEntry>
        {
            private IEnumerator<DirectoryEntry> entries;
            bool repeat;

            public EntryEnumerator(IEnumerator<DirectoryEntry> entries)
            {
                this.entries = entries;
            }

            public DirectoryEntry Current
            {
                get { return entries.Current; }
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            public bool Repeat
            {
                set { repeat = value; }
            }

            public bool MoveNext()
            {
                if (repeat)
                {
                    repeat = false;
                    return true;
                }
                return entries.MoveNext();
            }

            public void Reset()
            {
                throw new InvalidOperationException();
            }

            public void Dispose()
            {
                entries.Dispose();
            }
        }

        private bool FillEntries(
            IntPtr filler,
            IntPtr buf,
            IntPtr stbuf,
            long offset,
            EntryEnumerator entries)
        {
            while (entries.MoveNext())
            {
                DirectoryEntry entry = entries.Current;
                IntPtr _stbuf = IntPtr.Zero;
                int r = Interop.adaptor_invoke_filler(filler, buf, entry.Name, _stbuf, offset);
                if (r != 0)
                {
                    entries.Repeat = true;
                    return false;
                }
            }
            return true;
        }

        private int _OnReleaseDirectory(string path, IntPtr fi)
        {
            Trace.WriteLine("_OnReleaseDirectory");

            Errno errno;
            try
            {
                PathInfo info = new PathInfo();
                PathInfo.CopyFromPtr(fi, info);
                errno = _filesystem.OnReleaseDirectory(path, info);
                if (errno == 0)
                    PathInfo.CopyToPtr(info, fi);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
                errno = Errno.EIO;
            }
            return Interop.ConvertErrno(errno);
        }

        private int _OnSynchronizeDirectory(
            string path, bool onlyUserData, IntPtr fi)
        {
            Errno errno;
            try
            {
                PathInfo info = new PathInfo();
                PathInfo.CopyFromPtr(fi, info);
                errno = _filesystem.OnSynchronizeDirectory(
                    path, info, onlyUserData);

                if (errno == 0)
                    PathInfo.CopyToPtr(info, fi);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
                errno = Errno.EIO;
            }
            return Interop.ConvertErrno(errno);
        }

        private IntPtr _OnInit(IntPtr conn)
        {
            Trace.WriteLine("_OnInit");

            try
            {
                _filesystem.OnInit(new ConnectionInfo(conn));
                return _opPtr;
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
                return IntPtr.Zero;
            }
        }

        private void _OnDestroy(IntPtr opPtr)
        {
            Trace.WriteLine("_OnDestroy");

            Debug.Assert(opPtr == _opPtr);

            Dispose();
        }

        private int _OnAccessPath(string path, int mode)
        {
            Trace.WriteLine("_OnAccessPath");

            Trace.WriteLine($"_OnAccessPath {path} {mode}");
            Errno errno;
            try
            {
                AccessModes _mode = NativeConvert.ToAccessModes(mode);
                errno = _filesystem.OnAccessPath(path, _mode);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
                errno = Errno.EIO;
            }
            return Interop.ConvertErrno(errno);
        }

        private int _OnCreateHandle(string path, uint mode, IntPtr fi)
        {
            Trace.WriteLine("_OnCreateHandle");

            Errno errno;
            try
            {
                PathInfo info = new PathInfo();
                PathInfo.CopyFromPtr(fi, info);
                FilePermissions _mode = NativeConvert.ToFilePermissions(mode);
                errno = _filesystem.OnCreateHandle(path, info, _mode);
                if (errno == 0)
                    PathInfo.CopyToPtr(info, fi);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
                errno = Errno.EIO;
            }
            return Interop.ConvertErrno(errno);
        }

        private int _OnTruncateHandle(string path, long length, IntPtr fi)
        {
            Trace.WriteLine("_OnTruncateHandle");

            Errno errno;
            try
            {
                PathInfo info = new PathInfo();
                PathInfo.CopyFromPtr(fi, info);
                errno = _filesystem.OnTruncateHandle(path, info, length);
                if (errno == 0)
                    PathInfo.CopyToPtr(info, fi);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
                errno = Errno.EIO;
            }
            return Interop.ConvertErrno(errno);
        }

        private int _OnGetHandleStatus(string path, IntPtr buf, IntPtr fi)
        {
            Trace.WriteLine("_OnGetHandleStatus");

            Errno errno;
            try
            {
                PathInfo info = new PathInfo();
                PathInfo.CopyFromPtr(fi, info);
                Stat b;
                Interop.CopyStat(buf, out b);
                errno = _filesystem.OnGetHandleStatus(path, info, out b);
                if (errno == 0)
                {
                    Interop.CopyStat(ref b, buf);
                    PathInfo.CopyToPtr(info, fi);
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
                errno = Errno.EIO;
            }
            return Interop.ConvertErrno(errno);
        }

        public int _OnLockHandle(string file, IntPtr fi, int cmd, IntPtr lockp)
        {
            Errno errno;
            try
            {
                PathInfo info = new PathInfo();
                PathInfo.CopyFromPtr(fi, info);
                FcntlCommand _cmd = NativeConvert.ToFcntlCommand(cmd);
                Flock @lock;
                Interop.CopyFlock(lockp, out @lock);
                errno = _filesystem.OnLockHandle(file, info, _cmd, ref @lock);
                if (errno == 0)
                {
                    Interop.CopyFlock(ref @lock, lockp);
                    PathInfo.CopyToPtr(info, fi);
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
                errno = Errno.EIO;
            }
            return Interop.ConvertErrno(errno);
        }

        private int _OnMapPathLogicalToPhysicalIndex(
            string path, ulong logical, out ulong physical)
        {
            Errno errno;
            try
            {
                errno = _filesystem.OnMapPathLogicalToPhysicalIndex(
                    path, logical, out physical);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
                physical = ulong.MaxValue;
                errno = Errno.EIO;
            }
            return Interop.ConvertErrno(errno);
        }
    }
}