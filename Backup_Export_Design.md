# Backup / Excel Export – Design Document

## Overview

The backup feature allows authorized admin users to export project data to Excel (.xlsx) files by module. A single optional date range (From/To) applies to all selected modules. The user can select one or more modules and receive one Excel file per module (multiple separate downloads).

## Architecture

- **Frontend**: One page at `/admin/backup` with module checkboxes, optional From/To date, and an Export button. For each selected module, the frontend calls `GET api/BackupAdmin/Export?module=<key>&from=&to=` and triggers a file download sequentially.
- **Backend**: Single endpoint `GET api/BackupAdmin/Export` that accepts `module`, `from`, and `to`. A MediatR query is sent; the handler resolves the requested module to a dedicated exporter and returns the Excel file (streamed).

## Permission

- **CanExportData**: New permission in `Domain/Constants/PermissionNames.cs`. Only users with this permission can access the backup page and call the export API. The permission is seeded for the Admin role via `PermissionNames.All` in `RolePermissionsSeed`.

## Module List and Date Support

| Module Key         | Date filter support   | Notes                                      |
|--------------------|-----------------------|--------------------------------------------|
| Orders             | Yes (CreationDate)    | GetAllOrdersQuery with FromDate/ToDate     |
| OrderPackages      | No                    | All records                                |
| Vehicles           | No                    | VehicleTypes + VehicleBrands (two sheets)  |
| SystemUsers        | No                    | All system users                           |
| DeliveryMen        | No                    | Approved delivery men                      |
| MainCategories     | No                    | All main categories                        |
| WalletTransactions | Yes (CreatedDate)     | GetAllWalletTransactionsForExportQuery     |
| Complains          | Yes (CreationDate)    | GetAllComplainsQuery with FromDate/ToDate  |
| Suggestions        | Yes (CreationDate)    | GetAllSuggestionsQuery with FromDate/ToDate|
| Notifications      | Yes (CreationDate)    | GetNotificationsQuery with FromDate/ToDate |
| Regions            | No                    | All regions                                |
| Cities             | No                    | All cities                                 |
| Neighborhoods      | No                    | All neighborhoods                          |
| AssistantWorks     | No                    | All assistant works                        |

For modules that support date filter, if the user provides From and/or To on the backup page, the same range is sent for each export request and applied at the database level. Modules without a date field ignore the range and export all data.

## Backend Structure

- **Application**
  - `Application/Features/AdminSection/BackupFeature/`
    - `Dtos/ExportResult.cs` – holds Stream, FileName, ContentType.
    - `Constants/BackupModuleKeys.cs` – allowed module keys and which support date filter.
    - `Queries/ExportModuleToExcelQuery.cs` – ModuleKey, FromDate, ToDate, LanguageId.
    - `Queries/ExportModuleToExcelQueryHandler.cs` – Validates module key and delegates to the right exporter.
    - `Abstractions/IModuleExporter.cs` – Interface for per-module exporters.
    - `Exporters/*.cs` – One class per module (e.g. OrdersExcelExporter, VehiclesExcelExporter).
  - Each exporter uses existing or dedicated queries (with high Take or unbounded for export), builds an Excel workbook with ClosedXML, and returns an `ExportResult` with a `MemoryStream` and suggested filename.
- **Presentation**
  - `BackupAdminController` – `[RequirePermission(PermissionNames.CanExportData)]`, `GET Export` with query params, returns `File(stream, contentType, fileName)`.

## Frontend Structure

- **Route**: `/admin/backup` with `permissionGuard` and `data: { requiredPermission: 'CanExportData' }`.
- **Page**: Backup component with module checkboxes (grouped), optional From/To date, Select all / Clear all, and Export button. On Export, for each selected module it calls the export API and triggers download sequentially (with a short delay between calls to avoid blocking).
- **Service**: `BackupService` in `Core/services/backup.service.ts` – builds export URL (using AppConfigService base URL), calls API with `responseType: 'blob'` and `observe: 'response'` to read `Content-Disposition` for filename, and provides `triggerDownload(blob, fileName)`.
- **Sidebar**: Menu entry “Data Backup” (or translated equivalent) visible only when the user has `CanExportData`.

## Performance and Limits

- Export queries use a maximum row limit (e.g. 50,000) where applicable to avoid excessive memory and timeouts.
- Excel is built in memory (ClosedXML) and returned as a stream; for very large data, consider batching or background jobs in a future iteration.

## Adding a New Module

1. Add the module key to `BackupModuleKeys` in `BackupModuleKeys.cs` and to the `All` (and optionally `DateFilterable`) sets.
2. Create a new exporter class implementing `IModuleExporter` in `Exporters/`, using the appropriate query to fetch data and ClosedXML to build the sheet(s).
3. Register the exporter in `Application/DependencyInjection/ServicesDependencyInjection.cs` as `services.AddScoped<IModuleExporter, YourNewExporter>();`.
4. Add the module to the frontend list in `backup.component.ts` (`BACKUP_MODULES`) and add translation keys for its label and group in `en.json` and `ar.json`.

## Date Filter Fix (Orders)

- In `GetAllOrdersQuery`, the previous FromDate/ToDate logic incorrectly used `Order.Id`. It was replaced with filtering on `Order.CreationDate` (UTC, date-only range).

## Optional Future Improvements

- Background job for very large exports with notification when the file is ready.
- Configurable max rows per module and a clear error when exceeded.
- Audit log entry when an export is performed (who, which modules, date range).
