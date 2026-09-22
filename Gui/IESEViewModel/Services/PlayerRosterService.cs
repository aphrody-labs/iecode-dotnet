using IESEViewModel.InazumaEleven.Games;
using IESEViewModel.InazumaEleven.Logic;

namespace IESEViewModel.Services;

/// <summary>
/// Save-player mutations shared by every IECODE UI surface.
/// </summary>
public static class PlayerRosterService
{
    public static IReadOnlyList<Player> RecruitablePlayers(IGame game)
    {
        var reservedIds = game.Reserve.Select(player => player.Id).ToHashSet();
        return game.Players
            .Where(pair => pair.Key != 0 && !reservedIds.Contains(pair.Key))
            .OrderBy(pair => pair.Value.Name, StringComparer.OrdinalIgnoreCase)
            .Select(pair => new Player(pair.Value, pair.Key))
            .ToArray();
    }

    public static bool Recruit(IGame game, Player player)
    {
        if (game.Reserve.Count >= game.MaximumPlayer || game.Reserve.Any(item => item.Id == player.Id))
        {
            return false;
        }

        game.RecruitPlayer(player, configure: true);
        return true;
    }

    public static bool Dismiss(IGame game, Player player)
    {
        if (!game.Reserve.Remove(player))
        {
            return false;
        }

        foreach (var linkedPlayer in game.Reserve)
        {
            if (linkedPlayer.MixiMax?.AuraPlayer == player)
            {
                linkedPlayer.MixiMax = null;
            }
        }

        player.IsAura = false;
        return true;
    }

    public static bool Replace(IGame game, Player selectedPlayer, Player replacement)
    {
        var index = game.Reserve.IndexOf(selectedPlayer);
        if (index < 0)
        {
            return false;
        }

        game.Reserve[index] = game.ChangePlayer(selectedPlayer, replacement, keepOldPlayerInformation: true);
        return true;
    }
}
