using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TRPExtractor {
    static class Program {
        /// <summary>
        /// 應用程式的主要進入點。
        /// </summary>
        [STAThread]
        static void Main( string[] args ) {
            if (args.Length != 1) {
                PrintHelp();
                return;
            }

            BigEndianBinaryReader bebr = null;
            try {
                bebr = new BigEndianBinaryReader( new FileStream(args[0], FileMode.Open) );
            } catch (Exception ex) {
                Console.WriteLine("Error : {0}", ex.Message);
                return;
            }
            Header header = bebr.ReadBytes(Marshal.SizeOf(typeof(Header))).ToStruct<Header>();

            if (header.magic != 0x01000000004DA2DC) {
                Console.WriteLine("Not a valid TRP file.");
                return;
            }
            FileOffsetInfo[] fileOffsetInfoArray = new FileOffsetInfo[header.AllFileCount];
            for (int i = 0; i < header.AllFileCount; i++) {
                fileOffsetInfoArray[i] = bebr.ReadBytes(Marshal.SizeOf(typeof(FileOffsetInfo))).ToStruct<FileOffsetInfo>();
            }

            foreach (FileOffsetInfo foi in fileOffsetInfoArray) {
                bebr.BaseStream.Position = foi.Offset;
                FileStream fs = new FileStream(foi.fileName, FileMode.Create);
                byte [] towritefiledata = bebr.ReadBytes( (int) foi.FileSize );
                fs.Write(towritefiledata, 0, towritefiledata.Length);
                fs.Flush();
                fs.Close();
                Console.WriteLine(foi.fileName);
            }


            Console.WriteLine("Extract finished");
            bebr.Close();
            return;
        }
        public static void PrintHelp() {
            Console.WriteLine("usage: trpextractor trpfile");
        }

        #region Structs
        [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct Header {

            /// int
            public ulong magic;

            /// int
            public ulong _fileSize;

            /// int
            private int u1;
            public int AllFileCount {
                get {
                    return u1.ChangeEndian();
                }
            }

            /// int
            public int u2;

            /// byte[40]
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst = 40, ArraySubType = System.Runtime.InteropServices.UnmanagedType.I1)]
            public byte[] padding;
        }

        [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Ansi)]
        public struct FileOffsetInfo {

            /// char[32]
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 32)]
            public string fileName;

            /// int
            private long _offset;
            public long Offset {
                get {
                    return _offset.ChangeEndian();
                }
            }

            /// int
            private long _fileSize;
            public long FileSize {
                get {
                    return _fileSize.ChangeEndian();
                }
            }

            /// int
            public int u1;

            /// byte[12]
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst = 12, ArraySubType = System.Runtime.InteropServices.UnmanagedType.I1)]
            public byte[] padding;
        }

        #endregion
    }
}
