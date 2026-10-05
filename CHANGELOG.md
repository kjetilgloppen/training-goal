# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project aims to follow [Semantic Versioning](https://semver.org/).

<!--
Workflow:
- Add entries under [Unreleased] as you work, grouped by:
  Added / Changed / Fixed / Removed / Security.
- When you deploy, rename [Unreleased] to a new version + date, and start a fresh
  [Unreleased] section on top. Optionally tag the commit (e.g. `git tag v0.2.0`).
-->

## [Unreleased]

### Added

- History list highlights repeat entries on the same day: the first entry is untinted and
  each subsequent same-day entry gets a progressively deeper blue background.
- Google sign-in with an email allowlist (`ALLOWED_EMAILS`, `OWNER_EMAIL`) so others can test.
- `Users` table; goals are owned by a user. Existing goals are claimed by `OWNER_EMAIL` on its
  first sign-in.

### Changed

- All goal and log endpoints are scoped to the signed-in user; other users' ids return 404.
- Login cookie is `Secure` outside development, and forwarded headers are honoured behind
  Render's TLS proxy.
- `/api/auth/me` now also returns the user's name and email; the header shows the name.

### Removed

- Shared passcode login (`APP_PASSCODE`).

### Security

- Closed an access hole where any signed-in session could read or delete any goal or log by id.

## [0.1.0] - 2026-09-06

### Added

- Initial skeleton of the Training Goals tracker.
- ASP.NET Core 10 Web API + EF Core (Npgsql) backed by PostgreSQL.
- Vue 3 + Vite + TypeScript SPA, served by ASP.NET from `wwwroot` as a single combined deploy.
- Goals: create, list, delete — with target count, period start/end, and optional unit label.
- Log entries: add (date, optional **h/m/s** duration, optional note) and delete.
- Pace tracking: cumulative actual-vs-expected line chart (Chart.js), ahead/behind badge,
  run-rate projection, needed-per-week, and total/average duration stats.
- Passcode cookie authentication; Data Protection keys persisted in the database so the
  login survives container restarts.
- Multi-stage `Dockerfile` and `render.yaml` for free hosting on Render + Neon Postgres.
- README with local-dev instructions and Render/Neon deploy steps (incl. Neon URI → Npgsql
  connection-string conversion).

[Unreleased]: https://github.com/kjetilgloppen/training-goal/compare/v0.1.0...HEAD
[0.1.0]: https://github.com/kjetilgloppen/training-goal/releases/tag/v0.1.0
