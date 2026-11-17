using OopLab.Cli.Graphics;
using OopLab.Core.Mapping;
using OopLab.Core.Models;
using OopLab.Core.Rendering;
using OopLab.Core.ViewModels;

namespace OopLab.Cli.Rendering;

/// <summary>
/// Рендерить доменну модель та підготовлену для GUI ViewModel у консоль.
/// </summary>
public class ConsoleCharacterRenderer : IRenderer<CharacterProfile>, IRenderer<CharacterViewModel>
{
    private readonly CharacterViewModelMapper _mapper = new();

    public void Render(CharacterProfile item)
    {
        var vm = _mapper.ToViewModel(item);
        Render(vm);
    }

    public void RenderCollection(IEnumerable<CharacterProfile> items)
    {
        var vms = _mapper.ToViewModels(items);
        RenderCollection(vms);
    }

    public void Render(CharacterViewModel vm)
    {
        Console.WriteLine();
        CliGraphic.PrintBanner($"VIEWMODEL КАРТКА: {vm.DisplayName.ToUpperInvariant()} [{vm.TierRank}]");
        Console.WriteLine($"  ID:               {vm.Id}");
        Console.WriteLine($"  Ім'я / Опис:      {vm.DisplayName} — {vm.Subtitle}");
        Console.WriteLine($"  Статус (UI):      {vm.StatusBadge} (Колір індикатора: {vm.StatusColorHex})");
        Console.WriteLine($"  Вид / Стать:      {vm.SpeciesLabel} | {vm.GenderLabel}");
        Console.WriteLine($"  Світ / Локація:   {vm.OriginAndLocation}");
        Console.WriteLine($"  Рідкість:         {vm.RarityStars} ({vm.RarityValue}★)");
        Console.WriteLine($"  Характеристики:   {vm.StatsSummary}");
        Console.WriteLine($"  Джерело:          {vm.SourceBadge}");
        Console.WriteLine($"  Біографія:        {vm.Biography}");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"  Avatar JPEG URL:  {vm.AvatarImageUrl}");
        Console.ResetColor();
        Console.WriteLine();
    }

    public void RenderCollection(IEnumerable<CharacterViewModel> items)
    {
        var list = items.ToList();
        if (list.Count == 0)
        {
            CliGraphic.PrintInfo("За заданими критеріями персонажів не знайдено.");
            return;
        }

        Console.WriteLine();
        Console.WriteLine("┌────────────┬──────────────────────┬──────────┬──────────────┬────────┬───────┐");
        Console.WriteLine("│ ID         │ Ім'я                 │ Статус   │ Вид          │ Епіз.  │ Ранг  │");
        Console.WriteLine("├────────────┼──────────────────────┼──────────┼──────────────┼────────┼───────┤");
        foreach (var vm in list)
        {
            var idShort = vm.Id.Length > 10 ? vm.Id[..10] : vm.Id;
            var nameShort = vm.DisplayName.Length > 20 ? vm.DisplayName[..20] : vm.DisplayName;
            var specShort = vm.SpeciesLabel.Length > 12 ? vm.SpeciesLabel[..12] : vm.SpeciesLabel;
            Console.WriteLine($"│ {idShort,-10} │ {nameShort,-20} │ {vm.StatusBadge,-8} │ {specShort,-12} │ {vm.EpisodeAppearances,-6} │ {vm.TierRank,-5} │");
        }
        Console.WriteLine("└────────────┴──────────────────────┴──────────┴──────────────┴────────┴───────┘");
        Console.WriteLine($"Всього записів: {list.Count}. Деталі ViewModel: info <ім'я або ID>");
        Console.WriteLine();
    }
}
