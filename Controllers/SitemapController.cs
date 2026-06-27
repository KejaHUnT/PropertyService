using KejaHUnt_PropertiesAPI.Repositories.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace KejaHUnt_PropertiesAPI.Controllers
{
    [ApiController]
    public class SitemapController : ControllerBase
    {
        private readonly IPropertyRepository _propertyRepository;

        public SitemapController(IPropertyRepository propertyRepository)
        {
            _propertyRepository = propertyRepository;
        }

        [HttpGet("/sitemap.xml")]
        [HttpGet("/properties/sitemap.xml")]
        public async Task<IActionResult> Sitemap()
        {
            var baseUrl = "https://kejahunt.co.ke";

            // Static public routes (from Angular routing)
            var urls = new List<string>
            {
                $"{baseUrl}/",
                $"{baseUrl}/houses",
                $"{baseUrl}/properties",
                $"{baseUrl}/get-started",
            };

            // Dynamic property detail pages
            var properties = await _propertyRepository.GetAllAsync();
            foreach (var property in properties)
            {
                urls.Add($"{baseUrl}/property/details/{property.Id}");
            }

            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");
            foreach (var url in urls)
            {
                sb.AppendLine($"  <url><loc>{url}</loc></url>");
            }
            sb.AppendLine("</urlset>");

            return Content(sb.ToString(), "application/xml", Encoding.UTF8);
        }

        [HttpGet("/robots.txt")]
        [HttpGet("/properties/robots.txt")]
        public IActionResult Robots()
        {
            var content = @"User-agent: *
Allow: /
Disallow: /admin/
Disallow: /portal/
Disallow: /dashboard/
Disallow: /payment/
Disallow: /signin
Disallow: /booking/

Sitemap: https://kejahunt.co.ke/sitemap.xml";

            return Content(content, "text/plain", Encoding.UTF8);
        }
    }
}