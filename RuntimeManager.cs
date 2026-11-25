using System.IO.Compression;
using System.Net.Http.Headers;
using System.Text.Json;

namespace LlamaMaster;

public static class RuntimeManager
{
    private static readonly HttpClient Http = new()
    {
        Timeout = TimeSpan.FromMinutes(45)
    };

    static RuntimeManager()
    {
        Http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("LlamaMaster", "1.0"));
    }

    public record DownloadResult(string RuntimeDir, string ServerExe, string RpcExe);

    public static async Task<DownloadResult> EnsureRuntimeAsync(
        string baseDir,
        string cudaTag,
        string buildTag,
        IProgress<(string message, int percent)>? progress = null,
        CancellationToken ct = default)
    {
        string runtimeDir = Path.Combine(baseDir, "runtimes", $"cuda-{cudaTag}");
        Directory.CreateDirectory(runtimeDir);

        string? serverExe = FindExe(runtimeDir, "llama-server.exe");
        string? rpcExe = FindExe(runtimeDir, "ggml-rpc-server.exe")
                      ?? FindExe(runtimeDir, "rpc-server.exe");

        if (serverExe != null && rpcExe != null)
        {
            progress?.Report(("Runtime already ready", 100));
            return new DownloadResult(runtimeDir, serverExe, rpcExe);
        }

        string tag = buildTag.Trim();
        if (string.Equals(tag, "latest", StringComparison.OrdinalIgnoreCase))
        {
            progress?.Report(("Looking up latest release...", 5));
            tag = await GetLatestTagAsync(ct);
            progress?.Report(($"Using tag: {tag}", 10));
        }

        string llamaZipName = $"llama-{tag}-bin-win-cuda-{cudaTag}-x64.zip";
        string cudartZipName = $"cudart-llama-bin-win-cuda-{cudaTag}-x64.zip";

        string llamaUrl = $"https://github.com/ggml-org/llama.cpp/releases/download/{tag}/{llamaZipName}";
        string cudartUrl = $"https://github.com/ggml-org/llama.cpp/releases/download/{tag}/{cudartZipName}";

        string cacheDir = Path.Combine(baseDir, "downloads");
        Directory.CreateDirectory(cacheDir);

        string llamaZip = Path.Combine(cacheDir, llamaZipName);
        string cudartZip = Path.Combine(cacheDir, cudartZipName);

        await DownloadIfMissingAsync(llamaUrl, llamaZip, progress, 10, 55, ct);
        await DownloadIfMissingAsync(cudartUrl, cudartZip, progress, 55, 85, ct);

        progress?.Report(("Extracting llama binaries...", 88));
        await Task.Run(() => ExtractZipFlatten(llamaZip, runtimeDir), ct);

        progress?.Report(("Extracting CUDA runtime DLLs...", 94));
        await Task.Run(() => ExtractZipFlatten(cudartZip, runtimeDir), ct);

        serverExe = FindExe(runtimeDir, "llama-server.exe")
            ?? throw new FileNotFoundException("llama-server.exe not found after extraction");
        rpcExe = FindExe(runtimeDir, "ggml-rpc-server.exe")
              ?? FindExe(runtimeDir, "rpc-server.exe")
              ?? throw new FileNotFoundException("RPC server executable not found after extraction");

        progress?.Report(("Runtime ready", 100));
        return new DownloadResult(runtimeDir, serverExe, rpcExe);
    }

    private static async Task<string> GetLatestTagAsync(CancellationToken ct)
    {
        using var resp = await Http.GetAsync(
            "https://api.github.com/repos/ggml-org/llama.cpp/releases?per_page=8", ct);
        resp.EnsureSuccessStatusCode();

        using var doc = await JsonDocument.ParseAsync(
            await resp.Content.ReadAsStreamAsync(ct), cancellationToken: ct);

        foreach (var rel in doc.RootElement.EnumerateArray())
        {
            if (rel.TryGetProperty("draft", out var d) && d.GetBoolean()) continue;
            if (rel.TryGetProperty("tag_name", out var t))
                return t.GetString()!;
        }
        throw new Exception("Could not find any release tag");
    }

    private static async Task DownloadIfMissingAsync(
        string url, string dest,
        IProgress<(string message, int percent)>? progress,
        int startPercent, int endPercent,
        CancellationToken ct)
    {
        if (File.Exists(dest) && new FileInfo(dest).Length > 1_000_000)
        {
            progress?.Report(($"Cached: {Path.GetFileName(dest)}", endPercent));
            return;
        }

        progress?.Report(($"Downloading {Path.GetFileName(dest)}...", startPercent));

        using var resp = await Http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
        if (!resp.IsSuccessStatusCode)
            throw new Exception($"Download failed ({(int)resp.StatusCode})\n{url}");

        long? total = resp.Content.Headers.ContentLength;
        await using var src = await resp.Content.ReadAsStreamAsync(ct);
        await using var dst = File.Create(dest);

        var buffer = new byte[81920];
        long read = 0;
        int n;
        while ((n = await src.ReadAsync(buffer, ct)) > 0)
        {
            await dst.WriteAsync(buffer.AsMemory(0, n), ct);
            read += n;

            if (total > 0)
            {
                double frac = (double)read / total.Value;
                int pct = startPercent + (int)((endPercent - startPercent) * frac);
                progress?.Report(($"Downloading {Path.GetFileName(dest)}... {read / 1_048_576}/{total.Value / 1_048_576} MB", pct));
            }
        }
    }

    private static void ExtractZipFlatten(string zipPath, string destDir)
    {
        using var archive = ZipFile.OpenRead(zipPath);
        foreach (var entry in archive.Entries)
        {
            if (string.IsNullOrEmpty(entry.Name)) continue;
            string target = Path.Combine(destDir, entry.Name);
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            entry.ExtractToFile(target, overwrite: true);
        }
    }

    private static string? FindExe(string dir, string name)
    {
        string direct = Path.Combine(dir, name);
        if (File.Exists(direct)) return direct;
        return Directory.EnumerateFiles(dir, name, SearchOption.AllDirectories).FirstOrDefault();
    }
}