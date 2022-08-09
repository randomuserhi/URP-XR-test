## MagnetSim
Add Toggle to freeze filings in place while moving magnet.
## Optical Sim
https://math.stackexchange.com/questions/1403126/what-is-the-general-equation-for-rotated-ellipsoid

float r = c / l.refractiveIndex;
float cosI = -Vector3.Dot(n, d);
float sinT2 = r * r * (1f - cosI * cosI);
if (sinT2 > 1.0) Debug.LogWarning("TIR"); // TIR
float cosT = Mathf.Sqrt(1f - sinT2);
d = r * d + (r * cosI - cosT) * n;

https://ricktu288.github.io/ray-optics/simulator/?en