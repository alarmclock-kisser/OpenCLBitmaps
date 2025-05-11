__kernel void Kaleidoscope01(
    __global const uchar* pixels,   // Eingabebild (RGBA oder RGB assumed)
    __global uchar* output,         // Ausgabebild
    int width,                      // Breite des Bildes
    int height,                     // Höhe des Bildes
    int channels,                   // 3 für RGB, 4 für RGBA
    float centerX,                  // Zentrum X in [0..1] (z. B. 0.5 = Mitte)
    float centerY,                  // Zentrum Y in [0..1]
    int sectors,                    // Anzahl Spiegelachsen (2..12 empfohlen)
    float twistFactor               // Rotation pro Sektor (z. B. 0.0..0.3)
)
{
    int x = get_global_id(0);
    int y = get_global_id(1);
    if (x >= width || y >= height) return;

    float u = (float)x / (float)width;
    float v = (float)y / (float)height;

    // relativer Vektor zum Zentrum
    float dx = u - centerX;
    float dy = v - centerY;

    float r = sqrt(dx * dx + dy * dy);
    float angle = atan2(dy, dx); // in [-π, π]

    // Winkel in Sektoren einteilen
    float sectorAngle = (2.0f * M_PI_F) / (float)sectors;
    angle = fmod(angle + M_PI_F, sectorAngle); // fmod in [0, sectorAngle]
    angle -= sectorAngle / 2.0f;               // zentrieren
    angle *= (1.0f + twistFactor * r);         // Twist proportional zur Entfernung

    // zurück in kartesisch
    float newX = r * cos(angle) + centerX;
    float newY = r * sin(angle) + centerY;

    // Koordinaten zurück in Pixel
    int sx = clamp((int)(newX * width), 0, width - 1);
    int sy = clamp((int)(newY * height), 0, height - 1);

    int srcIdx = (sy * width + sx) * channels;
    int dstIdx = (y * width + x) * channels;

    for (int c = 0; c < channels; c++)
        output[dstIdx + c] = pixels[srcIdx + c];
}
