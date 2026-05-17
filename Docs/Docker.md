# Docker — Complete Guide

## What is Docker?

Docker is a tool that packages your application along with everything it needs to run — code, runtime, libraries, config — into a single unit called a **container**.

Without Docker, when you share your project with someone:
- They need to install .NET 10, Node.js, PostgreSQL, correct versions of everything
- It works on your machine but breaks on theirs ("works on my machine" problem)

With Docker:
- You package everything into an image
- They just run the image — no installations needed
- Runs identically on every machine

---

## Core Concepts

**Image**
A blueprint/snapshot of your application. Built once, run anywhere. Like a class in OOP — it's the definition, not the running thing.

**Container**
A running instance of an image. Like an object created from a class. You can run multiple containers from the same image.

**Dockerfile**
A text file with step-by-step instructions to build an image. Think of it as a script that sets up your app from scratch.

**Docker Compose**
A tool to define and run multiple containers together. Instead of starting backend, frontend, and database separately, one command starts all of them.

**Docker Hub**
A cloud registry to store and share images publicly or privately. Like GitHub but for Docker images.

**Volume**
Persistent storage for containers. Containers are stateless by default — when a container stops, data is lost. Volumes keep data alive (used for the PostgreSQL database).

**Port Mapping**
Containers run in isolation. Port mapping exposes a container's internal port to your machine:
```
"4200:80" means → your machine's port 4200 maps to container's port 80
```

---

## Why Docker is Needed

| Problem | Without Docker | With Docker |
|---|---|---|
| Sharing project | Send code + setup instructions | Send image, just run it |
| Different OS | "Works on my machine" | Same behavior everywhere |
| Multiple services | Install each manually | One `docker-compose up` |
| Deployment | Configure server manually | Push image, pull and run |
| Version conflicts | Node 18 vs 20 issues | Each container has its own |
| Onboarding new dev | Hours of setup | Minutes |

---

## Project Structure

This project has 3 Dockerfiles:

```
CaseStudy/
  docker-compose.yml              ← runs all 3 services together
  MultiClientPlatform.Api/
    Dockerfile                    ← builds the .NET 10 backend image
  marketplace-ui/
    Dockerfile                    ← builds the Angular frontend image
    nginx.conf                    ← nginx config for serving frontend
```

---

## Multi-Stage Builds (How Our Dockerfiles Work)

Both Dockerfiles use **multi-stage builds** — two FROM instructions in one file.

**Why?**
The tools needed to BUILD an app (SDK, compilers, node_modules) are much larger than what's needed to RUN it. Multi-stage builds discard the build tools and only keep the final output.

**Backend example:**
```dockerfile
# Stage 1 — Build (uses full .NET SDK ~700MB)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
RUN dotnet publish -c Release -o /app/publish

# Stage 2 — Runtime (uses only ASP.NET runtime ~200MB)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
COPY --from=build /app/publish .   ← only copies the compiled output
```

Final image is ~200MB instead of ~700MB.

**Frontend example:**
```dockerfile
# Stage 1 — Build (Node.js + Angular CLI compiles the app)
FROM node:22-alpine AS build
RUN npm ci && npm run build

# Stage 2 — Runtime (tiny nginx image just serves static files)
FROM nginx:alpine AS runtime
COPY --from=build /app/dist/marketplace-ui/browser /usr/share/nginx/html
```

---

## Why nginx for the Frontend?

After `ng build`, Angular produces **static files** — just HTML, CSS, and JavaScript. There is no Node.js server running in production. You need a web server to serve those files to the browser.

**nginx** is a lightweight, high-performance web server perfect for this job.

**What our nginx.conf does:**

```nginx
# Serves the Angular app
location / {
    root /usr/share/nginx/html;
    try_files $uri $uri/ /index.html;   ← SPA routing fix
}

# Proxies API calls to the backend container
location /api/ {
    proxy_pass http://backend:8080/api/;
}
```

**`try_files $uri $uri/ /index.html`** — This is critical for Angular routing. When a user refreshes on `/catalog`, nginx looks for a file called `catalog` — it doesn't exist. Without this line, nginx returns 404. With it, nginx falls back to `index.html` and Angular's router takes over.

**The proxy** — In Docker, the frontend container can't call `localhost:8080` for the API because `localhost` inside a container refers to the container itself, not the host machine. nginx proxies `/api/` calls to `http://backend:8080` where `backend` is the service name in `docker-compose.yml` — Docker's internal DNS resolves it automatically.

### Alternatives to nginx

| Option | Use Case |
|---|---|
| **nginx** | Standard choice, lightweight, fast, handles proxy well |
| **Apache HTTP Server** | Older alternative, more config overhead |
| **Caddy** | Simpler config, automatic HTTPS, good for small projects |
| **Node.js `serve` package** | Quick and simple but not production-grade |
| **AWS S3 + CloudFront** | Skip the server entirely — host static files on S3, serve via CDN |
| **Azure Static Web Apps** | Same idea on Azure — no server needed |

For this project nginx is the right choice — it's minimal, fast, and handles the API proxy cleanly.

---

## docker-compose.yml Explained

```yaml
services:

  db:                              # PostgreSQL database
    image: postgres:16-alpine      # pulls official postgres image
    environment:                   # sets DB credentials
    volumes:
      - pgdata:/var/lib/postgresql/data   # persists DB data

  backend:                         # .NET API
    build:
      context: ./MultiClientPlatform.Api  # where to find the Dockerfile
    environment:
      ConnectionStrings__DefaultConnection: "Host=db;..."  # db = service name above
    depends_on:
      - db                         # starts db before backend

  frontend:                        # Angular app
    build:
      context: ./marketplace-ui
    ports:
      - "4200:80"                  # your browser hits 4200, nginx listens on 80
    depends_on:
      - backend

volumes:
  pgdata:                          # named volume — survives container restarts
```

`depends_on` controls startup order — db starts first, then backend, then frontend.

`Host=db` in the connection string works because Docker Compose creates an internal network where each service is reachable by its service name.

---

## Useful Docker Commands

### Images
```bash
docker images                          # list all local images
docker build -t myapp .                # build image from Dockerfile in current folder
docker rmi myapp                       # delete an image
docker pull postgres:16                # download image from Docker Hub
```

### Containers
```bash
docker ps                              # list running containers
docker ps -a                           # list all containers (including stopped)
docker run -p 8080:8080 myapp          # run a container from an image
docker stop <container_id>             # stop a running container
docker rm <container_id>               # delete a stopped container
docker logs <container_id>             # view container logs
docker exec -it <container_id> bash    # open terminal inside a container
```

### Docker Compose
```bash
docker-compose up                      # start all services
docker-compose up --build              # rebuild images then start
docker-compose up -d                   # start in background (detached)
docker-compose down                    # stop and remove containers
docker-compose down -v                 # also delete volumes (wipes DB data)
docker-compose logs                    # view logs from all services
docker-compose logs backend            # logs from specific service
docker-compose ps                      # status of all services
```

### Docker Hub
```bash
docker login                           # log in to Docker Hub
docker tag myapp yourusername/myapp    # tag image for Docker Hub
docker push yourusername/myapp         # push image to Docker Hub
docker pull yourusername/myapp         # pull image from Docker Hub
```

### Cleanup
```bash
docker system prune                    # remove all unused containers, images, networks
docker volume prune                    # remove unused volumes
```

---

## How to Share This Project

### Option 1 — Docker Hub (Recommended)

**You do (once):**
```bash
# Build images
docker build -t yourusername/marketplace-backend ./MultiClientPlatform.Api
docker build -t yourusername/marketplace-frontend ./marketplace-ui

# Push to Docker Hub
docker push yourusername/marketplace-backend
docker push yourusername/marketplace-frontend
```

Update `docker-compose.yml` to use images instead of build:
```yaml
backend:
  image: yourusername/marketplace-backend

frontend:
  image: yourusername/marketplace-frontend
```

**Your friend does:**
```bash
# Only needs docker-compose.yml — no source code
docker-compose up
```
Docker automatically pulls the images from Docker Hub.

---

### Option 2 — Save as .tar File (Offline sharing)

**You do:**
```bash
docker save yourusername/marketplace-backend -o backend.tar
docker save yourusername/marketplace-frontend -o frontend.tar
```
Send the `.tar` files + `docker-compose.yml`.

**Your friend does:**
```bash
docker load -o backend.tar
docker load -o frontend.tar
docker-compose up
```

---

### Option 3 — Share Source Code + Dockerfiles

Send the entire project folder. Your friend builds and runs:
```bash
docker-compose up --build
```

This requires them to have Docker installed but not .NET or Node.js.

---

## Where to Run Docker — Deployment Options

### Local / Development
- **Docker Desktop** (Windows/Mac) — GUI app that installs Docker Engine, includes a dashboard to manage containers, images, volumes visually. Best for development.
- **Docker Engine** (Linux) — command line only, no GUI overhead. Used on servers.

### Cloud Options

**AWS EC2**
Spin up a Linux virtual machine on AWS, install Docker, push your images to Docker Hub, pull and run them on the EC2 instance. Full control but you manage the server yourself (updates, security, scaling).

```
EC2 instance → install Docker → docker pull → docker-compose up
```

**AWS ECS (Elastic Container Service)**
AWS-managed container service. You provide the image, AWS handles running, scaling, and restarting containers. No server management. More expensive but production-grade.

**AWS EKS (Elastic Kubernetes Service)**
Kubernetes on AWS. For large-scale apps with many containers. Overkill for this project.

**Azure Container Apps / Azure App Service**
Microsoft's equivalent of ECS. Good choice if already on Azure ecosystem.

**Railway / Render / Fly.io**
Simpler platforms — push your Dockerfile, they handle everything. Good for small projects and demos. Free tiers available.

**Docker Hub + Play with Docker**
Free browser-based Docker environment for testing. Not for production.

### Comparison

| Option | Effort | Cost | Best For |
|---|---|---|---|
| Docker Desktop | None | Free | Local development |
| AWS EC2 + Docker | Medium | Low | Full control, small budget |
| AWS ECS | Low | Medium | Production, managed |
| Railway/Render | Very Low | Free/Low | Demos, side projects |
| Kubernetes (EKS) | High | High | Large scale apps |

---

## Running This Project

**Prerequisites:** Docker Desktop installed

```bash
# From the CaseStudy root folder
docker-compose up --build
```

| Service | URL |
|---|---|
| Frontend | http://localhost:4200 |
| Backend API | http://localhost:8080 |
| Swagger | http://localhost:8080/swagger |
| PostgreSQL | localhost:5432 |

**Stop everything:**
```bash
docker-compose down
```

**Stop and wipe database:**
```bash
docker-compose down -v
```

---

## Common Issues

**Port already in use**
Something else is running on 4200 or 8080. Either stop it or change the port mapping in `docker-compose.yml`.

**Database connection refused**
Backend starts before PostgreSQL is ready. Add a retry/health check or just run `docker-compose up` again — db will be ready on second start.

**Angular build fails**
Check the output path in `angular.json` matches the `COPY` path in the frontend Dockerfile (`dist/marketplace-ui/browser`).

**Changes not reflected**
Docker uses cached layers. Force a full rebuild:
```bash
docker-compose up --build --force-recreate
```
