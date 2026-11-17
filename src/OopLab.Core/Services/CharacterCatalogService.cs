using OopLab.Core.Abstractions;
using OopLab.Core.Mapping;
using OopLab.Core.Models;
using OopLab.Core.Specifications;
using OopLab.Core.ViewModels;

namespace OopLab.Core.Services;

/// <summary>
/// Головний сервіс бізнес-логіки каталогу персонажів (Core Facade).
/// </summary>
public class CharacterCatalogService
{
    private readonly ICharacterRepository _repository;
    private readonly IRemoteCharacterProvider _remoteProvider;
    private readonly CharacterViewModelMapper _viewModelMapper;

    public CharacterCatalogService(
        ICharacterRepository repository,
        IRemoteCharacterProvider remoteProvider,
        CharacterViewModelMapper? viewModelMapper = null)
    {
        _repository = repository;
        _remoteProvider = remoteProvider;
        _viewModelMapper = viewModelMapper ?? new CharacterViewModelMapper();
    }

    public string RemoteSourceName => _remoteProvider.SourceName;

    public IReadOnlyList<CharacterProfile> GetAllCharacters() => _repository.GetAll();

    public IReadOnlyList<CharacterProfile> QueryCharacters(CharacterFilterCriteria criteria)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        IEnumerable<CharacterProfile> query = _repository.GetAll().Where(criteria.IsSatisfiedBy);

        query = criteria.SortBy?.ToLowerInvariant() switch
        {
            "rarity" => query.OrderByDescending(c => c.Rarity).ThenBy(c => c.Name),
            "episodes" => query.OrderByDescending(c => c.EpisodeCount).ThenBy(c => c.Name),
            "power" => query.OrderByDescending(c => _viewModelMapper.ToViewModel(c).PopularityScore),
            _ => query.OrderBy(c => int.TryParse(c.Id, out int numId) ? numId : int.MaxValue).ThenBy(c => c.Name)
        };

        return query.ToList();
    }

    public CharacterProfile? FindCharacter(string idOrName) => _repository.GetByIdOrName(idOrName);

    public CharacterViewModel? GetCharacterViewModel(string idOrName)
    {
        var profile = _repository.GetByIdOrName(idOrName);
        return profile is null ? null : _viewModelMapper.ToViewModel(profile);
    }

    public IReadOnlyList<CharacterViewModel> GetAllViewModels(CharacterFilterCriteria? criteria = null)
    {
        var items = criteria is null ? GetAllCharacters() : QueryCharacters(criteria);
        return _viewModelMapper.ToViewModels(items);
    }

    public async Task<int> SyncWithRemoteAsync(int? limit = null, CancellationToken cancellationToken = default)
    {
        var remote = await _remoteProvider.FetchRemoteCharactersAsync(limit, cancellationToken);
        _repository.UpsertMany(remote);
        _repository.SaveChanges();
        return remote.Count;
    }

    public async Task<CharacterProfile?> ImportFromRemoteAsync(string idOrName, CancellationToken cancellationToken = default)
    {
        var character = await _remoteProvider.FetchCharacterByIdOrNameAsync(idOrName, cancellationToken);
        if (character is null) return null;

        _repository.UpsertMany(new[] { character });
        _repository.SaveChanges();
        return character;
    }

    public CharacterProfile CreateCustomCharacter(
        string name,
        string status,
        string species,
        string gender,
        string originWorld,
        int rarity,
        int baseHp,
        int baseAtk,
        string description,
        string avatarUrl)
    {
        var slug = name.Trim().ToLowerInvariant().Replace(' ', '-');
        var character = new CharacterProfile
        {
            Id = $"custom-{slug}",
            Name = name.Trim(),
            Status = string.IsNullOrWhiteSpace(status) ? "Alive" : status.Trim(),
            Species = string.IsNullOrWhiteSpace(species) ? "Human" : species.Trim(),
            Gender = string.IsNullOrWhiteSpace(gender) ? "Unknown" : gender.Trim(),
            OriginWorld = string.IsNullOrWhiteSpace(originWorld) ? "Earth (C-137)" : originWorld.Trim(),
            CurrentLocation = string.IsNullOrWhiteSpace(originWorld) ? "Earth (C-137)" : originWorld.Trim(),
            EpisodeCount = rarity * 8,
            Rarity = Math.Clamp(rarity, 1, 5),
            BaseHp = Math.Max(100, baseHp),
            BaseAtk = Math.Max(10, baseAtk),
            Description = description.Trim(),
            AvatarUrl = string.IsNullOrWhiteSpace(avatarUrl)
                ? "https://rickandmortyapi.com/api/character/avatar/19.jpeg"
                : avatarUrl.Trim(),
            IsCustom = true
        };

        _repository.Add(character);
        _repository.SaveChanges();
        return character;
    }
}
