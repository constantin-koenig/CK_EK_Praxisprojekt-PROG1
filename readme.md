# ArchivSoftware

WPF-Archivsoftware (C# / .NET 8) mit SQL Server + Entity Framework Core.
Funktionen: Ordnerhierarchie, Dokumentimport (PDF/DOCX), Auto-Import (Watcher),
Volltextsuche (PlainText), Trefferliste, Detailansicht.

## Voraussetzungen

- .NET 8 SDK
- SQL Server Express (SQLEXPRESS) + SSMS
- (Optional) Import-Ordner für Auto-Import

## Setup

1. Repository klonen
2. `ArchivSoftware.Ui/appsettings.json` prüfen (ConnectionStrings, ImportWatcher)
3. Migration/DB erstellen:
   ```bash
   dotnet ef database update --project ArchivSoftware.Infrastructure --startup-project ArchivSoftware.Ui
   ```
