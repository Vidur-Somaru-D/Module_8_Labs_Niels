# _shared — Reference Domain Models

This folder contains canonical domain model definitions for use across all training modules.

**This is not a compiled project.** There is no `.csproj` here. These files exist so module authors have a single authoritative source for the Derivco training domain: `Player`, `Transaction`, `Bet`, `Money`, `PlayerEvent`, and the repository interfaces.

## How to use

When authoring a module, copy the files you need into your module's project and adjust the namespace from `Derivco.Shared.*` to `Derivco.Module0N.*`. Do not add a reference to this folder as a project dependency.

## Contents

| Path | Purpose |
|------|---------|
| `Models/Player.cs` | Core player entity with balance, region, and navigation collections |
| `Models/Transaction.cs` | Debit/Credit transaction record |
| `Models/Bet.cs` | Bet record with stake, payout, and computed `IsWin` |
| `Models/Money.cs` | Immutable value type for monetary amounts — always use `decimal`, never `float`/`double` |
| `Models/PlayerEvent.cs` | Lightweight value-type event record (small struct, value fields only) |
| `Interfaces/IPlayerRepository.cs` | Async repository contract for `Player` |
| `Interfaces/ITransactionRepository.cs` | Async repository contract for `Transaction` |

## Design notes

- `Money` is a `readonly struct` — immutable, safe to copy, equatable by value.
- `PlayerEvent` is intentionally kept small (only value-type fields) because it is designed to be stored in high-throughput, allocation-sensitive paths. Module 1 uses a deliberately bloated version of this struct as a teaching example.
- Repository interfaces return `Task<T>` throughout — all data access in Derivco is async.
