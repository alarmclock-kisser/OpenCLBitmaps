__kernel void DetectEdges00(
    __global const uchar* inputPixels,
    __global uchar* outputPixels,
    const int width,
    const int height,
    float threshold,
    const int thickness,
    const int edgeR,
    const int edgeG,
    const int edgeB)
{
    int x = get_global_id(0);
    int y = get_global_id(1);
    const int pixelPos = (y * width + x) * 4;  // 4 channels per pixel (RGBA)

    // Clamp color values to 0-255
    const uchar clampedB = (uchar)min(max(edgeR, 0), 255);
    const uchar clampedG = (uchar)min(max(edgeG, 0), 255);
    const uchar clampedR = (uchar)min(max(edgeB, 0), 255);
    
    // Clamp thickness to reasonable values (0-10)
    const int clampedThickness = min(max(thickness, 0), 10);
    
    // Absolute threshold (allow values > 1.0 for stronger effects)
    const float absThreshold = fabs(threshold);

    // Only process pixels that aren't in the border area
    if (x >= clampedThickness && x < width - clampedThickness &&
        y >= clampedThickness && y < height - clampedThickness)
    {
        // Sobel kernels
        const int sobelX[3][3] = {
            {-1, 0, 1},
            {-2, 0, 2},
            {-1, 0, 1}
        };
        
        const int sobelY[3][3] = {
            {-1, -2, -1},
            { 0,  0,  0},
            { 1,  2,  1}
        };

        float3 gradientX = (float3)(0.0f);
        float3 gradientY = (float3)(0.0f);

        // Process 3x3 neighborhood
        for (int dy = -1; dy <= 1; dy++) {
            for (int dx = -1; dx <= 1; dx++) {
                int neighborPos = ((y + dy) * width + (x + dx)) * 4;
                
                float3 rgb = {
                    inputPixels[neighborPos]     / 255.0f,  // R
                    inputPixels[neighborPos + 1] / 255.0f,  // G
                    inputPixels[neighborPos + 2] / 255.0f   // B
                };

                int kernelX = dx + 1;
                int kernelY = dy + 1;
                
                gradientX += rgb * sobelX[kernelY][kernelX];
                gradientY += rgb * sobelY[kernelY][kernelX];
            }
        }

        // Calculate gradient magnitude
        float3 magnitude = sqrt(gradientX * gradientX + gradientY * gradientY);
        float avgMagnitude = (magnitude.x + magnitude.y + magnitude.z) / 3.0f;

        // Edge detection with threshold
        if (avgMagnitude > absThreshold) {
            // Draw thick edges (circular pattern)
            for (int dy = -clampedThickness; dy <= clampedThickness; dy++) {
                for (int dx = -clampedThickness; dx <= clampedThickness; dx++) {
                    if (dx*dx + dy*dy <= clampedThickness*clampedThickness) {
                        int px = x + dx;
                        int py = y + dy;
                        if (px >= 0 && px < width && py >= 0 && py < height) {
                            int outPos = (py * width + px) * 4;
                            outputPixels[outPos]     = clampedR;  // R
                            outputPixels[outPos + 1] = clampedG;  // G
                            outputPixels[outPos + 2] = clampedB;  // B
                            outputPixels[outPos + 3] = 255;       // A (opaque)
                        }
                    }
                }
            }
        } else {
            // Copy original pixel
            outputPixels[pixelPos]     = inputPixels[pixelPos];
            outputPixels[pixelPos + 1] = inputPixels[pixelPos + 1];
            outputPixels[pixelPos + 2] = inputPixels[pixelPos + 2];
            outputPixels[pixelPos + 3] = inputPixels[pixelPos + 3];
        }
    } else {
        // Copy border pixels as-is
        outputPixels[pixelPos]     = inputPixels[pixelPos];
        outputPixels[pixelPos + 1] = inputPixels[pixelPos + 1];
        outputPixels[pixelPos + 2] = inputPixels[pixelPos + 2];
        outputPixels[pixelPos + 3] = inputPixels[pixelPos + 3];
    }
}