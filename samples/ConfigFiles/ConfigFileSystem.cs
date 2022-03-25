using System;
using System.Text;
using System.Collections.Generic;
using Tmds.Fuse;
using Tmds.Linux;
using static Tmds.Linux.LibC;

namespace ConfigFiles
{
    class ConfigFileSystem : FuseFileSystemBase
    {
        private static readonly byte[] _helloFilePath = Encoding.UTF8.GetBytes("/hello.txt");
        private static readonly byte[] _helloFileContent = Encoding.UTF8.GetBytes("hello world!/n");

        public override bool SupportsMultiThreading => true;

        public Dictionary<string, ConfigFile> _files = new();

        public void AddFile(string fileName, string sourceFilePath)
        {
            _files.Add(fileName, new ConfigFile(fileName, sourceFilePath));
        }

        public override int GetAttr(ReadOnlySpan<byte> path, ref stat stat, FuseFileInfoRef fiRef)
        {
            if (path.SequenceEqual(RootPath))
            {
                stat.st_mode = S_IFDIR | 0b111_101_101; // rwxr-xr-x
                stat.st_nlink = 2; // 2 + nr of subdirectories
                return 0;
            }

            foreach(var file in _files.Values)
            {
                if (path.SequenceEqual(file.Path))
                {
                    stat.st_mode = S_IFREG | 0b100_100_100; // r--r--r--
                    stat.st_nlink = 1;
                    stat.st_size = file.Size;
                    return 0;
                }
            }

            return -ENOENT;
        }

        public override int Open(ReadOnlySpan<byte> path, ref FuseFileInfo fi)
        {
            foreach(var file in _files.Values)
            {
                if (path.SequenceEqual(file.Path))
                {
                    if (Environment.UserName != "mivansch")
                    {
                        return -EACCES; // Error Access: permission denied
                    }
                    if ((fi.flags & O_ACCMODE) != O_RDONLY)
                    {
                        return -EACCES; // Error Access: permission denied
                    }
                    return 0;
                }
            }

            return -ENOENT; // Error NO ENTry: no such file or directory
        }

        public override int Read(ReadOnlySpan<byte> path, ulong offset, Span<byte> buffer, ref FuseFileInfo fi)
        {
            foreach(var file in _files.Values)
            {
                if (path.SequenceEqual(file.Path))
                {
                    if (offset <= (ulong)file.Size)
                    {
                        int intOffset = (int)offset;
                        int length = (int)Math.Min(file.Size - intOffset, buffer.Length);
                        file.Content.AsSpan().Slice(intOffset, length).CopyTo(buffer);
                        return length;
                    }
                    return 0;
                }
            }
            return 0;
        }

        public override int ReadDir(ReadOnlySpan<byte> path, ulong offset, ReadDirFlags flags, DirectoryContent content, ref FuseFileInfo fi)
        {
            if (path.SequenceEqual(RootPath))
            {
                content.AddEntry(".");
                content.AddEntry("..");
                foreach(var file in _files.Values)
                {
                    content.AddEntry(file.Name);
                }
                return 0;
            }
            return -ENOENT; // Error NO ENTry: no such file or directory
        }
    }
}