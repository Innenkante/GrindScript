using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.IO;
using LevelLoading;

namespace SoG.Modding.Extensions
{
    /// <summary>
    /// Extension methods for Level-related stuff that expose some vanilla methods and add new ones.
    /// </summary>
    public static class LevelExtension
    {
		internal static void CheckForConsistency(this LevelBlueprint bp, bool withSpawns = false)
		{
			// aiSpawnColliderLayer and aiLayerDefaultHeight must either be null,
			// or be at least as long as av2SpawnPoint

			// av2SpawnPoint.Length must be larger than entry point index,
			// but we can't check that here.

			if (bp == null) return;

			if (bp.lxStaticLevelObjects == null) bp.lxStaticLevelObjects = new List<LevelBlueprint.LevelObjectBlueprint>();
			if (bp.lxDynamicLevelObjects == null) bp.lxDynamicLevelObjects = new List<LevelBlueprint.LevelObjectBlueprint>();
			if (bp.lxInvisibleWalls == null) bp.lxInvisibleWalls = new List<Collider>();
			if (bp.lxBackgroundSprites == null) bp.lxBackgroundSprites = new List<LevelBlueprint.BackgroundSpriteBP>();
			if (bp.lxZoningFields == null) bp.lxZoningFields = new List<Level.ZoningField>();
			if (bp.lxPhasingFields == null) bp.lxPhasingFields = new List<Level.PhasingField>();
			if (bp.lxDescriptionSquares == null) bp.lxDescriptionSquares = new List<DescriptionSquare>();
			if (bp.lrecBattleBoxes == null) bp.lrecBattleBoxes = new List<Rectangle>();
			if (bp.lrecBattleBoxes == null) bp.lrecBattleBoxes = new List<Rectangle>();
			if (bp.dsv2GrindeaSpriteFiles == null) bp.dsv2GrindeaSpriteFiles = new Dictionary<string, Vector2>();
			if (bp.dsv2GrindeaWaypointFiles == null) bp.dsv2GrindeaWaypointFiles = new Dictionary<string, Vector2>();
			if (bp.lenPuzzles == null) bp.lenPuzzles = new List<IPuzzle.PuzzleID>();

			if (withSpawns)
			{
				if (bp.av2SpawnPoint == null || bp.av2SpawnPoint.Length == 0)
					bp.av2SpawnPoint = new Vector2[] { new Vector2(0, 0) };

				int spawnpoints = bp.av2SpawnPoint.Length;

				if (bp.aiSpawnColliderLayer != null && bp.aiSpawnColliderLayer.Length < spawnpoints)
				{
					var old = bp.aiSpawnColliderLayer;
					bp.aiSpawnColliderLayer = new int[spawnpoints];
					old.CopyTo(bp.aiSpawnColliderLayer, 0);
				}

				if (bp.aiLayerDefaultHeight != null && bp.aiLayerDefaultHeight.Length < spawnpoints)
				{
					var old = bp.aiLayerDefaultHeight;
					bp.aiLayerDefaultHeight = new int[spawnpoints];
					old.CopyTo(bp.aiLayerDefaultHeight, 0);
				}
			}

			if (bp.lxLevelPartitions == null)
				bp.lxLevelPartitions = new List<Level.LevelPartition> { new Level.LevelPartition(bp.recLevelBounds, false, Level.LevelPartition.ReverbSetting.Normal) };
		}

		/// <summary>
		/// Adds a collider that acts as an invisible wall, and applies an offset to its position.
		/// </summary>
		public static void AddCollider(this LevelBlueprint blueprint, Collider col, int bitLayers, bool isFlat, Vector2 offset, bool isOutline = true)
        {
            if (blueprint.lxInvisibleWalls != null && offset != null)
            {
				col.xLocalTransform.v2Pos += offset;
				col.ibitLayers = Utility.CreateIntMask(bitLayers);
				col.bIsFlatCollider = isFlat;
				col.bIsLarge = !isFlat;
				if (isOutline)
				{
					col.ibitLayers = Utility.AddIntFlag(col.ibitLayers, 30);
				}
				col.ibitLayers = Utility.AddIntFlag(col.ibitLayers, 29);
				col.AddSpecialLayer(Collider.SpecialLayer.StaticOutlineCollider);
				blueprint.lxInvisibleWalls.Add(col);
			}
        }

		/// <summary>
		/// Adds colliders from a file at the path given.
		/// The path is relative to the SoG executable.
		/// </summary>
		public static void AddCollidersFromFile(this LevelBlueprint blueprint, string relativePath, Vector2 offset, int overrideLayer = 0)
		{
			relativePath = Directory.GetCurrentDirectory() + relativePath;

			if (!File.Exists(relativePath))
				return;

			using (BinaryReader br = new BinaryReader(new FileStream(relativePath, FileMode.Open, FileAccess.Read)))
			{
				int iLoadedIteration = br.ReadInt32();
				int iCount = br.ReadInt32();
				for (int i = 0; i < iCount; i++)
				{
					Collider col = Loader.LoadCollider(br, blueprint.lxInvisibleWalls, offset, iLoadedIteration);
					if (overrideLayer > 0)
						col.ibitLayers = overrideLayer;
				}
			}
		}

		/// <summary>
		/// Adds a bush to the level.
		/// </summary>
		public static void AddBush(this LevelBlueprint blueprint, Vector2 position, int layer, bool orangeBush = false)
        {
			LevelBlueprint.StaticObject whackToUse = LevelBlueprint.StaticObject._Forest_BushWhacked;
			LevelBlueprint.StaticObject bushToUse = LevelBlueprint.StaticObject._Dynamic_Bush;

			if (orangeBush)
			{
				whackToUse = LevelBlueprint.StaticObject._Forest_BushWhackedOrange;
				bushToUse = LevelBlueprint.StaticObject._Dynamic_BushOrange;
			}

			blueprint.lxStaticLevelObjects.Add(new LevelBlueprint.LevelObjectBlueprint(whackToUse, position, true, 0f, layer));
			blueprint.lxDynamicLevelObjects.Add(new LevelBlueprint.LevelObjectBlueprint(bushToUse, position, true, 0f, layer));
		}

		/// <summary>
		/// Adds multiple bushes to the level, in a grid pattern.
		/// </summary>
		public static void AddBushGrid(this LevelBlueprint blueprint, Vector2 topLeftBush, int spacing, int collumns, int rows, int layer, bool orangeBushes = false)
        {
			for (int i = 0; i < collumns; i++)
            {
				for (int j = 0; j < rows; j++)
                {
					Vector2 position = topLeftBush + new Vector2(spacing * i, spacing * j);

					blueprint.AddBush(position, layer, orangeBushes);
				}
            }
        }

		/// <summary>
		/// Adds a level switch.
		/// This causes a level switch if everyone is within the respective bounds.
		/// </summary>
		public static void AddLevelSwitch(this LevelBlueprint blueprint, Rectangle leaderBounds, Rectangle teamBounds, Level.ZoneEnum targetZone, int spawnpoint)
        {
			blueprint.lxZoningFields.Add(new Level.ZoningField(leaderBounds, teamBounds, targetZone, spawnpoint));
        }

		/// <summary>
		/// Add a transport within this level.
		/// This is normally used for indoors / outdoors transitions.
		/// </summary>
		public static void AddTransport(this LevelBlueprint blueprint, Rectangle bounds, Vector2 targetPosition, int layer)
		{
			blueprint.lxPhasingFields.Add(new Level.PhasingField(bounds, targetPosition, layer));
		}

		/// <summary>
		/// Adds a new spawnpoint in this level, and returns its index.
		/// </summary>
		public static int AddSpawnpoint(this LevelBlueprint blueprint, Vector2 spawnpoint, int layer = 0, int virtualHeight = 0)
        {
			int newSize = 1;

			if (blueprint.av2SpawnPoint != null)
				newSize = blueprint.av2SpawnPoint.Length + 1;

			Vector2[] oldSP = blueprint.av2SpawnPoint;
			blueprint.av2SpawnPoint = new Vector2[newSize];
			oldSP?.CopyTo(blueprint.av2SpawnPoint, 0);

			int[] oldSPL = blueprint.aiSpawnColliderLayer;
			blueprint.aiSpawnColliderLayer = new int[newSize];
			oldSPL?.CopyTo(blueprint.aiSpawnColliderLayer, 0);

			int[] oldSPH = blueprint.aiLayerDefaultHeight;
			blueprint.aiLayerDefaultHeight = new int[newSize];
			oldSPH?.CopyTo(blueprint.aiLayerDefaultHeight, 0);


			blueprint.av2SpawnPoint[newSize - 1] = spawnpoint;
			blueprint.aiSpawnColliderLayer[newSize - 1] = layer;
			blueprint.aiLayerDefaultHeight[newSize - 1] = virtualHeight;

			return newSize - 1;
		}

		public static void AddStaticObjects(this LevelBlueprint blueprint, params LevelBlueprint.LevelObjectBlueprint[] objects)
        {
			if (blueprint.lxStaticLevelObjects == null)
				blueprint.lxStaticLevelObjects = new List<LevelBlueprint.LevelObjectBlueprint>();

			foreach (var obj in objects)
				blueprint.lxStaticLevelObjects.Add(obj);
		}

		public static void AddDynamicObjects(this LevelBlueprint blueprint, params LevelBlueprint.LevelObjectBlueprint[] objects)
		{
			if (blueprint.lxDynamicLevelObjects == null)
				blueprint.lxDynamicLevelObjects = new List<LevelBlueprint.LevelObjectBlueprint>();

			foreach (var obj in objects)
				blueprint.lxDynamicLevelObjects.Add(obj);
		}

		public static void AddBackgroundSprites(this LevelBlueprint blueprint, params LevelBlueprint.BackgroundSpriteBP[] objects)
		{
			if (blueprint.lxBackgroundSprites == null)
				blueprint.lxBackgroundSprites = new List<LevelBlueprint.BackgroundSpriteBP>();

			foreach (var obj in objects)
				blueprint.lxBackgroundSprites.Add(obj);
		}

		public static void SetLevelBounds(this LevelBlueprint blueprint, int x, int y, int width, int height)
		{
			blueprint.recLevelBounds = new Rectangle(x, y, width, height);
		}

		public static void AddLevelPartition(this LevelBlueprint blueprint, int x, int y, int width, int height)
		{
			blueprint.recLevelBounds = new Rectangle(x, y, width, height);
		}
	}
}
