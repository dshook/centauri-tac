FROM node:8.1.4-alpine
MAINTAINER Dillon Shook "dshook@alumni.nmt.edu"

# location layout
ENV APP_BASE /opt/app
RUN mkdir -p $APP_BASE
WORKDIR ${APP_BASE}

# server options
ENV PORT 10123
EXPOSE ${PORT}/tcp

# app source and package-level deps
COPY ./package.json .
COPY ./yarn.lock .
COPY ./cards ./cards
COPY ./lang ./lang
COPY ./lib ./lib
COPY ./maps ./maps
COPY ./migrations ./migrations
COPY ./server ./server
COPY ./tests ./tests

#script to wait for db to come up
COPY ./docker/wait-for-db.sh .
RUN chmod 755 wait-for-db.sh

RUN yarn install

CMD ["./wait-for-db.sh", "db", "yarn run dev-server"]