using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;

namespace DotNetReelGenerator
{
    /// <summary>
    /// Multi-AI Script Generator with Intelligent Fallback
    /// Tries multiple AI providers in order: Grok → Gemini → OpenAI → Claude → Fallback
    /// FIXED: Proper API key handling (uses environment variables)
    /// </summary>
    public class MultiAIScriptGenerator
    {
        private readonly HttpClient _httpClient;
        private readonly Dictionary<string, string> _apiKeys;
        private List<string> _successLog = new();
        private List<string> _failureLog = new();

        // AI Provider Configuration
        private const string GROK_API_URL = "https://api.x.ai/v1/chat/completions";
        private const string GEMINI_API_URL = "https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent";
        private const string OPENAI_API_URL = "https://api.openai.com/v1/chat/completions";
        private const string CLAUDE_API_URL = "https://api.anthropic.com/v1/messages";

        // Models to use
        private const string GROK_MODEL = "grok-beta";
        private const string GEMINI_MODEL = "gemini-pro";
        private const string OPENAI_MODEL = "gpt-3.5-turbo";
        private const string CLAUDE_MODEL = "claude-opus-4-5-20251101";

        public enum AIProvider
        {
            Grok,
            Gemini,
            OpenAI,
            Claude,
            Fallback
        }

        public MultiAIScriptGenerator()
        {
            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            _apiKeys = LoadAPIKeys();
        }

        /// <summary>
        /// Load API keys from environment variables (FIXED)
        /// </summary>
        private Dictionary<string, string> LoadAPIKeys()
        {
            return new Dictionary<string, string>
            {
                ["grok"] = Environment.GetEnvironmentVariable("GROK_API_KEY") ?? "",
                ["gemini"] = Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? "",
                ["openai"] = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "",
                ["claude"] = Environment.GetEnvironmentVariable("CLAUDE_API_KEY") ?? ""
            };
        }

        /// <summary>
        /// Generate script with intelligent multi-AI fallback
        /// </summary>
        public async Task<(ReelScript script, AIProvider usedProvider)> GenerateScriptWithFallback(string topic)
        {
            Console.WriteLine($"\n🤖 Attempting to generate script for: '{topic}'");
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

            // Try each AI provider in order
            var providers = new[]
            {
                (AIProvider.Grok, "🟠 Grok (X.AI)"),
                (AIProvider.Gemini, "🔵 Google Gemini"),
                (AIProvider.OpenAI, "⚫ OpenAI GPT"),
                (AIProvider.Claude, "🔴 Claude (Anthropic)"),
                (AIProvider.Fallback, "📋 Fallback (Hardcoded)")
            };

            foreach (var (provider, label) in providers)
            {
                Console.Write($"  {label}... ");

                try
                {
                    var script = provider switch
                    {
                        AIProvider.Grok => await GenerateWithGrok(topic),
                        AIProvider.Gemini => await GenerateWithGemini(topic),
                        AIProvider.OpenAI => await GenerateWithOpenAI(topic),
                        AIProvider.Claude => await GenerateWithClaude(topic),
                        AIProvider.Fallback => GenerateFallbackScript(topic),
                        _ => null
                    };

                    if (script != null)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("✅ SUCCESS!");
                        Console.ResetColor();
                        _successLog.Add($"[{DateTime.Now:HH:mm:ss}] {label} - Topic: {topic}");
                        return (script, provider);
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    var errorMsg = ex.Message.Length > 40 ? ex.Message.Substring(0, 40) : ex.Message;
                    Console.WriteLine($"⚠️  Failed ({errorMsg})");
                    Console.ResetColor();
                    _failureLog.Add($"[{DateTime.Now:HH:mm:ss}] {label} - {ex.Message}");
                }
            }

            // Final fallback
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("📋 Using Hardcoded Fallback Script");
            Console.ResetColor();

            return (GenerateFallbackScript(topic), AIProvider.Fallback);
        }

        /// <summary>
        /// Generate using Grok (X.AI)
        /// </summary>
        private async Task<ReelScript> GenerateWithGrok(string topic)
        {
            var apiKey = _apiKeys["grok"];
            if (string.IsNullOrEmpty(apiKey))
                throw new InvalidOperationException("GROK_API_KEY not set. Set environment variable: GROK_API_KEY");

            var prompt = BuildPrompt(topic);

            var request = new
            {
                model = GROK_MODEL,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                temperature = 0.7,
                max_tokens = 200
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var response = await _httpClient.PostAsync(GROK_API_URL, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Grok API error: {response.StatusCode} - {responseBody}");

            try
            {
                var result = JsonSerializer.Deserialize<JsonElement>(responseBody);
                var narration = result
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                return BuildReelScript(topic, narration);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to parse Grok response: {ex.Message}");
            }
        }

        /// <summary>
        /// Generate using Google Gemini
        /// </summary>
        private async Task<ReelScript> GenerateWithGemini(string topic)
        {
            var apiKey = _apiKeys["gemini"];
            if (string.IsNullOrEmpty(apiKey))
                throw new InvalidOperationException("GEMINI_API_KEY not set. Set environment variable: GEMINI_API_KEY");

            var prompt = BuildPrompt(topic);

            var request = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.7,
                    maxOutputTokens = 200
                }
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();

            var url = $"{GEMINI_API_URL}?key={apiKey}";
            var response = await _httpClient.PostAsync(url, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Gemini API error: {response.StatusCode} - {responseBody}");

            try
            {
                var result = JsonSerializer.Deserialize<JsonElement>(responseBody);
                var narration = result
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                return BuildReelScript(topic, narration);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to parse Gemini response: {ex.Message}");
            }
        }

        /// <summary>
        /// Generate using OpenAI GPT
        /// </summary>
        private async Task<ReelScript> GenerateWithOpenAI(string topic)
        {
            var apiKey = _apiKeys["openai"];
            if (string.IsNullOrEmpty(apiKey))
                throw new InvalidOperationException("OPENAI_API_KEY not set. Set environment variable: OPENAI_API_KEY");

            var prompt = BuildPrompt(topic);

            var request = new
            {
                model = OPENAI_MODEL,
                messages = new[]
                {
                    new { role = "system", content = "You are a professional .NET educator." },
                    new { role = "user", content = prompt }
                },
                temperature = 0.7,
                max_tokens = 200
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var response = await _httpClient.PostAsync(OPENAI_API_URL, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"OpenAI API error: {response.StatusCode} - {responseBody}");

            try
            {
                var result = JsonSerializer.Deserialize<JsonElement>(responseBody);
                var narration = result
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                return BuildReelScript(topic, narration);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to parse OpenAI response: {ex.Message}");
            }
        }

        /// <summary>
        /// Generate using Claude (Anthropic)
        /// </summary>
        private async Task<ReelScript> GenerateWithClaude(string topic)
        {
            var apiKey = _apiKeys["claude"];
            if (string.IsNullOrEmpty(apiKey))
                throw new InvalidOperationException("CLAUDE_API_KEY not set. Set environment variable: CLAUDE_API_KEY");

            var prompt = BuildPrompt(topic);

            var request = new
            {
                model = CLAUDE_MODEL,
                max_tokens = 200,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
            _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

            var response = await _httpClient.PostAsync(CLAUDE_API_URL, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Claude API error: {response.StatusCode} - {responseBody}");

            try
            {
                var result = JsonSerializer.Deserialize<JsonElement>(responseBody);
                var narration = result
                    .GetProperty("content")[0]
                    .GetProperty("text")
                    .GetString();

                return BuildReelScript(topic, narration);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to parse Claude response: {ex.Message}");
            }
        }

        /// <summary>
        /// Fallback: Use hardcoded scripts for 7 pre-built topics
        /// </summary>
        private ReelScript GenerateFallbackScript(string topic)
        {
            var narrations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["LINQ Performance Tips"] =
                    "Did you know? Most developers write inefficient LINQ queries. Here's the secret: avoid multiple iterations. Instead of chaining Where then Select then ToList, combine them. Use deferred execution wisely. Profile your code! This simple change can reduce query time by 70%. That's the power of understanding LINQ!",

                ["Async Await Best Practices"] =
                    "Async Await can be tricky. First mistake: using async void except for event handlers. Always return Task or Task<T>. Second: forgetting to await. This creates fire-and-forget bugs. Third: mixing sync and async code. Use async all the way. Never use Result or Wait on async code. These changes prevent deadlocks and make your app responsive. Master async, master .NET!",

                ["Dependency Injection in .NET"] =
                    "Dependency Injection is NOT optional. It makes testing easy, code modular, and dependencies explicit. In .NET, it's built-in! Use the service collection in Startup. Register your services: AddScoped, AddSingleton, AddTransient. Understand the difference. Scoped dies with the request. Singleton lives forever. Transient is always new. Get this right, your architecture improves dramatically!",

                ["Entity Framework Core Optimization"] =
                    "Entity Framework Core is powerful but slow if misused. Number one mistake: N plus one queries. Use Include to eager load. Number two: fetching too much data. Use Select to project only what you need. Number three: not using AsNoTracking when you're just reading. These three tricks can speed up your app 10x times!",

                ["Unit Testing with xUnit"] =
                    "xUnit is the modern test framework for .NET. Why? It's clean, extensible, and has no static dependencies. Write Arrange Act Assert. Keep tests focused. Test one thing. Mock external dependencies with Moq. Run tests in CI CD every commit. Good tests catch bugs before production. They're insurance for your code!",

                ["Docker for .NET Applications"] =
                    "Containerize your .NET app with Docker. It's not hard. Create a Dockerfile, define your base image, copy your code, expose a port, run it. Docker ensures your app runs the same everywhere. Dev, staging, production. No more it works on my machine excuses! Plus, it's container orchestration ready with Kubernetes!",

                ["Minimal APIs in .NET"] =
                    "Minimal APIs are the future. No controllers, no ceremony. Just pure endpoint logic. Map Get Post Put Delete in a few lines. Less boilerplate, more clarity. Perfect for microservices and prototypes. Combine with dependency injection for power. This is modern dotnet!"
            };

            // Try exact match first
            if (narrations.TryGetValue(topic, out var narration))
                return BuildReelScript(topic, narration);

            // If topic not found, generate a generic script
            var genericScript = $"Master {topic.ToLower()} today! This technique transforms your .NET skills. " +
                $"Learn the pattern, apply it, see results. Your code gets faster, cleaner, and more maintainable. " +
                $"Performance improves, bugs decrease, and your team will thank you. Let's dive in!";

            return BuildReelScript(topic, genericScript);
        }

        /// <summary>
        /// Build the AI prompt
        /// </summary>
        private string BuildPrompt(string topic)
        {
            return $@"You are a professional .NET educator creating a 30-second Instagram Reel script.

Topic: {topic}

Requirements:
1. Write EXACTLY 60-75 words (crucial for 30-second timing)
2. Start with a hook that grabs attention immediately
3. Explain ONE key concept clearly and concisely
4. Include one practical, actionable example
5. End with a clear call to action
6. Use conversational, energetic tone
7. Include relevant technical terms but keep it accessible
8. Make it memorable and shareable

Format your response as just the narration text, nothing else. No preamble, no explanation, just the script.";
        }

        /// <summary>
        /// Build a ReelScript from narration
        /// </summary>
        private ReelScript BuildReelScript(string topic, string narration)
        {
            if (string.IsNullOrWhiteSpace(narration))
                throw new InvalidOperationException("Generated narration is empty");

            return new ReelScript
            {
                Topic = topic,
                Narration = narration.Trim(),
                BackgroundStyle = "Tech minimal with modern animations",
                MusicTheme = "Upbeat tech background (royalty-free)",
                KeyPoints = ExtractKeyPoints(narration),
                CallToAction = "Follow for more .NET tips daily! 👍"
            };
        }

        /// <summary>
        /// Extract key points from narration
        /// </summary>
        private List<string> ExtractKeyPoints(string narration)
        {
            var sentences = narration.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
            var keyPoints = new List<string>();

            foreach (var sentence in sentences)
            {
                var trimmed = sentence.Trim();
                if (trimmed.Length > 10)
                {
                    keyPoints.Add(trimmed);
                    if (keyPoints.Count >= 3) break;
                }
            }

            return keyPoints.Count == 0 ? new List<string> { narration.Substring(0, Math.Min(50, narration.Length)) } : keyPoints;
        }

        /// <summary>
        /// Get status report on AI providers
        /// </summary>
        public void PrintStatusReport()
        {
            Console.WriteLine("\n╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║  🤖 AI PROVIDER STATUS REPORT                              ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");

            var providers = new[]
            {
                ("Grok (X.AI)", _apiKeys["grok"]),
                ("Google Gemini", _apiKeys["gemini"]),
                ("OpenAI GPT", _apiKeys["openai"]),
                ("Claude (Anthropic)", _apiKeys["claude"])
            };

            foreach (var (name, key) in providers)
            {
                var status = string.IsNullOrEmpty(key) ? "❌ Not configured" : "✅ Ready";
                Console.WriteLine($"  {name:,-25} {status}");
            }

            Console.WriteLine("\n📝 Success Log:");
            if (_successLog.Count > 0)
            {
                foreach (var log in _successLog.TakeLast(5))
                {
                    Console.WriteLine($"  {log}");
                }
            }
            else
            {
                Console.WriteLine("  (No successes yet)");
            }

            Console.WriteLine("\n⚠️  Failure Log:");
            if (_failureLog.Count > 0)
            {
                foreach (var log in _failureLog.TakeLast(5))
                {
                    Console.WriteLine($"  {log}");
                }
            }
            else
            {
                Console.WriteLine("  (No failures)");
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Get list of available hardcoded topics
        /// </summary>
        public static List<string> GetAvailableTopics()
        {
            return new List<string>
            {
                "LINQ Performance Tips",
                "Async Await Best Practices",
                "Dependency Injection in .NET",
                "Entity Framework Core Optimization",
                "Unit Testing with xUnit",
                "Docker for .NET Applications",
                "Minimal APIs in .NET"
            };
        }
    }

    /// <summary>
    /// Configuration for multi-AI setup
    /// </summary>
    public class MultiAIConfig
    {
        public bool EnableGrok { get; set; } = true;
        public bool EnableGemini { get; set; } = true;
        public bool EnableOpenAI { get; set; } = true;
        public bool EnableClaude { get; set; } = true;
        public int TimeoutSeconds { get; set; } = 30;
        public int MaxRetries { get; set; } = 2;
        public bool LogDetailedFailures { get; set; } = true;
    }
}
