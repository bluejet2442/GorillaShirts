# Changes in this revision

Based on the supplied standalone source. This is a review revision, not a new upstream release. No commit history or in-game test results are claimed.

| File | Change and reason |
| --- | --- |
| `README.md` | Replaced promotional and repetitive wording with fork-specific usage, installation, build, and troubleshooting steps. Corrected the dependency instructions to require BepInEx. Kept upstream links, contributor credits, and the legal disclaimer. Clearly separated upstream downloads from this fork. |
| `STANDALONE-FIX.txt` | Added concise compatibility notes based on the supplied earlier note, which was outside the source folder. Retained its source revision and attribution, and labelled its old validation results as historical. Removed task-specific backup filenames and installation claims that do not apply to this delivery. |
| `GorillaShirts/Behaviours/DataManager.cs` | Replaced concurrent background saves with ordered synchronous saves through a temporary file and replacement backup. Preserves malformed JSON before defaults are saved, reports read/write failures, handles out-of-range integer preferences without throwing, and clears its singleton on destruction. Small settings saves now perform disk I/O on the calling thread; slow storage may cause a brief pause. The existing JSON format is retained. |
| `GorillaShirts/Tools/Logging.cs` | Keeps warnings and errors in release builds so failures can be diagnosed. Routine informational logging remains debug-only. |
| `GorillaShirts/Behaviours/HumanoidContainer.cs` | Shortened the IK comment. Guarded missing fallback bones and name tags, calls base cleanup, clears the local reference, unsubscribes before cleanup, and skips destroyed objects during visibility updates. Missing bones leave the affected clothing anchor unset. |
| `GorillaShirts/Behaviours/Appearance/ShirtHumanoid.cs` | Retains the actual content-loader subscription for cleanup, handles already-destroyed shirt objects, and removes generated anchors along with clothing objects. |
| `GorillaShirts/Behaviours/Networking/NetworkSolution.cs` | Clears pending room state on leave, ignores delayed work from older room sessions, prevents sends outside rooms, and bounds player-rig waits to about ten seconds. A rig that appears after the timeout will need a later property update to receive the outfit. |
| `GorillaShirts/Behaviours/Networking/NetworkSolution_RaiseEvent.cs` | Checks event shape and player/rig availability before accessing them, avoids empty-recipient sends, and removes the Photon event subscription on destruction. Keeps the existing event ID and payload format. |
| `GorillaShirts/Behaviours/Networking/NetworkedPlayer.cs` | Applies removals and additions in the same outfit update, clears old fallback/colour entries when shorter arrays arrive, ignores empty shirt IDs, guards an out-of-range default-shirt selection after packs change, and guards manager access during destruction. |
| `GorillaShirts/Tools/AssetLoader.cs` | Completes all waiting preview requests on failure and allows retries instead of leaving a cached task pending forever. Rejects invalid image data and sets a 15-second request timeout. |
| `GorillaShirts/Behaviours/ShirtManager.cs` | Sets timeouts for the pack-list and version requests, handles malformed/null pack lists, and reuses an existing local humanoid component. |

`LICENSE`, contributor names, assets, packs, project IDs, and version values were preserved. `.vs`, `bin`, and `obj` caches are omitted from the source archive. The supplied originals and installed game files were not edited.

This revision was prepared with AI assistance. See VALIDATION.md for the checks actually performed and their limits.
