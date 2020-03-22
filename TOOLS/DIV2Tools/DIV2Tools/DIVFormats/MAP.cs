﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ImageMagick;
using DIV2Tools.Helpers;
using DIV2Tools.MethodExtensions;

namespace DIV2Tools.DIVFormats
{
    /// <summary>
    /// MAP file.
    /// </summary>
    public class MAP
    {
        #region Structs
        /// <summary>
        /// Control Point value.
        /// </summary>
        public struct ControlPoint
        {
            #region Public vars
            public short x, y;
            #endregion

            #region Constructor
            public ControlPoint(BinaryReader file)
            {
                x = file.ReadInt16();
                y = file.ReadInt16();
            }
            #endregion

            #region Methods & Functions
            public void Write(BinaryWriter file)
            {
                file.Write(this.x);
                file.Write(this.y);
            }

            public override string ToString()
            {
                return $"[{x:0000}.{y:0000}]";
            }
            #endregion
        }
        #endregion

        #region Classes
        class Header : DIVFormatBaseHeader // 48 bytes.
        {
            #region Constants
            const string HEADER_ID = "map";

            public const int GRAPHID_MIN = 1;
            public const int GRAPHID_MAX = 999;
            public const int DESCRIPTION_LENGTH = 32;
            #endregion

            #region Public vars
            public short Width { get; set; }        // 1 word (2 bytes)
            public short Height { get; set; }       // 1 word (2 bytes)
            public int GraphId { get; set; }        // 1 doble word (4 bytes)
            public string Description { get; set; } // 32 bytes (ASCII)
            #endregion

            #region Constructor
            public Header() : base(Header.HEADER_ID)
            {
            }

            public Header(short width, short height, short graphId, string description) : base(Header.HEADER_ID)
            {
                this.Width = width;
                this.Height = height;
                this.GraphId = graphId;
                this.Description = description;
            }

            public Header(BinaryReader file) : base(Header.HEADER_ID, file)
            {
                this.Width = file.ReadInt16();
                this.Height = file.ReadInt16();
                this.GraphId = file.ReadInt32();
                this.Description = file.ReadBytes(Header.DESCRIPTION_LENGTH).GetNullTerminatedASCIIString();
            }
            #endregion

            #region Methods & Functions
            public override void Write(BinaryWriter file)
            {
                base.Write(file);
                file.Write(this.Width);
                file.Write(this.Height);
                file.Write(this.GraphId);
                file.Write(this.Description.GetASCIIBytes());
            }

            public override string ToString()
            {
                return $"{base.ToString()}\n- Width: {this.Width}\n- Height: {this.Height}\n- Graph Id: {this.GraphId}\n- Description: {this.Description}\n";
            }
            #endregion
        }

        public class ControlPointList
        {
            #region Internal vars
            List<ControlPoint> _points;
            #endregion

            #region Properties
            public int Length => this._points.Count;

            public ControlPoint this[int index]
            {
                get { return this._points[index]; }
                set { this._points[index] = value; }
            }
            #endregion

            #region Constructor
            public ControlPointList()
            {
                this._points = new List<ControlPoint>();
            }

            /// <summary>
            /// Read the control point array.
            /// </summary>
            /// <param name="file"><see cref="BinaryReader"/> instance of the MAP or FPG file format that contain control point array data.</param>
            /// <remarks>The <see cref="BinaryReader"/> instance must be setup in the byte of the control point counter value.</remarks>
            public ControlPointList(BinaryReader file)
            {
                this._points = new List<ControlPoint>(file.ReadInt16());

                for (int i = 0; i < this._points.Capacity; i++)
                {
                    this._points.Add(new ControlPoint(file));
                }
            }
            #endregion

            #region Methods & Functions
            public void Add(short x, short y)
            {
                this._points.Add(new ControlPoint() { x = x, y = y });
            }

            public void Remove(int index)
            {
                this._points.RemoveAt(index);
            }

            public void Write(BinaryWriter file)
            {
                file.Write((short)this._points.Count);

                foreach (var point in this._points)
                {
                    point.Write(file);
                }
            }

            public override string ToString()
            {
                if (this._points.Count == 0)
                {
                    return "The MAP not contain control points.";
                }

                var sb = new StringBuilder();
                sb.AppendLine($"Control points:");

                int column = 0;
                for (int i = 0; i < this._points.Count; i++)
                {
                    sb.Append($"{i:000}:{this._points[i].ToString()} ");
                    if (++column == 6)
                    {
                        column = 0;
                        sb.AppendLine();
                    }
                }

                sb.AppendLine();

                return sb.ToString();
            } 
            #endregion
        }

        /// <summary>
        /// Represents the all pixels from a MAP.
        /// </summary>
        public class Bitmap
        {
            #region Internal vars
            byte[] _pixels;
            #endregion

            #region Properties
            public byte this[int index]
            {
                get { return this._pixels[index]; }
                set { this._pixels[index] = value; }
            }

            public byte this[int x, int y]
            {
                get { return this._pixels[this.GetIndex(x, y)]; }
                set { this._pixels[this.GetIndex(x, y)] = value; }
            }

            public int Width { get; private set; }
            public int Height { get; private set; }
            public int Count => this._pixels.Length;
            #endregion

            #region Constructors
            public Bitmap(int width, int height)
            {
                this.Width = width;
                this.Height = height;
                this._pixels = new byte[width * height];
            }

            public Bitmap(int width, int height, byte[] pixels) : this(width, height)
            {
                if (this._pixels.Length == pixels.Length)
                {
                    this._pixels = pixels;
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(pixels), $"The {nameof(pixels)} array length not match with the Bitmap size ({this._pixels.Length}).");
                }
            }

            /// <summary>
            /// Reads all pixels from a MAP file.
            /// </summary>
            /// <param name="width">Width of the MAP.</param>
            /// <param name="height">Height of the MAP.</param>
            /// <param name="file"><see cref="BinaryReader"/> instance of the MAP or FPG file format that contain pixel array data.</param>
            /// <remarks>The <see cref="BinaryReader"/> instance must be setup in the byte of the first pixel.</remarks>
            public Bitmap(int width, int height, BinaryReader file) : this(width, height)
            {
                this._pixels = file.ReadBytes(this._pixels.Length);
            }
            #endregion

            #region Methods & Functions
            int GetIndex(int x, int y)
            {
                return (this.Width * y) + x;
            }

            public void Write(BinaryWriter file)
            {
                file.Write(this._pixels);
            } 

            public byte[] ToByteArray()
            {
                return this._pixels;
            }
            #endregion
        }
        #endregion

        #region Internal vars
        Header _header;
        PAL.ColorPalette _palette;
        PAL.ColorRangeTable _colorRanges;
        ControlPointList _controlPoints;
        Bitmap _pixels;
        #endregion

        #region Properties
        public short Width => this._header.Width;
        public short Height => this._header.Height;
        public int GraphId
        {
            get
            { 
                return this._header.GraphId; 
            }
            set
            {
                if (!value.IsClamped(Header.GRAPHID_MIN, Header.GRAPHID_MAX))
                {
                    throw new ArgumentOutOfRangeException("The GraphID must be a value between 1 and 999.");
                }

                this._header.GraphId = value;
            }
        }
        public string Description 
        { 
            get
            {
                return this._header.Description;
            }
            set
            {
                this._header.Description = value.GetFixedLengthString(Header.DESCRIPTION_LENGTH);
            }
        }
        public PAL.ColorPalette Palette => this._palette;
        public PAL.ColorRangeTable Ranges => this._colorRanges;
        public ControlPointList ControlPoints => this._controlPoints;
        public Bitmap Pixels => this._pixels;
        #endregion

        #region Constructor
        public MAP()
        {
            this._header = new Header();
            this._controlPoints = new ControlPointList();
        }

        /// <summary>
        /// Import a MAP file.
        /// </summary>
        /// <param name="filename">MAP file.</param>
        /// <param name="verbose">Log <see cref="MAP"/> import data in console. By default is <see cref="true"/>.</param>
        public MAP(string filename, bool verbose = true)
        {
            using (var file = new BinaryReader(File.OpenRead(filename)))
            {
                Helper.Log($"Reading \"{filename}\"...\n", verbose);

                this._header = new Header(file);
                Helper.Log(this._header.ToString(), verbose);

                if (this._header.Check())
                {
                    this._palette = new PAL.ColorPalette(file);
                    Helper.Log(this._palette.ToString(), verbose);

                    this._colorRanges = new PAL.ColorRangeTable(file);
                    Helper.Log(this._colorRanges.ToString(), verbose);

                    this._controlPoints = new ControlPointList(file);
                    Helper.Log(this._controlPoints.ToString(), verbose);

                    this._pixels = new Bitmap(this._header.Width, this._header.Height, file);
                    Helper.Log($"Readed {this._pixels.Count} pixels in MAP.", verbose);
                }
                else
                {
                    throw new FormatException("Invalid MAP file!");
                }
            }
        }
        #endregion

        #region Methods & Functions
        /// <summary>
        /// Import an external <see cref="PAL"/> file to use in this <see cref="MAP"/>.
        /// </summary>
        /// <param name="filename"><see cref="PAL"/> file to import.</param>
        public void ImportPalette(string filename)
        {
            var pal = new PAL(filename, false);
            this._palette = pal.Palette;
            this._colorRanges = pal.Ranges;
        }

        /// <summary>
        /// Convert PNG to <see cref="PCX"/> format in memory and import it as <see cref="MAP"/> format.
        /// </summary>
        /// <param name="pngfile">PNG file to convert to 256 color indexed <see cref="PCX"/> image.</param>
        public void ImportPNG(string pngfile)
        {
            // Load PNG image and convert to PCX using ImageMagick framework:
            using (MagickImage png = new MagickImage(pngfile))
            {
                png.ColorType = ColorType.Palette;
                png.Format = MagickFormat.Pcx;

                // Load the PCX image using custom importer and get color palette and uncompreseed pixel data:
                var pcx = new PCX(new MagickImage(png.ToByteArray()).ToByteArray(), false);
                {
                    this._header.Width = pcx.Width;
                    this._header.Height = pcx.Height;
                    this._palette = new PAL.ColorPalette(pcx);
                    this._colorRanges = new PAL.ColorRangeTable();
                    this._pixels = new Bitmap(this._header.Width, this._header.Height, pcx.Pixels);
                }
            }
        }

        /// <summary>
        /// Write all data in a file.
        /// </summary>
        /// <param name="filename"><see cref="MAP"/> filename.</param>
        public void Write(string filename)
        {
            using (var file = new BinaryWriter(File.OpenWrite(filename)))
            {
                this._header.Write(file);
                this._palette.Write(file);
                this._colorRanges.Write(file);
                this._controlPoints.Write(file);
                this._pixels.Write(file);
            }
        }

        public override string ToString()
        {
            return $"{this._header.ToString()}\n{this._palette.ToString()}\n{this._colorRanges.ToString()}\n{this._controlPoints.ToString()}\n{this._pixels.Count} pixels stored.";
        }
        #endregion
    }
}
