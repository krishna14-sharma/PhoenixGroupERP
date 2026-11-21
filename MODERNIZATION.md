# Phoenix Group ERP – UI modernisation & performance pass

Still VB.NET WinForms, .NET Framework 4.5, same database, same screens and menus.
Open `PhoenixGroupERP.sln` and build as before. Screenshots: `Modernization-screenshots/`.

## 1. New look

| Area | What changed |
|---|---|
| **All 245 forms** | Inherit `UI\ThemedForm` → `UI\ThemeManager` restyles every screen at runtime: Segoe UI fonts (same sizes, layouts do not move), flat buttons (Save/OK/Show… = blue primary, Delete = red outline), soft input focus highlight, modern grids (flat headers, zebra rows, double-buffered), modern menus. Red/green/blue label meanings are kept. |
| **Login** (`frmlogin`) | Redesigned: brand panel + sign-in card, inline validation messages instead of pop-ups, drag-to-move, close button, drop shadow, server/database shown at the bottom. |
| **Main window** (`frmMDI`) | Stretched background picture replaced by a **dashboard** (`UI\Dashboard.vb`): greeting, company, FY/role/server-date chips, clock, *Frequently used* tiles (learn from each user's clicks), and one card per menu module. Built from the real menu, so role rights from login apply automatically. |
| **Ctrl+K / F3 search** | `UI\CommandPalette.vb`: type part of any menu name (e.g. `led`, `trial add`) → Enter. Also from the search box in the header / menu bar, or just start typing on the dashboard. |
| **Change company / session** | Tools > Change Company / Session (had no code before), your name at top-right > Change company / session, or click the FY chip on the dashboard. Lists the companies assigned to the user (Cmp_assignment) and the years in SessionMaster; the app re-opens in the chosen company + year without asking for the password again (same login rules, menus and permissions). |
| **Status bar / banners** | Clean status bar (server date · company · session · user · version). Previous-year warning is an amber banner instead of 30pt red italic text. |

**Switches** (`app.config` → `<appSettings>`):
`<add key="UiTheme" value="Classic"/>` turns the whole theme off; `<add key="UiModernFonts" value="false"/>` keeps the original fonts.
Combo-box colour: `<add key="UiComboBack" value="#CFDDED"/>` (default steel-blue tint; use a darker value such as `#B9CDE5` for more contrast).
Per form/control: set `Tag = "NoTheme"` to exclude it.

## 2. Bugs fixed

* **SQL injection on login**: the password check is now parameterised (`' or '1'='1` no longer logs in). User-name look-ups escape quotes (`GMod.Q`).
* Debug `MsgBox` that popped the "number of days" value on every login – removed.
* Leaving the user-name box empty (e.g. clicking Exit) no longer shows "User name doesn't exist".
* `cmbcmpname_Leave` crashed when the user had no sessions.
* Closing the main window closed/disposed the shared connection and then re-opened it at exit; now closed once (`GMod.ShutdownConnection`).
* Server-detection screen: connection timeout 3000 s → 30 s (a wrong server no longer freezes the screen for 50 minutes).
* **Closing the app emptied the Windows Temp folder** (all files, then tried to delete the folder). That broke Visual Studio builds ("Unable to open module file ...AssemblyAttributes.vb") and other programs. Now only Crystal Reports' leftover `*.rpt` copies older than 1 hour are removed. The project also keeps that build file in `obj\` (`TargetFrameworkMonikerAssemblyAttributesPath`).
* `ApplicationEvents.vb` (was empty): unhandled errors are logged to `%LOCALAPPDATA%\PhoenixGroupERP\logs\` and shown in a friendly message; the application no longer closes and loses unsaved work.

## 3. Performance

* **18 empty busy-wait loops removed** (`For K = 0 To 10000000 : Next`) in General Ledger, Unauthorised Ledger, Advance Ledger, Supplementary Head List and Payment & Cheque Print – these burned CPU on every ledger printed/exported and on every payment save.
* **42 list/combo fills** wrapped in `BeginUpdate/EndUpdate` (no repaint per item) – e.g. the ledger account list with thousands of heads.
* **`select getdate()`** (≈80 call sites) is answered from a cached server-clock offset (re-read every 10 min) instead of a network round-trip – transparent inside `GMod.DataSetRet`.
* **General Ledger opens faster**: the account list was fetched and re-bound 4-6 times on open (role query, an all-heads list that was immediately thrown away, and the group change-handler firing 3 times). Now each step runs once; the screen ends in the same state.
* **General Account Head, Party Account Head, Expense entry open faster**: account-head grids are filled with one `Rows.AddRange` (`UI\GridFill.vb`) instead of 6-24 cell updates per row (3,000 rows: ~6 s -> ~0.05 s in testing; also makes search-as-you-type in the name box fast); the next-code lookup and the expense account list no longer re-run for every combo bound while opening; the expense form loads only the 4 columns it shows and copies the TDS party list instead of running the same query twice.
* **19 more grid fills in 15 screens** converted to build rows off-screen and add them in one call (only where the grid is provably empty before the loop and nothing reacts to individual cell changes).
* **Report screens**: the Crystal Reports libraries are pre-loaded in the background after login (`UI\Warmup.vb`), so the first report no longer waits for them.
* **Screen-open timer** (`UI\FormTimings.vb`): every screen logs how long it took to open and how many queries it ran to `%LOCALAPPDATA%\PhoenixGroupERP\logs\screen-open-times-yyyyMM.csv`. Your name (top-right) > *Screen open times...* shows the slowest screens. Off switch: `<add key="UiLogScreenTimes" value="false"/>`.
* **Voucher entry forms** (all forms under the Voucher's menu checked): 85 `select *` account-list queries in 24 forms now fetch only the columns each form reads/shows (verified per table: no column-by-position reads, no grids bound to them, no other form reading them). Payment, Receipt, Journal, BT Voucher, Sale Other and Purchase BM no longer load their account list twice while opening (the Area list's change event re-ran the load).
* **Bank voucher entry** (Bank > Payment And Others Entries): the heavy party UNION query that ran on open was always replaced a moment later by the Area list's load, so it no longer runs on open (same list shown); TDS party list and the All Vendor list fetch only the columns shown.
* Heartbeat timer: 1 query instead of 2, server time set on the server.
* `DataSetRet` (earlier pass): one forward-only read, tables refilled in place, 10-minute timeout for large reports.
* Grids/lists double-buffered by the theme (smoother scrolling).

## 4. For new code

* Parameterised queries: `Data\Db.vb` → `Db.GetTable(sql, Db.P("@x", v))`, `Db.Scalar`, `Db.Execute`.
* Server time without a round-trip: `GMod.ServerNow()`.

## 5. Verification done

* Whole project (663 files, including the new shared-connection handling used by the login) compiled with the VB compiler against the .NET Framework 4.5 reference assemblies (Crystal Reports stubbed): **0 errors**. The original project compiled the same way gives the same result, i.e. no regressions.
* Login, dashboard (admin and restricted operator in a previous year) and Ctrl+K search rendered and screenshot from the real designer code.
* **Not tested against a live SQL Server.** Please run a smoke test (login, a voucher save, a ledger print, cheque print) before rolling out.

## 6. Recommended next steps (not changed – business behaviour)

* `app.config` contains plaintext `sa` passwords, including one for a public IP (117.240.18.180). Rotate them and use a low-privilege SQL login.
* `Config_ERP.txt` "encryption" is a fixed character shift – it is readable by anyone; user passwords in `Usertab4` are stored in plain text.
* `frmMDI` drops the shared `Daybook` table when any user closes the app – can break another user's running report.
* 143 loops in 62 files still run one query per row (biggest: `frmBankReconcilation`, `frmMISsalepurexp_rep`, ledger print loops). Rewriting them as set-based SQL is the next big speed win but needs testing against real data.
