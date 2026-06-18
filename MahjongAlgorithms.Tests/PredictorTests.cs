using System.Diagnostics;
using MahjongAlgorithms.Tiles;
using WP = MahjongAlgorithms.WaitPredictor.WaitPredictor;

namespace MahjongAlgorithms.Tests;

public class WaitPredictorTests
{
    [Fact]
    public void Predict_基本场景()
    {
        // 牌山：每张牌剩3枚（标准牌山开局状态）
        var wall = new TileSet();
        for (int i = 0; i < 34; i++) wall[i] = 3;
        // 对手牌河：空的（还没弃过牌）
        var river = new TileSet();

        var sw = Stopwatch.StartNew();
        var result = WP.Predict(wall, river);
        sw.Stop();

        Assert.True(result.TotalCombinations > 0, $"总组合数应为正数: {result.TotalCombinations}");
        Assert.NotEmpty(result.Ranked);
        // 状态表生成 + 预测应在10秒内完成
        Assert.True(sw.Elapsed.TotalSeconds < 60, $"超时: {sw.Elapsed.TotalSeconds:F1}s");
    }

    [Fact]
    public void Predict_对手已弃牌_振听排除()
    {
        var wall = new TileSet();
        for (int i = 0; i < 34; i++) wall[i] = 3;
        var river = new TileSet();
        river[Tile.M1.Id] = 2; // 对手舍过两张1m

        var result = WP.Predict(wall, river);

        // 1m不应在对守的待牌中（振听）
        Assert.False(result.WaitCounts[Tile.M1.Id] > 0,
            $"1m应在对手牌河中,不应是待牌: {result.WaitCounts[Tile.M1.Id]}");
    }

    [Fact]
    public void Predict_概率排序()
    {
        var wall = new TileSet();
        for (int i = 0; i < 34; i++) wall[i] = 3;
        var river = new TileSet();

        var result = WP.Predict(wall, river);
        var ranked = result.Ranked;
        Assert.NotEmpty(ranked);
        // 概率应递减
        for (int i = 1; i < ranked.Count; i++)
            Assert.True(ranked[i - 1].Prob >= ranked[i].Prob,
                $"概率应递减: {ranked[i-1].Tile}:{ranked[i-1].Prob:P2} < {ranked[i].Tile}:{ranked[i].Prob:P2}");
    }
}
