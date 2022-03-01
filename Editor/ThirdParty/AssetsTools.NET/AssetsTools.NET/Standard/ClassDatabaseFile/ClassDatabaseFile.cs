using AssetsTools.NET.Extra;
using AssetsTools.NET.Extra.Decompressors.LZ4;
using SevenZip.Compression.LZMA;
using System;
using System.Collections.Generic;
using System.IO;

namespace AssetsTools.NET
{
    public class ClassDatabaseFile
    {
        public bool valid;
        public ClassDatabaseFileHeader header;

        public List<ClassDatabaseType> classes;

        public byte[] stringTable;

        public bool bodyParsed;
        public byte[] unparsedData;

        public bool ReadHeader(AssetsFileReader reader)
        {
            header = new ClassDatabaseFileHeader();
            header.Read(reader);
            if (header.header != "cldb" || header.fileVersion > 4 || header.fileVersion < 1)
            {
                return valid = false;
            }

            unparsedData = reader.ReadBytes((int)header.compressedSize);
            
            return valid = true;
        }
        public void ParseBody()
        {
            if (bodyParsed)
            {
                return;
            }
            classes = new List<ClassDatabaseType>();

            MemoryStream stream;
            if (header.compressionType == 0)
            {
                stream = new MemoryStream(unparsedData);
            }
            else if (header.compressionType == 1) //lz4
            {
                byte[] uncompressedBytes = new byte[header.uncompressedSize];
                using (MemoryStream tempMs = new MemoryStream(unparsedData))
                {
                    var decoder = new Lz4DecoderStream(tempMs);
                    decoder.Read(uncompressedBytes, 0, (int)header.uncompressedSize);
                    decoder.Dispose();
                }
                stream = new MemoryStream(uncompressedBytes);
            }
            else if (header.compressionType == 2) //lzma
            {
                using (MemoryStream tempMs = new MemoryStream(unparsedData))
                {
                    stream = SevenZipHelper.StreamDecompress(tempMs, header.uncompressedSize);
                }
            }
            else
            {
                throw new NotSupportedException();
            }
            using (var reader = new AssetsFileReader(stream))
            {
                reader.bigEndian = false;

                reader.Position = header.stringTablePos;
                stringTable = reader.ReadBytes((int)header.stringTableLen);
                reader.Position = 0;
                var size = reader.ReadUInt32();
                for (int i = 0; i < size; i++)
                {
                    var cdt = new ClassDatabaseType();
                    cdt.Read(reader, header.fileVersion, header.flags);
                    classes.Add(cdt);
                }
            }

            unparsedData = null;
            bodyParsed = true;
            stream.Dispose();
        }

        public void Write(AssetsFileWriter writer)
        {
            if (!valid)
            {
                throw new InvalidDataException("cldb is not valid");
            }

            if (!bodyParsed)
            {
                header.Write(writer);
                writer.Write(unparsedData);
                return;
            }

            var stringTablePos = 0L;
            var uncompressedSize = 0L;
            var compressedBytes = Net35Polyfill.ArrayEmpty<byte>();

            using (var uncompressedStream = new MemoryStream())
            using (var uncompressedWriter = new AssetsFileWriter(uncompressedStream))
            {
                uncompressedWriter.bigEndian = writer.bigEndian;
                uncompressedWriter.Write(classes.Count);
                for (int i = 0; i < classes.Count; i++)
                {
                    classes[i].Write(uncompressedWriter, header.fileVersion, header.flags);
                }
                stringTablePos = uncompressedWriter.Position;
                uncompressedWriter.Write(stringTable);
                uncompressedSize = uncompressedWriter.Position;
                switch (header.compressionType)
                {
                    case 0:
                        compressedBytes = uncompressedStream.ToArray();
                        break;
                    case 2:
                        using (var compressedStream = new MemoryStream())
                        {
                            uncompressedStream.Position = 0;
                            SevenZipHelper.Compress(uncompressedStream, compressedStream);
                            compressedBytes = compressedStream.ToArray();
                        }
                        break;
                    default:
                        throw new NotSupportedException("Only lzma (2) compression is supported, or no compression at all (0)");
                }
            }
            
            header.stringTablePos = (uint)stringTablePos;
            header.stringTableLen = (uint)(uncompressedSize - stringTablePos);
            header.uncompressedSize = (uint)uncompressedSize;
            header.compressedSize = (uint)compressedBytes.Length;

            header.Write(writer);
            writer.Write(compressedBytes);
        }

        public bool IsValid()
        {
            return valid;
        }
        
        public ClassDatabaseFile() { }
    }
}
