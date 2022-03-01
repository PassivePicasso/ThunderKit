using System;
using System.Collections.Generic;
using System.Text;

namespace AssetsTools.NET
{
    public struct ClassDatabasePackageHeader
    {
        public string header;
        public byte fileVersion;
        public byte compressionType;
        public uint stringTableOffset, stringTableLenUncompressed, stringTableLenCompressed;
        public uint fileBlockSize;
        public uint fileCount;
        public List<ClassDatabaseFileRef> files;

        public void Read(AssetsFileReader reader)
        {
            reader.bigEndian = false;
            header = reader.ReadStringLength(4);
            if (header != "CLPK")
            {
                throw new Exception("header not detected. is this a tpk file?");
            }
            fileVersion = reader.ReadByte();
            compressionType = reader.ReadByte();
            stringTableOffset = reader.ReadUInt32();
            stringTableLenUncompressed = reader.ReadUInt32();
            stringTableLenCompressed = reader.ReadUInt32();
            if (fileVersion == 1)
                fileBlockSize = reader.ReadUInt32();
            else
                fileBlockSize = 0;
            fileCount = reader.ReadUInt32();
            files = new List<ClassDatabaseFileRef>();
            for (int i = 0; i < fileCount; i++)
            {
                files.Add(new ClassDatabaseFileRef()
                {
                    offset = reader.ReadUInt32(),
                    length = reader.ReadUInt32(),
                    name = reader.ReadStringLength(15)
                });
            }
        }

        public void Write(AssetsFileWriter writer)
        {
            writer.bigEndian = false;
            writer.Write(Encoding.ASCII.GetBytes(header), 0, 4);
            writer.Write(fileVersion);
            writer.Write(compressionType);
            writer.Write(stringTableOffset);
            writer.Write(stringTableLenUncompressed);
            writer.Write(stringTableLenCompressed);
            if (fileVersion == 1)
            {
                writer.Write(fileBlockSize);
            }
            writer.Write(fileCount);
            for (int i = 0; i < fileCount; i++)
            {
                var file = files[i];
                writer.Write(file.offset);
                writer.Write(file.length);
                writer.Write(file.name.Substring(0, Math.Min(file.name.Length, 15)).PadRight(15, '\0'));
            }
        }
    }

    public struct ClassDatabaseFileRef
    {
        public uint offset;
        public uint length;
        public string name;
    }
}
