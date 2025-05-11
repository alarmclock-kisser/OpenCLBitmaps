__kernel void SmoothEdges01(
    __global uchar* pixels,
    __global uchar* output,
    const int width,
    const int height,
    const float strength)
{
    int x = get_global_id(0);
    int y = get_global_id(1);

    if (x >= width || y >= height)
        return;

    int channels = 4; // RGBA
    int idx = (y * width + x) * channels;

    float sum[4] = {0.0f, 0.0f, 0.0f, 0.0f};
    int count = 0;

    // 3x3 Box Blur
    for (int dy = -1; dy <= 1; ++dy) {
        for (int dx = -1; dx <= 1; ++dx) {
            int nx = x + dx;
            int ny = y + dy;

            if (nx >= 0 && ny >= 0 && nx < width && ny < height) {
                int nidx = (ny * width + nx) * channels;
                for (int c = 0; c < 4; ++c) {
                    sum[c] += (float)pixels[nidx + c];
                }
                count++;
            }
        }
    }

    // Mittelwert berechnen
    for (int c = 0; c < 4; ++c) {
        float avg = sum[c] / (float)count;
        float orig = (float)pixels[idx + c];
        float mixed = (1.0f - strength) * orig + strength * avg;
        output[idx + c] = (uchar)clamp(mixed, 0.0f, 255.0f);
    }
}
