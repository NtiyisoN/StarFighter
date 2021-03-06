﻿using DIV2.Format.Exporter;
using DIV2.Format.Exporter.MethodExtensions;
using System;
using System.IO;

namespace DIV2.Format.Importer
{
    class PCX
    {
        #region Constants
        const int HEADER_LENGTH = 128;

        const byte HEADER_SIGNATURE = 0x0A;
        const byte HEADER_MIN_VERSION = 0;
        const byte HEADER_MAX_VERSION = 5;
        const byte HEADER_UNCOMPRESSED = 0;
        const byte HEADER_RLE_ENCODED = 1;
        const byte HEADER_BPP_8 = 8;

        const int HEADER_BPP_POSITION = 3;
        const int HEADER_WIDTH_POSITION = 8;
        const int HEADER_HEIGHT_POSITION = 10;

        const int RLE_COUNTER_MASK = 0xC0; // Mask for check if bits 6 and 7 ar set (to check if is a counter byte).
        const int RLE_CLEAR_MASK = 0x3F; // Mask for clear bits 6 and 7 (to get the counter value).
        const int PALETTE_MARKER = 0x0C; // Marker of the 256 color palette at the end of image data.

        static readonly FormatException NOT_256_COLORS_EXCEPTION = new FormatException("The PCX image is not a 256 color image.");
        #endregion

        #region Constructor
        internal static bool IsPCX(byte[] buffer)
        {
            return buffer[0] == PCX.HEADER_SIGNATURE &&
                   buffer[1].IsClamped(PCX.HEADER_MIN_VERSION, PCX.HEADER_MAX_VERSION) &&
                   buffer[2].IsClamped(PCX.HEADER_UNCOMPRESSED, PCX.HEADER_RLE_ENCODED);
        }

        internal static bool IsPCX256(byte[] buffer)
        {
            return PCX.IsPCX(buffer) &&
                   buffer[3] == PCX.HEADER_BPP_8 &&
                   buffer[buffer.Length - PAL.COLOR_TABLE_LENGTH - 1] == PCX.PALETTE_MARKER;
        }

        internal static void Import(byte[] buffer, out short width, out short height, out byte[] bitmap, out PAL palette)
        {
            if (PCX.IsPCX256(buffer))
            {
                using (var file = new BinaryReader(new MemoryStream(buffer)))
                {
                    file.BaseStream.Position = PCX.HEADER_BPP_POSITION;

                    file.BaseStream.Position = PCX.HEADER_WIDTH_POSITION;
                    width = file.ReadInt16();

                    file.BaseStream.Position = PCX.HEADER_HEIGHT_POSITION;
                    height = file.ReadInt16();

                    // Lambda function to clear bits 6 and 7 in a byte value:
                    Func<byte, byte> clearBits = (arg) =>
                    {
                        // .NET bit operations works in Int32 values. A conversion is needed to work with bytes.
                        int i = arg;
                        return (byte)(i & PCX.RLE_CLEAR_MASK);
                    };

                    int imageSize = (int)(file.BaseStream.Length - (PCX.HEADER_LENGTH + (PAL.COLOR_TABLE_LENGTH + 1)));
                    byte value, write;
                    int index = 0;

                    // Read and decompress RLE image data:
                    bitmap = new byte[imageSize];

                    file.BaseStream.Position = PCX.HEADER_LENGTH;
                    for (int i = 0; i < imageSize; i++)
                    {
                        value = file.ReadByte();
                        if ((value & PCX.RLE_COUNTER_MASK) == PCX.RLE_COUNTER_MASK) // Checks if is a counter byte:
                        {
                            value = clearBits(value); // Clear bits 6 and 7 to get the counter value.
                            write = file.ReadByte(); // Next byte is the pixel value to write.
                            for (byte j = 0; j < value; j++) // Write n times the pixel value:
                            {
                                bitmap[index] = write;
                                index++;
                            }
                            i++;
                        }
                        else // Single pixel value:
                        {
                            bitmap[index] = value;
                            index++;
                        }
                    }

                    palette = PCX.CreatePalette(file);
                } 
            }
            else
            {
                throw PCX.NOT_256_COLORS_EXCEPTION;
            }
        }

        internal static PAL CreatePalette(BinaryReader file)
        {
            file.BaseStream.Position = file.BaseStream.Length - PAL.COLOR_TABLE_LENGTH - 1;

            if (file.ReadByte() == PCX.PALETTE_MARKER)
            {
                return PAL.CreatePalette(file.ReadBytes(PAL.COLOR_TABLE_LENGTH), true);
            }
            else
            {
                throw PCX.NOT_256_COLORS_EXCEPTION;
            }
        }
        #endregion
    }
}
