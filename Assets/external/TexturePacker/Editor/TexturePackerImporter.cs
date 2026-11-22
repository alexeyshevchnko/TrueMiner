/*
 *  TexturePacker Importer
 *  (c) CodeAndWeb GmbH, Saalbaustraße 61, 89233 Neu-Ulm, Germany
 * 
 *  Use this script to import sprite sheets generated with TexturePacker.
 *  For more information see http://www.codeandweb.com/texturepacker/unity
 * 
 *  Thanks to Brendan Campbell for providing a first version of this script!
 *
 */

using UnityEditor;
using UnityEditor.U2D.Sprites;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TexturePackerImporter : AssetPostprocessor
{
	const string IMPORTER_VERSION = "3.5.0";

	static string[] textureExtensions = {
		".png",
		".jpg",
		".jpeg",
		".tiff",
		".tga",
		".bmp"
	};

    private static bool importPivotPoints = false;// EditorPrefs.GetBool("TPImporter.ImportPivotPoints", true);

	/*
	 *  Pivot point import can be disabled in the Preferences dialog (menu item Unity->Preferences, TexturePacker sheet).
	 *  Замена устаревшего [PreferenceItem] (CS0618) на современный [SettingsProvider]:
	 *  та же секция в Preferences и тот же EditorPrefs-ключ "TPImporter.ImportPivotPoints".
	 */
	[SettingsProvider]
	static SettingsProvider PreferencesSettingsProvider()
	{
		var provider = new SettingsProvider("Preferences/TexturePacker", SettingsScope.User)
		{
			label = "TexturePacker",
			guiHandler = searchContext =>
			{
				importPivotPoints = EditorGUILayout.Toggle("Always import pivot points", importPivotPoints);
				if (GUI.changed)
					EditorPrefs.SetBool("TPImporter.ImportPivotPoints", importPivotPoints);
			}
		};
		return provider;
	}


	/*
	 *  Trigger a texture file re-import each time the .tpsheet file changes (or is manually re-imported)
	 */
	static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
	{
		foreach (var asset in importedAssets) {
			if (!Path.GetExtension (asset).Equals (".tpsheet"))
				continue;
			foreach (string ext in textureExtensions) {
				string pathToTexture = Path.ChangeExtension (asset, ext);
				if (File.Exists (pathToTexture)) {
					AssetDatabase.ImportAsset (pathToTexture, ImportAssetOptions.ForceUpdate);
					break;
				}
			}
		}
	}


	/*
	 *  Trigger a sprite sheet update each time the texture file changes (or is manually re-imported)
	 */
	void OnPreprocessTexture ()
	{
		TextureImporter importer = assetImporter as TextureImporter;

		string pathToData = Path.ChangeExtension (assetPath, ".tpsheet");
		if (File.Exists (pathToData)) {
			updateSpriteMetaData (importer, pathToData);
		}
	}

	static void updateSpriteMetaData (TextureImporter importer, string pathToData)
	{
		if (importer.textureType != TextureImporterType.Default) {
			importer.textureType = TextureImporterType.Sprite;
		}
		importer.maxTextureSize = 4096;
		importer.spriteImportMode = SpriteImportMode.Multiple;

		string[] dataFileContent = File.ReadAllLines(pathToData);
		int format = 30302;

		foreach (string row in dataFileContent)
		{
			if (row.StartsWith(":format=")) {
				format = int.Parse(row.Remove(0,8));
			}
		}
		if (format != 30302) {
			EditorUtility.DisplayDialog("Please update TexturePacker Importer", "Your TexturePacker Importer is too old, \nplease load a new version from the asset store!", "Ok");
			return;
		}

		// Современный API данных спрайтов (замена удалённого importer.spritesheet, CS0618).
		var factory = new SpriteDataProviderFactories();
		factory.Init();
		ISpriteEditorDataProvider dataProvider = factory.GetSpriteEditorDataProviderFromObject(importer);
		if (dataProvider == null)
			return; // нет поддержки редактора спрайтов — оставляем импорт без изменений
		dataProvider.InitSpriteEditorDataProvider();

		// Существующие спрайты: сохраняются по имени (pivot/alignment/border и GUID),
		// при этом SpriteRect.spriteID переносится — ссылки на спрайты в проекте остаются валидными.
		Dictionary<string, SpriteRect> existingSprites = new Dictionary<string, SpriteRect>();
		foreach (SpriteRect sprite in dataProvider.GetSpriteRects())
		{
			existingSprites.Add(sprite.name, sprite);
		}

		List<SpriteRect> metaData = new List<SpriteRect> ();
		foreach (string row in dataFileContent) {
			if (string.IsNullOrEmpty (row) || row.StartsWith ("#") || row.StartsWith (":"))
				continue; // comment lines start with #, additional atlas properties with :

			string [] cols = row.Split (';');
			if (cols.Length < 7)
				return; // format error

			SpriteRect smd = new SpriteRect ();
			smd.name = cols [0].Replace ("/", "-");  // unity has problems with "/" in sprite names...
			float x = float.Parse (cols [1]);
			float y = float.Parse (cols [2]);
			float w = float.Parse (cols [3]);
			float h = float.Parse (cols [4]);
			float px = float.Parse (cols [5]);
			float py = float.Parse (cols [6]);

			smd.rect = new UnityEngine.Rect (x, y, w, h);

			if (existingSprites.ContainsKey(smd.name))
			{
				SpriteRect sprite = existingSprites[smd.name];
				smd.spriteID = sprite.spriteID;
				smd.pivot = sprite.pivot;
				smd.alignment = sprite.alignment;
				smd.border = sprite.border;
			}
			else
			{
				// новый спрайт — стабильный уникальный идентификатор
				smd.spriteID = GUID.Generate();
			}

			if (importPivotPoints || !existingSprites.ContainsKey(smd.name))
			{
				smd.pivot = new UnityEngine.Vector2 (px, py);

				if (px == 0 && py == 0)
					smd.alignment = UnityEngine.SpriteAlignment.BottomLeft;
				else if (px == 0.5 && py == 0)
					smd.alignment = UnityEngine.SpriteAlignment.BottomCenter;
				else if (px == 1 && py == 0)
					smd.alignment = UnityEngine.SpriteAlignment.BottomRight;
				else if (px == 0 && py == 0.5)
					smd.alignment = UnityEngine.SpriteAlignment.LeftCenter;
				else if (px == 0.5 && py == 0.5)
					smd.alignment = UnityEngine.SpriteAlignment.Center;
				else if (px == 1 && py == 0.5)
					smd.alignment = UnityEngine.SpriteAlignment.RightCenter;
				else if (px == 0 && py == 1)
					smd.alignment = UnityEngine.SpriteAlignment.TopLeft;
				else if (px == 0.5 && py == 1)
					smd.alignment = UnityEngine.SpriteAlignment.TopCenter;
				else if (px == 1 && py == 1)
					smd.alignment = UnityEngine.SpriteAlignment.TopRight;
				else
					smd.alignment = UnityEngine.SpriteAlignment.Custom;
			}
			metaData.Add (smd);
		}

		dataProvider.SetSpriteRects(metaData.ToArray());
		dataProvider.Apply();
	}
}
