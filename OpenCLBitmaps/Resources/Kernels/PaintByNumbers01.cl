__kernel void PaintByNumbers01(
    __global const uchar* pixels,       // Eingabe (RGBA)
    __global uchar* output,             // Ausgabe (RGBA)
    const int width,
    const int height,
    const int bitdepth,                 // Ziel-Bittiefe je Kanal (z. B. 4, 5, 6, 8)
    const float strength,               // 0.0 = Originalfarben, 1.0 = starke Vereinfachung
    const int minEdgeWidth,             // Schwelle für Kantenkontrast (z. B. 10–30)
    const int numbersCount              // Regionen-Zahl (nur relevant auf Host-Seite)
)
{
    int x = get_global_id(0);
    int y = get_global_id(1);
    if (x >= width || y >= height)
        return;

    int idx = (y * width + x) * 4;
    int channels = 4;

    // Farbwert laden
    uchar4 color = vload4(0, &pixels[idx]);

    // Kantenerkennung (Sobel-artig, nur grob)
    float edge = 0.0f;

    if (x > 0 && x < width - 1 && y > 0 && y < height - 1)
    {
        int idxL = ((y) * width + (x - 1)) * 4;
        int idxR = ((y) * width + (x + 1)) * 4;
        int idxT = ((y - 1) * width + x) * 4;
        int idxB = ((y + 1) * width + x) * 4;

        float3 left  = (float3)(pixels[idxL], pixels[idxL+1], pixels[idxL+2]);
        float3 right = (float3)(pixels[idxR], pixels[idxR+1], pixels[idxR+2]);
        float3 top   = (float3)(pixels[idxT], pixels[idxT+1], pixels[idxT+2]);
        float3 bot   = (float3)(pixels[idxB], pixels[idxB+1], pixels[idxB+2]);

        float3 dx = right - left;
        float3 dy = bot - top;

        edge = length(dx) + length(dy); // Kantenstärke als grobe Norm
    }

    // Wenn starke Kante: pixel unverändert kopieren (Kanten beibehalten)
    if (edge > (float)minEdgeWidth)
    {
        vstore4(color, 0, &output[idx]);
        return;
    }

    // Sonst: Farbe glätten + quantisieren (bitdepth)
    uchar3 rgb = (uchar3)(color.x, color.y, color.z);
    float3 smoothRgb = convert_float3(rgb);

    // Vereinfachung per Stärke
    float3 reduced = floor(smoothRgb * (1.0f - strength));

    // Bitdepth-Quantisierung
    int shift = 8 - bitdepth;
    uchar3 quantized = convert_uchar3((convert_int3(reduced) >> shift) << shift);

    // Output schreiben
    output[idx + 0] = quantized.x;
    output[idx + 1] = quantized.y;
    output[idx + 2] = quantized.z;
    output[idx + 3] = color.w; // Alpha erhalten
}
