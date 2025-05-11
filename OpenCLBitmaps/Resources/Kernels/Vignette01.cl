__kernel void Vignette01(
    __global uchar* pixels,
    const int width,
    const int height,
    const float value) // 0.0 - 1.0 Anteil, z. B. 0.1 für 10% Randbreite
{
    int x = get_global_id(0);
    int y = get_global_id(1);

    if (x >= width || y >= height)
        return;

    int idx = (y * width + x) * 4; // RGBA

    // Abstand zum nächsten Rand
    float dx = min((float)x, (float)(width - 1 - x));
    float dy = min((float)y, (float)(height - 1 - y));

    // Normierter Abstand zum Rand (0.0 = Rand, 1.0 = weit entfernt)
    float fx = dx / (width * value);
    float fy = dy / (height * value);

    float f = min(fx, fy);  // Wie tief im Bild drin?

    f = clamp(f, 0.0f, 1.0f); // Begrenzung auf [0,1]

    // Optional: weiche Kurve (Smoothstep)
    f = f * f * (3.0f - 2.0f * f);

    // Alpha skalieren
    pixels[idx + 3] = (uchar)(pixels[idx + 3] * f);
}
