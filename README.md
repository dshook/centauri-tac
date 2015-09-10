# centauri-tac

Tactical Card Game

## Backend and Tooling

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

### Components

A server is composed of one of more components that provide various
functionality:

* `master` Central service locator
* `auth` Player login and authentication service
* `portal` Frontend application to manage backend services
* `workbench` Server app to run locally to enable game content management via
  the portal

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
