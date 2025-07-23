# 🎵 Spotify Clone – Full Stack Music Streaming Platform

A full-featured music streaming web app inspired by Spotify. Includes user and admin dashboards, real-time audio playback, subscription payments, AI-powered music recommendations, cloud storage, and DevOps automation.

---

## 🚀 Tech Stack

| Area            | Technology                          |
|-----------------|--------------------------------------|
| Frontend        | React + Tailwind CSS                |
| Backend         | ASP.NET Core Web API (.NET 8)       |
| Database        | PostgreSQL                          |
| Caching         | Redis                               |
| AI Recommender  | Python (FastAPI + scikit-learn)     |
| Payments        | Stripe                              |
| Messaging       | Twilio (SMS)                        |
| File Storage    | AWS S3                              |
| DevOps          | Docker + GitHub Actions             |

---

## 📁 Project Structure

```
/
├── backend/ # ASP.NET Core API
│ ├── API/
│ ├── Domain/
│ ├── Infrastructure/
│ ├── Services/
│ └── Tests/
│
├── frontend/ # React + Tailwind
│ ├── src/
│ ├── public/
│ └── ...
│
├── ai-recommender/ # Python (FastAPI)
│ ├── recommender_api/
│ ├── model_training/
│ └── notebooks/
│
├── infra/ # Docker, GitHub Actions, Terraform (optional)
│ ├── docker/
│ ├── github-workflows/
│ └── ...
│
├── docs/ # Diagrams and specs
├── docker-compose.yml
├── README.md
└── .gitignore
```



---

## 🔑 Features

### 🎧 User Dashboard
- User registration & login (JWT)
- Stream music from AWS S3
- Create & manage playlists
- AI-powered personalized recommendations
- Subscription with Stripe
- Listening history

### 🛠️ Admin Dashboard
- Manage songs, users, and artists
- View platform statistics
- Manage payments and subscriptions

### 🔒 Security
- JWT authentication
- Role-based access control (User/Admin)
- Input validation on frontend/backend

### 🤖 AI Music Recommender
- Content-based recommendation engine (phase 1)
- FastAPI microservice (Python)
- Future: collaborative filtering with implicit feedback

### ⚙️ DevOps & CI/CD
- GitHub Actions for CI/CD
- Docker & docker-compose for local dev
- GitHub Secrets for API keys and credentials
- Optional: AWS deployment (ECS, EC2, or Elastic Beanstalk)

---

## 🛠️ Local Setup

### ✅ Prerequisites
- Docker & Docker Compose
- .NET 8 SDK
- Node.js & npm
- Python 3.10+

### ⚙️ Start with Docker

```bash
git clone https://github.com/your-username/spotify-clone.git
cd spotify-clone
docker-compose up --build
```
