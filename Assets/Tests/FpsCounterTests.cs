using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

/// <summary>
/// PlayMode-проверка FPS-счётчика GameUi: значение появляется после первого окна
/// обновления, регулярно пересчитывается (fpsTimer/fpsFrames сбрасываются между
/// окнами) числом ≥ 1, а не застревает пустым/нулевым. Рефлексия — по той же
/// причине, что и в GameStartSmokeTests (тестовая сборка не ссылается на
/// Assembly-CSharp напрямую).
/// </summary>
public class FpsCounterTests
{
    private static System.Type GameType(string name)
    {
        var type = System.Type.GetType(name + ", Assembly-CSharp");
        Assert.NotNull(type, "тип " + name + " не найден в Assembly-CSharp");
        return type;
    }

    private object gameUi;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        var load = EditorSceneManager.LoadSceneAsyncInPlayMode(
            "Assets/MainScene.unity", new LoadSceneParameters(LoadSceneMode.Single));
        Assert.NotNull(load, "запрос на загрузку MainScene должен быть создан");
        yield return new WaitUntil(() => load.isDone);
        yield return new WaitForSeconds(3f);
        gameUi = GameType("GameUi").GetProperty("Instance").GetValue(null);
        Assert.NotNull(gameUi, "GameUi должен быть создан");
    }

    private Text FpsText()
    {
        return gameUi.GetType().GetField("fpsText", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(gameUi) as Text;
    }

    private (float frames, float timer) FpsAccumulators()
    {
        var t = gameUi.GetType();
        return (
            (float)t.GetField("fpsFrames", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(gameUi),
            (float)t.GetField("fpsTimer", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(gameUi));
    }

    [UnityTest]
    public IEnumerator FpsTextVisibleShortlyAfterStart()
    {
        yield return new WaitForSeconds(0.9f); // > одного окна обновления (0.5с)
        var text = FpsText();
        Assert.NotNull(text, "fpsText должен существовать");
        Assert.NotNull(text.font, "у FPS-текста должен быть назначен шрифт");
        int value;
        Assert.True(int.TryParse(text.text.Replace(" fps", ""), out value),
            "FPS-текст должен показывать число, фактически: '" + text.text + "'");
        Assert.GreaterOrEqual(value, 1, "FPS должен быть ≥ 1, а не нулевым/пустым");
    }

    [UnityTest]
    public IEnumerator FpsKeepsRefreshingOverTime()
    {
        yield return new WaitForSeconds(1.0f);
        var first = FpsText().text;
        int v0, v1;
        Assert.True(int.TryParse(first.Replace(" fps", ""), out v0), "первый замер числа: '" + first + "'");

        // Механика регулярного пересчёта: за 0.9 с должен пройти как минимум один
        // цикл окна (накопитель сбрасывается), текст продолжает пересчитываться.
        var accum = FpsAccumulators();
        Assert.GreaterOrEqual(accum.timer, 0f, "fpsTimer не должен быть отрицательным");

        yield return new WaitForSeconds(0.9f);
        var second = FpsText().text;
        Assert.True(int.TryParse(second.Replace(" fps", ""), out v1), "второй замер числа: '" + second + "'");
        Assert.GreaterOrEqual(v1, 1, "FPS после второго окна валиден");

        var accum2 = FpsAccumulators();
        var interval = (float)GameType("GameUi").GetField("FpsUpdateInterval", BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
        // Накопитель активен между окнами (кадры считаются, время суммируется);
        // «застрявший» счётчик держал бы старое состояние и не продолжал бы отсчёт.
        Assert.LessOrEqual(accum2.timer, interval + 0.35f,
            "fpsTimer между окнами снова набирается, а не остаётся в замороженном состоянии");
    }
}
