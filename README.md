# 📰 Social News Aggregator

[.NET Build & Test][https://github.com/tsts83/social-media-auto-poster/actions]

A .NET-based microservice that fetches news from external APIs, processes them using an AI model via Hugging Face, and delivers concise summaries to a social media-style frontend platform ("Sandor's Book").

---

## 📌 Features

- ✅ Fetches real-time news from [NewsAPI][https://newsdata.io/]
- 🤖 Uses Hugging Face transformers to summarize articles into short social posts
- 🗃️ Stores configuration secrets securely with AWS SSM
- 🌐 Communicates with external frontend hosted on Vercel
- 🐳 Dockerized for easy deployment (currently hosted on AWS Elastic Beanstalk)
- 🛠️ Uses MySQL via Aiven as backend database

---

## 🚀 Architecture Overview
```
                +--------------+         
                |  NewsData.io |         
                +------+-------+         
                       |                    
                       v                    
        +---------------------------+       
        |  SocialNewsAggregator     |       
        |   (.NET microservice)     |       
        +-----------+---------------+       
                    |                                    
      +-------------+--------------+                      
      |                            |                      
      v                            v                      
+------------+             +---------------+              
| HuggingFace|             | MySQL (Aiven) |              
|   (AI)     |             +---------------+              
+------------+                                    
                    |                                    
                    v                                    
          +---------------------+                         
          |  Sandor's Book      |                         
          |  (React, Node @ Vercel)   |                         
          +---------------------+        
```
## 🧰 Tech Stack

- **Backend:** C#, .NET
- **Database:** MySQL (Aiven)
- **Containerization:** Docker
- **Hosting:** AWS Elastic Beanstalk
- **Secrets Management:** AWS Parameter Store (SSM)
- **External APIs:** NewsAPI, Hugging Face
- **Frontend Consumer:** [Sandor's Book](https://mini-social-media-demo.vercel.app/)


---

## 📦 Database Structure

The service uses a **MySQL** database hosted on **Aiven** to store configuration and processing data. Here's an overview of the core tables:

### `AppConfig`

Stores configuration settings for how the news fetcher and AI processor operate.

---

### `NewsArticles`

Tracks all news articles fetched and processed by the system.

---

### `Users` *(Not in active use yet)*

Designed to support user-specific configurations in the future.

---

## ⚙️ Setup & Run Locally

# Clone the repo
```bash

git clone https://github.com/tsts83/social-media-auto-poster.git
cd SocialAggregatorAPI
```

# Set environment variables (preferably use .env or Parameter Store in production)
```bash
export NEWSDATA_API_KEY=your_key_at_newsApi
export HUGGINGFACE_API_KEY=your_key_at_huggigface
export DEFAULT_CONNECTION=your_mysql_connection_string
export JWT_KEY=some_secret_jwt_key
export ADMIN_API_KEY=your_secret_api_key
export MINIAPP_USER_PASSWORD=password_for_sandorsbook
```

# Build and run
```bash
dotnet build
dotnet run
```

# Docker
The project is docker containerized. To run the project locally, run
```bash
./rundev.sh
```

# To run the project with the latest uploaded image to Docker Hub, run 
```bash
./run.sh
```
---

# 🚀 Deploying to AWS Elastic Beanstalk (Docker)

Ensure AWS CLI and EB CLI are installed and configured (eb init, eb create):

```bash
# Build Docker image and zip for Elastic Beanstalk
eb init -p docker social-news-aggregator
eb create social-news-env
eb deploy
```

Ensure your .elasticbeanstalk/config.yml and Dockerrun.aws.json (if needed) are set up correctly.
Secrets (API keys, DB connections) should be stored in AWS SSM Parameter Store and referenced in .ebextensions config using ARNs.

