using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MarkopTest
{
    public static class Extensions
    {
        public static string GetUrlWithQuery(string url, dynamic query)
        {
            if (query == null)
                return url;
            var result = url + "?";
            var properties = query.GetType().GetProperties();
            foreach (var p in properties)
            {
                var value = p.GetValue(query, null);
                result += p.Name + "=" + (value?.ToString() ?? "null") + "&";
            }
            return result.TrimEnd('&');
        }
        public static async Task<string> GetContent(this HttpResponseMessage message)
        {
            var content = Regex.Replace(
                await message.Content.ReadAsStringAsync(),
                @"\\u(?<Value>[a-fA-F0-9]{4})",
                m => ((char) int.Parse(m.Groups["Value"].Value, NumberStyles.HexNumber)).ToString());

            try
            {
                if (!content.StartsWith("{"))
                    throw new Exception();

                var indentation = 0;
                var quoteCount = 0;
                const string indentString = "    ";
                var result =
                    from ch in content
                    let quotes = ch == '"' ? quoteCount++ : quoteCount
                    let lineBreak = ch == ',' && quotes % 2 == 0
                        ? ch + Environment.NewLine + string.Concat(Enumerable.Repeat(indentString, indentation))
                        : null
                    let openChar = ch == '{' || ch == '['
                        ? ch + Environment.NewLine + string.Concat(Enumerable.Repeat(indentString, ++indentation))
                        : ch.ToString()
                    let closeChar = ch == '}' || ch == ']'
                        ? Environment.NewLine + string.Concat(Enumerable.Repeat(indentString, --indentation)) + ch
                        : ch.ToString()
                    select lineBreak ?? (openChar.Length > 1
                        ? openChar
                        : closeChar);

                return string.Concat(result);
            }
            catch
            {
                return content;
            }
        }
    }
}