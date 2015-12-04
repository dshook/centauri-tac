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

* NodeJS 0.12.x

### Setup

Install dependencies:

```
$ npm install
```

### Running

Start a server instance and run all migrations:

```
$ npm start
```

Build all frontend applications:

```
$ npm run build
```

Build static assets and watch for changes:

```
$ npm run dev
```

Run the server with `nodemon` to automatically restart on changes:

```
$ npm run dev-server
```

To create a new database migration:

```
$ npm run create-migration MIGRATION_NAME
```

Run all migrations up (happens implicitly on `npm start` as well):

```
$ npm run migrate
```

Run a single migration down (step back 1 migration):

```
$ npm run migrate-down
```

Run all tests

```
$ npm test
```

If card lang is updated run this to rebuild the parser:
```
$ npm run build-lang
```

### Components

A server is composed of one of more components that provide various
functionality:

* `master` Central service locator
* `dispatch` Relays events around components
* `auth` Player login and authentication service
* `gamelist` Master game registry
* `game` Hosts running game instances
* `portal` Frontend application to manage backend services
* `matchmaker` Automatch queue
* `workbench` Server app to run locally to enable game content management via
  the portal
* `clientlog` Server app to monitor log files and serve them up

### Environment Variables

Environment variables can also be set locally with a `.env` file (see `dot-env-example`)

Variable | Notes | Example
 --- | --- | ---
`COMPONENTS` | Comma-separated list of components to start | `master,portal`
`DATABASE_URL` | Postgres connection string | `postgres://user:pass@url:1234/schema?ssl=true`
`PORT` | Static file and REST endpoint HTTP port | `10123`
`BCRYPT_ROUNDS` | How many rounds to use for hashing work for passwords | `10`
`PUBLIC_URL` | Publically-visible URL for all components in this process | `http://localhost:10123`
`AUTH_SECRET` | Secret used to sign API auth tokens | `aReallyDamnGoodSecret`
`PING_INTERVAL` | How frequently the server pings clients to update latency | `2000`
`PG_USE_SSL` | Whether to use SSL for postgres (defaults true) | `false`
`CLIENT_LOG` | Where to find the client log file to serve up | `./client_log.json`

