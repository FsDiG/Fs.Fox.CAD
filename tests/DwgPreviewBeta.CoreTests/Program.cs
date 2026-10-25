using Fs.Fox.Cad;

var checks = 0;
void Check(bool condition, string message)
{
    checks++;
    if (!condition) throw new InvalidOperationException(message);
}
void Close(double actual, double expected, string message) => Check(Math.Abs(actual - expected) < 1e-9, message);
void Throws<T>(Action action, string message) where T : Exception
{
    checks++;
    try { action(); }
    catch (T) { return; }
    throw new InvalidOperationException(message);
}

var fit = DwgPreviewCoreBeta.Fit(0, 0, -5, 20, 10, 5, 200, 100);
Close(fit.CenterX, 10, "center x");
Close(fit.CenterY, 5, "center y");
Close(fit.CenterZ, 0, "center z");
Close(fit.FieldWidth, 22, "10 percent total horizontal padding");
Close(fit.FieldHeight, 11, "10 percent total vertical padding");
Check(fit.EyeZ > 5, "eye above all model-space depths");
fit = DwgPreviewCoreBeta.Fit(0, 0, 0, 20, 10, 0, 100, 200);
Close(fit.FieldWidth, 22, "portrait retains x extent");
Close(fit.FieldHeight, 44, "portrait ratio does not crop x extent");
fit = DwgPreviewCoreBeta.Fit(7, -4, 0, 7, -4, 0, 512, 512);
Close(fit.CenterX, 7, "point center x");
Close(fit.CenterY, -4, "point center y");
Close(fit.FieldWidth, 1.1, "point fallback field");
Check(fit.FieldHeight > 0, "point camera nondegenerate");
fit = DwgPreviewCoreBeta.Fit(0, 0, 0, 0, 20, 0, 800, 400);
Close(fit.FieldHeight, 22, "vertical line view");
Close(fit.FieldWidth, 44, "vertical line preserves ratio");
fit = DwgPreviewCoreBeta.Fit(-30, -20, 1e20, -10, -10, 1e20, 100, 100);
Check(fit.EyeZ > fit.CenterZ, "large shared elevation needs representable camera direction");
foreach (var bad in new[] { double.NaN, double.PositiveInfinity, double.NegativeInfinity })
    Throws<ArgumentException>(() => DwgPreviewCoreBeta.Fit(bad, 0, 0, 10, 10, 0, 100, 100), "nonfinite bounds");
Throws<ArgumentException>(() => DwgPreviewCoreBeta.Fit(10, 0, 0, 0, 10, 0, 100, 100), "inverted bounds");
Throws<ArgumentException>(() => DwgPreviewCoreBeta.Fit(-double.MaxValue, 0, 0, double.MaxValue, 10, 0, 100, 100), "overflow span");
Throws<ArgumentException>(() => DwgPreviewCoreBeta.Fit(0, 0, double.MaxValue, 1, 1, double.MaxValue, 100, 100), "unrepresentable top camera");
foreach (var size in new[] { 0, -1, 8193, int.MaxValue })
{
    Throws<ArgumentOutOfRangeException>(() => DwgPreviewCoreBeta.ValidateDimensions(size, 100), "invalid width");
    Throws<ArgumentOutOfRangeException>(() => DwgPreviewCoreBeta.ValidateDimensions(100, size), "invalid height");
}
DwgPreviewCoreBeta.ValidateDimensions(4096, 4096);
DwgPreviewCoreBeta.ValidateDimensions(8192, 1);
Throws<ArgumentOutOfRangeException>(() => DwgPreviewCoreBeta.ValidateDimensions(8192, 8192), "total pixels bound");

var temporaryRoot = Path.GetFullPath(Path.GetTempPath());
var folder = Path.GetFullPath(Path.Combine(temporaryRoot, "FsFoxPreviewCore-" + Guid.NewGuid().ToString("N")));
Directory.CreateDirectory(folder);
try
{
    // These are path-policy fixtures, not fake DWG rendering tests.
    var source = Path.Combine(folder, "input.DWG");
    var destination = Path.Combine(folder, "result.PNG");
    File.WriteAllBytes(source, new byte[] { 1, 2, 3 });
    var request = DwgPreviewCoreBeta.ValidateRequest(source, destination, 100, 100);
    Check(request.DrawingPath == source && request.OutputPath == destination, "normalized explicit paths");
    Check(!File.Exists(destination), "validation does not create output");
    Throws<ArgumentException>(() => DwgPreviewCoreBeta.ValidateRequest(source, source + ":preview.png", 100, 100), "reject output data stream on source");
    Throws<ArgumentException>(() => DwgPreviewCoreBeta.ValidateRequest(source + ":hidden.dwg", destination, 100, 100), "reject input data stream");
    File.WriteAllText(destination, "keep");
    Throws<IOException>(() => DwgPreviewCoreBeta.ValidateRequest(source, destination, 100, 100), "no output overwrite");
    Check(File.ReadAllText(destination) == "keep", "existing bytes unchanged");
    Throws<ArgumentException>(() => DwgPreviewCoreBeta.ValidateRequest(source, source, 100, 100), "source cannot be output");
    Throws<FileNotFoundException>(() => DwgPreviewCoreBeta.ValidateRequest(Path.Combine(folder, "missing.dwg"), Path.Combine(folder, "new.png"), 100, 100), "missing drawing");
    Throws<DirectoryNotFoundException>(() => DwgPreviewCoreBeta.ValidateRequest(source, Path.Combine(folder, "missing", "out.png"), 100, 100), "parent must exist");
    Throws<ArgumentException>(() => DwgPreviewCoreBeta.ValidateRequest(source, Path.Combine(folder, "out.jpg"), 100, 100), "output format explicit");
    Throws<ArgumentException>(() => DwgPreviewCoreBeta.ValidateRequest(" ", destination, 100, 100), "empty source");
    Throws<ArgumentException>(() => DwgPreviewCoreBeta.ValidateRequest(source, " ", 100, 100), "empty output");
    Check(File.ReadAllBytes(source).SequenceEqual(new byte[] { 1, 2, 3 }), "source bytes unchanged");

    // Invoke the production publication helper with real files; no PNG codec or CAD rendering is simulated.
    var temporary = Path.Combine(folder, "owned.tmp");
    var published = Path.Combine(folder, "published.png");
    var content = new byte[] { 4, 5, 6 };
    DwgPreviewCoreBeta.WriteNewOutput(temporary, published, stream => stream.Write(content, 0, content.Length));
    Check(File.ReadAllBytes(published).SequenceEqual(content), "successful publication retains encoded bytes");
    Check(!File.Exists(temporary), "successful publication consumes the temporary file");

    File.WriteAllText(temporary, "foreign temporary file");
    var uncreated = Path.Combine(folder, "uncreated.png");
    var writerCalled = false;
    Throws<IOException>(() => DwgPreviewCoreBeta.WriteNewOutput(temporary, uncreated, _ => writerCalled = true),
        "CreateNew rejects a temporary path owned by another writer");
    Check(!writerCalled, "a temporary name collision does not invoke the encoder");
    Check(File.ReadAllText(temporary) == "foreign temporary file", "a temporary name collision preserves existing bytes");
    Check(!File.Exists(uncreated), "a temporary name collision cannot publish output");

    var failedTemporary = Path.Combine(folder, "encoding-failed.tmp");
    var failedOutput = Path.Combine(folder, "encoding-failed.png");
    var encodingFailure = new InvalidDataException("Expected encoder failure after a partial write.");
    try
    {
        DwgPreviewCoreBeta.WriteNewOutput(failedTemporary, failedOutput, stream =>
        {
            stream.WriteByte(7);
            throw encodingFailure;
        });
        throw new InvalidOperationException("An encoder failure must propagate.");
    }
    catch (InvalidDataException exception)
    {
        Check(ReferenceEquals(exception, encodingFailure), "cleanup preserves the original encoder exception");
    }
    Check(!File.Exists(failedTemporary), "an encoder failure cleans its partial temporary file");
    Check(!File.Exists(failedOutput), "an encoder failure does not publish partial output");

    var racingTemporary = Path.Combine(folder, "racing.tmp");
    var racingOutput = Path.Combine(folder, "racing.png");
    Throws<IOException>(() => DwgPreviewCoreBeta.WriteNewOutput(racingTemporary, racingOutput, stream =>
    {
        stream.WriteByte(8);
        // Deterministically create a competing output after encoding begins, before publication.
        File.WriteAllText(racingOutput, "competing output");
    }), "publication rejects an output created after request validation");
    Check(File.ReadAllText(racingOutput) == "competing output", "publication never overwrites a competing output");
    Check(!File.Exists(racingTemporary), "an output collision cleans only the owned temporary file");
}
finally
{
    // Check the resolved absolute target before recursively removing this owned fixture directory.
    if (!folder.StartsWith(Path.Combine(temporaryRoot, "FsFoxPreviewCore-"), StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException("Fixture cleanup path escaped its owned temporary prefix.");
    Directory.Delete(folder, recursive: true);
}
Console.WriteLine($"DWG preview core: {checks} checks passed (production source linked; no CAD rendering).");
