# syntax=docker/dockerfile:1

# El proyecto tiene dos stacks, y por eso va en tres etapas.
#
# La idea es que la imagen final no lleve nada que no sirva para ejecutar:
# ni Node, ni el SDK de .NET, ni el código fuente. Eso la deja chica y
# arranca más rápido, que es lo que importa cuando el plan gratis apaga
# el servicio a los 15 minutos de inactividad y el primer visitante tiene
# que esperar el arranque.

# ── 1. El frontend ──────────────────────────────────────────────────────
# Va aparte porque el runtime de .NET no incluye Node, y el build del
# frontend lo necesita.
FROM node:24-alpine AS frontend
WORKDIR /app/frontend

# Primero solo los manifiestos: mientras no cambien, esta capa se
# reutiliza y no se reinstalan las dependencias en cada build.
COPY frontend/package.json frontend/package-lock.json ./
RUN npm ci

COPY frontend/ ./
RUN npm run build

# ── 2. La API ───────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS api
WORKDIR /src

COPY backend/ backend/

# El frontend ya compilado entra directo a wwwroot. SkipFrontendBuild le
# dice al .csproj que no intente compilarlo otra vez: lo haría con npm, y
# en esta imagen no hay Node.
COPY --from=frontend /app/frontend/dist/ backend/wwwroot/
RUN dotnet publish backend/Conectando.Api.csproj \
      --configuration Release \
      --output /app/publish \
      -p:SkipFrontendBuild=true

# ── 3. El runtime ───────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=api /app/publish .

# Render espera el puerto 10000. ASP.NET Core no lee la variable PORT que
# Render define: usa la suya, y si no encuentra ninguna se queda escuchando
# en 80, que es un puerto que el contenedor no puede tomar.
ENV ASPNETCORE_HTTP_PORTS=10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "Conectando.Api.dll"]
