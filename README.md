# centauri-tac
Tactical Card Game

## Game Client

### Config

The Unity client uses a `config.json` file in the centauri-tac folder. Example:
```json
{
    "baseUrl": "http://localhost:10123/",
    "realm": "dev",
    "players": [
        {
            "username": "player1@gmail.com",
            "password": "pw"
        },
        {
            "username": "player2@gmail.com",
            "password": "pw"
        }
    ]
}
```

## Backend and Tooling
[![Build Status](https://travis-ci.org/dshook/centauri-tac.svg?branch=master)](https://travis-ci.org/dshook/centauri-tac)


### Requirements

* NodeJS 6.x

### Setup

Install dependencies:

```
$ yarn
```

Set up a docker volume for windows

https://forums.docker.com/t/trying-to-get-postgres-to-work-on-persistent-windows-mount-two-issues/12456/4?u=friism
```
docker volume create --name postgres-volume -d local
```

### Running

Start a server instance and run all migrations:

```
$ yarn start
```

Run the server with `nodemon` to automatically restart on changes:

```
$ yarn run dev-server
```

To create a new database migration:

```
$ yarn run create-migration MIGRATION_NAME
```

Run all migrations up (happens implicitly on `yarn start` as well):

```
$ yarn run migrate
```

Run a single migration down (step back 1 migration):

```
$ yarn run migrate-down
```

Run all tests

```
$ yarn test
```

If card lang is updated run this to rebuild the parser:
```
$ yarn run build-lang
```

### Components

A server is composed of one of more components that provide various
functionality:

* `auth` Player login and authentication service
* `gamelist` Master game registry
* `game` Hosts running game instances
* `matchmaker` Automatch queue
* `log` Server app to display client json logs in a nice table

### Environment Variables

Environment variables can also be set locally with a `.env` file (see `dot-env-example`)

Variable | Notes | Example
 --- | --- | ---
`COMPONENTS` | Comma-separated list of components to start | `auth,game,gamelist,matchmaker,log`
`DATABASE_URL` | Postgres connection string | `postgres://user:pass@url:1234/schema?ssl=true`
`PORT` | Static file and REST endpoint HTTP port | `10123`
`BCRYPT_ROUNDS` | How many rounds to use for hashing work for passwords | `10`
`PUBLIC_URL` | Publically-visible URL for all components in this process | `http://localhost:10123`
`AUTH_SECRET` | Secret used to sign API auth tokens | `aReallyDamnGoodSecret`
`PING_INTERVAL` | How frequently the server pings clients to update latency | `2000`
`PG_USE_SSL` | Whether to use SSL for postgres (defaults true) | `false`
`CLIENT_LOG` | Where to find the client log file to serve up | `./client_log.json`
`DISCONNECT_TIMEOUT` | How long clients have to reconnect before the boot | `5000`
`DEV` | Allows some developer commands to be sent to the server | `true`
`CARD_SETS` | Which card sets are enabled and going to be loaded | `test,basic`


## Ops

To do DB operations on the server, exec into the db:
`docker exec -it stac_db_1 bash`

Run the psql cmd line tool:
`psql -h localhost -U postgres`

Then you can run your commands
`select * from players;` <-- Must end with semicolon

`\q` to quit