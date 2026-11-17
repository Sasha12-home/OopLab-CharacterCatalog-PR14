using OopLab.Cli.Commands;
using OopLab.Cli.Graphics;
using OopLab.Cli.Rendering;
using OopLab.Core.Services;

namespace OopLab.Cli.Strategies;

public sealed class TransformativeCommandStrategy : CommandStrategyBase
{
    public override string CategoryName => "Трансформативні дії (Створення персонажів)";

    public TransformativeCommandStrategy(
        CharacterCatalogService catalogService,
        ConsoleCharacterRenderer renderer)
    {
        RegisterCommand(new Command(
            selector: "create",
            description: "Створити власного персонажа через діалог і зберегти у JSON-сховище.",
            usage: "create",
            handler: _ =>
            {
                Console.WriteLine();
                CliGraphic.PrintBanner("СТВОРЕННЯ НОВОГО ПЕРСОНАЖА");
                var name = CliGraphic.PromptRequired("Ім'я персонажа");
                var status = CliGraphic.PromptOptional("Статус (Alive/Dead/unknown)", "Alive");
                var species = CliGraphic.PromptOptional("Вид (Human/Alien/Robot)", "Human");
                var gender = CliGraphic.PromptOptional("Стать (Male/Female/unknown)", "Male");
                var origin = CliGraphic.PromptOptional("Рідний світ", "Earth (C-137)");
                var rarity = CliGraphic.PromptInt("Рідкість (1-5)", defaultValue: 4, min: 1, max: 5);
                var hp = CliGraphic.PromptInt("Базове HP", defaultValue: 12000, min: 100, max: 100000);
                var atk = CliGraphic.PromptInt("Базова ATK", defaultValue: 310, min: 10, max: 10000);
                var desc = CliGraphic.PromptOptional("Короткий опис", "Новий герой мультивсесвіту.");
                var avatarUrl = CliGraphic.PromptOptional("URL JPEG-аватара (Enter — стандартний)");

                var created = catalogService.CreateCustomCharacter(
                    name, status, species, gender, origin, rarity, hp, atk, desc, avatarUrl);

                CliGraphic.PrintSuccess($"Персонажа '{created.Name}' створено та збережено!");
                created.Render(renderer);
                return Task.CompletedTask;
            }));
    }
}
