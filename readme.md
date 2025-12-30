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
4. App starten: dotnet run --project ArchivSoftware.Ui

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
