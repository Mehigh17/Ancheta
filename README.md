# Ancheta

Ancheta is a web API that exposes a simple set of endpoints allowing to manage, cast and remove polls. It is indended to be used publicly by any user and implements no concept of authentication. It can be a good simple backend for frontend developers who want to develop a small website as starter project.

# Configuration

The configuration is straightforward and quite easy. This API is reliant on Google Recaptcha, so you must create yourself a google account and obtain the recaptcha key. You must also setup a PostgreSQL database.

Environment variables used:

```
RECAPTCHA__KEY=... your key here ...
DATABASE__CONNECTIONSTRING=... postgresql connection string ...
```

# Docker

## Docker Image

A docker image is available on dockerhub, you can use the latest image simply by pulling `mehigh17/ancheta:latest`.

## Sample

Here is a *docker-compose.yml* sample allowing you to quickly set it up.

```yaml
version: "3"
services:
  db:
    image: postgres
    restart: always
    environment:
      POSTGRES_USER: ${PG_USER}
      POSTGRES_PASSWORD: ${PG_PASSWORD}
      POSTGRES_DB: ${PG_DB}
  ancheta:
    image: mehigh17/ancheta
    depends_on: 
      - db
    ports:
      - "8080:5050"
    environment:
      RECAPTCHA__KEY: ${RECAPTCHA}
      DATABASE__CONNECTIONSTRING: ${CONNECTION_STRING}
```

Run docker-compose on it and the API will be available on `http://localhost:8080`.

You can use the following *.env* file if you don't have your own setup.

```
PG_USER=dbuser
PG_PASSWORD=dbpass
PG_DB=anchetadb

RECAPTCHA=6LeIxAcTAAAAAGG-vFI1TnRWxMZNFuojJ4WifJWe # !!! THIS IS A TEST ONLY SECRET KEY PROVIDED PUBLICLY BY GOOGLE, RECAPTCHA WILL ALWAYS BE VALID !!!
CONNECTION_STRING=User ID=dbuser;Password=dbpass;Host=db;Port=5432;Database=anchetadb;
```

# Swagger

Swagger is a perfect tool for you to explore the API. Ancheta **must** run in development mode in order for Swagger to be available.

To set Ancheta in development mode use `ASPNETCORE_ENVIRONMENT=Development`.

# Contributing

If you are willing to contribute, you should first fork this repository. Therefore, you should create your own branch (git checkout -b \<name\>) from **master**, since it is the development branch. Check the tags if you are willing to checkout a specific version. Give your branch a meaningful name, such as an issue number if it is supposed to fix a specific issue. When you create a pull request, always pull request against the **master** branch and write a summary of at least of couple of lines describing your addition.

# License

MIT