
using Xunit;
using TableTop.Core.Services;
namespace TableTop.Tests;
public class DiceServiceTests 
{
    private readonly DiceService _service;
    public DiceServiceTests ()
    {
        _service = new DiceService();
    }
    [Fact]
    public void Roll_ShouldCorrectlyParseNotation()
    {
        var result = _service.Roll("2d6+3");
        Assert.Equal(2,result.Count);
        Assert.Equal(6,result.Sides);
        Assert.Equal(3,result.Modifier);
    }
    
    [Theory]
    
    [InlineData("abc")]
    [InlineData("0d6")]      // count < 1
    [InlineData("101d6")]    // count > 100
    [InlineData("2d7")]      // нет такой кости
    [InlineData("2d")]       // грани не указаны
    [InlineData("d")]
    [InlineData("2d6+1001")] // модификатор за пределами
    [InlineData("")]
    public void Roll_ShouldThrowFormatException_OnInvalidNotation(string notation)
    {
        Assert.Throws<FormatException>(() => _service.Roll(notation));
    }

    [Fact]
    public void TestName()
    {
        var result = _service.Roll("100d6");
        Assert.Equal(100,result.Rolls.Length);
        Assert.All(result.Rolls, r => Assert.InRange(r, 1, 6));
    }
    [Fact]
    public void Roll_WithoutModifier_TotalShouldEqualSumOfRolls()
    {
        var result = _service.Roll("2d6");
        Assert.Equal(result.Rolls.Sum(), result.Total);
    }

    [Fact]
    public void Roll_WithModifier_TotalShouldEqualSumOfRollsPlusModifier()
    {
        var result = _service.Roll("2d6+3");
        Assert.Equal(result.Rolls.Sum() + 3, result.Total);
    }
}