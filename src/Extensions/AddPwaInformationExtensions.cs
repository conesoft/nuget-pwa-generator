using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;
using System.IO;

namespace Conesoft.PwaGenerator;

[Browsable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public static class AddPwaInformationExtensions
{
    public static WebApplication MapPwaInformationFromAppSettings(this WebApplication app)
    {
        var configuration = app.Services.GetRequiredService<IConfiguration>();
        var name = configuration.GetValue<string>("pwa:name") ?? throw new Exception("appsettings.json configuration wrong, pwa:name not found");
        var url = configuration.GetValue<string>("pwa:url") ?? throw new Exception("appsettings.json configuration wrong, pwa:url not found");
        var description = configuration.GetValue<string>("pwa:description") ?? throw new Exception("appsettings.json configuration wrong, pwa:description not found");

        return app.MapPwaInformation(name, url, description);
    }

    public static WebApplication MapPwaInformation(this WebApplication app, string name, string url, string description)
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

        app.MapGet("/pwa/site.webmanifest", () => Results.Json(json, contentType: "application/manifest+json"));
        return app;
    }
}