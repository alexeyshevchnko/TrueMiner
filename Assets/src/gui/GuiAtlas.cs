using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Загрузчик спрайтов из атласа интерфейса Resources/gui (gui.png + gui.txt).
/// gui.png импортирован как одиночная текстура (spriteMode: Single), нарезанных
/// Unity-спрайтов в мете нет, а gui.txt — это TexturePacker JSON с координатами кадров.
/// Координаты в gui.txt отсчитываются от верхнего левого угла, Sprite.Create требует
/// прямоугольник от нижнего левого, поэтому Y кадра пересчитывается.
/// Спрайты создаются один раз и кэшируются; ключи — имя кадра как с расширением .png,
/// так и без него, регистр не учитывается (иконки предметов: "Axe", "Item1", "kirka"...).
/// </summary>
public static class GuiAtlas
{
    private const string AtlasTexturePath = "gui/gui"; // Resources/gui/gui.png
    private const string AtlasFramesPath = "gui/gui";  // Resources/gui/gui.txt

    private static Dictionary<string, Sprite> sprites;
    private static readonly HashSet<string> warnedMissing = new HashSet<string>();

    /// <summary>Возвращает спрайт из атласа GUI по имени (например "kirka" или "kirka.png"); null, если нет.</summary>
    public static Sprite Get(string iconName)
    {
        if (string.IsNullOrEmpty(iconName))
            return null;

        if (sprites == null)
        {
            lock (warnedMissing) { } // no-op guard: Get вызывается только из main thread
            Load();
        }

        Sprite sprite;
        if (sprites.TryGetValue(NormalizeKey(iconName), out sprite))
            return sprite;

        WarnMissing(iconName);
        return null;
    }

    private static void WarnMissing(string iconName)
    {
        if (warnedMissing.Add(NormalizeKey(iconName)))
            Debug.LogWarning("GuiAtlas: иконка '" + iconName + "' не найдена в атласе gui.txt");
    }

    private static string NormalizeKey(string name)
    {
        return name.Trim().ToLowerInvariant();
    }

    private static void Load()
    {
        sprites = new Dictionary<string, Sprite>();

        var texture = Resources.Load<Texture2D>(AtlasTexturePath);
        if (texture == null)
        {
            Debug.LogError("GuiAtlas: не найдена текстура атласа Resources/" + AtlasTexturePath + ".png");
            return;
        }

        var framesAsset = Resources.Load<TextAsset>(AtlasFramesPath);
        if (framesAsset == null)
        {
            Debug.LogError("GuiAtlas: не найдено описание кадров Resources/" + AtlasFramesPath + ".txt");
            return;
        }

        object parsed;
        if (!MiniJson.TryParse(framesAsset.text, out parsed))
        {
            Debug.LogError("GuiAtlas: не удалось разобрать gui.txt как JSON");
            return;
        }

        var root = parsed as Dictionary<string, object>;
        var frames = (root != null) ? root["frames"] as Dictionary<string, object> : null;
        if (frames == null)
        {
            Debug.LogError("GuiAtlas: в gui.txt отсутствует секция frames");
            return;
        }

        foreach (var frame in frames)
        {
            var data = frame.Value as Dictionary<string, object>;
            var rectData = (data != null) ? data["frame"] as Dictionary<string, object> : null;
            if (rectData == null)
                continue;

            int x = Convert.ToInt32(rectData["x"]);
            int y = Convert.ToInt32(rectData["y"]);
            int w = Convert.ToInt32(rectData["w"]);
            int h = Convert.ToInt32(rectData["h"]);

            if (w <= 0 || h <= 0)
                continue;

            // TexturePacker: Y от верхнего края; Unity: прямоугольник от нижнего края.
            var unityRect = new Rect(x, texture.height - (y + h), w, h);
            var sprite = Sprite.Create(texture, unityRect, new Vector2(0.5f, 0.5f), 1f);
            sprite.name = frame.Key;

            string key = NormalizeKey(frame.Key);
            sprites[key] = sprite;
            sprites[NormalizeKey(StripExtension(frame.Key))] = sprite;
        }
    }

    private static string StripExtension(string name)
    {
        int dot = name.LastIndexOf('.');
        return (dot > 0) ? name.Substring(0, dot) : name;
    }

    /// <summary>
    /// Минимальный парсер JSON для описания атласа (объекты, массивы, строки, числа,
    /// true/false/null). В проекте нет внешних JSON-библиотек, а встроенный
    /// JsonUtility не умеет разбирать объекты с произвольными ключами ("frames": {"Axe.png": ...}).
    /// </summary>
    private static class MiniJson
    {
        public static bool TryParse(string json, out object value)
        {
            int index = 0;
            try
            {
                value = ParseValue(json, ref index);
                SkipWhitespace(json, ref index);
                return value != null && index >= json.Length;
            }
            catch (Exception e)
            {
                Debug.LogWarning("GuiAtlas.MiniJson: ошибка разбора — " + e.Message);
                value = null;
                return false;
            }
        }

        private static object ParseValue(string s, ref int i)
        {
            SkipWhitespace(s, ref i);
            if (i >= s.Length)
                throw new FormatException("неожиданный конец строки");

            char c = s[i];
            switch (c)
            {
                case '{':
                    return ParseObject(s, ref i);
                case '[':
                    return ParseArray(s, ref i);
                case '"':
                    return ParseString(s, ref i);
                case 't':
                    Expect(s, ref i, "true");
                    return true;
                case 'f':
                    Expect(s, ref i, "false");
                    return false;
                case 'n':
                    Expect(s, ref i, "null");
                    return null;
                default:
                    return ParseNumber(s, ref i);
            }
        }

        private static Dictionary<string, object> ParseObject(string s, ref int i)
        {
            var result = new Dictionary<string, object>(StringComparer.Ordinal);
            i++; // '{'
            SkipWhitespace(s, ref i);
            if (i < s.Length && s[i] == '}')
            {
                i++;
                return result;
            }

            while (true)
            {
                SkipWhitespace(s, ref i);
                string key = ParseString(s, ref i);
                SkipWhitespace(s, ref i);
                ExpectChar(s, ref i, ':');
                result[key] = ParseValue(s, ref i);
                SkipWhitespace(s, ref i);
                if (i >= s.Length)
                    throw new FormatException("незакрытый объект");

                char c = s[i];
                i++;
                if (c == '}')
                    return result;
                if (c != ',')
                    throw new FormatException("ожидались ',' или '}'");
            }
        }

        private static List<object> ParseArray(string s, ref int i)
        {
            var result = new List<object>();
            i++; // '['
            SkipWhitespace(s, ref i);
            if (i < s.Length && s[i] == ']')
            {
                i++;
                return result;
            }

            while (true)
            {
                result.Add(ParseValue(s, ref i));
                SkipWhitespace(s, ref i);
                if (i >= s.Length)
                    throw new FormatException("незакрытый массив");

                char c = s[i];
                i++;
                if (c == ']')
                    return result;
                if (c != ',')
                    throw new FormatException("ожидались ',' или ']'");
            }
        }

        private static string ParseString(string s, ref int i)
        {
            ExpectChar(s, ref i, '"');
            var sb = new System.Text.StringBuilder();
            while (true)
            {
                if (i >= s.Length)
                    throw new FormatException("незакрытая строка");

                char c = s[i++];
                if (c == '"')
                    break;
                if (c == '\\')
                {
                    if (i >= s.Length)
                        throw new FormatException("незавершённый escape");
                    char esc = s[i++];
                    switch (esc)
                    {
                        case '"': sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        case '/': sb.Append('/'); break;
                        case 'b': sb.Append('\b'); break;
                        case 'f': sb.Append('\f'); break;
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        case 'u':
                            sb.Append((char)Convert.ToInt32(s.Substring(i, 4), 16));
                            i += 4;
                            break;
                        default:
                            throw new FormatException("неизвестный escape \\" + esc);
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        private static object ParseNumber(string s, ref int i)
        {
            int start = i;
            while (i < s.Length && "-+.eE0123456789".IndexOf(s[i]) >= 0)
                i++;
            string token = s.Substring(start, i - start);
            if (token.Length == 0)
                throw new FormatException("не удалось разобрать значение в позиции " + start);
            return double.Parse(token, System.Globalization.CultureInfo.InvariantCulture);
        }

        private static void SkipWhitespace(string s, ref int i)
        {
            while (i < s.Length && char.IsWhiteSpace(s[i]))
                i++;
        }

        private static void Expect(string s, ref int i, string literal)
        {
            if (string.CompareOrdinal(s, i, literal, 0, literal.Length) != 0)
                throw new FormatException("ожидалось '" + literal + "' в позиции " + i);
            i += literal.Length;
        }

        private static void ExpectChar(string s, ref int i, char c)
        {
            if (i >= s.Length || s[i] != c)
                throw new FormatException("ожидался '" + c + "' в позиции " + i);
            i++;
        }
    }
}
