# Docker Steps — MultiClientPlatform

## Prerequisites
- Docker Desktop installed and running
- Docker Hub account created at https://hub.docker.com

---

## Part 1 — Build and Push Images to Docker Hub

### Step 1 — Open terminal and navigate to project root
```bash
cd CaseStudy
```

### Step 2 — Log in to Docker Hub
```bash
docker login
```
Enter your Docker Hub username and password when prompted.

### Step 3 — Build the backend image
```bash
docker build -t yourusername/marketplace-backend ./MultiClientPlatform.Api
```

### Step 4 — Build the frontend image
```bash
docker build -t yourusername/marketplace-frontend ./marketplace-ui
```

### Step 5 — Verify images are created
```bash
docker images
```
You should see `marketplace-backend` and `marketplace-frontend` listed.

### Step 6 — Push backend image to Docker Hub
```bash
docker push yourusername/marketplace-backend
```

### Step 7 — Push frontend image to Docker Hub
```bash
docker push yourusername/marketplace-frontend
```

### Step 8 — Update docker-compose.yml to use pushed images
Replace the `build` sections with `image` in `docker-compose.yml`:

```yaml
backend:
  image: yourusername/marketplace-backend

frontend:
  image: yourusername/marketplace-frontend
```

---

## Part 2 — Run Using Docker Compose (Your Machine)

### Step 1 — Navigate to project root
```bash
cd CaseStudy
```

### Step 2 — Build and start all services
```bash
docker-compose up --build
```

### Step 3 — Or start in background
```bash
docker-compose up --build -d
```

### Step 4 — Verify all containers are running
```bash
docker-compose ps
```
You should see `db`, `backend`, and `frontend` all with status `running`.

### Step 5 — Open the application
| Service | URL |
|---|---|
| Frontend | http://localhost:4200 |
| Backend API | http://localhost:8080 |
| Swagger | http://localhost:8080/swagger |

### Step 6 — View logs if something is wrong
```bash
# All services
docker-compose logs

# Specific service
docker-compose logs backend
docker-compose logs frontend
docker-compose logs db
```

### Step 7 — Stop all services
```bash
docker-compose down
```

### Step 8 — Stop and delete database data
```bash
docker-compose down -v
```

---

## Part 3 — Someone Wants to Pull and Run Your Application

Your friend only needs two things:
- Docker Desktop installed
- The `docker-compose.yml` file (send them this one file only)

### Step 1 — Get the docker-compose.yml file
Your friend receives `docker-compose.yml` from you (via email, GitHub, USB — anything).

Make sure `docker-compose.yml` uses images not build:
```yaml
backend:
  image: yourusername/marketplace-backend

frontend:
  image: yourusername/marketplace-frontend
```

### Step 2 — Open terminal where docker-compose.yml is saved
```bash
cd path/to/folder/containing/docker-compose.yml
```

### Step 3 — Pull and run everything
```bash
docker-compose up
```
Docker automatically pulls `marketplace-backend`, `marketplace-frontend`, and `postgres` images from Docker Hub and starts all three containers.

### Step 4 — Open the application
| Service | URL |
|---|---|
| Frontend | http://localhost:4200 |
| Backend API | http://localhost:8080 |
| Swagger | http://localhost:8080/swagger |

### Step 5 — Stop
```bash
docker-compose down
```

---

## Part 4 — Useful Commands for Managing This Project

### Rebuild after code changes
```bash
docker-compose up --build --force-recreate
```

### Remove old images to free space
```bash
docker image prune
```

### Check container logs live
```bash
docker-compose logs -f backend
```

### Open terminal inside a running container
```bash
docker exec -it <container_name> bash

# Example
docker exec -it casestudy-backend-1 bash
```

### Check running containers
```bash
docker ps
```

### Stop a single container
```bash
docker stop casestudy-backend-1
```

### Remove everything (containers, images, volumes)
```bash
docker system prune -a --volumes
```
> Warning — this deletes all Docker data including database.
