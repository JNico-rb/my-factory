// The personal meter of foundry-gasto: a local pass-through in front of an Azure AI Foundry
// resource. An app points its endpoint at http://127.0.0.1:<port>; every call is forwarded as it
// is, its token usage is priced and added to a ledger on this machine, and once a personal limit
// is reached the call is refused here, before it reaches Azure, so nothing more is spent.
//
// It stores token counts and euros only: never a header, a prompt or an answer.
//
// An app that keeps calling Azure directly needs no change either: the meter watches which
// processes of this machine hold a connection to the resource, charges this person the tokens
// Azure Monitor reports for those minutes (an estimate), and at the limit stops those processes.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

public static class Contador
{
    public const string Version = "2";

    class Route { public int Port; public string Name = ""; public string Upstream = ""; public string ResourceId = ""; }
    class Price { public double In; public double Out; public double Cached; public bool HasCached; }
    class Config
    {
        public string Version = "";
        public bool Stop;
        public string Hold = "";
        // At a limit: "stop" also stops the processes that call the resource directly; "warn" leaves them.
        public string OnLimit = "stop";
        public HashSet<string> Protect = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        // Resource name to why its calls are held ("grupo"): a hold of one resource only.
        public Dictionary<string, string> Holds = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public double Day; public double Week; public double Month;
        // What a deployment with no known price is charged, so that it still counts toward the limit.
        public Price Fallback = new Price { In = 0.000005, Out = 0.000015 };
        public List<Route> Routes = new List<Route>();
        public Dictionary<string, Price> Prices = new Dictionary<string, Price>(StringComparer.OrdinalIgnoreCase);
    }
    // Direct: the part of Eur estimated from calls that did not pass through the meter.
    class DayTotal { public double Eur; public double Direct; public int Calls; public int Unpriced; public int Estimated; public int Rejected; }
    class Conn { public string Remote = ""; public int Port; public int Pid; }
    class Usage { public long In; public long Cached; public long Out; public bool Found; }

    static readonly object Gate = new object();
    static readonly Dictionary<string, DayTotal> ByDay = new Dictionary<string, DayTotal>();
    static readonly Dictionary<int, string> Listening = new Dictionary<int, string>();
    static readonly List<string> Errors = new List<string>();
    static Config Current = new Config();
    static DateTime ConfigStamp = DateTime.MinValue;
    static string Dir = "";
    static HttpClient Http;
    static Mutex Single;

    // Direct calls. Targets: "ip|port" of a resource's addresses to the resource names behind it.
    static Dictionary<string, HashSet<string>> Targets = new Dictionary<string, HashSet<string>>();
    // Resource name to its UTC minutes ("yyyy-MM-ddTHH:mm") not yet priced, each with the seconds of
    // connection the processes of this machine held to it in that minute (two connections count twice).
    static readonly Dictionary<string, Dictionary<string, double>> Active = new Dictionary<string, Dictionary<string, double>>(StringComparer.OrdinalIgnoreCase);
    // "resource|deployment|minute" to the tokens that passed through the meter: already counted, not to charge twice.
    static readonly Dictionary<string, long[]> Proxied = new Dictionary<string, long[]>(StringComparer.OrdinalIgnoreCase);
    static readonly Dictionary<string, DateTime> Callers = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
    static readonly List<Dictionary<string, object>> Stopped = new List<Dictionary<string, object>>();
    static readonly HashSet<string> Spared = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    // Never stopped: closing them loses more than it saves, or they are not the spender itself.
    static readonly HashSet<string> NeverStop = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "chrome", "msedge", "firefox", "brave", "opera", "iexplore", "code", "code - insiders", "devenv", "rider64",
        "idea64", "pycharm64", "explorer", "claude", "teams", "ms-teams", "outlook", "svchost", "system",
        "com.docker.backend", "vpnkit", "wslrelay",
    };

    static readonly HashSet<string> SkipRequest = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    { "Host", "Connection", "Keep-Alive", "Proxy-Connection", "Transfer-Encoding", "Content-Length", "Expect", "Upgrade", "Accept-Encoding" };
    static readonly HashSet<string> SkipResponse = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    { "Connection", "Keep-Alive", "Transfer-Encoding", "Content-Length", "Server", "Date" };
    static readonly Regex DeploymentInPath = new Regex("/deployments/([^/?]+)", RegexOptions.IgnoreCase);
    static readonly Regex ModelInJson = new Regex("\"model\"\\s*:\\s*\"([^\"]+)\"");
    static readonly Regex ModelInForm = new Regex("name=\"model\"\\s+([^\\r\\n]+)");
    static readonly Regex UsageStart = new Regex("\"usage\"\\s*:\\s*\\{");
    static readonly Regex TokenPath = new Regex("/(chat/completions|completions|responses|embeddings|messages)(\\?|$)", RegexOptions.IgnoreCase);

    public static void Run(string dir)
    {
        Dir = dir;
        Directory.CreateDirectory(Dir);
        bool isFirst;
        Single = new Mutex(true, "Local\\foundry-gasto-contador", out isFirst);
        if (!isFirst) return; // another meter already serves this user

        var handler = new SocketsHttpHandler { AutomaticDecompression = DecompressionMethods.None, AllowAutoRedirect = false, UseCookies = false };
        Http = new HttpClient(handler) { Timeout = Timeout.InfiniteTimeSpan };
        LoadLedger(DateTime.Now.AddMonths(-1));
        LoadLedger(DateTime.Now);
        LoadPending();
        try { Tick(); } catch (Exception ex) { Note("tick: " + ex.Message); }
        new Thread(Watch) { IsBackground = true }.Start();
        new Thread(PriceLoop) { IsBackground = true }.Start();

        while (true)
        {
            try { Tick(); } catch (Exception ex) { Note("tick: " + ex.Message); }
            Thread.Sleep(2000);
        }
    }

    // ---- configuration, state and ledger -------------------------------------------------

    static void Tick()
    {
        var path = Path.Combine(Dir, "contador.json");
        if (File.Exists(path))
        {
            var stamp = File.GetLastWriteTimeUtc(path);
            if (stamp != ConfigStamp)
            {
                var parsed = ParseConfig(File.ReadAllText(path));
                ConfigStamp = stamp;
                Current = parsed;
            }
        }
        var cfg = Current;
        if (cfg.Stop || (cfg.Version != "" && cfg.Version != Version)) { WriteState(true); Environment.Exit(0); }
        foreach (var route in cfg.Routes) Listen(route.Port);
        SavePending();
        WriteState(false);
    }

    static Config ParseConfig(string text)
    {
        var cfg = new Config();
        using (var doc = JsonDocument.Parse(text))
        {
            var root = doc.RootElement;
            cfg.Version = Str(root, "version");
            cfg.Hold = Str(root, "hold");
            JsonElement el;
            cfg.Stop = root.TryGetProperty("stop", out el) && el.ValueKind == JsonValueKind.True;
            if (Str(root, "onLimit") == "warn") cfg.OnLimit = "warn";
            if (root.TryGetProperty("protect", out el) && el.ValueKind == JsonValueKind.Array)
                foreach (var name in el.EnumerateArray())
                    if (name.ValueKind == JsonValueKind.String) cfg.Protect.Add(name.GetString() ?? "");
            if (root.TryGetProperty("limits", out el) && el.ValueKind == JsonValueKind.Object)
            {
                cfg.Day = Num(el, "day"); cfg.Week = Num(el, "week"); cfg.Month = Num(el, "month");
            }
            if (root.TryGetProperty("holds", out el) && el.ValueKind == JsonValueKind.Object)
                foreach (var h in el.EnumerateObject())
                    if (h.Value.ValueKind == JsonValueKind.String && h.Value.GetString() != "") cfg.Holds[h.Name] = h.Value.GetString();
            if (root.TryGetProperty("fallback", out el) && Num(el, "input") > 0 && Num(el, "output") > 0)
                cfg.Fallback = new Price { In = Num(el, "input"), Out = Num(el, "output") };
            if (root.TryGetProperty("routes", out el) && el.ValueKind == JsonValueKind.Array)
                foreach (var r in el.EnumerateArray())
                {
                    var route = new Route { Port = (int)Num(r, "port"), Name = Str(r, "name"), Upstream = Str(r, "upstream").TrimEnd('/'), ResourceId = Str(r, "resourceId") };
                    // https only; plain http is for a test double on this machine.
                    bool isSafe = route.Upstream.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                        || route.Upstream.StartsWith("http://127.0.0.1:", StringComparison.Ordinal);
                    if (route.Port > 0 && isSafe) cfg.Routes.Add(route);
                }
            if (root.TryGetProperty("prices", out el) && el.ValueKind == JsonValueKind.Object)
                foreach (var p in el.EnumerateObject())
                {
                    JsonElement c;
                    var price = new Price { In = Num(p.Value, "input"), Out = Num(p.Value, "output") };
                    price.HasCached = p.Value.TryGetProperty("cached", out c) && c.ValueKind == JsonValueKind.Number;
                    if (price.HasCached) price.Cached = c.GetDouble();
                    cfg.Prices[p.Name] = price;
                }
        }
        return cfg;
    }

    static string Str(JsonElement o, string name)
    {
        JsonElement v;
        return o.ValueKind == JsonValueKind.Object && o.TryGetProperty(name, out v) && v.ValueKind == JsonValueKind.String ? v.GetString() ?? "" : "";
    }

    static double Num(JsonElement o, string name)
    {
        JsonElement v;
        return o.ValueKind == JsonValueKind.Object && o.TryGetProperty(name, out v) && v.ValueKind == JsonValueKind.Number ? v.GetDouble() : 0;
    }

    static string Today() { return DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture); }

    static string LedgerPath(DateTime month) { return Path.Combine(Dir, "gasto-" + month.ToString("yyyy-MM", CultureInfo.InvariantCulture) + ".jsonl"); }

    static void LoadLedger(DateTime month)
    {
        var path = LedgerPath(month);
        if (!File.Exists(path)) return;
        foreach (var line in File.ReadLines(path))
        {
            if (line.Length < 2) continue;
            try
            {
                using (var doc = JsonDocument.Parse(line))
                {
                    var e = doc.RootElement;
                    var day = Str(e, "day");
                    if (day == "") continue;
                    JsonElement flag;
                    var t = Total(day);
                    t.Eur += Num(e, "eur");
                    if (Str(e, "src") == "directo") { t.Direct += Num(e, "eur"); continue; }
                    t.Calls++;
                    if (e.TryGetProperty("np", out flag) && flag.ValueKind == JsonValueKind.True) t.Unpriced++;
                    if (e.TryGetProperty("est", out flag) && flag.ValueKind == JsonValueKind.True) t.Estimated++;
                }
            }
            catch { /* a torn line: skip it */ }
        }
    }

    static DayTotal Total(string day)
    {
        DayTotal t;
        if (!ByDay.TryGetValue(day, out t)) { t = new DayTotal(); ByDay[day] = t; }
        return t;
    }

    /// Euros of today, of this week (from Monday) and of this month.
    static void Sums(out double day, out double week, out double month)
    {
        var now = DateTime.Now.Date;
        var monday = now.AddDays(-(((int)now.DayOfWeek + 6) % 7));
        var prefix = now.ToString("yyyy-MM", CultureInfo.InvariantCulture);
        day = 0; week = 0; month = 0;
        lock (Gate)
        {
            foreach (var pair in ByDay)
            {
                DateTime d;
                if (!DateTime.TryParseExact(pair.Key, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out d)) continue;
                if (d == now) day += pair.Value.Eur;
                if (d >= monday && d <= now) week += pair.Value.Eur;
                if (pair.Key.StartsWith(prefix, StringComparison.Ordinal)) month += pair.Value.Eur;
            }
        }
    }

    /// Why calls are refused now: "pausa", "grupo" (set by the panel), "dia", "semana", "mes", or "".
    static string BlockReason(Config cfg, string resource)
    {
        if (cfg.Hold != "") return cfg.Hold;
        string held;
        if (resource != null && cfg.Holds.TryGetValue(resource, out held)) return held;
        double day, week, month;
        Sums(out day, out week, out month);
        if (cfg.Day > 0 && day >= cfg.Day) return "dia";
        if (cfg.Week > 0 && week >= cfg.Week) return "semana";
        if (cfg.Month > 0 && month >= cfg.Month) return "mes";
        return "";
    }

    static void Note(string error)
    {
        lock (Gate) { if (Errors.Count < 20 && !Errors.Contains(error)) Errors.Add(error); }
    }

    static string StateJson(bool isStopping)
    {
        var cfg = Current;
        var state = new Dictionary<string, object>();
        state["version"] = Version;
        state["pid"] = Environment.ProcessId;
        state["beat"] = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
        state["isStopping"] = isStopping;
        state["blocked"] = BlockReason(cfg, null);
        state["held"] = new List<string>(cfg.Holds.Keys);
        lock (Gate)
        {
            var recent = DateTime.UtcNow.AddMinutes(-10);
            state["callers"] = Callers.Where(c => c.Value >= recent).Select(c => c.Key).OrderBy(n => n).ToList();
            state["stopped"] = new List<Dictionary<string, object>>(Stopped);
            state["spared"] = Spared.OrderBy(n => n).ToList();
            state["pending"] = Active.Values.Sum(m => m.Count);
        }
        var days = new Dictionary<string, object>();
        lock (Gate)
        {
            state["ports"] = new List<int>(Listening.Keys);
            state["errors"] = new List<string>(Errors);
            var oldest = DateTime.Now.AddDays(-62).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            foreach (var pair in ByDay)
            {
                if (string.CompareOrdinal(pair.Key, oldest) < 0) continue;
                var d = new Dictionary<string, object>();
                d["eur"] = pair.Value.Eur; d["calls"] = pair.Value.Calls; d["unpriced"] = pair.Value.Unpriced;
                d["estimated"] = pair.Value.Estimated; d["rejected"] = pair.Value.Rejected; d["direct"] = pair.Value.Direct;
                days[pair.Key] = d;
            }
        }
        state["byDay"] = days;
        return JsonSerializer.Serialize(state);
    }

    static void WriteState(bool isStopping)
    {
        var path = Path.Combine(Dir, "estado.json");
        try
        {
            File.WriteAllText(path + ".tmp", StateJson(isStopping));
            File.Move(path + ".tmp", path, true);
        }
        catch { /* a reader had it open: the next tick writes it */ }
    }

    // ---- serving ------------------------------------------------------------------------

    static void Listen(int port)
    {
        lock (Gate) { if (Listening.ContainsKey(port)) return; Listening[port] = ""; }
        try
        {
            var listener = new HttpListener();
            listener.Prefixes.Add("http://127.0.0.1:" + port.ToString(CultureInfo.InvariantCulture) + "/");
            listener.Start();
            Task.Run(async () =>
            {
                while (true)
                {
                    HttpListenerContext ctx;
                    try { ctx = await listener.GetContextAsync(); } catch { break; }
                    var taken = ctx;
                    _ = Task.Run(() => Handle(taken, port));
                }
            });
        }
        catch (Exception ex)
        {
            lock (Gate) { Listening.Remove(port); }
            Note("puerto " + port.ToString(CultureInfo.InvariantCulture) + ": " + ex.Message);
        }
    }

    static async Task Reply(HttpListenerResponse res, int status, string json)
    {
        var bytes = Encoding.UTF8.GetBytes(json);
        res.StatusCode = status;
        res.ContentType = "application/json; charset=utf-8";
        res.ContentLength64 = bytes.Length;
        await res.OutputStream.WriteAsync(bytes, 0, bytes.Length);
    }

    static string Refusal(Config cfg, string reason)
    {
        double day, week, month;
        Sums(out day, out week, out month);
        Func<double, string> eur = n => n.ToString("0.00", CultureInfo.InvariantCulture) + " EUR";
        string why =
            reason == "dia" ? "limite personal de hoy alcanzado (" + eur(day) + " de " + eur(cfg.Day) + "); se reabre manana" :
            reason == "semana" ? "limite personal de la semana alcanzado (" + eur(week) + " de " + eur(cfg.Week) + "); se reabre el lunes" :
            reason == "mes" ? "limite personal del mes alcanzado (" + eur(month) + " de " + eur(cfg.Month) + "); se reabre el dia 1" :
            reason == "grupo" ? "el grupo de recursos ha pasado su presupuesto del mes" :
            "gasto en pausa desde el panel";
        var error = new Dictionary<string, object>();
        error["code"] = "insufficient_quota";
        error["type"] = "insufficient_quota";
        error["message"] = "foundry-gasto: " + why + ". Cambialo en el panel de Foundry de Claude Code (/foundry).";
        var body = new Dictionary<string, object>();
        body["error"] = error;
        return JsonSerializer.Serialize(body);
    }

    static async Task Handle(HttpListenerContext ctx, int port)
    {
        var req = ctx.Request;
        var res = ctx.Response;
        try
        {
            if (req.Url.AbsolutePath == "/__contador") { await Reply(res, 200, StateJson(false)); return; }
            var cfg = Current;
            Route route = null;
            foreach (var r in cfg.Routes) if (r.Port == port) route = r;
            if (route == null) { await Reply(res, 502, "{\"error\":{\"code\":\"sin_ruta\",\"message\":\"foundry-gasto: este puerto ya no tiene recurso configurado.\"}}"); return; }

            var reason = BlockReason(cfg, route.Name);
            if (reason != "")
            {
                lock (Gate) { Total(Today()).Rejected++; }
                res.Headers["x-should-retry"] = "false"; // the OpenAI SDKs read it: no retry storm
                await Reply(res, 429, Refusal(cfg, reason));
                return;
            }

            byte[] body = new byte[0];
            if (req.HasEntityBody)
                using (var buffer = new MemoryStream())
                {
                    await req.InputStream.CopyToAsync(buffer);
                    body = buffer.ToArray();
                }

            var msg = new HttpRequestMessage(new HttpMethod(req.HttpMethod), route.Upstream + req.RawUrl);
            if (req.HasEntityBody) msg.Content = new ByteArrayContent(body);
            foreach (string name in req.Headers.AllKeys)
            {
                if (name == null || SkipRequest.Contains(name)) continue;
                var value = req.Headers[name];
                if (name.StartsWith("Content-", StringComparison.OrdinalIgnoreCase))
                {
                    if (msg.Content != null) msg.Content.Headers.TryAddWithoutValidation(name, value);
                }
                else msg.Headers.TryAddWithoutValidation(name, value);
            }
            msg.Headers.TryAddWithoutValidation("Accept-Encoding", "identity"); // plain text, so the usage can be read

            using (var up = await Http.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead))
            {
                res.StatusCode = (int)up.StatusCode;
                foreach (var h in up.Headers) CopyHeader(res, h.Key, h.Value);
                foreach (var h in up.Content.Headers) CopyHeader(res, h.Key, h.Value);
                var length = up.Content.Headers.ContentLength;
                if (length.HasValue) res.ContentLength64 = length.Value; else res.SendChunked = true;

                var head = new MemoryStream();
                var tail = new MemoryStream();
                long received = 0;
                try
                {
                    using (var stream = await up.Content.ReadAsStreamAsync())
                    {
                        var chunk = new byte[16384];
                        int n;
                        while ((n = await stream.ReadAsync(chunk, 0, chunk.Length)) > 0)
                        {
                            received += n;
                            if (head.Length < 4 * 1024 * 1024) head.Write(chunk, 0, n);
                            else
                            {
                                tail.Write(chunk, 0, n);
                                if (tail.Length > 512 * 1024)
                                {
                                    var kept = tail.ToArray();
                                    tail = new MemoryStream();
                                    tail.Write(kept, kept.Length - 256 * 1024, 256 * 1024);
                                }
                            }
                            await res.OutputStream.WriteAsync(chunk, 0, n);
                            await res.OutputStream.FlushAsync();
                        }
                    }
                }
                catch { /* the caller hung up or the stream broke: what was generated is billed anyway */ }

                var status = (int)up.StatusCode;
                if (status >= 200 && status < 300 && req.HttpMethod != "GET")
                {
                    var text = Encoding.UTF8.GetString(head.ToArray()) + Encoding.UTF8.GetString(tail.ToArray());
                    var media = up.Content.Headers.ContentType != null ? up.Content.Headers.ContentType.MediaType ?? "" : "";
                    Record(cfg, route, req.RawUrl, body, text, received, media);
                }
            }
        }
        catch (Exception ex)
        {
            try { await Reply(res, 502, "{\"error\":{\"code\":\"contador\",\"message\":" + JsonSerializer.Serialize("foundry-gasto: " + ex.Message) + "}}"); } catch { }
        }
        finally
        {
            try { res.Close(); } catch { }
        }
    }

    static void CopyHeader(HttpListenerResponse res, string name, IEnumerable<string> values)
    {
        if (SkipResponse.Contains(name)) return;
        foreach (var value in values)
        {
            try { res.AddHeader(name, value); } catch { /* a header the listener keeps for itself */ }
        }
    }

    // ---- metering -----------------------------------------------------------------------

    static long Field(JsonElement o, string name)
    {
        JsonElement v;
        return o.ValueKind == JsonValueKind.Object && o.TryGetProperty(name, out v) && v.ValueKind == JsonValueKind.Number ? (long)v.GetDouble() : 0;
    }

    /// The largest figures among every "usage" object of the answer (a stream repeats or splits them).
    static Usage ReadUsage(string text)
    {
        var usage = new Usage();
        foreach (Match m in UsageStart.Matches(text))
        {
            int start = m.Index + m.Length - 1, depth = 0, end = -1;
            for (int i = start; i < text.Length && i < start + 4000; i++)
            {
                if (text[i] == '{') depth++;
                else if (text[i] == '}' && --depth == 0) { end = i; break; }
            }
            if (end < 0) continue;
            try
            {
                using (var doc = JsonDocument.Parse(text.Substring(start, end - start + 1)))
                {
                    var u = doc.RootElement;
                    JsonElement details;
                    long cached = 0;
                    if (u.TryGetProperty("prompt_tokens_details", out details)) cached = Field(details, "cached_tokens");
                    if (u.TryGetProperty("input_tokens_details", out details)) cached = Math.Max(cached, Field(details, "cached_tokens"));
                    long input = Math.Max(Field(u, "prompt_tokens"), Field(u, "input_tokens"));
                    // Anthropic counts what it read from or wrote to the cache apart from input_tokens.
                    long read = Field(u, "cache_read_input_tokens");
                    if (read > 0 || Field(u, "cache_creation_input_tokens") > 0)
                    {
                        input += read + Field(u, "cache_creation_input_tokens");
                        cached = read;
                    }
                    long output = Math.Max(Field(u, "completion_tokens"), Field(u, "output_tokens"));
                    if (input > 0 || output > 0) usage.Found = true;
                    usage.In = Math.Max(usage.In, input);
                    usage.Cached = Math.Max(usage.Cached, cached);
                    usage.Out = Math.Max(usage.Out, output);
                }
            }
            catch { /* not a usage object after all */ }
        }
        return usage;
    }

    static void Record(Config cfg, Route route, string url, byte[] body, string answer, long received, string media)
    {
        var deployment = "";
        var inPath = DeploymentInPath.Match(url);
        if (inPath.Success) deployment = Uri.UnescapeDataString(inPath.Groups[1].Value);
        else
        {
            var start = Encoding.UTF8.GetString(body, 0, Math.Min(body.Length, 65536));
            var m = ModelInJson.Match(start);
            if (!m.Success) m = ModelInForm.Match(start);
            if (m.Success) deployment = m.Groups[1].Value.Trim();
        }
        deployment = deployment.ToLowerInvariant();

        var usage = ReadUsage(answer);
        bool isEstimated = false;
        if (!usage.Found)
        {
            if (!TokenPath.IsMatch(url)) return; // not a token-billed call (or one this meter cannot read)
            // The answer carried no usage (a stream that did not ask for it): about 4 bytes per token in,
            // one token per streamed event out.
            isEstimated = true;
            usage.In = body.Length / 4;
            usage.Out = media.Contains("event-stream") ? Regex.Matches(answer, "(^|\\n)data:").Count : received / 4;
        }

        Price price;
        bool isPriced = cfg.Prices.TryGetValue(route.Name + "|" + deployment, out price);
        if (!isPriced) price = cfg.Fallback; // an assumed price: a call nobody priced must not be free
        var cached = Math.Min(usage.Cached, usage.In);
        double eur = (usage.In - cached) * price.In + cached * (price.HasCached ? price.Cached : price.In) + usage.Out * price.Out;

        var now = DateTime.Now;
        var line = new Dictionary<string, object>();
        line["t"] = now.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture);
        line["day"] = Today();
        line["r"] = route.Name;
        line["d"] = deployment;
        line["in"] = usage.In;
        line["cin"] = usage.Cached;
        line["out"] = usage.Out;
        line["eur"] = eur;
        if (isEstimated) line["est"] = true;
        if (!isPriced) line["np"] = true;
        var json = JsonSerializer.Serialize(line);
        lock (Gate)
        {
            var key = route.Name + "|" + deployment + "|" + now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture);
            long[] through;
            if (!Proxied.TryGetValue(key, out through)) { through = new long[2]; Proxied[key] = through; }
            through[0] += usage.In; through[1] += usage.Out;
            var t = Total(Today());
            t.Eur += eur;
            t.Calls++;
            if (!isPriced) t.Unpriced++;
            if (isEstimated) t.Estimated++;
            try { File.AppendAllText(LedgerPath(now), json + "\n"); } catch (Exception ex) { if (Errors.Count < 20) Errors.Add("libro: " + ex.Message); }
        }
    }

    // ---- direct calls: watch, price and stop ---------------------------------------------

    [DllImport("iphlpapi.dll", SetLastError = true)]
    static extern uint GetExtendedTcpTable(IntPtr table, ref int size, bool order, int family, int tableClass, uint reserved);

    static readonly Dictionary<string, DateTime> TargetSeen = new Dictionary<string, DateTime>();

    /// Every TCP connection of this machine with the process that owns it (IPv4 and IPv6).
    static List<Conn> Connections()
    {
        var list = new List<Conn>();
        foreach (int family in new[] { 2, 23 })
        {
            int size = 0;
            GetExtendedTcpTable(IntPtr.Zero, ref size, false, family, 4, 0);
            if (size <= 0) continue;
            size += 8192; // the table may grow between the two calls
            IntPtr buffer = Marshal.AllocHGlobal(size);
            try
            {
                if (GetExtendedTcpTable(buffer, ref size, false, family, 4, 0) != 0) continue;
                int count = Marshal.ReadInt32(buffer);
                int row = family == 2 ? 24 : 56;
                for (int i = 0; i < count; i++)
                {
                    IntPtr at = IntPtr.Add(buffer, 4 + i * row);
                    var address = new byte[family == 2 ? 4 : 16];
                    Marshal.Copy(IntPtr.Add(at, family == 2 ? 12 : 24), address, 0, address.Length);
                    int port = Marshal.ReadInt32(at, family == 2 ? 16 : 44);
                    var ip = new IPAddress(address);
                    if (ip.IsIPv4MappedToIPv6) ip = ip.MapToIPv4();
                    list.Add(new Conn
                    {
                        Remote = ip.ToString(),
                        Port = ((port & 0xFF) << 8) | ((port >> 8) & 0xFF),
                        Pid = Marshal.ReadInt32(at, family == 2 ? 20 : 52),
                    });
                }
            }
            finally { Marshal.FreeHGlobal(buffer); }
        }
        return list;
    }

    /// The addresses each resource answers at now. One seen before stays six hours: DNS rotates them.
    static void ResolveTargets()
    {
        var cfg = Current;
        var now = DateTime.UtcNow;
        var next = new Dictionary<string, HashSet<string>>();
        foreach (var pair in Targets)
        {
            DateTime seen;
            if (TargetSeen.TryGetValue(pair.Key, out seen) && now - seen < TimeSpan.FromHours(6)) next[pair.Key] = pair.Value;
        }
        foreach (var route in cfg.Routes)
        {
            Uri uri;
            if (!Uri.TryCreate(route.Upstream, UriKind.Absolute, out uri)) continue;
            IPAddress[] addresses;
            try { addresses = Dns.GetHostAddresses(uri.Host); } catch { continue; }
            foreach (var a in addresses)
            {
                var key = (a.IsIPv4MappedToIPv6 ? a.MapToIPv4() : a).ToString() + "|" + uri.Port.ToString(CultureInfo.InvariantCulture);
                HashSet<string> names;
                names = next.TryGetValue(key, out names)
                    ? new HashSet<string>(names, StringComparer.OrdinalIgnoreCase)
                    : new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                names.Add(route.Name);
                next[key] = names;
                TargetSeen[key] = now;
            }
        }
        Targets = next;
    }

    static void Watch()
    {
        var clock = Stopwatch.StartNew();
        while (true)
        {
            // What one sample stands for: the time since the last one (a stalled machine does not count as minutes).
            var seconds = Math.Min(2.0, clock.Elapsed.TotalSeconds);
            clock.Restart();
            try { WatchOnce(seconds); } catch (Exception ex) { Note("vigia: " + ex.Message); }
            Thread.Sleep(250); // a model call lasts longer than this; reading the table is cheap
        }
    }

    /// Adds this sample's connection time to the minute of each resource a process is connected to and,
    /// at a limit, stops those processes.
    static void WatchOnce(double seconds)
    {
        var cfg = Current;
        var targets = Targets;
        if (targets.Count == 0) return;
        if (BlockReason(cfg, null) == "" && cfg.Holds.Count == 0) lock (Gate) { Spared.Clear(); }
        int own = Environment.ProcessId;
        var minute = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture);
        var reasons = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var judged = new HashSet<int>();
        foreach (var c in Connections())
        {
            if (c.Pid == own || c.Pid <= 4) continue; // the meter's own calls are counted exactly already
            HashSet<string> names;
            if (!targets.TryGetValue(c.Remote + "|" + c.Port.ToString(CultureInfo.InvariantCulture), out names)) continue;
            string process;
            try { process = Process.GetProcessById(c.Pid).ProcessName; } catch { continue; }
            lock (Gate)
            {
                Callers[process] = DateTime.UtcNow;
                foreach (var name in names)
                {
                    Dictionary<string, double> minutes;
                    if (!Active.TryGetValue(name, out minutes)) { minutes = new Dictionary<string, double>(); Active[name] = minutes; }
                    double had;
                    minutes[minute] = (minutes.TryGetValue(minute, out had) ? had : 0) + seconds;
                }
            }
            if (cfg.OnLimit != "stop" || !judged.Add(c.Pid)) continue;
            var reason = "";
            foreach (var name in names)
            {
                string r;
                if (!reasons.TryGetValue(name, out r)) { r = BlockReason(cfg, name); reasons[name] = r; }
                if (r != "") reason = r;
            }
            if (reason == "") continue;
            if (NeverStop.Contains(process) || cfg.Protect.Contains(process)) { lock (Gate) { Spared.Add(process); } continue; }
            try
            {
                Process.GetProcessById(c.Pid).Kill();
                var entry = new Dictionary<string, object>();
                entry["t"] = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
                entry["name"] = process;
                entry["pid"] = c.Pid;
                entry["why"] = reason;
                lock (Gate)
                {
                    Stopped.Add(entry);
                    if (Stopped.Count > 30) Stopped.RemoveAt(0);
                    Total(Today()).Rejected++;
                }
            }
            catch { /* already gone, or not this user's to stop */ }
        }
    }

    static DateTime Minute(string key)
    {
        return DateTime.ParseExact(key, "yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
    }

    static void SavePending()
    {
        Dictionary<string, Dictionary<string, double>> copy;
        lock (Gate) { copy = Active.ToDictionary(p => p.Key, p => new Dictionary<string, double>(p.Value)); }
        try { File.WriteAllText(Path.Combine(Dir, "pendiente.json"), JsonSerializer.Serialize(copy)); } catch { }
    }

    static void LoadPending()
    {
        try
        {
            var path = Path.Combine(Dir, "pendiente.json");
            if (!File.Exists(path)) return;
            using (var doc = JsonDocument.Parse(File.ReadAllText(path)))
                foreach (var resource in doc.RootElement.EnumerateObject())
                {
                    var minutes = new Dictionary<string, double>();
                    foreach (var m in resource.Value.EnumerateObject()) if (m.Value.ValueKind == JsonValueKind.Number) minutes[m.Name] = m.Value.GetDouble();
                    Active[resource.Name] = minutes;
                }
        }
        catch { /* an unreadable file: those minutes are lost */ }
    }

    static void PriceLoop()
    {
        while (true)
        {
            try { ResolveTargets(); } catch (Exception ex) { Note("dns: " + ex.Message); }
            try { PriceDirect(); } catch (Exception ex) { Note("gasto directo: " + ex.Message); }
            Thread.Sleep(30000);
        }
    }

    /// Azure Monitor's token metrics of one resource, per minute and deployment (a free read with the az CLI).
    static string Metrics(string resourceId, DateTime start, DateTime end)
    {
        var psi = new ProcessStartInfo("cmd.exe") { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false, CreateNoWindow = true };
        foreach (var a in new[]
        {
            "/d", "/c", "az", "monitor", "metrics", "list", "--resource", resourceId, "--metrics", "InputTokens", "OutputTokens", "ModelRequests",
            "--interval", "PT1M", "--aggregation", "Total",
            "--start-time", start.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture),
            "--end-time", end.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture),
            "--filter", "ModelDeploymentName eq '*'", "-o", "json",
        }) psi.ArgumentList.Add(a);
        using (var p = Process.Start(psi))
        {
            var output = p.StandardOutput.ReadToEndAsync();
            var error = p.StandardError.ReadToEndAsync();
            if (!p.WaitForExit(120000)) { try { p.Kill(true); } catch { } throw new Exception("az no respondió a tiempo"); }
            if (p.ExitCode != 0)
            {
                var first = error.Result.Split('\n').FirstOrDefault(l => l.Trim() != "" && !l.StartsWith("WARNING")) ?? "";
                throw new Exception("az: " + first.Trim());
            }
            return output.Result;
        }
    }

    /// minute to deployment to { input, output } tokens.
    static Dictionary<string, Dictionary<string, long[]>> ParseMetrics(string json)
    {
        var byMinute = new Dictionary<string, Dictionary<string, long[]>>();
        using (var doc = JsonDocument.Parse(json))
        {
            JsonElement value;
            if (!doc.RootElement.TryGetProperty("value", out value)) return byMinute;
            foreach (var metric in value.EnumerateArray())
            {
                JsonElement nameEl, series;
                var metricName = metric.TryGetProperty("name", out nameEl) ? Str(nameEl, "value") : "";
                int slot = metricName == "OutputTokens" ? 1 : metricName == "ModelRequests" ? 2 : 0; // input, output, requests
                if (!metric.TryGetProperty("timeseries", out series)) continue;
                foreach (var s in series.EnumerateArray())
                {
                    var deployment = "";
                    JsonElement meta, data;
                    if (s.TryGetProperty("metadatavalues", out meta))
                        foreach (var mv in meta.EnumerateArray())
                        {
                            JsonElement dim;
                            if (mv.TryGetProperty("name", out dim) && Str(dim, "value").Equals("modeldeploymentname", StringComparison.OrdinalIgnoreCase))
                                deployment = Str(mv, "value").ToLowerInvariant();
                        }
                    if (!s.TryGetProperty("data", out data)) continue;
                    foreach (var point in data.EnumerateArray())
                    {
                        var total = (long)Num(point, "total");
                        var stamp = Str(point, "timeStamp");
                        if (total <= 0 || stamp.Length < 16) continue;
                        var minute = stamp.Substring(0, 16);
                        Dictionary<string, long[]> deployments;
                        if (!byMinute.TryGetValue(minute, out deployments)) { deployments = new Dictionary<string, long[]>(); byMinute[minute] = deployments; }
                        long[] tokens;
                        if (!deployments.TryGetValue(deployment, out tokens)) { tokens = new long[3]; deployments[deployment] = tokens; }
                        tokens[slot] += total;
                    }
                }
            }
        }
        return byMinute;
    }

    // How long the resource's requests of a minute lasted, estimated: Azure has no duration metric for
    // every model, so half a second per request plus the time to write its output tokens. On the low
    // side on purpose: this person's share then errs upward, the safe side for a limit.
    const double SecondsPerRequest = 0.5;
    const double SecondsPerOutputToken = 0.012;

    /// Charges this person a share of the resource's tokens of each minute a process here was connected
    /// to it: this machine's seconds of connection over the seconds all that minute's requests took
    /// (never more than all of it), on what did not pass through the meter. An estimate.
    static void PriceDirect()
    {
        var cfg = Current;
        var now = DateTime.UtcNow;
        var ready = now.AddMinutes(-4); // Azure Monitor takes a few minutes to have a minute's tokens
        var done = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var route in cfg.Routes)
        {
            if (route.ResourceId == "" || !done.Add(route.Name)) continue;
            List<string> due;
            var held = new Dictionary<string, double>();
            lock (Gate)
            {
                Dictionary<string, double> minutes;
                if (!Active.TryGetValue(route.Name, out minutes)) continue;
                foreach (var m in minutes.Keys.Where(m => now - Minute(m) > TimeSpan.FromHours(24)).ToList()) minutes.Remove(m); // too old to ask for: lost
                due = minutes.Keys.Where(m => Minute(m) <= ready).OrderBy(m => m).ToList();
                foreach (var m in due) held[m] = minutes[m];
            }
            if (due.Count == 0) continue;
            var byMinute = ParseMetrics(Metrics(route.ResourceId, Minute(due[0]), Minute(due[due.Count - 1]).AddMinutes(1)));
            foreach (var minute in due)
            {
                Dictionary<string, long[]> deployments;
                if (!byMinute.TryGetValue(minute, out deployments)) continue;
                double took = deployments.Values.Sum(v => v[2] * SecondsPerRequest + v[1] * SecondsPerOutputToken);
                double share = took > 0 ? Math.Min(1.0, held[minute] / took) : 1.0;
                foreach (var d in deployments)
                {
                    long input = d.Value[0], output = d.Value[1];
                    Price price;
                    bool isPriced = cfg.Prices.TryGetValue(route.Name + "|" + d.Key, out price);
                    if (!isPriced) price = cfg.Fallback;
                    var local = Minute(minute).ToLocalTime();
                    var day = local.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                    lock (Gate)
                    {
                        long[] through;
                        if (Proxied.TryGetValue(route.Name + "|" + d.Key + "|" + minute, out through))
                        {
                            input = Math.Max(0, input - through[0]);
                            output = Math.Max(0, output - through[1]);
                        }
                        // What passed through the meter is already counted; of the rest, this machine's share.
                        input = (long)Math.Round(input * share);
                        output = (long)Math.Round(output * share);
                        if (input + output == 0) continue;
                        double eur = input * price.In + output * price.Out;
                        var line = new Dictionary<string, object>();
                        line["t"] = minute + ":00Z";
                        line["day"] = day;
                        line["r"] = route.Name;
                        line["d"] = d.Key;
                        line["in"] = input;
                        line["cin"] = 0;
                        line["out"] = output;
                        line["eur"] = eur;
                        line["src"] = "directo";
                        line["share"] = Math.Round(share, 3);
                        if (!isPriced) line["np"] = true;
                        var t = Total(day);
                        t.Eur += eur;
                        t.Direct += eur;
                        try { File.AppendAllText(LedgerPath(local), JsonSerializer.Serialize(line) + "\n"); } catch (Exception ex) { if (Errors.Count < 20) Errors.Add("libro: " + ex.Message); }
                    }
                }
            }
            lock (Gate)
            {
                Dictionary<string, double> minutes;
                if (Active.TryGetValue(route.Name, out minutes)) foreach (var m in due) minutes.Remove(m);
                var old = now.AddHours(-26).ToString("yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture);
                foreach (var key in Proxied.Keys.Where(k => string.CompareOrdinal(k.Substring(k.LastIndexOf('|') + 1), old) < 0).ToList()) Proxied.Remove(key);
            }
        }
    }
}
