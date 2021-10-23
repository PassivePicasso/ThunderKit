using ColorCode;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ThunderKit.Markdown.SyntaxHighlighting
{
    public class LanguageTypeAdapter
    {
        public Languages Languages { get; }
        private readonly Dictionary<string, ILanguage> languageMap;

        public LanguageTypeAdapter(Languages languages)
        {
            Languages = languages;
            languageMap = new Dictionary<string, ILanguage> 
            {
                {"cs", Languages.CSharp},
                {"cpp", Languages.Cpp},
                {"css", Languages.Css},
                {"js", Languages.JavaScript}
            };
        }


        public ILanguage Parse(string id, string firstLine = null)
        {
            if (id == null)
            {
                return null;
            }

            if (languageMap.ContainsKey(id))
            {
                return languageMap[id];
            }

            if (!string.IsNullOrWhiteSpace(firstLine))
            {
                foreach (var lang in Languages.All)
                {
                    if (lang.FirstLinePattern == null)
                    {
                        continue;
                    }

                    var firstLineMatcher = new Regex(lang.FirstLinePattern, RegexOptions.IgnoreCase);

                    if (firstLineMatcher.IsMatch(firstLine))
                    {
                        return lang;
                    }
                }
            }

            var byIdCanidate = Languages.FindById(id);

            return byIdCanidate;
        }
    }
}