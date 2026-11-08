# Agent Knowledge Base: Not-a-Noble

This document provides technical insights, architectural patterns, and lessons learned from other mods (like JGUM and SellMeSlave) to ensure future developments for **Not-a-Noble** run smoothly.

## Critical Bannerlord Modding Lessons

### 1. The "Dependency Conflict" Crash (Bannerlord 1.4.x+)
In recent Bannerlord updates (e.g., v1.4.x), the game launcher's XML parser became much stricter.
- **DO NOT** use `Optional="true"` inside the `<DependedModules>` block of your `SubModule.xml` for mods like Harmony, MCM, or UIEx. The launcher ignores this attribute and treats them as mandatory hard dependencies, causing a "Dependency Conflict" crash on launch for users who don't have them installed.
- **SOLUTION:** Put optional dependencies **ONLY** in the `<DependedModuleMetadatas>` block with `optional="true"`. Do not list them in `<DependedModules>` at all.

### 2. The `IEncyclopediaLog` API Change (Bannerlord 1.4.7+)
TaleWorlds recently updated the API for `IEncyclopediaLog`.
- **Pre-1.4.7 Signature:** `bool IsVisibleInEncyclopediaPageOf<T>(T obj) where T : MBObjectBase`
- **1.4.7+ Signature:** `bool IsVisibleInEncyclopediaPageOf(MBObjectBase obj)`
- **SOLUTION (The Universal Overload Hack):** To support both old and new versions of Bannerlord in a single `.dll` without needing a complex multi-version release structure, simply implement **both** method signatures in your class. The .NET CLR will implicitly match the correct interface method at load time depending on the player's game version.

### 3. MCM Bridge Architecture
If `Not-a-Noble` will feature a Mod Configuration Menu (MCM), you should decouple it to prevent crashes for players without MCM.
- Build a core assembly (`NaN.dll`) and a separate bridge assembly (`NaN.MCMBridge.dll`).
- Dynamically load the bridge via reflection in `OnSubModuleLoad` only if the `MCMv5` assembly is present in the current AppDomain.
- Maintain a JSON-based settings fallback for users without MCM.

### 4. Build Environment
- Mod `.csproj` files typically reference `TaleWorlds.*.dll` assemblies directly from the active Steam installation (`$(GameFolder)\bin\Win64_Shipping_Client`). 
- When Steam auto-updates Bannerlord, your compilation environment immediately targets the new game version. Always be mindful of API breakages during automated updates.

### 5. Developer Release & Build Lifecycle (Mandatory)
Every developer and agent must strictly follow this 5-step lifecycle:
1. **Code (Kodla)**: Make necessary changes to code or XML.
2. **Debug Build (Debug buildi al)**: Build in `Debug` configuration (`dotnet build -c Debug`). Increments the debug version segment.
3. **Test (Test et)**: Verify gameplay and sanity check in-game or via tests.
4. **Hotfix / Update Build (Hotfix/Update buildi al)**:
   - Run `dotnet build -c Hotfix` for bugfixes, or `dotnet build -c Update` for feature releases.
   - Automatically increments the version in `SubModule.xml` (and `AssemblyInfo.cs`) via `UpdateVersion.ps1`.
5. **Commit & Push with Changelog (Commit ve Push)**:
   - Write action-oriented commit messages (`Added: ...`, `fix: ...`) because commit messages automatically generate the Nexus Mods changelog.
   - Always ask for explicit user permission before `git push origin master` (or `main`).
