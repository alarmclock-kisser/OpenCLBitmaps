__kernel void Downsample01(
    __global uchar* pixels,
    const int width,
    const int height,
    const int bitsPerChannel) // Ziel-Bittiefe pro Farbkanal (z.B. 8, 4, 2)
{
    int x = get_global_id(0);
    int y = get_global_id(1);

    if (x >= width || y >= height)
        return;

    int idx = (y * width + x) * 4; // 4 Kan�le: R, G, B, A

    // Bitmaske f�r Zielbitanzahl berechnen (z.B. 0xF0 f�r 4 Bit)
    int shift = 8 - bitsPerChannel;
    uchar mask = (0xFF << shift) & 0xFF;

    // Reduziere die Bittiefe f�r R, G, B (Alpha optional)
    for (int i = 0; i < 3; ++i) // nur RGB
    {
        pixels[idx + i] = pixels[idx + i] & mask;
    }

    // Optional: Alpha auch reduzieren
    // pixels[idx + 3] &= mask;
}
