# Playback recovery tests

Run from the repository root with the .NET 10 SDK:

```sh
dotnet run --project tools/playback-tests/PlaybackTests.csproj
```

This dependency-free host runner compiles the production `PlaybackRecovery`
class directly. It supplies playback positions and monotonic timestamps without
sleeping or requiring a TV. It checks background retries, sustained stalls,
recovery, pause/seek/end-of-stream suppression, and player restarts.

These tests do not change the TV application's target framework and do not
exercise native playback or NUI rendering. Confirm those on a real TV.
