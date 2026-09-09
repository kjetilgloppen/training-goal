# Cross-origin / CORS notes

Reference for if we ever split the frontend and backend into **separate deployments**.
Today they ship as one combined origin, which avoids everything below — read the
"Why the default avoids all this" section first.

## The current model (single origin)

- **Dev:** two servers — Vite (`localhost:5173`) serves the SPA with hot-reload; Kestrel
  (`localhost:5107`) serves the API. The Vue code calls **relative** URLs (`/api/...`), and
  Vite's dev proxy forwards `/api/*` to Kestrel (`client/vite.config.ts` → `server.proxy`).
  From the browser's view the page and API share one origin (`5173`), so no CORS applies.
- **Prod:** one server — Kestrel serves the built SPA from `wwwroot` **and** `/api/*` on a
  single origin. No proxy, no CORS, relative URLs resolve to the same host.

> Note: both dev ports are directly reachable in a browser (you can open `:5107/swagger`).
> "Same origin" is about the origin of the page's JavaScript making a request, enforced
> per-request by the browser — it's independent of whether a port is reachable.

## What "origin" means

An origin is the tuple `(scheme, host, port)`. Any difference = different origin:

```
http://localhost:5173   ← page
http://localhost:5107   ← API   (different port ⇒ cross-origin)
```

The Same-Origin Policy lets a page *send* a cross-origin request but **withholds the response**
from JS unless the server opts in via CORS headers. Requests with a non-safelisted header
(e.g. `Content-Type: application/json`, which `client/src/api.ts` sets on every call) first
trigger a **preflight** `OPTIONS` request; if the server returns no CORS headers, the real
request is blocked.

## Checklist: splitting into two deployments

Scenario: frontend on e.g. `https://goals.example.com`, API on `https://api.example.com`.

### 1. Enable CORS in `server/Program.cs`

Cannot use `AllowAnyOrigin` together with credentials — must name the exact origin.

```csharp
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .WithOrigins("https://goals.example.com")   // exact origin(s), not "*"
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()));                        // required for the auth cookie

// ... after building the app, BEFORE UseAuthentication:
app.UseCors();
```

### 2. Loosen the auth cookie's SameSite (`server/Program.cs`)

A `SameSite=Lax` cookie is **not sent on cross-site requests**, so login silently fails across
origins. Cross-site cookies must be `None` + `Secure` (HTTPS only):

```csharp
options.Cookie.SameSite   = SameSiteMode.None;            // was Lax
options.Cookie.SecurePolicy = CookieSecurePolicy.Always;  // None requires Secure
```

### 3. Point the frontend at an absolute API base URL (`client/src/api.ts`)

Replace relative `/api` with a base URL from an env var; keep `credentials: 'include'`.

```ts
const BASE = import.meta.env.VITE_API_BASE ?? ''   // '' keeps same-origin/dev-proxy behavior
// e.g. fetch(`${BASE}/api/goals`, { credentials: 'include', ... })
```

Set `VITE_API_BASE=https://api.example.com` in the frontend's build environment.

### 4. Mind CSRF

`SameSite=None` cookies are exposed to CSRF. Add anti-forgery tokens, **or** switch from cookie
auth to a bearer token sent in the `Authorization` header (not sent automatically by the browser,
so not CSRF-prone).

## Why the default avoids all this

The single combined deploy means one origin: **no CORS config, `SameSite=Lax` stays safe, no
cross-site-cookie CSRF surface, and the frontend keeps using relative URLs.** Only split the
deployment if there's a concrete reason (e.g. wanting the SPA on a CDN with no cold starts) — and
then work through the checklist above.
