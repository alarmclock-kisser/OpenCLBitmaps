__kernel void Greyscale01(
    __global uchar* pixels,
    const int width,
    const int height,
    int intensity)
{
    // Get pixel coordinates
    int x = get_global_id(0);
    int y = get_global_id(1);
    
    // Check if we're within image bounds
    if (x >= width || y >= height)
        return;
    
    // Calculate 1D index (assuming RGBA format, 4 channels per pixel)
    int idx = (y * width + x) * 4;
    
    // Clamp intensity to 0-100 range
    intensity = clamp(intensity, 0, 100);
    float intensity_factor = intensity / 100.0f;
    
    // Get original color values (convert byte to float for calculations)
    float r = pixels[idx]   / 255.0f;
    float g = pixels[idx+1] / 255.0f;
    float b = pixels[idx+2] / 255.0f;
    
    // Calculate luminance (standard grayscale conversion weights)
    float luminance = 0.2126f * r + 0.7152f * g + 0.0722f * b;
    
    // Apply intensity factor (lerp between original and grayscale)
    float result_r = mix(r, luminance, intensity_factor);
    float result_g = mix(g, luminance, intensity_factor);
    float result_b = mix(b, luminance, intensity_factor);
    
    // Convert back to byte and store
    pixels[idx]   = (uchar)(result_r * 255.0f);
    pixels[idx+1] = (uchar)(result_g * 255.0f);
    pixels[idx+2] = (uchar)(result_b * 255.0f);
    // Alpha channel (idx + 3) remains unchanged
}