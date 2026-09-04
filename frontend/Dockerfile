# Build stage — compiles the Vite app. VITE_API_BASE_URL is baked into the static bundle at build
# time (Vite env vars are compile-time, not runtime), so it must be the URL the *browser* can reach,
# not a Docker-internal service name like http://ops.api:8080.
FROM node:20-alpine AS build
WORKDIR /app

COPY package*.json ./
RUN npm ci

COPY . .

ARG VITE_API_BASE_URL=http://localhost:8080/api
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL

RUN npm run build

# Serve stage — static files behind nginx, with SPA fallback so client-side routes
# (e.g. /orders/123) survive a hard refresh instead of 404ing.
FROM nginx:alpine AS final
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
