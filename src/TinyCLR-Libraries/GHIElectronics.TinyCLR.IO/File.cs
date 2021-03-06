namespace System.IO {
    // Class for creating FileStream objects, and some basic file management
    // routines such as Delete, etc.
    public static class File
    {
        // Copies an existing file to a new file. An exception is raised if the
        // destination file already exists. Use the
        // Copy(String, String, boolean) method to allow
        // overwriting an existing file.
        //
        // The caller must have certain FileIOPermissions.  The caller must have
        // Read permission to sourceFileName and Create
        // and Write permissions to destFileName.
        //
        public static void Copy(string sourceFileName, string destFileName) => Copy(sourceFileName, destFileName, false, false);

        // Copies an existing file to a new file. If overwrite is
        // false, then an IOException is thrown if the destination file
        // already exists.  If overwrite is true, the file is
        // overwritten.
        //
        // The caller must have certain FileIOPermissions.  The caller must have
        // Read permission to sourceFileName
        // and Write permissions to destFileName.
        //
        public static void Copy(string sourceFileName, string destFileName, bool overwrite) => Copy(sourceFileName, destFileName, overwrite, false);

        private const int _defaultCopyBufferSize = 2048; /// Experiment on desktop shows 2k-4k is ideal size perfwise.

        internal static void Copy(string sourceFileName, string destFileName, bool overwrite, bool deleteOriginal)
        {
            // sourceFileName and destFileName validation in Path.GetFullPath()

            sourceFileName = Path.GetFullPath(sourceFileName);
            destFileName = Path.GetFullPath(destFileName);

            var writerMode = (overwrite) ? FileMode.Create : FileMode.CreateNew;

            var reader = new FileStream(sourceFileName, FileMode.Open, FileAccess.Read, FileShare.Read, FileStream.BufferSizeDefault);

            try
            {
                using (var writer = new FileStream(destFileName, writerMode, FileAccess.Write, FileShare.None, FileStream.BufferSizeDefault))
                {
                    var fileLength = reader.Length;
                    writer.SetLength(fileLength);

                    var buffer = new byte[_defaultCopyBufferSize];
                    for (; ; )
                    {
                        var readSize = reader.Read(buffer, 0, _defaultCopyBufferSize);
                        if (readSize <= 0)
                            break;

                        writer.Write(buffer, 0, readSize);
                    }

                    // Copy the attributes too
                    DriveInfo.GetForPath(destFileName).SetAttributes(destFileName, DriveInfo.GetForPath(sourceFileName).GetAttributes(sourceFileName));
                }
            }
            finally
            {
                if (deleteOriginal)
                {
                    reader.DisposeAndDelete();
                }
                else
                {
                    reader.Dispose();
                }
            }
        }

        // Creates a file in a particular path.  If the file exists, it is replaced.
        // The file is opened with ReadWrite accessand cannot be opened by another
        // application until it has been closed.  An IOException is thrown if the
        // directory specified doesn't exist.
        //
        // Your application must have Create, Read, and Write permissions to
        // the file.
        //
        public static FileStream Create(string path) => new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None, FileStream.BufferSizeDefault);

        // Creates a file in a particular path.  If the file exists, it is replaced.
        // The file is opened with ReadWrite access and cannot be opened by another
        // application until it has been closed.  An IOException is thrown if the
        // directory specified doesn't exist.
        //
        // Your application must have Create, Read, and Write permissions to
        // the file.
        //
        public static FileStream Create(string path, int bufferSize) => new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None, bufferSize);

        // Deletes a file. The file specified by the designated path is deleted.
        // If the file does not exist, Delete succeeds without throwing
        // an exception.
        //
        // On NT, Delete will fail for a file that is open for normal I/O
        // or a file that is memory mapped.  On Win95, the file will be
        // deleted irregardless of whether the file is being used.
        //
        // Your application must have Delete permission to the target file.
        //
        public static void Delete(string path)
        {
            // path validation in Path.GetFullPath()

            path = Path.GetFullPath(path);
            var folderPath = Path.GetDirectoryName(path);

            // We have to make sure no one else has the file opened, and no one else can modify it when we're deleting
            var record = FileSystemManager.AddToOpenList(path);

            try
            {
                var attributes = DriveInfo.GetForPath(path).GetAttributes(folderPath);
                /// If the folder does not exist or invalid we throw DirNotFound Exception (same as desktop).
                if ((uint)attributes == 0xFFFFFFFF)
                {
                    throw new IOException("", (int)IOException.IOExceptionErrorCode.DirectoryNotFound);
                }

                /// Folder exists, lets verify whether the file itself exists.
                attributes = DriveInfo.GetForPath(path).GetAttributes(path);
                if ((uint)attributes == 0xFFFFFFFF)
                {
                    // No-op on file not found
                    return;
                }

                if ((attributes & (FileAttributes.Directory | FileAttributes.ReadOnly)) != 0)
                {
                    /// it's a readonly file or an directory
                    throw new IOException("", (int)IOException.IOExceptionErrorCode.UnauthorizedAccess);
                }

                DriveInfo.GetForPath(path).Delete(path);
            }
            finally
            {
                // regardless of what happened, we need to release the file when we're done
                FileSystemManager.RemoveFromOpenList(record);
            }
        }

        // Tests if a file exists. The result is true if the file
        // given by the specified path exists; otherwise, the result is
        // false.  Note that if path describes a directory,
        // Exists will return true.
        //
        // Your application must have Read permission for the target directory.
        //
        public static bool Exists(string path)
        {
            try
            {

                // path validation in Path.GetFullPath()

                path = Path.GetFullPath(path);

                /// Is this the absolute root? this is not a file.
                var root = Path.GetPathRoot(path);
                if (string.Equals(root, path))
                {
                    return false;
                }
                else
                {
                    var attributes = DriveInfo.GetForPath(path).GetAttributes(path);

                    /// This is essentially file not found.
                    if ((uint)attributes == 0xFFFFFFFF)
                        return false;

                    if ((attributes & FileAttributes.Directory) == 0)
                    {
                        /// Not a directory, it must be a file.
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                /// Like desktop, exists here does not throw exception in
                /// a number of cases, instead returns false. For more
                /// details see MSDN.
            }

            return false;
        }

        public static FileStream Open(string path, FileMode mode) => new FileStream(path, mode, (mode == FileMode.Append ? FileAccess.Write : FileAccess.ReadWrite), FileShare.None, FileStream.BufferSizeDefault);

        public static FileStream Open(string path, FileMode mode, FileAccess access) => new FileStream(path, mode, access, FileShare.None, FileStream.BufferSizeDefault);

        public static FileStream Open(string path, FileMode mode, FileAccess access, FileShare share) => new FileStream(path, mode, access, share, FileStream.BufferSizeDefault);

        public static FileAttributes GetAttributes(string path)
        {
            // path validation in Path.GetFullPath()

            var fullPath = Path.GetFullPath(path);

            var attributes = DriveInfo.GetForPath(fullPath).GetAttributes(fullPath);
            if ((uint)attributes == 0xFFFFFFFF)
                throw new IOException("", (int)IOException.IOExceptionErrorCode.FileNotFound);
            else if (attributes == 0x0)
                return FileAttributes.Normal;
            else
                return (FileAttributes)attributes;
        }

        public static void SetAttributes(string path, FileAttributes fileAttributes)
        {
            // path validation in Path.GetFullPath()

            var fullPath = Path.GetFullPath(path);

            DriveInfo.GetForPath(fullPath).SetAttributes(fullPath, fileAttributes);
        }

        public static FileStream OpenRead(string path) => new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, FileStream.BufferSizeDefault);

        public static FileStream OpenWrite(string path) => new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, FileStream.BufferSizeDefault);

        public static byte[] ReadAllBytes(string path)
        {
            byte[] bytes;
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, FileStream.BufferSizeDefault))
            {
                // Do a blocking read
                var index = 0;
                var fileLength = fs.Length;
                if (fileLength > int.MaxValue)
                    throw new IOException();
                var count = (int)fileLength;
                bytes = new byte[count];
                while (count > 0)
                {
                    var n = fs.Read(bytes, index, count);
                    index += n;
                    count -= n;
                }
            }

            return bytes;
        }

        public static void WriteAllBytes(string path, byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException("bytes");

            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read, FileStream.BufferSizeDefault))
                fs.Write(bytes, 0, bytes.Length);
        }

        // Moves a specified file to a new location and potentially a new file name.
        // This method does work across volumes.
        //
        // The caller must have certain FileIOPermissions.  The caller must
        // have Read and Write permission to
        // sourceFileName and Write
        // permissions to destFileName.
        //
        public static void Move(string sourceFileName, string destFileName)
        {
            if (Path.GetPathRoot(sourceFileName) != Path.GetPathRoot(destFileName)) throw new ArgumentException();
            // sourceFileName and destFileName validation in Path.GetFullPath()

            sourceFileName = Path.GetFullPath(sourceFileName);
            destFileName = Path.GetFullPath(destFileName);

            var tryCopyAndDelete = false;

            // We only need to lock the source, not the dest because if dest is taken
            // Move() will failed at the driver's level anyway. (there will be no conflict even if
            // another thread is creating dest, as only one of the operations will succeed --
            // the native calls are atomic)
            var srcRecord = FileSystemManager.AddToOpenList(sourceFileName);

            try
            {
                if (!Exists(sourceFileName))
                {
                    throw new IOException("", (int)IOException.IOExceptionErrorCode.FileNotFound);
                }

                //We'll try copy and deleting if Move returns false
                tryCopyAndDelete = !DriveInfo.GetForPath(sourceFileName).Move(sourceFileName, destFileName);
            }
            finally
            {
                FileSystemManager.RemoveFromOpenList(srcRecord);
            }

            if (tryCopyAndDelete)
            {
                Copy(sourceFileName, destFileName, false, true);
            }
        }
    }
}


