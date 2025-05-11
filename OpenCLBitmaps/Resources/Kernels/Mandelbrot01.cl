__kernel void Mandelbrot01(
    __global uchar* pixels,
    int width,
    int height,
    float zoom,
    float offsetX,
    float offsetY,
    int maxIter)
{
    int px = get_global_id(0);
    int py = get_global_id(1);

    if (px >= width || py >= height)
        return;

    // Normalisierte Koordinaten [-2.0, 1.0] x [-1.5, 1.5]
    float x0 = (((float)px / width) - 0.5f) * 3.5f / zoom + offsetX;
    float y0 = (((float)py / height) - 0.5f) * 2.0f / zoom + offsetY;

    float x = 0.0f;
    float y = 0.0f;
    int iter = 0;

    while (x*x + y*y <= 4.0f && iter < maxIter)
    {
        float xtemp = x*x - y*y + x0;
        y = 2.0f * x * y + y0;
        x = xtemp;
        iter++;
    }

    int pixelIndex = (py * width + px) * 4;

    // Graustufen-Färbung je nach Iterationen
    uchar color = (uchar)(255.0f * iter / maxIter);
    pixels[pixelIndex + 0] = color; // R
    pixels[pixelIndex + 1] = color; // G
    pixels[pixelIndex + 2] = color; // B
    pixels[pixelIndex + 3] = 255;   // Alpha voll sichtbar
}
