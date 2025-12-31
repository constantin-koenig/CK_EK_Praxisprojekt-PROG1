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
2. `ArchivSoftware/src/ArchivSoftware.Ui/appsettings.json` prüfen (ConnectionStrings, ImportWatcher)
3. Migration/DB erstellen:
   ```bash
   cd ArchivSoftware
   dotnet ef database update --project src/ArchivSoftware.Infrastructure --startup-project src/ArchivSoftware.Ui
   ```

## Starten

**Option 1:** Startskript verwenden (aus dem `ArchivSoftware`-Ordner):

```bash
.\run.bat       # Windows Batch
.\run.ps1       # PowerShell
```

**Option 2:** Direkt aus dem UI-Verzeichnis:

```bash
cd ArchivSoftware/src/ArchivSoftware.Ui
dotnet run
```

## Mandanten / DB-Switch

In appsettings.json existieren mehrere ConnectionStrings (z.B. TenantA, TenantB).
Im UI kann der Mandant gewechselt werden. Beim Wechsel werden Daten neu geladen.

## Nutzung

Ordner im TreeView verwalten: Anlegen, Umbenennen, Löschen, Verschieben
Dokumente importieren: nur PDF/DOCX (manuell oder Auto-Import)
Suche: Volltext über Titel + Inhalt (PlainText), Snippet + Highlight
Dokumente: Öffnen/Exportieren/Verschieben/Löschen

## Projektstruktur

- ArchivSoftware.Ui (WPF)
- ArchivSoftware.Application (UseCases/Services)
- ArchivSoftware.Domain (Entities/Interfaces)
- ArchivSoftware.Infrastructure (EF Core, ImportWatcher, Text-Extraktion)
- Tests (Unit Tests)
