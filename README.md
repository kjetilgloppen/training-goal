# Training Goals

A small personal web app to track **cumulative count goals with linear pace tracking**.

Set a goal like *"hike a mountain 100 times this year"*, log each occurrence (with an optional
duration and note), and the app shows your cumulative **actual vs. expected** progress — so you can
see at a glance whether you're **ahead or behind** pace, year-to-date, with a graph and stats.

## Stack

- **Backend:** ASP.NET Core 10 Web API + EF Core (Npgsql) → PostgreSQL
- **Frontend:** Vue 3 + Vite + TypeScript + Chart.js
- **Auth:** single passcode → cookie session (Data Protection keys persisted in the DB)
- **Deploy:** one combined Docker image (ASP.NET serves the built SPA + the API) on Render's free tier,
  with a free [Neon](https://neon.tech) Postgres database.

```
server/   ASP.NET Core Web API (also serves the Vue build from wwwroot)
client/   Vue 3 SPA (built into server/wwwroot)
Dockerfile, render.yaml   deployment
```

## Local development

Prerequisites: .NET 10 SDK, Node 20+, and a Postgres database. The quickest DB is Docker:

```bash
docker run -d --name tg-postgres -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=training_goal -p 5432:5432 postgres:16-alpine
```

Then run the two dev servers in separate terminals:

```bash
# Terminal 1 — API (http://localhost:5107), applies migrations on startup
cd server
dotnet run

# Terminal 2 — Vue dev server (http://localhost:5173), proxies /api to the API
cd client
npm install
npm run dev
```

Open http://localhost:5173 and log in with the dev passcode (`dev-passcode`, from
`server/appsettings.Development.json`).

Connection string and passcode for local dev live in `server/appsettings.Development.json`.
To use your own values without editing that file, override via environment variables:
`ConnectionStrings__Default` and `APP_PASSCODE`.

## Build the production image locally

```bash
docker build -t training-goal .
docker run -p 8080:8080 \
  -e "ConnectionStrings__Default=Host=host.docker.internal;Port=5432;Database=training_goal;Username=postgres;Password=postgres" \
  -e "APP_PASSCODE=change-me" \
  training-goal
# open http://localhost:8080
```

## Deploy (Render + Neon, free)

1. **Neon:** create a project at [neon.tech](https://neon.tech), copy the connection string.
   Append `;SSL Mode=Require;Trust Server Certificate=true` if not already present (Neon requires SSL).
2. **Render:** New → **Web Service** → connect this Git repo. Render detects the `Dockerfile`
   (or use the `render.yaml` blueprint). Choose the **Free** plan.
3. Set environment variables in the Render dashboard:
   - `ConnectionStrings__Default` = your Neon connection string
   - `APP_PASSCODE` = your chosen login passcode
4. Deploy. Migrations run automatically on startup. Open the Render URL and log in.

> Note: the free Render service sleeps after ~15 min idle, so the first request after a nap
> takes ~30–60s to wake. Fine for personal use.

## Data model

- **Goal** — name, target count, period start/end, optional unit label.
- **LogEntry** — date, optional duration (entered as hours/minutes/seconds, stored as seconds), optional note.

Pace math (frontend, `client/src/pace.ts`): `expected = target × elapsedFraction`, and
`delta = actual − expected` drives the ahead/behind badge.
