using AssetRipper.Assets;
using AssetRipper.Assets.Metadata;
using AssetRipper.Assets.Traversal;
using AssetRipper.SourceGenerated.Classes.ClassID_213;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Classes.ClassID_687078895;
using System.Diagnostics;

namespace AssetRipper.Processing.Textures;

public sealed class SpriteInformationObject : AssetGroup, INamed
{
	public SpriteInformationObject(AssetInfo assetInfo, ITexture2D texture) : base(assetInfo)
	{
		Texture = texture;
	}

	public ITexture2D Texture { get; }
	public IReadOnlyDictionary<ISprite, ISpriteAtlas?> Sprites => dictionary;
	private readonly Dictionary<ISprite, ISpriteAtlas?> dictionary = new();

	Utf8String INamed.Name
	{
		get => Texture.Name;
		set { }
	}

	public override IEnumerable<IUnityObjectBase> Assets
	{
		get
		{
			yield return Texture;
			foreach ((ISprite sprite, ISpriteAtlas? atlas) in dictionary)
			{
				yield return sprite;
				if (atlas is not null)
				{
					yield return atlas;
				}
			}
		}
	}

	public override void WalkStandard(AssetWalker walker)
	{
		if (walker.EnterAsset(this))
		{
			this.WalkPPtrField(walker, Texture);
			walker.DivideAsset(this);
			this.WalkDictionaryPPtrField(walker, Sprites);
			walker.ExitAsset(this);
		}
	}

	public override IEnumerable<(string, PPtr)> FetchDependencies()
	{
		yield return (nameof(Texture), AssetToPPtr(Texture));
		foreach ((ISprite sprite, ISpriteAtlas? atlas) in dictionary)
		{
			yield return (nameof(Sprites) + "[].Key", AssetToPPtr(sprite));
			if (atlas is not null)
			{
				yield return (nameof(Sprites) + "[].Value", AssetToPPtr(atlas));
			}
		}
	}

	internal void AddToDictionary(ISprite sprite, ISpriteAtlas? atlas)
	{
		if (dictionary.TryGetValue(sprite, out ISpriteAtlas? mappedAtlas))
		{
			if (mappedAtlas is null)
			{
				dictionary[sprite] = atlas;
			}
			else if (atlas is not null && atlas != mappedAtlas)
			{
				string spriteCollection = sprite.Collection?.Name ?? "Unknown";
				string spriteBundle = sprite.Collection?.Bundle?.Name ?? "Unknown";
				string atlasCollection = atlas.Collection?.Name ?? "Unknown";
				string atlasBundle = atlas.Collection?.Bundle?.Name ?? "Unknown";
				string mappedAtlasCollection = mappedAtlas.Collection?.Name ?? "Unknown";
				string mappedAtlasBundle = mappedAtlas.Collection?.Bundle?.Name ?? "Unknown";
				Console.WriteLine($"WARNING: {nameof(atlas)} is not the same as {nameof(mappedAtlas)}. Sprite: {sprite.Name} (collection: {spriteCollection}, bundle: {spriteBundle}), Atlas: {atlas.Name} (collection: {atlasCollection}, bundle: {atlasBundle}), MappedAtlas: {mappedAtlas.Name} (collection: {mappedAtlasCollection}, bundle: {mappedAtlasBundle})");
				// Keep the existing mappedAtlas instead of overwriting
			}
		}
		else
		{
			dictionary.Add(sprite, atlas);
		}
	}

	public override void SetMainAsset()
	{
		Debug.Assert(Texture.MainAsset is null);
		base.SetMainAsset();
	}
}
