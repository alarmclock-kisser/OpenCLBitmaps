__kernel void Inverse00(
    __global uchar* pixels,
    int width,
    int height)
{
    int x = get_global_id(0);
    int y = get_global_id(1);

    if (x >= width || y >= height)
        return;

    int index = (y * width + x) * 4;

    // Invertiere R, G, B (0–255) → (255 - Wert)
    pixels[index + 0] = 255 - pixels[index + 0]; // R
    pixels[index + 1] = 255 - pixels[index + 1]; // G
    pixels[index + 2] = 255 - pixels[index + 2]; // B
    // Alpha bleibt gleich: pixels[index + 3]
}
