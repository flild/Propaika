using System.Text.RegularExpressions;

namespace Propaika_main_app.Extra
{
    public static class SlugGenerator
    {
        public static string GenerateSlug(string text)
        {
            var cyrillicToLatin = new Dictionary<char, string>
            {
            {'а', "a"}, {'б', "b"}, {'в', "v"}, {'г', "g"}, {'д', "d"},
            {'е', "e"}, {'ё', "yo"}, {'ж', "zh"}, {'з', "z"}, {'и', "i"},
            {'й', "y"}, {'к', "k"}, {'л', "l"}, {'м', "m"}, {'н', "n"},
            {'о', "o"}, {'п', "p"}, {'р', "r"}, {'с', "s"}, {'т', "t"},
            {'у', "u"}, {'ф', "f"}, {'х', "h"}, {'ц', "ts"}, {'ч', "ch"},
            {'ш', "sh"}, {'щ', "sch"}, {'ъ', ""}, {'ы', "y"}, {'ь', ""},
            {'э', "e"}, {'ю', "yu"}, {'я', "ya"}
            };

            var slug = text.ToLower();
            slug = string.Concat(slug.Select(c =>
            cyrillicToLatin.ContainsKey(c) ? cyrillicToLatin[c] : c.ToString()));

            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
            slug = Regex.Replace(slug, @"\s+", "-");

            return slug;
        }
    }
}
