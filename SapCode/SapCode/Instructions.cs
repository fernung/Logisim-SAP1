using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SapCode
{
    public class Instructions
    {
        public static string HEADER = "v2.0 raw"+Environment.NewLine;

        public const byte NOP = 0b00000000;
        public const byte LDA = 0b00000001;
        public const byte LDB = 0b00000010;
        public const byte LIA = 0b00000011;
        public const byte LIB = 0b00000100;
        public const byte STA = 0b00000101;
        public const byte STB = 0b00000110;
        public const byte ADD = 0b00000111;
        public const byte SUB = 0b00001000;
        public const byte JMP = 0b00001101;
        public const byte OUT = 0b00001110;
        public const byte HLT = 0b00001111;

        public const ushort OI = 0b0000000000000001;
        public const ushort SU = 0b0000000000000010;
        public const ushort EO = 0b0000000000000100;
        public const ushort BO = 0b0000000000001000;
        public const ushort BI = 0b0000000000010000;
        public const ushort AO = 0b0000000000100000;
        public const ushort AI = 0b0000000001000000;
        public const ushort IO = 0b0000000010000000;
        public const ushort II = 0b0000000100000000;
        public const ushort RO = 0b0000001000000000;
        public const ushort RI = 0b0000010000000000;
        public const ushort MI = 0b0000100000000000;
        public const ushort CE = 0b0001000000000000;
        public const ushort CO = 0b0010000000000000;
        public const ushort CI = 0b0100000000000000;
        public const ushort HL = 0b1000000000000000;

        public static Dictionary<string, byte> OpCodes = new Dictionary<string, byte>
        {
            { "NOP", NOP },
            { "LDA", LDA },
            { "LDB", LDB },
            { "LIA", LIA },
            { "LIB", LIB },
            { "STA", STA },
            { "STB", STB },
            { "ADD", ADD },
            { "SUB", SUB },
            { "JMP", JMP },
            { "OUT", OUT },
            { "HLT", HLT },
        };

        public static ushort[] MicroCode = new ushort[]
        {
            MI|CO, RO|II|CE, 0,     0,      0,          0, 0, //0000 - NOP
            MI|CO, RO|II|CE, IO|MI, RO|AI,  0,          0, 0, //0001 - LDA
            MI|CO, RO|II|CE, IO|MI, RO|BI,  0,          0, 0, //0010 - LDB
            MI|CO, RO|II|CE, IO|AI, 0,      0,          0, 0, //0011 - LIA
            MI|CO, RO|II|CE, IO|BI, 0,      0,          0, 0, //0100 - LIB
            MI|CO, RO|II|CE, IO|MI, AO|RI,  0,          0, 0, //0101 - STA
            MI|CO, RO|II|CE, IO|MI, BO|RI,  0,          0, 0, //0110 - STB
            MI|CO, RO|II|CE, IO|MI, RO|BI,  EO|AI,      0, 0, //0111 - ADD
            MI|CO, RO|II|CE, IO|MI, RO|BI,  EO|AI|SU,   0, 0, //1000 - SUB
            MI|CO, RO|II|CE, 0,     0,      0,          0, 0, //1001 - NOP
            MI|CO, RO|II|CE, 0,     0,      0,          0, 0, //1010 - NOP
            MI|CO, RO|II|CE, 0,     0,      0,          0, 0, //1011 - NOP
            MI|CO, RO|II|CE, 0,     0,      0,          0, 0, //1100 - NOP
            MI|CO, RO|II|CE, IO|CI, 0,      0,          0, 0, //1101 - JMP
            MI|CO, RO|II|CE, AO|OI, 0,      0,          0, 0, //1110 - OUT
            MI|CO, RO|II|CE, HL,    0,      0,          0, 0, //1111 - HLT
        };

        public static void Build()
        {
            string high = HEADER, low = HEADER;
            foreach(var code in MicroCode)
            {
                high += HexHL(HighByte(code)) + Environment.NewLine;
                low += HexHL(LowByte(code)) + Environment.NewLine;
            }
            Save("ROM_high", high);
            Save("ROM_low", low);
        }

        public static void Compile()
        {
            int i, j;
            string high = HEADER, low = HEADER;
            int index = 0;
            for (i = 0; i < 16; i++)
            {
                for (j = 0; j < 7; j++)
                {
                    index = (i * 7) + j;
                    high += HexHL(HighByte(MicroCode[index])) + Environment.NewLine;
                    low += HexHL(LowByte(MicroCode[index])) + Environment.NewLine;
                }
                high += "9*00" + Environment.NewLine;
                low += "9*00" + Environment.NewLine;
                //for (j = 0; j < 9; j++)
                //{
                //    high += "00" + Environment.NewLine;
                //    low += "00" + Environment.NewLine;
                //}
            }
            Save("ROM_high", high);
            Save("ROM_low", low);
        }

        public static void Assemble(string file)
        {
            string[] data = Load(file);
            string parsed = HEADER;
            foreach(var line in data)
            {
                parsed += Parse(line) + Environment.NewLine;
            }
            Save(file + "_assembled", parsed);
        }
        public static void AssembleHL(string file)
        {
            string[] data = Load(file);
            string high = HEADER, low = HEADER;
            foreach (var line in data)
            {
                var d = ParseHL(line);
                high += d.Item1 + Environment.NewLine;
                low += d.Item2 + Environment.NewLine;
            }
            Save(file + "_high", high);
            Save(file + "_low", low);
        }

        public static void Save(string file, string content) => File.WriteAllText(file, content);
        public static string[] Load(string file) => File.ReadAllLines(file);
        public static string Parse(string line)
        {
            string[] input = line.Trim().ToUpper().Split(' ');
            return OpCodes.ContainsKey(input[0]) ?
                   (input.Length > 1) ?
                    Hex(OpCodes[input[0]], byte.Parse(input[1])) :
                    Hex(OpCodes[input[0]]) :
                    "00";
        }
        public static Tuple<string, string> ParseHL(string line)
        {
            string[] input = line.Trim().ToUpper().Split(' ');
            return OpCodes.ContainsKey(input[0]) ?
                   (input.Length > 1) ?
                   new Tuple<string, string>(HexHL(OpCodes[input[0]]), HexHL(byte.Parse(input[1]))) :
                   new Tuple<string, string>(HexHL(OpCodes[input[0]]), "0") :
                   new Tuple<string, string>("0", "0");
        }
        public static byte Op(byte code, byte param=0x00) => (byte)((code << 4) | (param));
        public static string Hex(byte code, byte param=0x00) => Op(code, param).ToString("X2");
        public static string HexHL(byte data) => data.ToString("X2");

        public static byte HighByte(ushort data) => (byte)((data >> 8) & 0xFF);
        public static byte LowByte(ushort data) => (byte)(data & 0xFF);
    }
}
