__kernel void GreyscaleStatic01(
    __global uchar* pixels,
    const int width,
    const int height)
{
    // Get pixel coordinates
    int x = get_global_id(0);
    int y = get_global_id(1);
    
    // Check bounds
    if (x >= width || y >= height)
        return;
    
    // Calculate index (assuming RGBA format)
    int idx = (y * width + x) * 4;
    
    // Get RGB components
    uchar r = pixels[idx];
    uchar g = pixels[idx + 1];
    uchar b = pixels[idx + 2];
    
    // Calculate luminance (fast integer approximation)
    uchar luminance = (uchar)(0.2126f * r + 0.7152f * g + 0.0722f * b + 0.5f);
    
    // Apply grayscale (in-place)
    pixels[idx]     = luminance; // R
    pixels[idx + 1] = luminance; // G
    pixels[idx + 2] = luminance; // B
    // Alpha (idx + 3) remains unchanged
}