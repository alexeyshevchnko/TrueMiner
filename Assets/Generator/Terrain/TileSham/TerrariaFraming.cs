using System;

/// <summary>
/// РџСЂР°РІРёР»Р° СЃРѕРµРґРёРЅРµРЅРёСЏ (tile framing) РѕРґРЅРѕРіРѕ С‚Р°Р№Р»СЃРµС‚Р° РѕСЂРёРіРёРЅР°Р»СЊРЅРѕР№ СЃС‚СЂСѓРєС‚СѓСЂС‹ Terraria.
///
/// РўР°Р№Р»СЃРµС‚С‹ Tiles_0 (Р·РµРјР»СЏ) Рё Tiles_1 (РєР°РјРµРЅСЊ) СѓСЃС‚СЂРѕРµРЅС‹ С‚Р°Рє Р¶Рµ, РєР°Рє РІ Terraria:
/// РєР°РґСЂ 16x16 px Р»РµР¶РёС‚ РІ СЃРµС‚РєРµ СЃ С€Р°РіРѕРј 18 px (2 px РїР°РґРґРёРЅРіР°), РєРѕРѕСЂРґРёРЅР°С‚С‹ РєР°РґСЂР° вЂ”
/// (frameX, frameY) РІ РїРёРєСЃРµР»СЏС… Р»РёСЃС‚Р°. РћРґРёРЅ Рё С‚РѕС‚ Р¶Рµ Р»РёСЃС‚ СЃРѕРґРµСЂР¶РёС‚:
///   вЂў Р±Р°Р·РѕРІС‹Рµ РєР°РґСЂС‹ РєСЂР°С‘РІ/СѓРіР»РѕРІ/С†РµРЅС‚СЂР° (Y = 0..89);
///   вЂў РєР°РґСЂС‹ РІРЅСѓС‚СЂРµРЅРЅРёС… СѓРіР»РѕРІ (X = 108/180/198, Y = 18/36/72);
///   вЂў РєР°РґСЂС‹ СЃР»РёСЏРЅРёСЏ СЃ РґСЂСѓРіРёРј РјР°С‚РµСЂРёР°Р»РѕРј (Y = 90..252) вЂ” РёСЃРїРѕР»СЊР·СѓСЋС‚СЃСЏ, РєРѕРіРґР° СЃРѕСЃРµРґ
///     СЏРІР»СЏРµС‚СЃСЏ В«Р·Р°РїСЂРѕСЃРѕРј СЃР»РёСЏРЅРёСЏВ» (РІ РѕСЂРёРіРёРЅР°Р»Рµ вЂ” РјР°СЂРєРµСЂ -2).
///
/// РЎРµРјР°РЅС‚РёРєР° РѕСЂРёРіРёРЅР°Р»Р° РґР»СЏ РїР°СЂС‹ Р·РµРјР»СЏ/РєР°РјРµРЅСЊ:
///   вЂў Р—РµРјР»СЏ (dirt, tileMergeDirt[0] = false) РќР• СѓС‡Р°СЃС‚РІСѓРµС‚ РІ merge-Р±Р»РѕРєР°С…: Р·РµРјР»СЏ-Р·РµРјР»СЏ
///     в†’ SlotSelf (РѕР±С‹С‡РЅС‹Рµ РІРЅСѓС‚СЂРµРЅРЅРёРµ РєР°РґСЂС‹), РєР°РјРµРЅСЊ в†’ SlotEdge (РѕР±С‹С‡РЅС‹Р№ РєСЂР°Р№ Р·РµРјР»Рё;
///     Р±РµСЃС€РѕРІРЅРѕСЃС‚СЊ РЅР° РіСЂР°РЅРёС†Рµ РґР°С‘С‚СЃСЏ merge-РєР°РґСЂР°РјРё РєР°РјРЅСЏ).
///   вЂў РљР°РјРµРЅСЊ (tileMergeDirt[1] = true) СѓС‡Р°СЃС‚РІСѓРµС‚: РєР°РјРµРЅСЊ-РєР°РјРµРЅСЊ в†’ SlotSelf,
///     Р·РµРјР»СЏ в†’ SlotMerge (РєР°РґСЂС‹ СЃР»РёСЏРЅРёСЏ РєР°РјРЅСЏ РІ Р·РµРјР»СЋ).
///   вЂў MutualMergeCheckTypes вЂ” СЃРѕСЃРµРґ, РєРѕС‚РѕСЂС‹Р№ РїСЂРѕРІРµСЂСЏРµС‚СЃСЏ РЅР° В«РІСЃС‚СЂРµС‡РЅРѕРµВ» СЃР»РёСЏРЅРёРµ:
///     Р·РµРјР»СЏ РїСЂРѕРІРµСЂСЏРµС‚ РєР°РјРµРЅСЊ Рё, РµСЃР»Рё РєР°РјРµРЅСЊ СЂРёСЃСѓРµС‚ merge-РєР°РґСЂ РІ РµС‘ СЃС‚РѕСЂРѕРЅСѓ,
///     СЃС‡РёС‚Р°РµС‚ РµРіРѕ В«СЃРІРѕРёРјВ» (СѓР±РёСЂР°РµС‚ СЃРІРѕР№ РєСЂР°Р№ вЂ” РёРЅР°С‡Рµ РІРёРґРЅР° РґРІРѕР№РЅР°СЏ СЂР°РјРєР°).
/// </summary>
public sealed class TerrariaTilesetRules {
    /// <summary>РўРёРїС‹, РєРѕС‚РѕСЂС‹Рµ СЃС‡РёС‚Р°СЋС‚СЃСЏ В«СЃРІРѕРёРјРёВ» (SlotSelf: Р±РµР· РіСЂР°РЅРёС†С‹ РјРµР¶РґСѓ СЃРѕР±РѕР№).</summary>
    public readonly short[] SelfTypes;

    /// <summary>РўРёРїС‹-СЃРѕСЃРµРґРё, Р·Р°РїСЂР°С€РёРІР°СЋС‰РёРµ СЃР»РёСЏРЅРёРµ (SlotMerge: РєР°РґСЂС‹ СЃР»РёСЏРЅРёСЏ).</summary>
    public readonly short[] MergeTypes;

    /// <summary>
    /// РўРёРїС‹-СЃРѕСЃРµРґРё, РґР»СЏ РєРѕС‚РѕСЂС‹С… РЅСѓР¶РЅР° РІР·Р°РёРјРЅР°СЏ РїСЂРѕРІРµСЂРєР°: СЃРѕСЃРµРґ СЃС‡РёС‚Р°РµС‚СЃСЏ В«СЃРІРѕРёРјВ»,
    /// С‚РѕР»СЊРєРѕ РµСЃР»Рё РѕРЅ СЃР°Рј РїСЂРё РєР°РґСЂРёСЂРѕРІР°РЅРёРё СЃР»РёРІР°РµС‚СЃСЏ РІ РЅР°С€Сѓ СЃС‚РѕСЂРѕРЅСѓ.
    /// Р’ РѕСЂРёРіРёРЅР°Р»Рµ вЂ” TileMergeAttemptFrametest/TileFrame РґР»СЏ dirt-mergeable СЃРѕСЃРµРґРµР№.
    /// </summary>
    public readonly short[] MutualMergeCheckTypes;

    /// <summary>РџСЂР°РІРёР»Р° СЃРѕСЃРµРґР° РїСЂРё РІР·Р°РёРјРЅРѕР№ РїСЂРѕРІРµСЂРєРµ. null вЂ” РїСЂРѕРІРµСЂРєР° РЅРµ РЅСѓР¶РЅР°.</summary>
    public readonly TerrariaTilesetRules MutualMergeCheckRules;

    /// <summary>
    /// РЈС‡Р°СЃС‚РІСѓРµС‚ Р»Рё С‚Р°Р№Р» РІ merge-Р±Р»РѕРєР°С… РѕСЂРёРіРёРЅР°Р»Р° (Main.tileMergeDirt[num]).
    /// </summary>
    public readonly bool ParticipatesInMergeBlocks;

    /// <summary>РўРёРї РѕС‚РЅРѕСЃРёС‚СЃСЏ Рє В«С‚СЂР°РІСЏРЅРѕРјСѓВ» СЃРµРјРµР№СЃС‚РІСѓ (РґР»СЏ РЅРµРіРѕ РїСЂРѕРїСѓСЃРєР°СЋС‚СЃСЏ РєР°РґСЂС‹ 234-Р№ РєРѕР»РѕРЅРєРё).</summary>
    public readonly bool IsGrassLike;

    /// <summary>
    /// Р”РёР°РіРѕРЅР°Р»СЊРЅС‹Р№ СЃРѕСЃРµРґ РёР· MutualMergeCheckTypes СЃС‡РёС‚Р°РµС‚СЃСЏ В«СЃРІРѕРёРјВ» Р‘Р•Р— РїСЂРѕРІРµСЂРєРё
    /// РІСЃС‚СЂРµС‡РЅРѕРіРѕ СЃР»РёСЏРЅРёСЏ (РІ РѕСЂРёРіРёРЅР°Р»Рµ Terraria РґРёР°РіРѕРЅР°Р»СЊ dirt-mergeable СЃРѕСЃРµРґР°
    /// РЅРµ Р·Р°РїСѓСЃРєР°РµС‚ TileFrame вЂ” РїСЂРѕСЃС‚Рѕ СЃС‡РёС‚Р°РµС‚СЃСЏ СЃРІРѕРёРј).
    /// </summary>
    public readonly bool MutualMergeCheckDiagonalIsSelf;

    public TerrariaTilesetRules(
        short[] selfTypes,
        short[] mergeTypes,
        short[] mutualMergeCheckTypes,
        TerrariaTilesetRules mutualMergeCheckRules,
        bool participatesInMergeBlocks,
        bool isGrassLike,
        bool mutualMergeCheckDiagonalIsSelf = false) {
        SelfTypes = selfTypes ?? EmptyTypes;
        MergeTypes = mergeTypes ?? EmptyTypes;
        MutualMergeCheckTypes = mutualMergeCheckTypes ?? EmptyTypes;
        MutualMergeCheckRules = mutualMergeCheckRules;
        ParticipatesInMergeBlocks = participatesInMergeBlocks;
        IsGrassLike = isGrassLike;
        MutualMergeCheckDiagonalIsSelf = mutualMergeCheckDiagonalIsSelf;
    }

    private static readonly short[] EmptyTypes = new short[0];

    /// <summary>
    /// РќР°Р±РѕСЂ В«С‚РІС‘СЂРґС‹С…В» С‚РёРїРѕРІ С‚РµСЂСЂР°РёРЅР°. Р’СЃС‘, С‡РµРіРѕ РЅРµС‚ РІ РЅР°Р±РѕСЂРµ (РїСѓСЃС‚РѕС‚Р°, РґРµСЂРµРІСЊСЏ,
    /// Р»РµСЃС‚РЅРёС†С‹, РґРµРєРѕСЂ, РІРѕРґР°, СЃРІРµС‚), РїСЂРё РєР°РґСЂРёСЂРѕРІР°РЅРёРё СЃС‡РёС‚Р°РµС‚СЃСЏ РїСѓСЃС‚РѕС‚РѕР№.
    /// </summary>
    private static readonly short[] TerrainSolidTypes = {
        (short) TileTypeEnum.Coal,
        (short) TileTypeEnum.Terrain0,
        (short) TileTypeEnum.TerrainGrass,
        (short) TileTypeEnum.Gold,
        (short) TileTypeEnum.Wood,
        (short) TileTypeEnum.Brick,
        (short) TileTypeEnum.Bone,
        (short) TileTypeEnum.Meat,
        (short) TileTypeEnum.Med,
        (short) TileTypeEnum.Iron,
        (short) TileTypeEnum.Stone,
    };

    public static bool IsTerrainSolid(short type) {
        for (int i = 0; i < TerrainSolidTypes.Length; i++) {
            if (TerrainSolidTypes[i] == type) {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// РљР°РјРµРЅСЊ (Tiles_1): РєР°РјРµРЅСЊ-РєР°РјРµРЅСЊ в†’ SlotSelf, Р·РµРјР»СЏ в†’ SlotMerge (РєР°РґСЂС‹ СЃР»РёСЏРЅРёСЏ
    /// РєР°РјРЅСЏ РІ Р·РµРјР»СЋ), РѕСЃС‚Р°Р»СЊРЅРѕРµ в†’ SlotEdge. РЈС‡Р°СЃС‚РІСѓРµС‚ РІ merge-Р±Р»РѕРєР°С….
    /// </summary>
    public static readonly TerrariaTilesetRules Stone = new TerrariaTilesetRules(
        selfTypes: new[] { (short) TileTypeEnum.Stone },
        mergeTypes: new[] { (short) TileTypeEnum.Terrain0, (short) TileTypeEnum.TerrainGrass },
        mutualMergeCheckTypes: null,
        mutualMergeCheckRules: null,
        participatesInMergeBlocks: true,
        isGrassLike: false);

    /// <summary>
    /// Р—РµРјР»СЏ (Tiles_0): Р·РµРјР»СЏ-Р·РµРјР»СЏ Рё Р·РµРјР»СЏ-С‚СЂР°РІР° в†’ SlotSelf (РІРЅСѓС‚СЂРµРЅРЅРёРµ РєР°РґСЂС‹ Р±РµР·
    /// РіСЂР°РЅРёС†), РєР°РјРµРЅСЊ в†’ SlotEdge + РІР·Р°РёРјРЅР°СЏ РїСЂРѕРІРµСЂРєР° (РµСЃР»Рё РєР°РјРµРЅСЊ СЃР»РёРІР°РµС‚СЃСЏ РІ РЅР°С€Сѓ
    /// СЃС‚РѕСЂРѕРЅСѓ вЂ” Р·РµРјР»СЏ СѓР±РёСЂР°РµС‚ СЃРІРѕР№ РєСЂР°Р№, РёРЅР°С‡Рµ РІРёРґРЅР° РґРІРѕР№РЅР°СЏ СЂР°РјРєР°), РѕСЃС‚Р°Р»СЊРЅРѕРµ в†’
    /// SlotEdge. Р’ merge-Р±Р»РѕРєР°С… РЅРµ СѓС‡Р°СЃС‚РІСѓРµС‚ (РІ РѕСЂРёРіРёРЅР°Р»Рµ tileMergeDirt[0] = false).
    /// Р”РёР°РіРѕРЅР°Р»СЊРЅС‹Р№ РєР°РјРµРЅСЊ в†’ SlotSelf Р±РµР· РїСЂРѕРІРµСЂРєРё (РѕСЂРёРіРёРЅР°Р» Terraria).
    /// </summary>
    public static readonly TerrariaTilesetRules Dirt = new TerrariaTilesetRules(
        selfTypes: new[] { (short) TileTypeEnum.Terrain0, (short) TileTypeEnum.TerrainGrass },
        mergeTypes: null,
        mutualMergeCheckTypes: new[] { (short) TileTypeEnum.Stone },
        mutualMergeCheckRules: Stone,
        participatesInMergeBlocks: false,
        isGrassLike: false,
        mutualMergeCheckDiagonalIsSelf: true);
}

/// <summary>
/// Р§РёСЃС‚С‹Р№ СЂРµС€Р°С‚РµР»СЊ РєР°РґСЂРѕРІ Terraria РґР»СЏ С‚Р°Р№Р»РѕРІ РѕР±С‹С‡РЅРѕРіРѕ (В«РЅРµ frameImportantВ») С‚РёРїР°.
///
/// РђР»РіРѕСЂРёС‚Рј РїРѕРІС‚РѕСЂСЏРµС‚ РѕСЂРёРіРёРЅР°Р»СЊРЅС‹Р№ WorldGen.TileFrame (СЃРІРµСЂРµРЅРѕ СЃ РґРµРєРѕРјРїРёР»СЏС†РёРµР№
/// Terraria 1.3.5.3 Рё 1.4.4.9): merge-Р±Р»РѕРєРё (РІСЃРµ 4 СЃС‚РѕСЂРѕРЅС‹ РЅРµ РіСЂР°РЅРёС†Р°) в†’ С‡Р°СЃС‚РёС‡РЅС‹Рµ
/// СЃР»РёСЏРЅРёСЏ СЃ РѕС‚РєСЂС‹С‚С‹РјРё СЃС‚РѕСЂРѕРЅР°РјРё в†’ РѕР±С‰РёР№ РЅР°Р±РѕСЂ РєР°РґСЂРѕРІ в†’ Р±РµР·РѕРїР°СЃРЅС‹Р№ РІР°СЂРёР°РЅС‚ В«С†РµРЅС‚СЂВ».
///
/// Р’Р°Р¶РЅРѕРµ РѕС‚Р»РёС‡РёРµ С‡Р°СЃС‚РёС‡РЅС‹С… СЃР»РёСЏРЅРёР№ РѕС‚ РЅР°РёРІРЅРѕР№ СЂРµР°Р»РёР·Р°С†РёРё: В«СЃРѕРµРґРёРЅС‘РЅРЅР°СЏВ» СЃС‚РѕСЂРѕРЅР° РІ
/// РёС… СѓСЃР»РѕРІРёСЏС… вЂ” Р›Р®Р‘РђРЇ РЅРµ-РіСЂР°РЅРёС†Р° (SlotSelf РР›Р SlotMerge), РєР°Рє РІ РѕСЂРёРіРёРЅР°Р»Рµ, РіРґРµ
/// РїСЂРѕРІРµСЂРєРё РІРёРґР° `left == num || left == num32` РѕС…РІР°С‚С‹РІР°СЋС‚ Рё self, Рё merge-СЃРѕСЃРµРґРµР№.
/// Р­С‚Рѕ СѓСЃС‚СЂР°РЅСЏРµС‚ С‡С‘СЂРЅС‹Рµ СЂР°Р·СЂС‹РІС‹ РЅР° РіСЂР°РЅРёС†Р°С… РјР°С‚РµСЂРёР°Р»РѕРІ РІ СЃР»РѕР¶РЅС‹С… РєРѕРЅС„РёРіСѓСЂР°С†РёСЏС….
///
/// Р РµС€Р°С‚РµР»СЊ РЅРµ СЃРѕР·РґР°С‘С‚ РѕР±СЉРµРєС‚РѕРІ Рё РЅРµ РѕР±СЂР°С‰Р°РµС‚СЃСЏ Рє РєР°СЂС‚Рµ: РЅР° РІС…РѕРґ РїРѕРґР°СЋС‚СЃСЏ СѓР¶Рµ
/// РєР»Р°СЃСЃРёС„РёС†РёСЂРѕРІР°РЅРЅС‹Рµ 8 СЃРѕСЃРµРґРµР№, РЅР° РІС‹С…РѕРґРµ вЂ” РєРѕРѕСЂРґРёРЅР°С‚С‹ РєР°РґСЂР° Рё С„Р»Р°РіРё СЃР»РёСЏРЅРёСЏ
/// (С„Р»Р°РіРё РЅСѓР¶РЅС‹ Р·РµРјР»Рµ РґР»СЏ РІР·Р°РёРјРЅРѕР№ РїСЂРѕРІРµСЂРєРё СЃ РєР°РјРЅРµРј).
/// </summary>
public static class TerrariaFraming {
    /// <summary>РЎРѕСЃРµРґ вЂ” В«СЃРІРѕР№В» С‚РѕРіРѕ Р¶Рµ РјР°С‚РµСЂРёР°Р»Р° (РєР°РјРµРЅСЊ-РєР°РјРµРЅСЊ, Р·РµРјР»СЏ-Р·РµРјР»СЏ).</summary>
    public const int SlotSelf = 1;

    /// <summary>РЎРѕСЃРµРґ вЂ” Р·Р°РїСЂРѕСЃ СЃР»РёСЏРЅРёСЏ: РєР°РґСЂС‹ СЃР»РёСЏРЅРёСЏ РЅР° РіСЂР°РЅРёС†Рµ РјР°С‚РµСЂРёР°Р»РѕРІ.</summary>
    public const int SlotMerge = -2;

    /// <summary>РЎРѕСЃРµРґ вЂ” РіСЂР°РЅРёС†Р°: РїСѓСЃС‚РѕС‚Р°, С‡СѓР¶РѕР№ С‚РІС‘СЂРґС‹Р№ С‚РёРї РёР»Рё РЅРµ-С‚РІС‘СЂРґС‹Р№ С‚Р°Р№Р».</summary>
    public const int SlotEdge = -1;

    /// <summary>РЁР°Рі СЃРµС‚РєРё РєР°РґСЂРѕРІ РІ РїРёРєСЃРµР»СЏС… Р»РёСЃС‚Р° Terraria (16 px РєР°РґСЂ + 2 px РїР°РґРґРёРЅРі).</summary>
    public const int FrameStride = 18;

    /// <summary>
    /// РљР»Р°СЃСЃРёС„РёРєР°С†РёСЏ РѕРґРЅРѕРіРѕ СЃРѕСЃРµРґР° РїРѕ С‚РёРїСѓ.
    /// </summary>
    public static int ClassifyNeighbor(short neighborType, TerrariaTilesetRules rules) {
        if (!TerrariaTilesetRules.IsTerrainSolid(neighborType)) {
            return SlotEdge;
        }
        if (Contains(rules.SelfTypes, neighborType)) {
            return SlotSelf;
        }
        if (Contains(rules.MergeTypes, neighborType)) {
            return SlotMerge;
        }
        // Р§СѓР¶РѕР№ С‚РІС‘СЂРґС‹Р№ С‚РёРї: РІ РѕСЂРёРіРёРЅР°Р»Рµ TileMergeAttemptWeird РїСЂРµРІСЂР°С‰Р°РµС‚ РµРіРѕ
        // РІ РіСЂР°РЅРёС†Сѓ вЂ” СЂРёСЃСѓРµС‚СЃСЏ РѕР±С‹С‡РЅР°СЏ СЂР°РјРєР° РјРµР¶РґСѓ РјР°С‚РµСЂРёР°Р»Р°РјРё.
        return SlotEdge;
    }

    /// <summary>
    /// РљР»Р°СЃСЃРёС„РёРєР°С†РёСЏ 8 СЃРѕСЃРµРґРµР№ РІРѕРєСЂСѓРі РєР»РµС‚РєРё (cx, cy) РїСЂСЏРјРѕ РёР· РїСЂСЏРјРѕСѓРіРѕР»СЊРЅРёРєР° РєР°СЂС‚С‹.
    /// map[x, y]: x вЂ” РІРїСЂР°РІРѕ, y вЂ” РІРЅРёР· (y = 0 вЂ” РІРµСЂС…РЅСЏСЏ СЃС‚СЂРѕРєР° РїРµСЂРµРґР°РЅРЅРѕРіРѕ РѕРєРЅР°).
    /// </summary>
    public static void ClassifyNeighbors(
        short[,] map, int cx, int cy, TerrariaTilesetRules rules,
        out int up, out int down, out int left, out int right,
        out int upLeft, out int upRight, out int downLeft, out int downRight) {
        left = ClassifyNeighbor(map[cx - 1, cy], rules);
        right = ClassifyNeighbor(map[cx + 1, cy], rules);
        up = ClassifyNeighbor(map[cx, cy - 1], rules);
        down = ClassifyNeighbor(map[cx, cy + 1], rules);
        upLeft = ClassifyNeighbor(map[cx - 1, cy - 1], rules);
        upRight = ClassifyNeighbor(map[cx + 1, cy - 1], rules);
        downLeft = ClassifyNeighbor(map[cx - 1, cy + 1], rules);
        downRight = ClassifyNeighbor(map[cx + 1, cy + 1], rules);
    }

    /// <summary>
    /// РџРѕРґР±РѕСЂ РєР°РґСЂР° РїРѕ 8 РєР»Р°СЃСЃРёС„РёС†РёСЂРѕРІР°РЅРЅС‹Рј СЃРѕСЃРµРґСЏРј.
    /// variant вЂ” РІР°СЂРёР°РЅС‚ С‚РµРєСЃС‚СѓСЂС‹ 0..2 (РІ РѕСЂРёРіРёРЅР°Р»Рµ tile.frameNumber()).
    /// </summary>
    public static void Solve(
        int up, int down, int left, int right,
        int upLeft, int upRight, int downLeft, int downRight,
        int variant, TerrariaTilesetRules rules,
        out int frameX, out int frameY,
        out bool mergeUp, out bool mergeDown, out bool mergeLeft, out bool mergeRight) {
        frameX = -1;
        frameY = -1;
        mergeUp = false;
        mergeDown = false;
        mergeLeft = false;
        mergeRight = false;

        int v = variant % 3;
        if (v < 0) {
            v += 3;
        }

        bool selected = false;
        bool anyMerge = up == SlotMerge || down == SlotMerge || left == SlotMerge || right == SlotMerge;
        bool anySelf = up == SlotSelf || down == SlotSelf || left == SlotSelf || right == SlotSelf;

        // --- Р‘Р»РѕРє 1: merge-Р±Р»РѕРє, РІСЃРµ С‡РµС‚С‹СЂРµ СЃС‚РѕСЂРѕРЅС‹ РЅРµ РіСЂР°РЅРёС†Р° Р РµСЃС‚СЊ С…РѕС‚СЏ Р±С‹ РѕРґРЅР° Merge ---
        // (РІ РѕСЂРёРіРёРЅР°Р»Рµ вЂ” if (up != -1 && ...) СЃ РїРѕСЃР»РµРґСѓСЋС‰РёРјРё merge-РїСЂРѕРІРµСЂРєР°РјРё).
        // Р•СЃР»Рё РІСЃРµ СЃС‚РѕСЂРѕРЅС‹ Self Р±РµР· merge вЂ” СЌС‚Рѕ РѕР±С‹С‡РЅС‹Р№ РІРЅСѓС‚СЂРµРЅРЅРёР№ С‚Р°Р№Р», СѓС…РѕРґРёРј РІ Р‘Р»РѕРє 3.
        if (rules.ParticipatesInMergeBlocks && anyMerge) {
            if (up != SlotEdge && down != SlotEdge && left != SlotEdge && right != SlotEdge) {
                if (up == SlotMerge && down == SlotSelf && left == SlotSelf && right == SlotSelf) {
                    // РЎР»РёСЏРЅРёРµ СЃРІРµСЂС…Сѓ: РєР°РґСЂС‹ (144..180, 108)
                    frameX = 144 + 18 * v;
                    frameY = 108;
                    mergeUp = true;
                } else if (up == SlotSelf && down == SlotMerge && left == SlotSelf && right == SlotSelf) {
                    // РЎР»РёСЏРЅРёРµ СЃРЅРёР·Сѓ: (144..180, 90)
                    frameX = 144 + 18 * v;
                    frameY = 90;
                    mergeDown = true;
                } else if (up == SlotSelf && down == SlotSelf && left == SlotMerge && right == SlotSelf) {
                    // РЎР»РёСЏРЅРёРµ СЃР»РµРІР°: (162, 126..162)
                    frameX = 162;
                    frameY = 126 + 18 * v;
                    mergeLeft = true;
                } else if (up == SlotSelf && down == SlotSelf && left == SlotSelf && right == SlotMerge) {
                    // РЎР»РёСЏРЅРёРµ СЃРїСЂР°РІР°: (144, 126..162)
                    frameX = 144;
                    frameY = 126 + 18 * v;
                    mergeRight = true;
                } else if (up == SlotMerge && down == SlotSelf && left == SlotMerge && right == SlotSelf) {
                    // РЎР»РёСЏРЅРёРµ СЃРІРµСЂС…Сѓ Рё СЃР»РµРІР°: (36, 90..162)
                    frameX = 36;
                    frameY = 90 + 36 * v;
                    mergeUp = true;
                    mergeLeft = true;
                } else if (up == SlotMerge && down == SlotSelf && left == SlotSelf && right == SlotMerge) {
                    // РЎР»РёСЏРЅРёРµ СЃРІРµСЂС…Сѓ Рё СЃРїСЂР°РІР°: (54, 90..162)
                    frameX = 54;
                    frameY = 90 + 36 * v;
                    mergeUp = true;
                    mergeRight = true;
                } else if (up == SlotSelf && down == SlotMerge && left == SlotMerge && right == SlotSelf) {
                    // РЎР»РёСЏРЅРёРµ СЃРЅРёР·Сѓ Рё СЃР»РµРІР°: (36, 108..180)
                    frameX = 36;
                    frameY = 108 + 36 * v;
                    mergeDown = true;
                    mergeLeft = true;
                } else if (up == SlotSelf && down == SlotMerge && left == SlotSelf && right == SlotMerge) {
                    // РЎР»РёСЏРЅРёРµ СЃРЅРёР·Сѓ Рё СЃРїСЂР°РІР°: (54, 108..180)
                    frameX = 54;
                    frameY = 108 + 36 * v;
                    mergeDown = true;
                    mergeRight = true;
                } else if (up == SlotSelf && down == SlotSelf && left == SlotMerge && right == SlotMerge) {
                    // РЎР»РёСЏРЅРёРµ СЃР»РµРІР° Рё СЃРїСЂР°РІР°: (180, 126..162)
                    frameX = 180;
                    frameY = 126 + 18 * v;
                    mergeLeft = true;
                    mergeRight = true;
                } else if (up == SlotMerge && down == SlotMerge && left == SlotSelf && right == SlotSelf) {
                    // РЎР»РёСЏРЅРёРµ СЃРІРµСЂС…Сѓ Рё СЃРЅРёР·Сѓ: (144..180, 180)
                    frameX = 144 + 18 * v;
                    frameY = 180;
                    mergeUp = true;
                    mergeDown = true;
                } else if (up == SlotMerge && down == SlotSelf && left == SlotMerge && right == SlotMerge) {
                    // РЎР»РёСЏРЅРёРµ СЃ С‚СЂС‘С… СЃС‚РѕСЂРѕРЅ (СЃРІРµСЂС…Сѓ, СЃР»РµРІР°, СЃРїСЂР°РІР°): (198, 90..126)
                    frameX = 198;
                    frameY = 90 + 18 * v;
                    mergeUp = true;
                    mergeLeft = true;
                    mergeRight = true;
                } else if (up == SlotSelf && down == SlotMerge && left == SlotMerge && right == SlotMerge) {
                    // РЎР»РёСЏРЅРёРµ СЃ С‚СЂС‘С… СЃС‚РѕСЂРѕРЅ (СЃРЅРёР·Сѓ, СЃР»РµРІР°, СЃРїСЂР°РІР°): (198, 144..180)
                    frameX = 198;
                    frameY = 144 + 18 * v;
                    mergeDown = true;
                    mergeLeft = true;
                    mergeRight = true;
                } else if (up == SlotMerge && down == SlotMerge && left == SlotSelf && right == SlotMerge) {
                    // РЎР»РёСЏРЅРёРµ СЃ С‚СЂС‘С… СЃС‚РѕСЂРѕРЅ (СЃРІРµСЂС…Сѓ, СЃРЅРёР·Сѓ, СЃРїСЂР°РІР°): (216, 144..180)
                    frameX = 216;
                    frameY = 144 + 18 * v;
                    mergeUp = true;
                    mergeDown = true;
                    mergeRight = true;
                } else if (up == SlotMerge && down == SlotMerge && left == SlotMerge && right == SlotSelf) {
                    // РЎР»РёСЏРЅРёРµ СЃ С‚СЂС‘С… СЃС‚РѕСЂРѕРЅ (СЃРІРµСЂС…Сѓ, СЃРЅРёР·Сѓ, СЃР»РµРІР°): (216, 90..126)
                    frameX = 216;
                    frameY = 90 + 18 * v;
                    mergeUp = true;
                    mergeDown = true;
                    mergeLeft = true;
                } else if (up == SlotMerge && down == SlotMerge && left == SlotMerge && right == SlotMerge) {
                    // РЎР»РёСЏРЅРёРµ СЃРѕ РІСЃРµС… СЃС‚РѕСЂРѕРЅ: (108..144, 198)
                    frameX = 108 + 18 * v;
                    frameY = 198;
                    mergeUp = true;
                    mergeDown = true;
                    mergeLeft = true;
                    mergeRight = true;
                } else {
                    // Р’СЃРµ СЃС‚РѕСЂРѕРЅС‹ В«СЃРІРѕРёВ»: РІРЅСѓС‚СЂРµРЅРЅРёРµ СѓРіР»С‹ СЃР»РёСЏРЅРёСЏ РїРѕ РґРёР°РіРѕРЅР°Р»СЏРј
                    // (РІ РѕСЂРёРіРёРЅР°Р»Рµ вЂ” РїРѕСЃР»РµРґРѕРІР°С‚РµР»СЊРЅС‹Рµ if, РїРѕР±РµР¶РґР°РµС‚ РїРѕР·РґРЅСЏСЏ РїСЂРѕРІРµСЂРєР°).
                    bool diagonalMerge = false;
                    if (upLeft == SlotMerge) {
                        frameX = 18;
                        frameY = 108 + 36 * v;
                        diagonalMerge = true;
                    }
                    if (upRight == SlotMerge) {
                        frameX = 0;
                        frameY = 108 + 36 * v;
                        diagonalMerge = true;
                    }
                    if (downLeft == SlotMerge) {
                        frameX = 18;
                        frameY = 90 + 36 * v;
                        diagonalMerge = true;
                    }
                    if (downRight == SlotMerge) {
                        frameX = 0;
                        frameY = 90 + 36 * v;
                        diagonalMerge = true;
                    }
                    if (diagonalMerge) {
                        selected = true;
                    }
                }
                if (frameX >= 0 && frameY >= 0) {
                    // РћРґРЅР° РёР· combo-РІРµС‚РѕРє (РёР»Рё РґРёР°РіРѕРЅР°Р»СЊРЅРѕРµ СЃР»РёСЏРЅРёРµ) РІС‹Р±СЂР°Р»Р° РєР°РґСЂ.
                    selected = true;
                }
            } else if (!rules.IsGrassLike) {
                // --- Р‘Р»РѕРє 2: С‡Р°СЃС‚РёС‡РЅС‹Рµ СЃР»РёСЏРЅРёСЏ, С…РѕС‚СЏ Р±С‹ РѕРґРЅР° СЃС‚РѕСЂРѕРЅР° вЂ” РіСЂР°РЅРёС†Р° ---
                // Р’РђР–РќРћ: В«СЃРѕРµРґРёРЅС‘РЅРЅР°СЏВ» СЃС‚РѕСЂРѕРЅР° Р·РґРµСЃСЊ вЂ” Р»СЋР±Р°СЏ РЅРµ-РіСЂР°РЅРёС†Р°
                // (SlotSelf РР›Р SlotMerge), РєР°Рє РІ РѕСЂРёРіРёРЅР°Р»Рµ, РіРґРµ РїСЂРѕРІРµСЂРєРё РІРёРґР°
                // `left == num || left == num32` РѕС…РІР°С‚С‹РІР°СЋС‚ РѕР±Р° РІР°СЂРёР°РЅС‚Р°.
                // РРЅР°С‡Рµ РєР°РјРµРЅСЊ РІ СЃР»РѕР¶РЅС‹С… РєРѕРЅС„РёРіСѓСЂР°С†РёСЏС… СЂРёСЃСѓРµС‚ generic-РєР°РґСЂС‹

                // --- 2Р°: РѕРґРЅРѕ СЃР»РёСЏРЅРёРµ + РїСЂРѕС‚РёРІРѕРїРѕР»РѕР¶РЅР°СЏ СЃС‚РѕСЂРѕРЅР° РіСЂР°РЅРёС†Р° ---
                if (up == SlotEdge && down == SlotMerge && left != SlotEdge && right != SlotEdge) {
                    // РЎР»РёСЏРЅРёРµ СЃРЅРёР·Сѓ РїСЂРё РѕС‚РєСЂС‹С‚РѕРј РІРµСЂС…Рµ: (234..270, 0)
                    frameX = 234 + 18 * v;
                    frameY = 0;
                    mergeDown = true;
                    selected = true;
                } else if (up == SlotMerge && down == SlotEdge && left != SlotEdge && right != SlotEdge) {
                    // РЎР»РёСЏРЅРёРµ СЃРІРµСЂС…Сѓ РїСЂРё РѕС‚РєСЂС‹С‚РѕРј РЅРёР·Рµ: (234..270, 18)
                    frameX = 234 + 18 * v;
                    frameY = 18;
                    mergeUp = true;
                    selected = true;
                } else if (up != SlotEdge && down != SlotEdge && left == SlotEdge && right == SlotMerge) {
                    // РЎР»РёСЏРЅРёРµ СЃРїСЂР°РІР° РїСЂРё РѕС‚РєСЂС‹С‚РѕРј Р»РµРІРµ: (234..270, 36)
                    frameX = 234 + 18 * v;
                    frameY = 36;
                    mergeRight = true;
                    selected = true;
                } else if (up != SlotEdge && down != SlotEdge && left == SlotMerge && right == SlotEdge) {
                    // РЎР»РёСЏРЅРёРµ СЃР»РµРІР° РїСЂРё РѕС‚РєСЂС‹С‚РѕРј РїСЂР°РІРµ: (234..270, 54)
                    frameX = 234 + 18 * v;
                    frameY = 54;
                    mergeLeft = true;
                    selected = true;
                }
                // --- 2Р±: СЃР»РёСЏРЅРёРµ + В«СЃРѕРµРґРёРЅС‘РЅРЅС‹РµВ» СЃС‚РѕСЂРѕРЅС‹ (СЃС‚СЂРѕРіРѕ Self, РєР°Рє == num РІ РѕСЂРёРіРёРЅР°Р»Рµ) ---
                if (!selected && up != SlotEdge && down != SlotEdge && left == SlotEdge && right == SlotSelf) {
                    // РЎР»РёСЏРЅРёРµ СЃРІРµСЂС…Сѓ/СЃРЅРёР·Сѓ РїСЂРё РѕС‚РєСЂС‹С‚РѕРј Р»РµРІРµ, СЃРїСЂР°РІР° Self
                    if (up == SlotMerge && down == SlotSelf) {
                        frameX = 72;
                        frameY = 144 + 18 * v; // (72, 144..180)
                        mergeUp = true;
                        selected = true;
                    } else if (down == SlotMerge && up == SlotSelf) {
                        frameX = 72;
                        frameY = 90 + 18 * v; // (72, 90..126)
                        mergeDown = true;
                        selected = true;
                    }
                }
                if (!selected && up != SlotEdge && down != SlotEdge && left == SlotSelf && right == SlotEdge) {
                    // РЎР»РёСЏРЅРёРµ СЃРІРµСЂС…Сѓ/СЃРЅРёР·Сѓ РїСЂРё РѕС‚РєСЂС‹С‚РѕРј РїСЂР°РІРµ, СЃР»РµРІР° Self
                    if (up == SlotMerge && down == SlotSelf) {
                        frameX = 90;
                        frameY = 144 + 18 * v; // (90, 144..180)
                        mergeUp = true;
                        selected = true;
                    } else if (down == SlotMerge && up == SlotSelf) {
                        frameX = 90;
                        frameY = 90 + 18 * v; // (90, 90..126)
                        mergeDown = true;
                        selected = true;
                    }
                }
                if (!selected && up == SlotEdge && down == SlotSelf && left != SlotEdge && right != SlotEdge) {
                    // РЎР»РёСЏРЅРёРµ СЃР»РµРІР°/СЃРїСЂР°РІР° РїСЂРё РѕС‚РєСЂС‹С‚РѕРј РІРµСЂС…Рµ, СЃРЅРёР·Сѓ Self
                    if (left == SlotMerge && right == SlotSelf) {
                        frameX = 18 * v; // (0..36, 198)
                        frameY = 198;
                        mergeLeft = true;
                        selected = true;
                    } else if (right == SlotMerge && left == SlotSelf) {
                        frameX = 54 + 18 * v; // (54..90, 198)
                        frameY = 198;
                        mergeRight = true;
                        selected = true;
                    }
                }
                if (!selected && up == SlotSelf && down == SlotEdge && left != SlotEdge && right != SlotEdge) {
                    // РЎР»РёСЏРЅРёРµ СЃР»РµРІР°/СЃРїСЂР°РІР° РїСЂРё РѕС‚РєСЂС‹С‚РѕРј РЅРёР·Рµ, СЃРІРµСЂС…Сѓ Self
                    if (left == SlotMerge && right == SlotSelf) {
                        frameX = 18 * v; // (0..36, 216)
                        frameY = 216;
                        mergeLeft = true;
                        selected = true;
                    } else if (right == SlotMerge && left == SlotSelf) {
                        frameX = 54 + 18 * v; // (54..90, 216)
                        frameY = 216;
                        mergeRight = true;
                        selected = true;
                    }
                }
                // --- 2РІ: РґРІР° СЃР»РёСЏРЅРёСЏ + РґРІРµ РѕС‚РєСЂС‹С‚С‹Рµ СЃС‚РѕСЂРѕРЅС‹ ---
                if (!selected && up != SlotEdge && down != SlotEdge && left == SlotEdge && right == SlotEdge) {
                    // РЎР»РёСЏРЅРёРµ СЃРІРµСЂС…Сѓ/СЃРЅРёР·Сѓ РїСЂРё РѕС‚РєСЂС‹С‚С‹С… Р»РµРІРµ Рё РїСЂР°РІРµ
                    if (up == SlotMerge && down == SlotMerge) {
                        frameX = 108;
                        frameY = 216 + 18 * v; // (108, 216..252)
                        mergeUp = true;
                        mergeDown = true;
                        selected = true;
                    } else if (up == SlotMerge) {
                        frameX = 126;
                        frameY = 144 + 18 * v; // (126, 144..180)
                        mergeUp = true;
                        selected = true;
                    } else if (down == SlotMerge) {
                        frameX = 126;
                        frameY = 90 + 18 * v; // (126, 90..126)
                        mergeDown = true;
                        selected = true;
                    }
                }
                if (!selected && up == SlotEdge && down == SlotEdge && left != SlotEdge && right != SlotEdge) {
                    // РЎР»РёСЏРЅРёРµ СЃР»РµРІР°/СЃРїСЂР°РІР° РїСЂРё РѕС‚РєСЂС‹С‚С‹С… РІРµСЂС…Рµ Рё РЅРёР·Рµ
                    if (left == SlotMerge && right == SlotMerge) {
                        frameX = 162 + 18 * v; // (162..198, 198)
                        frameY = 198;
                        mergeLeft = true;
                        mergeRight = true;
                        selected = true;
                    } else if (left == SlotMerge) {
                        frameX = 18 * v; // (0..36, 252)
                        frameY = 252;
                        mergeLeft = true;
                        selected = true;
                    } else if (right == SlotMerge) {
                        frameX = 54 + 18 * v; // (54..90, 252)
                        frameY = 252;
                        mergeRight = true;
                        selected = true;
                    }
                }
                // --- 2Рґ: РґРІР° СЃР»РёСЏРЅРёСЏ (РІРµСЂС…+РЅРёР·) РїСЂРё РѕС‚РєСЂС‹С‚С‹С… Р»РµРІРµ Рё РїСЂР°РІРµ ---
                if (!selected && up == SlotMerge && down == SlotMerge && left == SlotEdge && right == SlotEdge) {
                    // РЎР»РёСЏРЅРёРµ СЃРІРµСЂС…Сѓ Рё СЃРЅРёР·Сѓ РїСЂРё РѕС‚РєСЂС‹С‚С‹С… Р»РµРІРµ Рё РїСЂР°РІРµ: (108, 216..252)
                    frameX = 108;
                    frameY = 216 + 18 * v;
                    mergeUp = true;
                    mergeDown = true;
                    selected = true;
                }
                // --- 2e: dva sliyaniya (levo+pravo) pri otkrytykh verkh i niz ---
                if (!selected && up == SlotEdge && down == SlotEdge && left == SlotMerge && right == SlotMerge) {
                    frameX = 162 + 18 * v;
                    frameY = 198;
                    mergeLeft = true;
                    mergeRight = true;
                    selected = true;
                }
                // --- 2g: odno sliyaniye, ostalnyye otkryty ---
                if (!selected && up == SlotEdge && down == SlotEdge && left == SlotMerge && right == SlotEdge) {
                    frameX = 18 * v;
                    frameY = 234;
                    mergeLeft = true;
                    selected = true;
                } else if (!selected && up == SlotEdge && down == SlotEdge && left == SlotEdge && right == SlotMerge) {
                    frameX = 54 + 18 * v;
                    frameY = 234;
                    mergeRight = true;
                    selected = true;
                }
            }
        }

        // --- Р‘Р»РѕРє 3: РѕР±С‰РёР№ РЅР°Р±РѕСЂ РєР°РґСЂРѕРІ РїРѕ 4 СЃС‚РѕСЂРѕРЅР°Рј Рё РґРёР°РіРѕРЅР°Р»СЏРј ---
        if (!selected) {
            if (IsConnected(up) && IsConnected(down) && IsConnected(left) && IsConnected(right)) {
                // Р’РЅСѓС‚СЂРµРЅРЅРёР№ С‚Р°Р№Р»: РІР°СЂРёР°РЅС‚С‹ Р±РµР· РіСЂР°РЅРёС†, СЃ РІРЅСѓС‚СЂРµРЅРЅРёРјРё СѓРіР»Р°РјРё РїРѕ РґРёР°РіРѕРЅР°Р»СЏРј.
                if (!IsConnected(upLeft) && !IsConnected(upRight)) {
                    frameX = 108 + 18 * v; // (108..144, 18)
                    frameY = 18;
                } else if (!IsConnected(downLeft) && !IsConnected(downRight)) {
                    frameX = 108 + 18 * v; // (108..144, 36)
                    frameY = 36;
                } else if (!IsConnected(upLeft) && !IsConnected(downLeft)) {
                    frameX = 180; // (180, 0..36)
                    frameY = 18 * v;
                } else if (!IsConnected(upRight) && !IsConnected(downRight)) {
                    frameX = 198; // (198, 0..36)
                    frameY = 18 * v;
                } else {
                    frameX = 18 + 18 * v; // (18..54, 18)
                    frameY = 18;
                }
            } else if (!IsConnected(up) && IsConnected(down) && IsConnected(left) && IsConnected(right)) {
                // Р’РµСЂС…РЅРёР№ РєСЂР°Р№: (18..54, 0)
                frameX = 18 + 18 * v;
                frameY = 0;
            } else if (IsConnected(up) && !IsConnected(down) && IsConnected(left) && IsConnected(right)) {
                // РќРёР¶РЅРёР№ РєСЂР°Р№: (18..54, 36)
                frameX = 18 + 18 * v;
                frameY = 36;
            } else if (IsConnected(up) && IsConnected(down) && !IsConnected(left) && IsConnected(right)) {
                // Р›РµРІС‹Р№ РєСЂР°Р№: (0, 0..36)
                frameX = 0;
                frameY = 18 * v;
            } else if (IsConnected(up) && IsConnected(down) && IsConnected(left) && !IsConnected(right)) {
                // РџСЂР°РІС‹Р№ РєСЂР°Р№: (72, 0..36)
                frameX = 72;
                frameY = 18 * v;
            } else if (!IsConnected(up) && IsConnected(down) && !IsConnected(left) && IsConnected(right)) {
                // Р’РЅРµС€РЅРёР№ СѓРіРѕР» СЃРІРµСЂС…Сѓ-СЃР»РµРІР°: (0..72, 54)
                frameX = 36 * v;
                frameY = 54;
            } else if (!IsConnected(up) && IsConnected(down) && IsConnected(left) && !IsConnected(right)) {
                // Р’РЅРµС€РЅРёР№ СѓРіРѕР» СЃРІРµСЂС…Сѓ-СЃРїСЂР°РІР°: (18..90, 54)
                frameX = 18 + 36 * v;
                frameY = 54;
            } else if (IsConnected(up) && !IsConnected(down) && !IsConnected(left) && IsConnected(right)) {
                // Р’РЅРµС€РЅРёР№ СѓРіРѕР» СЃРЅРёР·Сѓ-СЃР»РµРІР°: (0..72, 72)
                frameX = 36 * v;
                frameY = 72;
            } else if (IsConnected(up) && !IsConnected(down) && IsConnected(left) && !IsConnected(right)) {
                // Р’РЅРµС€РЅРёР№ СѓРіРѕР» СЃРЅРёР·Сѓ-СЃРїСЂР°РІР°: (18..90, 72)
                frameX = 18 + 36 * v;
                frameY = 72;
            } else if (IsConnected(up) && IsConnected(down) && !IsConnected(left) && !IsConnected(right)) {
                // Р“РѕСЂРёР·РѕРЅС‚Р°Р»СЊРЅР°СЏ РїРѕР»РѕСЃР°: (90, 0..36)
                frameX = 90;
                frameY = 18 * v;
            } else if (!IsConnected(up) && !IsConnected(down) && IsConnected(left) && IsConnected(right)) {
                // Р’РµСЂС‚РёРєР°Р»СЊРЅР°СЏ РїРѕР»РѕСЃР°: (108..144, 72)
                frameX = 108 + 18 * v;
                frameY = 72;
            } else if (!IsConnected(up) && IsConnected(down) && !IsConnected(left) && !IsConnected(right)) {
                // В«РЁР°РїРєР°В» СЃРІРµСЂС…Сѓ: (108..144, 0)
                frameX = 108 + 18 * v;
                frameY = 0;
            } else if (IsConnected(up) && !IsConnected(down) && !IsConnected(left) && !IsConnected(right)) {
                // В«РЁР°РїРєР°В» СЃРЅРёР·Сѓ: (108..144, 54)
                frameX = 108 + 18 * v;
                frameY = 54;
            } else if (!IsConnected(up) && !IsConnected(down) && !IsConnected(left) && IsConnected(right)) {
                // РџРѕР»РѕСЃР°, РѕС‚РєСЂС‹С‚Р°СЏ СЃРІРµСЂС…Сѓ, СЃРЅРёР·Сѓ Рё СЃР»РµРІР°: (162, 0..36)
                frameX = 162;
                frameY = 18 * v;
            } else if (!IsConnected(up) && !IsConnected(down) && IsConnected(left) && !IsConnected(right)) {
                // РџРѕР»РѕСЃР°, РѕС‚РєСЂС‹С‚Р°СЏ СЃРІРµСЂС…Сѓ, СЃРЅРёР·Сѓ Рё СЃРїСЂР°РІР°: (216, 0..36)
                frameX = 216;
                frameY = 18 * v;
            } else {
                // РћРґРёРЅРѕС‡РЅС‹Р№ Р±Р»РѕРє: (162..198, 54)
                frameX = 162 + 18 * v;
                frameY = 54;
            }
            selected = true;
        }

        // --- Р‘Р»РѕРє 4: Р±РµР·РѕРїР°СЃРЅС‹Р№ РІР°СЂРёР°РЅС‚, РµСЃР»Рё РЅРёС‡РµРіРѕ РЅРµ РїРѕРґРѕС€Р»Рѕ ---
        if (!selected) {
            frameX = 18 + 18 * v;
            frameY = 18;
        }

        // --- Р¤Р»Р°РіРё СЃР»РёСЏРЅРёСЏ РґР»СЏ РІР·Р°РёРјРЅРѕР№ РїСЂРѕРІРµСЂРєРё Р·РµРјР»Рё СЃ РєР°РјРЅРµРј ---
        // Р’ РѕСЂРёРіРёРЅР°Р»Рµ Terraria: Р»СЋР±РѕР№ СЃРѕСЃРµРґ merge_type (РґР»СЏ РєР°РјРЅСЏ вЂ” Р·РµРјР»СЏ, РґР»СЏ
        // Р·РµРјР»Рё вЂ” РєР°РјРµРЅСЊ РїРѕСЃР»Рµ РІР·Р°РёРјРЅРѕР№ РїСЂРѕРІРµСЂРєРё) СЃС‡РёС‚Р°РµС‚СЃСЏ В«СЃРІРѕРёРјВ» вЂ” СЂРёСЃСѓРµС‚СЃСЏ
        // Р±РµСЃС€РѕРІРЅРѕ. РџРѕСЌС‚РѕРјСѓ С„Р»Р°Рі РІС‹СЃС‚Р°РІР»СЏРµС‚СЃСЏ РґР»СЏ РљРђР–Р”РћР™ СЃС‚РѕСЂРѕРЅС‹ СЃРѕ Р·РЅР°С‡РµРЅРёРµРј
        // SlotMerge, РЅРµР·Р°РІРёСЃРёРјРѕ РѕС‚ С‚РѕРіРѕ, РєР°РєРѕР№ РєР°РґСЂ (merge РёР»Рё generic) РІС‹Р±СЂР°РЅ.
        // Р­С‚Рѕ РіР°СЂР°РЅС‚РёСЂСѓРµС‚, С‡С‚Рѕ РІСЃРµ СЃРѕСЃРµРґРё-Р·РµРјР»СЏ РІРѕРєСЂСѓРі РєР°РјРЅСЏ РїСЂРѕС…РѕРґСЏС‚ РІР·Р°РёРјРЅСѓСЋ
        // РїСЂРѕРІРµСЂРєСѓ Рё РЅРµ СЂРёСЃСѓСЋС‚ СЃРІРѕРё РєСЂР°СЏ.
        mergeUp = up == SlotMerge;
        mergeDown = down == SlotMerge;
        mergeLeft = left == SlotMerge;
        mergeRight = right == SlotMerge;
    }

    /// <summary>
    /// РЎС‚РѕСЂРѕРЅР° СЃС‡РёС‚Р°РµС‚СЃСЏ В«СЃРѕРµРґРёРЅС‘РЅРЅРѕР№В» РµСЃР»Рё СЃРѕСЃРµРґ вЂ” Self (С‚РѕС‚ Р¶Рµ РјР°С‚РµСЂРёР°Р»)
    /// РР›Р Merge (РґСЂСѓРіРѕР№ РјР°С‚РµСЂРёР°Р», СЃ РєРѕС‚РѕСЂС‹Рј РµСЃС‚СЊ РєР°РґСЂС‹ СЃР»РёСЏРЅРёСЏ).
    /// Р’ РѕСЂРёРіРёРЅР°Р»Рµ Terraria dirt-mergeable С‚РёРїС‹ СЂРёСЃСѓСЋС‚ Р±РµСЃС€РѕРІРЅРѕРµ СЃРѕРµРґРёРЅРµРЅРёРµ
    /// Рё СЃ СЃРѕСЃРµРґСЏРјРё СЃРІРѕРµРіРѕ С‚РёРїР°, Рё СЃ СЃРѕСЃРµРґСЏРјРё merge-С‚РёРїР°.
    /// </summary>
    private static bool IsConnected(int slot) {
        return slot == SlotSelf || slot == SlotMerge;
    }

    private static bool Contains(short[] types, short type) {
        for (int i = 0; i < types.Length; i++) {
            if (types[i] == type) {
                return true;
            }
        }
        return false;
    }
}
