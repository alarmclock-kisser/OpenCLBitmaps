__kernel void DetectEdges01(
    __global const uchar* inputPixels,
    __global uchar* outputPixels,
    const int width,
    const int height,
    float threshold,
    const int thickness)
{
    // Clamp threshold to 0.0-1.0 range
    threshold = clamp(threshold, 0.0f, 1.0f);
    
    // Get pixel coordinates
    int x = get_global_id(0);
    int y = get_global_id(1);
    
    // Check bounds
    if (x >= width || y >= height)
        return;
    
    // Index calculation (RGBA format)
    int idx = (y * width + x) * 4;
    
    // Copy original pixel to output
    outputPixels[idx]     = inputPixels[idx];     // R
    outputPixels[idx + 1] = inputPixels[idx + 1]; // G
    outputPixels[idx + 2] = inputPixels[idx + 2]; // B
    outputPixels[idx + 3] = inputPixels[idx + 3]; // A
    
    // Sobel kernels
    const float sobelX[9] = {-1, 0, 1, -2, 0, 2, -1, 0, 1};
    const float sobelY[9] = {-1, -2, -1, 0, 0, 0, 1, 2, 1};
    
    float gradientX = 0.0f;
    float gradientY = 0.0f;
    
    // Apply Sobel operator (3x3 neighborhood)
    for (int ky = -1; ky <= 1; ky++)
    {
        for (int kx = -1; kx <= 1; kx++)
        {
            int nx = x + kx;
            int ny = y + ky;
            
            // Handle border pixels (clamp to edge)
            nx = clamp(nx, 0, width - 1);
            ny = clamp(ny, 0, height - 1);
            
            int kidx = (ky + 1) * 3 + (kx + 1);
            int nidx = (ny * width + nx) * 4;
            
            // Convert to grayscale using luminance
            float gray = 0.2126f * inputPixels[nidx] + 
                        0.7152f * inputPixels[nidx + 1] + 
                        0.0722f * inputPixels[nidx + 2];
            
            gradientX += gray * sobelX[kidx];
            gradientY += gray * sobelY[kidx];
        }
    }
    
    // Calculate gradient magnitude
    float magnitude = sqrt(gradientX * gradientX + gradientY * gradientY);
    float normalized = magnitude / (4.0f * sqrt(2.0f)); // Normalize to 0.0-1.0
    
    // Check if edge exceeds threshold
    if (normalized >= threshold)
    {
        // Mark edges with red color
        for (int t = -thickness; t <= thickness; t++)
        {
            for (int s = -thickness; s <= thickness; s++)
            {
                int tx = clamp(x + s, 0, width - 1);
                int ty = clamp(y + t, 0, height - 1);
                int tidx = (ty * width + tx) * 4;
                
                outputPixels[tidx]     = 255;   // R
                outputPixels[tidx + 1] = 0;     // G
                outputPixels[tidx + 2] = 0;     // B
                // Alpha remains unchanged
            }
        }
    }
}