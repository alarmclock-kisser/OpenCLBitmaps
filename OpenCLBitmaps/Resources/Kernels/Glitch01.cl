__kernel void Glitch01(
    __global uchar* pixels,         // Eingabebild
    __global uchar* output,         // Ausgabebild
    int width,
    int height,
    int channels,                   // RGB = 3, RGBA = 4
    int glitchPixelSize,            // Blockgröße (z. B. 4, 8, 16)
    int glitchProbability,          // 0–100 (% Wahrscheinlichkeit)
    int mode,                       // 0 = Pixel shift, 1 = color crush, 2 = noise
    int seed                        // Seed für Pseudo-Random
)
{
    int x = get_global_id(0);
    int y = get_global_id(1);
    if (x >= width || y >= height) return;

    int gid = y * width + x;
    int dstIdx = gid * channels;

    // Blockkoordinaten
    int blockX = (x / glitchPixelSize);
    int blockY = (y / glitchPixelSize);
    int blockID = blockY * (width / glitchPixelSize) + blockX;

    // Simpler Pseudo-Zufall
    int hash = (blockID * 928371 + seed * 653) & 0x7FFFFFFF;
    int doGlitch = (hash % 100) < glitchProbability;

    if (doGlitch) {
        int gx = x;
        int gy = y;

        if (mode == 0) {
            // ✴️ Modus 0: Pixelverschiebung (Shift)
            gx = (x + (hash % 20) - 10 + width) % width;
            gy = (y + (hash % 10) - 5 + height) % height;
        }
        else if (mode == 1) {
            // ✴️ Modus 1: Color Bit-Crush
            for (int c = 0; c < channels; c++) {
                uchar v = pixels[dstIdx + c];
                uchar crushed = (v & 0xE0); // nur obere 3 Bits
                output[dstIdx + c] = crushed;
            }
            return;
        }
        else if (mode == 2) {
            // ✴️ Modus 2: Noise
            for (int c = 0; c < channels; c++) {
                uchar noise = (uchar)(hash >> (c * 5)) & 0xFF;
                output[dstIdx + c] = noise;
            }
            return;
        }

        // verschobene Quelle
        int srcIdx = (gy * width + gx) * channels;
        for (int c = 0; c < channels; c++) {
            output[dstIdx + c] = pixels[srcIdx + c];
        }
    }
    else {
        // normal kopieren
        for (int c = 0; c < channels; c++) {
            output[dstIdx + c] = pixels[dstIdx + c];
        }
    }
}
