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

RUN yarn install --production

#script to wait for db to come up
COPY ./docker/wait-for-command.sh .
RUN chmod +x wait-for-command.sh

CMD ./wait-for-command.sh -c 'nc -z db 5432' && yarn run dev-server
