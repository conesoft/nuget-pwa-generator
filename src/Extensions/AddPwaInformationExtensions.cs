using Conesoft.Tools;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Conesoft.PwaGenerator;

[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public static class AddPwaInformationExtensions
{
    private static readonly string[] bots = ["GPTBot", "ChatGPT-User", "Google-Extended", "PerplexityBot", "Amazonbot", "ClaudeBot", "Omgilibot", "FacebookBot", "Applebot", "anthropic-ai", "Bytespider", "Claude-Web", "Diffbot", "ImagesiftBot", "Omgilibot", "Omgili", "YouBot"];

    public static WebApplication MapPwaInformationFromAppSettings(this WebApplication app)
    {
        var c = app.Services.GetRequiredService<IConfiguration>();
        var name = c.GetValue<string>("pwa:name") ?? throw new Exception("appsettings.json configuration wrong, pwa:name not found");
        var url = c.GetValue<string>("pwa:url") ?? throw new Exception("appsettings.json configuration wrong, pwa:url not found");
        var description = c.GetValue<string>("pwa:description") ?? throw new Exception("appsettings.json configuration wrong, pwa:description not found");
        var configuration = Safe.Try(() => File.ReadAllText(@"wwwroot\site.webmanifest")) ?? "{}";

        return app.MapPwaInformation(name, url, description, configuration);
    }

    public static WebApplication MapPwaInformation(this WebApplication app, string name, string url, string description, string configuration)
    {
        var svg = File.Exists($"wwwroot/meta/favicon.svg");
        var png = File.Exists($"wwwroot/meta/favicon.png");
        var jpg = File.Exists($"wwwroot/meta/opengraph.jpg");
        var jpgmobile = File.Exists($"wwwroot/meta/opengraph.narrow.jpg");
        if (png == false)
        {
            throw new Exception("wwwroot configuration wrong, wwwroot/meta/favicon.png is missing");
        }
        var svgicon = new
        {
            src = "/meta/favicon.svg",
            sizes = "48x48 72x72 96x96 128x128 256x256 512x512",
            type = "image/svg+xml",
            purpose = "any"
        };
        var pngicon = new
        {
            src = "/meta/favicon.png",
            sizes = "512x512",
            type = "image/png",
            purpose = "any"
        };
        var icons = svg ? [svgicon, pngicon] : new[] { pngicon };

        var screenshot0 = new
        {
            src = "/meta/opengraph.jpg",
            sizes = "1200x630",
            form_factor = "wide",
            label = "Desktop view of {{name}}"
        };

        var screenshot1 = new
        {
            src = "/meta/opengraph.narrow.jpg",
            sizes = "630x1200",
            form_factor = "narrow",
            label = "Mobile view of {{name}}"
        };
        var screenshots = jpg ? jpgmobile ? new[] { screenshot0, screenshot1 } : [screenshot0] : [];

        var json = new
        {
            name,
            short_name = name,
            description,
            id = url,
            orientation = "any",
            lang = "en",
            dir = "auto",
            categories = new[] { "utilities " },
            icons,
            screenshots,
            theme_color = "#000000",
            background_color = "#000000",
            start_url = url,
            display = "standalone",
            display_override = new[] { "window-controls-overlay" }
        };

        var main = JsonSerializer.SerializeToNode(json)!;
        var extensions = JsonNode.Parse(configuration)!;

        var output = main.Merge(extensions);

        app.MapGet("/pwa/site.webmanifest", () => Results.Json(output, contentType: "application/manifest+json"));
        app.MapGet("/robots.txt", () => string.Join("\r\n", bots.Select(bot => $"User-agent: {bot}\r\nDisallow: /")));
        return app;
    }
}