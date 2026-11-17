using OopLab.Cli.Commands;
using OopLab.Cli.Graphics;
using OopLab.Cli.Rendering;
using OopLab.Core.Services;

namespace OopLab.Cli.Strategies;

public sealed class InfrastructureCommandStrategy : CommandStrategyBase
{
    public override string CategoryName => "Інфраструктурні дії (Парсинг API https://rickandmortyapi.com/api/character)";

    public InfrastructureCommandStrategy(
        CharacterCatalogService catalogService,
        ConsoleCharacterRenderer renderer)
    {
        RegisterCommand(new Command(
            selector: "sync",
            description: "Завантажити та розпарсити персонажів із Rick and Morty API (опція --limit=20).",
            usage: "sync [--limit=20]",
            handler: async args =>
            {
                int? limit = int.TryParse(args.GetOption("limit"), out int l) ? l : 20;
                int count = 0;

                await CliGraphic.RunWithSpinnerAsync($"Завантаження та парсинг JSON з {catalogService.RemoteSourceName}...", async () =>
                {
                    count = await catalogService.SyncWithRemoteAsync(limit);
                });

                CliGraphic.PrintSuccess($"Успішно розпарсено та збережено {count} персонажів із {catalogService.RemoteSourceName}!");
            }));

        RegisterCommand(new Command(
            selector: "import",
            description: "Імпортувати персонажа за ID або іменем із API (наприклад: import \"Birdperson\" або import 47).",
            usage: "import <ім'я або ID>",
            handler: async args =>
            {
                var query = args.GetPositional(0);
                if (string.IsNullOrWhiteSpace(query))
                {
                    CliGraphic.PrintError("Вкажіть ім'я або ID персонажа. Приклад: import \"Birdperson\" або import 47");
                    return;
                }

                Core.Models.CharacterProfile? imported = null;
                await CliGraphic.RunWithSpinnerAsync($"Пошук та парсинг JSON для '{query}' із {catalogService.RemoteSourceName}...", async () =>
                {
                    imported = await catalogService.ImportFromRemoteAsync(query);
                });

                if (imported is null)
                {
                    CliGraphic.PrintError($"Персонажа '{query}' не знайдено у API.");
                    return;
                }

                CliGraphic.PrintSuccess($"Персонажа '{imported.Name}' (ID: {imported.Id}) імпортовано та підготовлено для UI!");
                imported.Render(renderer);
            }));
    }
}
