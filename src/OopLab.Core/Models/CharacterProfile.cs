using OopLab.Core.Rendering;

namespace OopLab.Core.Models;

/// <summary>
/// Універсальна доменна модель персонажа у шарі бізнес-логіки (Core).
/// Адаптована під багатоплатформні каталоги (Rick and Morty API, локальні герої тощо).
/// </summary>
public class CharacterProfile : IRenderable<CharacterProfile>
{
    private IRenderer<CharacterProfile>? _defaultRenderer;

    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..6];
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = "Alive";           // Alive | Dead | unknown
    public string Species { get; set; } = "Human";          // Human | Alien | Robot | Humanoid
    public string SubType { get; set; } = string.Empty;
    public string Gender { get; set; } = "Unknown";
    public string OriginWorld { get; set; } = "Earth (C-137)";
    public string CurrentLocation { get; set; } = "Unknown";
    public int EpisodeCount { get; set; } = 1;
    public int Rarity { get; set; } = 3;
    public int BaseHp { get; set; } = 10000;
    public int BaseAtk { get; set; } = 250;
    public string Description { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public bool IsCustom { get; set; }

    public void UseRenderer(IRenderer<CharacterProfile> renderer)
    {
        _defaultRenderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
    }

    public void Render(IRenderer<CharacterProfile> renderer)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        renderer.Render(this);
    }

    public void Render()
    {
        if (_defaultRenderer is null)
            throw new InvalidOperationException("Рендерер за замовчуванням не налаштований.");
        _defaultRenderer.Render(this);
    }
}
