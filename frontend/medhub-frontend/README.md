# MedHub Frontend

React + Vite frontend for MedHub LMS.

## Local Development

```powershell
npm install
npm run dev
```

The Vite dev server uses `/api` proxying to the backend at `http://localhost:5010`.

## Run With Docker

The frontend is integrated into the existing backend compose file.

From the repository root:

```powershell
docker compose -f backend/MedHub/docker-compose.yml up -d --build
```

Or from `backend/MedHub`:

```powershell
docker compose up -d --build
```

URLs:

- Frontend: `http://localhost:8080`
- Backend Swagger: `http://localhost:5010/swagger`
- MinIO console: `http://localhost:9001`
- Seq: `http://localhost:8081`

Rebuild only the frontend:

```powershell
docker compose -f backend/MedHub/docker-compose.yml build medhub.frontend
docker compose -f backend/MedHub/docker-compose.yml up -d medhub.frontend
```

View logs:

```powershell
docker compose -f backend/MedHub/docker-compose.yml logs -f medhub.frontend
docker compose -f backend/MedHub/docker-compose.yml logs -f medhub.api
```

The Docker image is a production build served by nginx. API calls use
`VITE_API_BASE=http://localhost:5010`, so the browser talks to the backend
through the host port.
