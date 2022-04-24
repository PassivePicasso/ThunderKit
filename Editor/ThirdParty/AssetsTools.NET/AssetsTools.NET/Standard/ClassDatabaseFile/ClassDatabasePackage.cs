using AssetsTools.NET.Extra;
using AssetsTools.NET.Extra.Decompressors.LZ4;
using SevenZip.Compression.LZMA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssetsTools.NET
{
    public class ClassDatabasePackage
    {
        public bool valid;

        public ClassDatabasePackageHeader header;
        public ClassDatabaseFile[] files;
        public byte[] stringTable;

        public bool Read(AssetsFileReader reader)
        {
            header = new ClassDatabasePackageHeader();
            header.Read(reader);
            files = new ClassDatabaseFile[header.fileCount];
            long firstFile = reader.Position;
            AssetsFileReader newReader = reader;
            if ((header.compressionType & 0x80) == 0) //multiple blocks
            {
                //untested!
                //the compression is handled by the cldbs themselves
                for (int i = 0; i < header.fileCount; i++)
                {
                    newReader.Position = firstFile + header.files[i].offset;
                    byte[] data = newReader.ReadBytes((int)header.files[i].length);
                    using (MemoryStream ms = new MemoryStream(data))
                    using (AssetsFileReader r = new AssetsFileReader(ms))
                    {
                        files[i] = new ClassDatabaseFile();
                        files[i].ReadHeader(r);
                    }
                }
            }
            else
            {
                if ((header.compressionType & 0x20) == 0) //not uncompressed
                {
                    firstFile = 0;
                    int compressedSize = (int)(header.stringTableOffset - newReader.Position);
                    int uncompressedSize = (int)header.fileBlockSize;
                    MemoryStream ms;
                    if ((header.compressionType & 0x1f) == 1) //lz4
                    {
                        byte[] uncompressedBytes = new byte[uncompressedSize];
                        using (MemoryStream tempMs = new MemoryStream(newReader.ReadBytes(compressedSize)))
                        {
                            Lz4DecoderStream decoder = new Lz4DecoderStream(tempMs);
                            decoder.Read(uncompressedBytes, 0, uncompressedSize);
                            decoder.Dispose();
                        }
                        ms = new MemoryStream(uncompressedBytes);
                    }
                    else if ((header.compressionType & 0x1f) == 2) //lzma
                    {
                        byte[] dbg = newReader.ReadBytes(compressedSize);
                        using (MemoryStream tempMs = new MemoryStream(dbg))
                        {
                            ms = SevenZipHelper.StreamDecompress(tempMs, uncompressedSize);
                        }
                    }
                    else
                    {
                        valid = false;
                        return valid;
                    }

                    newReader = new AssetsFileReader(ms);
                    newReader.bigEndian = false;
                }
                for (int i = 0; i < header.fileCount; i++)
                {
                    newReader.Position = firstFile + header.files[i].offset;
                    byte[] data = newReader.ReadBytes((int)header.files[i].length);
                    using (MemoryStream ms = new MemoryStream(data))
                    using (AssetsFileReader r = new AssetsFileReader(ms))
                    {
                        files[i] = new ClassDatabaseFile();
                        files[i].ReadHeader(r);
                    }
                }
            }

            newReader = reader;
            newReader.Position = header.stringTableOffset;
            if ((header.compressionType & 0x40) == 0) //string table is compressed
            {
                if ((header.compressionType & 0x20) == 0) //not uncompressed
                {
                    int compressedSize = (int)header.stringTableLenCompressed;
                    int uncompressedSize = (int)header.stringTableLenUncompressed;
                    MemoryStream ms;
                    if ((header.compressionType & 0x1f) == 1) //lz4
                    {
                        byte[] uncompressedBytes = new byte[uncompressedSize];
                        using (MemoryStream tempMs = new MemoryStream(newReader.ReadBytes(compressedSize)))
                        {
                            Lz4DecoderStream decoder = new Lz4DecoderStream(tempMs);
                            decoder.Read(uncompressedBytes, 0, uncompressedSize);
                            decoder.Dispose();
                        }
                        ms = new MemoryStream(uncompressedBytes);
                    }
                    else if ((header.compressionType & 0x1f) == 2) //lzma
                    {
                        using (MemoryStream tempMs = new MemoryStream(newReader.ReadBytes(compressedSize)))
                        {
                            ms = SevenZipHelper.StreamDecompress(tempMs, uncompressedSize);
                        }
                    }
                    else
                    {
                        valid = false;
                        return valid;
                    }

                    newReader = new AssetsFileReader(ms);
                    newReader.bigEndian = false;
                }
            }
            stringTable = newReader.ReadBytes((int)header.stringTableLenUncompressed);

            valid = true;
            return valid;
        }

        public void Write(AssetsFileWriter writer)
        {
            if (!valid)
            {
                throw new InvalidDataException("cldb is not valid");
            }

            var uncompressedFilesSize = 0L;
            var compressedBytes = Net35Polyfill.ArrayEmpty<byte>();

            using (var uncompressedStream = new MemoryStream())
            using (var uncompressedWriter = new AssetsFileWriter(uncompressedStream))
            {
                uncompressedWriter.bigEndian = writer.bigEndian;
                //the compression is handled by the cldbs themselves
                for (int i = 0; i < header.fileCount; i++)
                {
                    var file = header.files[i];
                    file.offset = (uint)uncompressedWriter.Position;
                    using (var tempStream = new MemoryStream())
                    using (var tempWriter = new AssetsFileWriter(tempStream))
                    {
                        tempWriter.bigEndian = writer.bigEndian;
                        files[i].Write(tempWriter);
                        uncompressedWriter.Write(tempStream.ToArray());
                    }
                    file.length = (uint)(uncompressedWriter.Position - file.offset);
                    header.files[i] = file;
                }
                uncompressedFilesSize = uncompressedWriter.Position;

                if ((header.compressionType & 0x20) != 0)
                {
                    compressedBytes = uncompressedStream.ToArray();
                }
                else if ((header.compressionType & 0x1f) == 2) //lzma
                {
                    using (var compressedStream = new MemoryStream())
                    {
                        uncompressedStream.Position = 0;
                        SevenZipHelper.Compress(uncompressedStream, compressedStream);
                        compressedBytes = compressedStream.ToArray();
                    }
                }
                else
                {
                    throw new NotSupportedException("Only lzma compression is supported");
                }
            }

            header.Write(writer);
            writer.Write(compressedBytes);

            if ((header.compressionType & 0x20) != 0) //not uncompressed
            {
                compressedBytes = stringTable;
            }
            else
            {
                if ((header.compressionType & 0x1f) == 2) //lzma
                {
                    using (var uncompressedStream = new MemoryStream(stringTable))
                    using (var compressedStream = new MemoryStream())
                    {
                        uncompressedStream.Position = 0;
                        SevenZipHelper.Compress(uncompressedStream, compressedStream);
                        compressedBytes = compressedStream.ToArray();
                    }
                }
                else
                {
                    throw new NotSupportedException("Only lzma compression is supported");
                }
            }

            header.fileBlockSize = (uint)uncompressedFilesSize;
            header.stringTableLenUncompressed = (uint)stringTable.Length;
            header.stringTableOffset = (uint)writer.Position;
            header.stringTableLenCompressed = (uint)compressedBytes.Length;

            //compressedBytes is reused for stringTable
            writer.Write(compressedBytes);

            writer.Position = 0;
            header.Write(writer);
        }
    }
}
