using OopLab.Core.Models;

namespace OopLab.Core.Abstractions;

public interface ICharacterRepository
{
    IReadOnlyList<CharacterProfile> GetAll();
    CharacterProfile? GetByIdOrName(string query);
    void Add(CharacterProfile character);
    void UpsertMany(IEnumerable<CharacterProfile> characters);
    void SaveChanges();
}
