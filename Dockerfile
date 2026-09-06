# ---- Stage 1: build the Vue client ----
FROM node:22-alpine AS client
WORKDIR /client
COPY client/package.json client/package-lock.json ./
RUN npm ci
COPY client/ ./
# Build into a local dist folder (overrides vite.config's outDir for the container).
RUN npm run build -- --outDir dist --emptyOutDir

# ---- Stage 2: build & publish the ASP.NET server ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS server
WORKDIR /src
COPY server/*.csproj ./
RUN dotnet restore
COPY server/ ./
# Drop in the freshly built SPA so ASP.NET serves it from wwwroot.
COPY --from=client /client/dist ./wwwroot
RUN dotnet publish -c Release -o /app --no-restore

# ---- Stage 3: runtime image ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=server /app ./
# Render injects $PORT; default to 8080 for local runs.
ENTRYPOINT ["sh", "-c", "dotnet TrainingGoal.Api.dll --urls http://0.0.0.0:${PORT:-8080}"]
