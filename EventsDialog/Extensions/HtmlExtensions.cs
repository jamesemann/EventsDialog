using HtmlAgilityPack;

namespace EventsDialog.Extensions
{
    public static class HtmlExtensions
    {
        public static string StripHtml(this string value)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(value);
            
            return htmlDoc.DocumentNode.InnerText;
        }
    }
}