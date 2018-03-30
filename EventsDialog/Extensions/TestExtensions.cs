using HtmlAgilityPack;

namespace EventsBot.Extensions
{
    public static class TestExtensions
    {
        public static string StripHtml(this string value)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(value);
            
            return htmlDoc.DocumentNode.InnerText;
        }
    }
}