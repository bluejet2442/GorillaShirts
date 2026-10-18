[README.md](https://github.com/user-attachments/files/33642958/README.md)
# GorillaShirts — standalone fork

GorillaShirts adds community-made shirts to Gorilla Tag. Use the Shirt Stand in supported maps to choose shirts, browse packs, change colours, and adjust your name tag.

This fork is based on [developer9998/GorillaShirts](https://github.com/developer9998/GorillaShirts). It uses the game's GorillaIK bones instead of GorillaLibrary and still requires BepInEx. It does not require Utilla. The plugin ID remains `dev.gorillashirts` and the internal version remains `2.4.7`.

See [CHANGES.md](CHANGES.md) for this revision and [STANDALONE-FIX.txt](STANDALONE-FIX.txt) for compatibility notes and testing limits. These changes were prepared with AI assistance.

## Install

1. Close Gorilla Tag.
2. Back up your current GorillaShirts DLL and packs outside `BepInEx/plugins`.
3. Replace the existing `GorillaShirts.dll` with the DLL built from this fork. Keep only one active copy under `BepInEx/plugins`.
4. Keep your packs beside the plugin. If supplied, place `GorillaShirts.pdb` beside the DLL for debugging.

GorillaLibrary can remain installed for other mods. This fork does not use it.

The [upstream releases](https://github.com/developer9998/GorillaShirts/releases) and MonkeModManager distribute upstream builds; they do not contain this revision. Upstream installation links:

- [MonkeModManager for Windows](https://github.com/arielthemonke/MonkeModManager/releases/latest/download/MonkeModManager.exe)
- [MonkeModManager for Linux](https://github.com/arielthemonke/MonkeModManager/releases/latest/download/MonkeModManager.Linux)

## Using the stand

- **Packs:** collections of shirts, managed through the Pack Browser.
- **Character identity:** choose Steady (Masculine) or Silly (Feminine) on the sidebar.
- **Tag offset:** move your name tag so clothing does not cover it.
- **Anchors:** shirts can reposition the name tag, badge, and chest/slingshot attachments.

To make shirts, use the upstream [GorillaShirtsUnityProject](https://github.com/developer9998/GorillaShirtsUnityProject).

## Multiplayer

GorillaShirts uses Photon PUN to share mod version information, worn shirts, colour data, fallbacks, and tag offsets. Other players need GorillaShirts to see your custom shirts. If they do not have a shirt installed, its assigned fallback from the Default pack can be shown instead. Shirt creators choose these fallbacks; players cannot change them in-game.

The pack browser and update check still use upstream URLs. This fork retains the upstream version-check behaviour, including the block on a newer major or minor release.

## Build

Use a .NET SDK that supports the project's C# syntax and an installed copy of Gorilla Tag with BepInEx:

```powershell
dotnet build GorillaShirts/GorillaShirts.csproj -c Release -p:DeployToGame=false
```

The default game location is set in `GorillaShirts/Directory.Build.props`. For another location, pass `-p:GamePath="D:\Games\Gorilla Tag"`.

The DLL is written to `GorillaShirts/bin/Release/netstandard2.1/`. Building does not install it unless you pass `-p:DeployToGame=true`. The game and BepInEx assembly references are read from your installation; they are not included in this source package.

## Settings and troubleshooting

Preferences are stored in `GorillaShirts.json` under Unity's persistent data folder. Successful replacement saves keep the previous file as `.bak`. Invalid JSON is copied to a uniquely named `.invalid-*` file before defaults can be saved. If the existing settings cannot be read or backed up, saving is disabled for that session.

Warnings and errors are included in release builds. Check the BepInEx log if the stand, packs, or settings fail to load. Include the log and a description of what happened when reporting a problem.

## Credits

*GorillaShirts* is a product contributed to by many during its continued development since 2022.

Supporters of Dev's Patreon and Ko-fi services are listed within the info section of the mod, in addition to everyone listed below.

### Lead Contributors

- **dev9998**: Creator (programmer and designer)

- **gizmogoat**: Tester

- **crescentmondo**: Artist (2D sprites and 3D models)
    
### Special Thanks

- **sushii64**: Testing

- **ch7ken**: Turtleneck idea and assistance

- **ojsauce**: Tag Offset and Photo Capture ideas and assistance

## Disclaimer

<h4 align="center">
This product is not affiliated with Gorilla Tag or Another Axiom LLC and is not endorsed or otherwise sponsored by Another Axiom LLC. Portions of the materials contained herein are property of Another Axiom LLC. © 2021 Another Axiom LLC.
</h4>
