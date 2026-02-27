using System;
using System.Collections.Generic;

namespace DotNetReelGenerator
{
    /// <summary>
    /// Represents a complete Reel script with all metadata
    /// </summary>
    public class ReelScript
    {
        public string Topic { get; set; }
        public string Narration { get; set; }
        public string BackgroundStyle { get; set; }
        public string MusicTheme { get; set; }
        public List<string> KeyPoints { get; set; } = new();
        public string CallToAction { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int WordCount => Narration?.Split(' ').Length ?? 0;
        public int EstimatedDurationSeconds => Math.Max(30, WordCount / 2); // ~120 WPM
    }

    /// <summary>
    /// Configuration for the Reel Generator
    /// </summary>
    public class ReelGeneratorConfig
    {
        public string TargetPlatform { get; set; } = "Instagram"; // Instagram, LinkedIn, TikTok
        public string VideoFormat { get; set; } = "1080x1920"; // Reel format
        public int DurationSeconds { get; set; } = 30;
        public string OutputQuality { get; set; } = "720p"; // 480p, 720p, 1080p
        public string VoiceStyle { get; set; } = "professional_energetic"; // professional, casual, technical
        public string BackgroundMusicGenre { get; set; } = "tech_upbeat"; // tech_upbeat, ambient, cinematic
        public bool IncludeSubtitles { get; set; } = true;
        public bool IncludeAnimations { get; set; } = true;
        public string BrandColor { get; set; } = "#0078D4"; // Microsoft .NET Blue
    }
}
