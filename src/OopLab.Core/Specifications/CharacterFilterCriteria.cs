using OopLab.Core.Models;

namespace OopLab.Core.Specifications;

/// <summary>
/// Критерії фільтрації та сортування каталогу (патерн Specification / Query Object).
/// </summary>
public sealed class CharacterFilterCriteria
{
    public string? Status { get; set; }       // Alive | Dead | unknown
    public string? Species { get; set; }      // Human | Alien | Robot
    public string? Gender { get; set; }
    public int? MinRarity { get; set; }
    public string? SearchText { get; set; }
    public string? SortBy { get; set; }       // name | rarity | power | episodes

    public bool IsSatisfiedBy(CharacterProfile c)
    {
        if (!string.IsNullOrWhiteSpace(Status) &&
            !c.Status.Equals(Status.Trim(), StringComparison.OrdinalIgnoreCase))
            return false;

        if (!string.IsNullOrWhiteSpace(Species) &&
            !c.Species.Contains(Species.Trim(), StringComparison.OrdinalIgnoreCase))
            return false;

        if (!string.IsNullOrWhiteSpace(Gender) &&
            !c.Gender.Equals(Gender.Trim(), StringComparison.OrdinalIgnoreCase))
            return false;

        if (MinRarity.HasValue && c.Rarity < MinRarity.Value)
            return false;

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var q = SearchText.Trim();
            bool matches = c.Name.Contains(q, StringComparison.OrdinalIgnoreCase)
                           || c.Id.Equals(q, StringComparison.OrdinalIgnoreCase)
                           || c.OriginWorld.Contains(q, StringComparison.OrdinalIgnoreCase)
                           || c.Description.Contains(q, StringComparison.OrdinalIgnoreCase);
            if (!matches) return false;
        }

        return true;
    }
}
