# 🤖 Multi-AI Fallback System Setup Guide

## Overview

Your .NET Reel Generator now uses an **intelligent multi-AI fallback system** that automatically tries multiple AI providers in sequence:

```
Grok (X.AI)
    ↓ (if fails)
Google Gemini  
    ↓ (if fails)
OpenAI GPT
    ↓ (if fails)
Claude (Anthropic)
    ↓ (if fails)
Hardcoded Fallback (7 pre-built topics)
```

**This ensures your Reel always gets generated**, regardless of which AI provider fails.

---

## 🚀 Quick Start (No Setup Required)

The system works **immediately without any configuration**. It will:
1. Attempt Grok first (if API key available)
2. Fall back to Gemini (if API key available)
3. Fall back to OpenAI (if API key available)
4. Fall back to Claude (if API key available)
5. **Always fall back to hardcoded scripts** (no internet needed)

No API keys required. It just works! ✅

---

## 🔑 Setup Each AI Provider (Optional)

To enable AI providers, set environment variables. Choose **any combination**:

### 1. Grok (X.AI) - Recommended ⭐

Get API key from: https://console.x.ai/

```bash
# Linux/macOS:
export GROK_API_KEY="your_grok_key_here"

# Windows PowerShell:
$env:GROK_API_KEY = "your_grok_key_here"

# Windows CMD:
set GROK_API_KEY=your_grok_key_here

# Verify:
echo $GROK_API_KEY
```

**Cost:** Free tier available, pay-as-you-go
**Speed:** Fast (~2-5 seconds per request)
**Quality:** Excellent for technical topics

---

### 2. Google Gemini

Get API key from: https://makersuite.google.com/app/apikey

```bash
export GEMINI_API_KEY="your_gemini_key_here"
```

**Cost:** Free tier (50 req/min), paid plans available
**Speed:** Fast (~3-7 seconds)
**Quality:** Great for explanations

---

### 3. OpenAI GPT

Get API key from: https://platform.openai.com/api-keys

```bash
export OPENAI_API_KEY="sk-proj-xxxxxxxxxx"
```

**Cost:** Pay-as-you-go (~$0.002-0.01 per request)
**Speed:** Medium (~2-5 seconds)
**Quality:** Excellent quality

---

### 4. Claude (Anthropic)

Get API key from: https://console.anthropic.com/

```bash
export CLAUDE_API_KEY="sk-ant-xxxxxxxxxx"
```

**Cost:** Pay-as-you-go (~$0.003-0.015 per request)
**Speed:** Medium (~3-8 seconds)
**Quality:** Excellent quality

---

## 📋 Setting All Keys at Once

### Option A: Bash Script (Linux/macOS)

Create `.env.sh`:
```bash
#!/bin/bash
export GROK_API_KEY="sk-xxxx"
export GEMINI_API_KEY="xxxx"
export OPENAI_API_KEY="sk-xxxx"
export CLAUDE_API_KEY="sk-ant-xxxx"
```

Then run before starting:
```bash
source .env.sh
dotnet run
```

### Option B: PowerShell (Windows)

Create `setup-keys.ps1`:
```powershell
$env:GROK_API_KEY = "sk-xxxx"
$env:GEMINI_API_KEY = "xxxx"
$env:OPENAI_API_KEY = "sk-xxxx"
$env:CLAUDE_API_KEY = "sk-ant-xxxx"
```

Run:
```powershell
.\setup-keys.ps1
dotnet run
```

### Option C: .NET User Secrets (Secure)

```bash
cd DotNetReelGenerator
dotnet user-secrets init

dotnet user-secrets set "GROK_API_KEY" "sk-xxxx"
dotnet user-secrets set "GEMINI_API_KEY" "xxxx"
dotnet user-secrets set "OPENAI_API_KEY" "sk-xxxx"
dotnet user-secrets set "CLAUDE_API_KEY" "sk-ant-xxxx"

# Verify
dotnet user-secrets list
```

---

## 🎯 Cost Comparison

| Provider | Cost per 100 Reels | Setup | Speed | Quality |
|----------|-------------------|-------|-------|---------|
| **Grok** | ~$0-2 | 5 min | ⚡⚡⚡ | ⭐⭐⭐⭐⭐ |
| **Gemini** | ~$0 (free tier) | 5 min | ⚡⚡ | ⭐⭐⭐⭐ |
| **OpenAI** | ~$0.10-1 | 5 min | ⚡⚡ | ⭐⭐⭐⭐⭐ |
| **Claude** | ~$0.15-1.50 | 5 min | ⚡⚡ | ⭐⭐⭐⭐⭐ |
| **Fallback** | FREE | 0 min | ⚡⚡⚡⚡ | ⭐⭐⭐⭐ |

**Recommendation:** Start with free Gemini tier, add Grok for speed!

---

## 🔍 How the Fallback Works

### Example Scenario

```
Topic: "Async Await Best Practices"

1️⃣ Try Grok API
   → Network timeout ❌
   
2️⃣ Try Gemini API
   → Rate limit exceeded ❌
   
3️⃣ Try OpenAI API
   → Insufficient credits ❌
   
4️⃣ Try Claude API
   → Success! ✅
   
Result: Uses Claude-generated script
```

### If All Fail

```
Topic: "Custom async patterns"

1️⃣ Try Grok → Fail ❌
2️⃣ Try Gemini → Fail ❌
3️⃣ Try OpenAI → Fail ❌
4️⃣ Try Claude → Fail ❌

5️⃣ Use Fallback Hardcoded
   → Generic script based on topic ✅
   
Result: Reel still generates (lower quality but functional)
```

---

## 📊 AI Provider Status Report

When you run the app, you'll see:

```
╔════════════════════════════════════════════════════════════╗
║  🤖 AI PROVIDER STATUS REPORT                              ║
╚════════════════════════════════════════════════════════════╝

  Grok (X.AI)              ✅ Ready
  Google Gemini            ✅ Ready
  OpenAI GPT               ❌ Not configured
  Claude (Anthropic)       ✅ Ready

📝 Success Log:
  [12:34:56] 🟠 Grok (X.AI) - Topic: LINQ Performance Tips
  [12:35:12] 🔵 Google Gemini - Topic: Async Await Best Practices

⚠️  Failure Log:
  (No failures)
```

---

## 🛠️ Configuration Options

Edit `MultiAIConfig` in code (optional):

```csharp
public class MultiAIConfig
{
    public bool EnableGrok { get; set; } = true;      // Skip Grok?
    public bool EnableGemini { get; set; } = true;    // Skip Gemini?
    public bool EnableOpenAI { get; set; } = true;    // Skip OpenAI?
    public bool EnableClaude { get; set; } = true;    // Skip Claude?
    public int TimeoutSeconds { get; set; } = 30;     // API timeout
    public int MaxRetries { get; set; } = 2;          // Retry count
    public bool LogDetailedFailures { get; set; } = true;
}
```

---

## 📈 Monitoring & Diagnostics

### Check Success Rate

The app logs successes and failures automatically:

```bash
# Linux/macOS - Watch real-time logs
tail -f reel_generation.log

# Windows PowerShell
Get-Content reel_generation.log -Wait
```

### Debug Failed Generation

If an AI provider fails:

```bash
# Check your internet connection
ping api.x.ai
ping api.openai.com
ping generativelanguage.googleapis.com
ping api.anthropic.com

# Verify API key is set
echo $GROK_API_KEY

# Test API directly
curl -H "Authorization: Bearer $GROK_API_KEY" \
  https://api.x.ai/v1/models
```

---

## ✨ Best Practices

### 1. Test Each Provider Individually

```bash
# Create test script
cat > test_ai.sh << 'EOF'
#!/bin/bash
export GROK_API_KEY="your_key"
dotnet run
# Select topic: "LINQ Performance Tips"
# Check output
EOF

chmod +x test_ai.sh
./test_ai.sh
```

### 2. Use Fastest Provider First

Order in code: **Grok → Gemini → OpenAI → Claude**
- Grok is fastest but needs newer account
- Gemini has free tier
- OpenAI/Claude are most reliable

### 3. Set Timeout Based on Your Network

```csharp
// In MultiAIConfig:
public int TimeoutSeconds { get; set; } = 30; // Increase if slow internet
```

### 4. Monitor API Credits

- **OpenAI:** Check https://platform.openai.com/usage
- **Claude:** Check https://console.anthropic.com
- **Gemini:** Check https://makersuite.google.com
- **Grok:** Check https://console.x.ai

### 5. Daily Cost Tracking

Create `track_costs.sh`:
```bash
#!/bin/bash
# Log each generation with timestamp and provider used
echo "$(date): Generated Reel - Provider: $1" >> daily_costs.log
```

---

## 🆘 Troubleshooting

### Problem: "API Key Not Set"

**Solution:**
```bash
# Verify key is actually set
printenv | grep API_KEY

# If empty, set it again
export GROK_API_KEY="sk-xxxx"

# Make permanent (Linux/macOS ~/.bashrc or ~/.zshrc):
echo 'export GROK_API_KEY="sk-xxxx"' >> ~/.bashrc
source ~/.bashrc
```

### Problem: "Timeout After 30 Seconds"

**Solution:**
```csharp
// Increase timeout in code
public int TimeoutSeconds { get; set; } = 60; // 60 seconds
```

Or:
```bash
# Check internet connection
ping google.com

# Check API endpoint
curl https://api.x.ai/v1/models
```

### Problem: "All Providers Failed, Using Fallback"

This is **normal and expected** if:
- No API keys are set (uses hardcoded topics)
- All providers are down
- Your internet is disconnected

The reel will still generate with fallback scripts! ✅

### Problem: "Rate Limited"

**Solution:** Wait a few minutes or switch providers
```bash
# If Grok fails, system automatically tries Gemini
# If Gemini fails, system tries OpenAI
# etc.
```

---

## 🎬 File Structure

```
DotNetReelGenerator/
├── DotNetReelGenerator_v2.cs       ← NEW: Multi-AI integrated main app
├── MultiAIScriptGenerator.cs       ← NEW: AI fallback system
├── Models.cs                        ← Shared data models
├── AIScriptGenerator.cs            ← Legacy (can delete)
└── DotNetReelGenerator.csproj      ← Project file
```

---

## 📝 Usage Examples

### Example 1: Using Free Gemini Only

```bash
export GEMINI_API_KEY="AIzaSyDxxxxxx"
# No GROK, OPENAI, or CLAUDE keys set
dotnet run
# Falls back to Gemini then hardcoded
```

### Example 2: Using All Providers

```bash
export GROK_API_KEY="grok-xxxx"
export GEMINI_API_KEY="AIzaSyDxxxxxx"
export OPENAI_API_KEY="sk-xxxx"
export CLAUDE_API_KEY="sk-ant-xxxx"
dotnet run
# Tries all in order, uses first that succeeds
```

### Example 3: Offline Mode (No API Keys)

```bash
# Don't set any API keys
dotnet run
# Always uses hardcoded fallback (works offline!)
```

---

## 🚀 Production Deployment

For automated daily posting:

```bash
#!/bin/bash
# setup_reel_generation.sh

# Set API keys securely
export GROK_API_KEY="$(cat ~/.secrets/grok_key)"
export OPENAI_API_KEY="$(cat ~/.secrets/openai_key)"

# Run generator
cd /opt/dotnet_reel_generator
dotnet run

# Monitor for failures
if [ $? -ne 0 ]; then
  # Send alert
  curl -X POST $SLACK_WEBHOOK -d "Reel generation failed!"
fi
```

Add to crontab:
```bash
0 10 * * * /opt/scripts/setup_reel_generation.sh >> /var/log/reel_gen.log 2>&1
```

---

## 📚 API Reference

### Grok (X.AI)
- **Docs:** https://docs.x.ai/
- **Models:** grok-beta
- **Status:** https://status.x.ai

### Google Gemini
- **Docs:** https://ai.google.dev/docs
- **Models:** gemini-pro
- **Status:** https://status.ai.google/

### OpenAI
- **Docs:** https://platform.openai.com/docs
- **Models:** gpt-3.5-turbo, gpt-4
- **Status:** https://status.openai.com/

### Claude (Anthropic)
- **Docs:** https://docs.anthropic.com/
- **Models:** claude-opus-4-5-20251101
- **Status:** https://www.anthropic.com/

---

## ✅ Verification Checklist

- [ ] At least one API key is set (optional)
- [ ] Can run `dotnet run` without errors
- [ ] Can select a topic from the list
- [ ] Reel generates (AI or fallback)
- [ ] Output appears in `output_reels/`
- [ ] Status report shows provider used

---

## 🎉 You're All Set!

Your Reel Generator now has:
- ✅ Multi-AI fallback system
- ✅ Automatic provider selection
- ✅ Hardcoded fallback (always works)
- ✅ Status reporting
- ✅ Error logging
- ✅ Zero required configuration

**Run it now:**
```bash
dotnet run
```

**It just works!** 🚀

