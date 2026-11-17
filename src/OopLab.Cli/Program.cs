using OopLab.Cli;
using OopLab.Cli.Graphics;
using OopLab.Cli.Rendering;
using OopLab.Cli.Strategies;
using OopLab.Core.Services;
using OopLab.Data.Persistence;
using OopLab.Data.Remote;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.InputEncoding = System.Text.Encoding.UTF8;

var storagePath = Path.Combine(AppContext.BaseDirectory, "data", "rm_characters_catalog.json");
var repository = new JsonCharacterRepository(storagePath);
var remoteProvider = new RickAndMortyApiClient();
var catalogService = new CharacterCatalogService(repository, remoteProvider);

var renderer = new ConsoleCharacterRenderer();
var cli = new CliFacade();

cli.UseStrategy(new InformationCommandStrategy(catalogService, renderer, () => cli.Strategies))
   .UseStrategy(new TransformativeCommandStrategy(catalogService, renderer))
   .UseStrategy(new InfrastructureCommandStrategy(catalogService, renderer));

if (args.Length > 0)
{
    var singleCommand = string.Join(' ', args.Select(a => a.Contains(' ') ? $"\"{a}\"" : a));
    await cli.ExecCommandAsync(singleCommand);
    return;
}

CliGraphic.PrintBanner("КАТАЛОГ ПЕРСОНАЖІВ — RICK & MORTY API (ПР 14)");
Console.WriteLine("Джерело API: https://rickandmortyapi.com/api/character");
Console.WriteLine("Введіть 'help' для списку команд (напр. 'sync --limit=20', 'import \"Birdperson\"', 'list --sort=power').\n");

bool isRunning = true;
while (isRunning)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write("catalog> ");
    Console.ResetColor();

    var input = Console.ReadLine();
    if (input is null) break;

    isRunning = await cli.ExecCommandAsync(input);
}
