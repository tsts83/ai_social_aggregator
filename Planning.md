## Tech stack plan:

🚀 Backend (C#/.NET 8)
✅ ASP.NET Core Web API – Main backend
✅ Entity Framework Core – Database ORM
✅ xUnit – Unit testing
✅ MediatR – For clean architecture
✅ Refit – For consuming external APIs
✅ Serilog – Logging
✅ Hangfire (Free) – Background job scheduling

🧠 AI (Open-Source)
✅ Ollama – Run AI models locally instead of OpenAI
✅ GPT4All – Free LLM running on your own server

🌍 Content Aggregation (Free APIs)
✅ Reddit API – Fetch trending posts
✅ NewsAPI (free tier) – Get latest news headlines
✅ Hacker News API – Fetch tech news
✅ Twitter API (alternative) – Use Nitter (a free, no-auth Twitter alternative)

🛢️ Database
✅ PostgreSQL (Free on Railway/Render) – OR
✅ MongoDB Atlas (Free tier)

🎨 Frontend (Minimal)
✅ React + MUI – Simple dashboard
✅ Vite – Fast frontend build

☁️ Deployment (Free)
✅ Backend: Railway / Render / Azure Free Tier
✅ Frontend: Vercel

## Work packages and tasks

🔹 Phase 1: Setup ASP.NET Core API & Database
🔹 (1.1) Create the API project

Initialize an ASP.NET Core Web API project

Set up Swagger for API documentation

🔹 (1.2) Configure Database (PostgreSQL/MongoDB)

Choose PostgreSQL (preferred) or MongoDB Atlas

Set up Entity Framework Core (or MongoDB Driver)

Create a User model and authentication system

🔹 (1.3) Implement Authentication (JWT)

User registration & login API

JWT-based authentication

Protect endpoints with authorization

🔹 (1.4) Set up Logging & Error Handling

Use Serilog for structured logging

Implement global exception handling

🔹 Phase 2: Integrate Free Content APIs
🔹 (2.1) Reddit API Integration

Fetch trending posts from subreddits

🔹 (2.2) NewsAPI Integration (free tier)

Get top news articles

🔹 (2.3) Hacker News API Integration

Fetch latest stories

🔹 (2.4) Twitter Alternative: Nitter API

Scrape tweets from Twitter without authentication

🔹 (2.5) Centralized Content Aggregator

Build a ContentService class

Normalize all API responses into a standard format

🔹 Phase 3: AI-Powered Content Summarization
🔹 (3.1) Set Up AI (Ollama/GPT4All - Open Source LLM)

Install and configure Ollama (runs GPT-3.5-like models locally)

🔹 (3.2) Implement AI Summarization API

Process content with LLM-generated summaries

Save summaries to the database

🔹 (3.3) User Feedback on AI Summaries

Allow users to edit summaries before posting

🔹 Phase 4: Create React/MUI Frontend
🔹 (4.1) Set Up React App with MUI

Create a React project with Vite

Install and configure MUI

🔹 (4.2) User Dashboard

Show fetched content in a table or list

Allow filtering by source (Reddit, News, Twitter, etc.)

🔹 (4.3) AI Summary Review & Approval UI

Display original content & AI-generated summary

Provide an edit option before scheduling

🔹 (4.4) Scheduling & History Page

Users can schedule posts

View a history of posted content

🔹 Phase 5: Implement Auto-Posting with Hangfire
🔹 (5.1) Set Up Hangfire (Job Scheduling)

Install Hangfire in ASP.NET

Configure persistent background jobs

🔹 (5.2) Implement Social Media Posting API

Store scheduled posts in the database

Auto-post to Social Media Demo App

🔹 (5.3) Handle Failures & Retries

If an API call fails, retry with exponential backoff

🔹 Phase 6: Testing with xUnit
🔹 (6.1) Unit Tests for Content Fetching

Test Reddit, NewsAPI, Hacker News integration

🔹 (6.2) AI Summarization Tests

Validate AI-generated summaries

🔹 (6.3) Authentication & Authorization Tests

Ensure JWT authentication works

🔹 (6.4) Hangfire Job Execution Tests

Test auto-posting jobs

🔹 Phase 7: Deployment
🔹 (7.1) Deploy Backend

Use Railway/Render/Azure Free Tier

🔹 (7.2) Deploy Frontend

Use Vercel

🔹 (7.3) CI/CD Pipeline (GitHub Actions)

Run tests on every push

Deploy on successful build
