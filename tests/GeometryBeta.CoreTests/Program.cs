using Fs.Fox.Cad;

var assertions = 0;
void Check(bool result, string message)
{
    assertions++;
    if (!result) throw new InvalidOperationException(message);
}
void Close(double actual, double expected, string message) =>
    Check(Math.Abs(actual - expected) <= 1e-10, $"{message}: expected {expected}, actual {actual}");
void Throws<T>(Action operation, string message) where T : Exception
{
    assertions++;
    try { operation(); }
    catch (T) { return; }
    throw new InvalidOperationException(message);
}

var origin = new TrianglePointDataBeta(0, 0, 0);
var eastUp = new TrianglePointDataBeta(2, 0, 2);
var north = new TrianglePointDataBeta(0, 2, 0);
var center = TriangleMathBeta.Centroid(origin, eastUp, north);
Close(center.X, 2d / 3, "centroid x");
Close(center.Y, 2d / 3, "centroid y");
Close(center.Z, 2d / 3, "centroid retains z");
Check(TriangleMathBeta.TrySlopeAndAspect(origin, eastUp, north, 1e-12, out var slope, out var aspect), "z=x slope");
Close(slope, 45, "inclination uses degrees");
Close(aspect, 270, "downhill points west");
Check(TriangleMathBeta.TrySlopeAndAspect(north, eastUp, origin, 1e-12, out slope, out aspect), "reversed winding");
Close(slope, 45, "winding-independent slope");
Close(aspect, 270, "winding-independent aspect");
Check(TriangleMathBeta.TrySlopeAndAspect(origin, new(2, 0, 0), new(0, 2, 2), 1e-12, out slope, out aspect), "z=y slope");
Close(aspect, 180, "downhill points south");
Check(TriangleMathBeta.TrySlopeAndAspect(origin, new(1, 0, 0), new(0, 1, 0), 1e-12, out slope, out aspect), "horizontal plane valid");
Close(slope, 0, "horizontal slope");
Check(double.IsNaN(aspect), "horizontal plane has no aspect");
Check(!TriangleMathBeta.TrySlopeAndAspect(origin, new(0, 1, 0), new(0, 0, 1), 1e-12, out slope, out aspect), "vertical plane rejected");
Check(double.IsNaN(slope) && double.IsNaN(aspect), "failure clears outputs");
Check(!TriangleMathBeta.TrySlopeAndAspect(origin, eastUp, new(4, 0, 4), 1e-12, out _, out _), "collinear rejected");
Check(!TriangleMathBeta.TrySlopeAndAspect(origin, origin, north, 1e-12, out _, out _), "duplicate rejected");
Check(TriangleMathBeta.TrySlopeAndAspect(origin, new(1e-300, 0, 1e-300), new(0, 1e-300, 0), 1e-12, out slope, out _), "tiny valid triangle");
Close(slope, 45, "tiny scale invariance");
Check(TriangleMathBeta.TrySlopeAndAspect(new(-1e308, 0, -1e308), new(1e308, 0, 1e308), new(0, 1e308, 0), 1e-12, out slope, out _), "huge finite triangle");
Close(slope, 45, "large scale invariance");
Check(TriangleMathBeta.TrySlopeAndAspect(new(0, 0, 1e308), new(1e-100, 0, 1e308), new(0, 1e-100, 1e308), 1e-12,
    out slope, out aspect), "large shared offset must not erase other axes");
Close(slope, 0, "offset horizontal slope");
Check(double.IsNaN(aspect), "offset horizontal aspect");
foreach (var invalid in new[] { 0d, -1, 1, double.NaN, double.PositiveInfinity })
    Throws<ArgumentOutOfRangeException>(() => TriangleMathBeta.TrySlopeAndAspect(origin, eastUp, north, invalid, out _, out _), "invalid tolerance");
Throws<ArgumentException>(() => TriangleMathBeta.Validate(new(double.NaN, 0, 0), "point"), "nonfinite point");
var hugeCenter = TriangleMathBeta.Centroid(new(1e308, 0, 0), new(1e308, 0, 0), new(1e308, 0, 0));
Check(double.IsFinite(hugeCenter.X) && Math.Abs(hugeCenter.X / 1e308 - 1) < 1e-14, "centroid overflow prevention");
var maxCenter = TriangleMathBeta.Centroid(new(double.MaxValue, 0, 0), new(double.MaxValue, 0, 0), new(double.MaxValue, 0, 0));
Check(double.IsFinite(maxCenter.X), "centroid maximum finite inputs");
var permutations = new[] { new[] { 0, 1, 2 }, new[] { 0, 2, 1 }, new[] { 1, 0, 2 }, new[] { 1, 2, 0 }, new[] { 2, 0, 1 }, new[] { 2, 1, 0 } };
var skinny = new[] { origin, new TrianglePointDataBeta(1, 0, 1), new TrianglePointDataBeta(1, 1e-4, 1) };
var cancellation = new[] { new TrianglePointDataBeta(1e16, 0, 0), new TrianglePointDataBeta(1, 0, 0), new TrianglePointDataBeta(-1e16, 0, 0) };
foreach (var order in permutations)
{
    Check(!TriangleMathBeta.TrySlopeAndAspect(skinny[order[0]], skinny[order[1]], skinny[order[2]], 1e-3, out _, out _),
        "degeneracy must not depend on the first vertex");
    Check(TriangleMathBeta.TrySlopeAndAspect(skinny[order[0]], skinny[order[1]], skinny[order[2]], 1e-12, out slope, out aspect),
        "skinny supported with sufficiently small tolerance");
    Close(slope, 45, "skinny winding slope");
    Close(aspect, 270, "skinny winding aspect");
    Close(TriangleMathBeta.Centroid(cancellation[order[0]], cancellation[order[1]], cancellation[order[2]]).X, 1d / 3,
        "centroid compensation retains cancellation residual");
}
var tinyCenter = TriangleMathBeta.Centroid(new(double.Epsilon, 0, 0), new(double.Epsilon, 0, 0), new(double.Epsilon, 0, 0));
Check(tinyCenter.X == double.Epsilon, "subnormal centroid");

var triangle = new[] { 0, 1, 2 };
var copied = PolyFaceMeshDataBeta.ValidateAndCopyFaces(4, new[] { triangle, new[] { 0, 1, 2, 3 } });
Check(copied[0].SequenceEqual(new short[] { 1, 2, 3, 0 }), "native triangle fourth index is zero");
Check(copied[1].SequenceEqual(new short[] { 1, 2, 3, 4 }), "native quad indices");
triangle[0] = 3;
Check(copied[0][0] == 1, "face snapshots independent");
foreach (var face in new[] { new[] { 0, 1 }, new[] { 0, 1, 2, 3, 4 }, new[] { 0, 0, 2 }, new[] { -1, 1, 2 }, new[] { 0, 1, 5 } })
    Throws<ArgumentException>(() => PolyFaceMeshDataBeta.ValidateAndCopyFaces(5, new[] { face }), "malformed face rejected");
Throws<ArgumentException>(() => PolyFaceMeshDataBeta.ValidateAndCopyFaces(4, Array.Empty<int[]>()), "empty faces");
Throws<ArgumentNullException>(() => PolyFaceMeshDataBeta.ValidateAndCopyFaces(4, null!), "null faces");
Throws<ArgumentException>(() => PolyFaceMeshDataBeta.ValidateAndCopyFaces(4, new IEnumerable<int>[] { null! }), "null face");
Throws<ArgumentOutOfRangeException>(() => PolyFaceMeshDataBeta.ValidateAndCopyFaces(2, new[] { new[] { 0, 1, 2 } }), "too few vertices");
Throws<ArgumentOutOfRangeException>(() => PolyFaceMeshDataBeta.ValidateAndCopyFaces(32768, new[] { new[] { 0, 1, 2 } }), "short index overflow");
var maxFace = PolyFaceMeshDataBeta.ValidateAndCopyFaces(32767, new[] { new[] { 0, 1, 32766 } });
Check(maxFace[0][2] == 32767, "maximum supported index");
Throws<ArgumentException>(() => PolyFaceMeshDataBeta.ValidateAndCopyFaces(3, Enumerable.Repeat(new[] { 0, 1, 2 }, 32768)), "face count limit");
Console.WriteLine($"Geometry Beta core: {assertions} assertions passed (production source linked; no CAD host).");
