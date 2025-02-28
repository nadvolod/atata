﻿using System.Linq;

namespace Atata
{
    public class SentenceTermFormatter : ITermFormatter
    {
        public string Format(string[] words) =>
            string.Join(
                " ",
                new[] { words.First().ToUpperFirstLetter() }.Concat(words.Skip(1).Select(x => x.Length >= 2 && x.IsUpper() ? x : x.ToLowerFirstLetter())));
    }
}
