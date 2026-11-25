using System;

public static class DebugRepro {
    public static void Run() {
        short S = (short)TileTypeEnum.Stone;
        short D = (short)TileTypeEnum.Terrain0;

        // Сценарий 7а: камень в центре, земля снизу
        var m = new short[5, 5];
        for (int x = 0; x < 5; x++)
            for (int y = 0; y < 5; y++)
                m[x, y] = S;
        m[2, 3] = D;

        Console.WriteLine("карта (y вниз):");
        for (int y = 0; y < 5; y++) {
            for (int x = 0; x < 5; x++)
                Console.Write(m[x, y] + " ");
            Console.WriteLine();
        }

        var rules = TerrariaTilesetRules.Stone;
        Console.WriteLine($"rules.ParticipatesInMergeBlocks = {rules.ParticipatesInMergeBlocks}");
        Console.WriteLine($"IsTerrainSolid(D) = {TerrariaTilesetRules.IsTerrainSolid(D)}");

        TerrariaFraming.ClassifyNeighbors(m, 2, 2, rules,
            out int up, out int down, out int left, out int right,
            out int ul, out int ur, out int dl, out int dr);
        Console.WriteLine($"classify: up={up} down={down} left={left} right={right} ul={ul} ur={ur} dl={dl} dr={dr}");

        TerrariaFraming.Solve(up, down, left, right, ul, ur, dl, dr, 0, rules,
            out int fx, out int fy, out bool mu, out bool md, out bool ml, out bool mr);
        Console.WriteLine($"solve: ({fx},{fy}) merges u={mu} d={md} l={ml} r={mr}");
        Console.WriteLine($"grid: ({fx / 18},{fy / 18})");

        var shema = new TerrainTileShema(rules);
        var f = shema.GetShema(m, new TileInfoStructure(S, 0));
        Console.WriteLine($"GetShema: ({f.FrameX},{f.FrameY})");
    }
}
