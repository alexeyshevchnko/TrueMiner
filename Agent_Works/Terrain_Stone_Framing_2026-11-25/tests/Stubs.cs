// Заглушки для компиляции игровых файлов вне Unity (только для теста решателя).
namespace UnityEngine {
}

// Минимальный аналог TileInfoStructure: движку кадрирования нужны только
// type (тип блока) и randomTile (вариант текстуры).
public class TileInfoStructure {
    public short type;
    public short randomTile;

    public TileInfoStructure(short type, short randomTile) {
        this.type = type;
        this.randomTile = randomTile;
    }
}
