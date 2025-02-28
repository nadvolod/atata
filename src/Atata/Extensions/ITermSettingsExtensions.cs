﻿namespace Atata
{
    public static class ITermSettingsExtensions
    {
        public static TermCase? GetCaseOrNull(this ITermSettings termSettings)
        {
            if (termSettings == null)
                return null;

            if (termSettings is IHasOptionalProperties castedTermSettings)
                return castedTermSettings.OptionalProperties.Contains(nameof(ITermSettings.Case))
                    ? termSettings.Case
                    : (TermCase?)null;
            else
                return termSettings.Case;
        }

        public static TermMatch? GetMatchOrNull(this ITermSettings termSettings)
        {
            if (termSettings == null)
                return null;

            if (termSettings is IHasOptionalProperties castedTermSettings)
                return castedTermSettings.OptionalProperties.Contains(nameof(ITermSettings.Match))
                    ? termSettings.Match
                    : (TermMatch?)null;
            else
                return termSettings.Match;
        }

        public static string GetFormatOrNull(this ITermSettings termSettings) =>
            termSettings?.Format;
    }
}
