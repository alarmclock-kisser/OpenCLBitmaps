__kernel void HueShift00(
    __global uchar* pixels,   // RGBA buffer (R,G,B,A,R,G,B,A,...)
    const int width,
    const int height,
    const float shiftValue)   // Hue shift [0.0, 1.0]
{
    int x = get_global_id(0);
    int y = get_global_id(1);
    
    if (x >= width || y >= height) return;
    
    int pos = (y * width + x) * 4;  // 4 Bytes pro Pixel
    
    // Lese RGBA-Werte
    uchar r = pixels[pos];
    uchar g = pixels[pos + 1];
    uchar b = pixels[pos + 2];
    uchar a = pixels[pos + 3];  // Alpha bleibt unverändert
    
    // RGB zu HSV Konvertierung (optimiert)
    float rf = r / 255.0f;
    float gf = g / 255.0f;
    float bf = b / 255.0f;
    
    float cmax = fmax(fmax(rf, gf), bf);
    float cmin = fmin(fmin(rf, gf), bf);
    float delta = cmax - cmin;
    
    float h = 0.0f;
    if (delta > 0.0f) {
        if (cmax == rf) {
            h = fmod((gf - bf) / delta, 6.0f);
        } else if (cmax == gf) {
            h = ((bf - rf) / delta) + 2.0f;
        } else {
            h = ((rf - gf) / delta) + 4.0f;
        }
        h = fmod((h / 6.0f) + shiftValue + 1.0f, 1.0f);  // Hue-Shift mit Wrap
    }
    
    // HSV zu RGB (optimiert)
    float v = cmax;
    float s = (cmax > 0.0f) ? (delta / cmax) : 0.0f;
    
    float3 rgb = (float3)(v, v, v);  // Grau als Fallback
    
    if (s > 0.0f) {
        h *= 6.0f;
        int sector = (int)h;
        float frac = h - sector;
        float p = v * (1.0f - s);
        float q = v * (1.0f - s * frac);
        float t = v * (1.0f - s * (1.0f - frac));
        
        switch (sector % 6) {
            case 0: rgb = (float3)(v, t, p); break;
            case 1: rgb = (float3)(q, v, p); break;
            case 2: rgb = (float3)(p, v, t); break;
            case 3: rgb = (float3)(p, q, v); break;
            case 4: rgb = (float3)(t, p, v); break;
            default: rgb = (float3)(v, p, q); break;
        }
    }
    
    // Zurückschreiben (in-place)
    pixels[pos]     = (uchar)(rgb.x * 255);  // R
    pixels[pos + 1] = (uchar)(rgb.y * 255);  // G
    pixels[pos + 2] = (uchar)(rgb.z * 255);  // B
    // Alpha (pos + 3) bleibt unverändert!
}