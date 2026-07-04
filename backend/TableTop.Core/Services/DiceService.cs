using System.Text.RegularExpressions;
namespace TableTop.Core.Services;

public record DiceRollResult(int Count, int Sides, int Modifier, int[] Rolls, int Total);

public partial class DiceService
{
    private readonly Random _random;

    public DiceService(Random? random = null)
    {
        _random = random ?? Random.Shared;
    }
    public DiceRollResult Roll(string notation)
    {
        var (count, sides, modifier) = Parse(notation);

        int[] rolls = new int[count];  
        int sum = 0;
        for (int i=0; i<count; i++)
        {
            rolls[i] = _random.Next(1, sides+1);
        }
        sum = rolls.Sum();
        int total = sum + modifier; 
        return new DiceRollResult(count, sides, modifier, rolls, total);
    }

    [GeneratedRegex(@"^(\d+)?d(\d+)([+-]\d+)?$")]
    private static partial Regex DiceNotation();
    private static readonly int[] allSides = [2,4,6,8,10,12,20,100];
        
    private static (int Count, int Sides, int Modifier) Parse(string notation)
    {
        notation = notation.Replace(" ", "").ToLowerInvariant();
        var match = DiceNotation().Match(notation);
        if (!match.Success)
        {
            throw new FormatException($"Неверный формат строки {notation}, пример правильного: 2d6+3");
        }
        
        int count = match.Groups[1].Success ? int.Parse(match.Groups[1].Value) : 1;
        int sides = int.Parse(match.Groups[2].Value);
        int modifier = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : 0;

        if (count>100 || count < 1)
        {
            throw new FormatException("Диапазон для кол-ва кидаемых костей от 1 до 100");
        }
        if (!allSides.Contains(sides))
        {
            throw new FormatException($"Неверно указаны грани кости, возможный диапазон: {string.Join(", ", allSides)}");
        }
        if (modifier>1000 || modifier < -1000)
        {
            throw new FormatException("Модификатор должен быть в диапазоне от -1000 до 1000");
        }

        return(count,sides,modifier);

    }
}