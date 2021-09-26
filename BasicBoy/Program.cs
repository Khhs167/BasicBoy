using System;
using System.IO;
using Raylib_cs;
namespace BasicBoy
{
    public static class BasicBoy
    {
        public static ushort codePtr;

        public static byte[] rom = new byte[0];

        public static byte[] ram = new byte[64 * 1024];
        public static byte[] vram = new byte[32 * 1024];
        public static byte[] displayBuffer = new byte[128 * 128 * 2];
        public static byte flags;

        public static string err = String.Empty;


        public static bool GetFlag(byte flag)
        {
            return (flags & (1 << flag)) != 0;
        }

        static void Main(string[] args)
        {
            Raylib.InitWindow(128, 128, "BasicBoy");

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.BLACK);
            Raylib.DrawText("Loading", 5, 5, 10, Color.WHITE);
            Raylib.EndDrawing();
            var gameLocation = Directory.GetCurrentDirectory() + "/test.bbg";
            rom = File.ReadAllBytes(gameLocation);

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.BLACK);
            Raylib.EndDrawing();

            flags = 0b00000001;
            displayBuffer[0] = 255;


            while (codePtr < rom.Length && !Raylib.WindowShouldClose())
            {
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.BLACK);
                for (int x = 0; x < 128; x++)
                {
                    for (int y = 0; y < 128; y++)
                    {
                        int i = (x + (GetFlag(0) ? 128 : 0)) * 128 + y;

                        Raylib.DrawPixel(x, y, new Color(displayBuffer[i], displayBuffer[i], displayBuffer[i], (byte)255));
                    }
                }

                Raylib.DrawText(err, 0, 0, 10, Color.RED);

                Raylib.EndDrawing();

                if (err != string.Empty)
                    continue;

                byte opcode = rom[codePtr];

                if(opcode == 0x00)
                {
                    codePtr++;
                    continue;
                }
                if(opcode == 0x01)
                {
                    codePtr++;
                    ushort ramLoc = GetValue(codePtr);
                    codePtr += 3;
                    ram[ramLoc] = (byte)GetValue(codePtr);
                    codePtr += 3;
                    continue;
                }
                if (opcode == 0x02)
                {
                    codePtr++;
                    ushort ramLoc = GetValue(codePtr);
                    codePtr += 3;
                    ram[ramLoc] = (byte)(ram[ramLoc] & (byte)GetValue(codePtr));
                    codePtr += 3;
                    continue;
                }
                if (opcode == 0x03)
                {
                    codePtr++;
                    ushort ramLoc = GetValue(codePtr);
                    codePtr += 3;
                    ram[ramLoc] = (byte)(ram[ramLoc] | (byte)GetValue(codePtr));
                    codePtr += 3;
                    continue;
                }
                if (opcode == 0x04)
                {
                    codePtr++;
                    ushort ramLoc = GetValue(codePtr);
                    codePtr += 3;
                    ram[ramLoc] = (byte)(ram[ramLoc] !& (byte)GetValue(codePtr));
                    codePtr += 3;
                    continue;
                }
                if (opcode == 0x05)
                {
                    codePtr++;
                    ushort ramLoc = GetValue(codePtr);
                    codePtr += 3;
                    ram[ramLoc] = (byte)(ram[ramLoc] ^ (byte)GetValue(codePtr));
                    codePtr += 3;
                    continue;
                }
                if (opcode == 0x06)
                {
                    codePtr++;
                    ushort ramLoc = GetValue(codePtr);
                    codePtr += 3;
                    ram[ramLoc] = (byte)(ram[ramLoc] !| (byte)GetValue(codePtr));
                    codePtr += 3;
                    continue;
                }

                if (opcode == 0x07)
                {
                    codePtr++;
                    ushort ramLoc = GetValue(codePtr);
                    codePtr += 3;
                    ram[ramLoc] = (byte)(ram[ramLoc] + (byte)GetValue(codePtr));
                    codePtr += 3;
                    continue;
                }
                if (opcode == 0x08)
                {
                    codePtr++;
                    ushort ramLoc = GetValue(codePtr);
                    codePtr += 3;
                    ram[ramLoc] = (byte)(ram[ramLoc] - (byte)GetValue(codePtr));
                    codePtr += 3;
                    continue;
                }
                if (opcode == 0x09)
                {
                    codePtr++;
                    ushort ramLoc = GetValue(codePtr);
                    codePtr += 3;
                    ram[ramLoc] = (byte)(ram[ramLoc] / (byte)GetValue(codePtr));
                    codePtr += 3;
                    continue;
                }
                if (opcode == 0x0A)
                {
                    codePtr++;
                    ushort ramLoc = GetValue(codePtr);
                    codePtr += 3;
                    ram[ramLoc] = (byte)(ram[ramLoc] * (byte)GetValue(codePtr));
                    codePtr += 3;
                    continue;
                }
                if (opcode == 0x0B)
                {
                    codePtr++;
                    ushort ramLoc = GetValue(codePtr);
                    codePtr += 3;
                    bool change = ram[ramLoc] == (byte)GetValue(codePtr);
                    codePtr += 3;
                    if (change)
                        codePtr = GetValue(codePtr);
                    codePtr += 3;
                    continue;
                }

                if (opcode == 0x0C)
                {
                    codePtr = GetValue((ushort)(codePtr + 1));
                    continue;
                }

                err = "Invalid opcode: 0x" + Convert.ToHexString(new byte[] { opcode});

                
            }
        }

        public static ushort GetValue(ushort loc)
        {
            if (rom[loc] == 0x00)
                return BitConverter.ToUInt16(new byte[] { rom[loc + 1], rom[loc + 2] });
            else if (rom[loc] == 0x01)
                return ram[loc];

            return 0;
        }
    }
}
