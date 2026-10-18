# Validation

Checks run on October 18, 2026:

- Release build with .NET SDK 10.0.401 and the installed Gorilla Tag/BepInEx assemblies: **0 errors, 76 warnings**. Deployment was disabled.
- Patch application check passed against the supplied source. Both ZIP archives passed integrity checks. The license, bundled packs, preview images, and embedded asset bundle match the supplied files byte for byte.
- Eight settings checks passed using the edited `DataManager.cs` and the installed Newtonsoft.Json assembly in a .NET 10 console harness. The harness substitutes Unity lifecycle/logging types; it does not run Unity or test vector conversion.
  - Latest of 100 saves survives reload.
  - A typed shirt list survives reload.
  - Replacing settings keeps a backup.
  - An out-of-range integer uses its default.
  - A locked-file failure leaves the previous settings intact.
  - Saving works after the lock is released.
  - Damaged JSON is preserved before defaults are saved.
  - Destruction clears the settings singleton.

The first harness attempt used the system temporary directory and encountered access-denied errors. The checks were rerun successfully in the writable workspace. Logs and the harness source are included in the delivery's `validation` folder.

No in-game, multiplayer, Unity editor, or offline-startup tests were run. The network, rig, and preview-download changes have been reviewed and compiled, but their runtime behaviour still needs the manual checks in STANDALONE-FIX.txt. No claim is made that every stability issue has been resolved.

The build command was:

```powershell
dotnet build GorillaShirts/GorillaShirts.csproj -c Release -p:DeployToGame=false -p:NuGetAudit=false --no-restore
```

Package restore was completed before this command. No new packages were added. The existing Newtonsoft.Json type-metadata settings were retained for preference-file compatibility.
