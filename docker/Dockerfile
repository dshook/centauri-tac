FROM node:6.11.1-alpine
MAINTAINER Dillon Shook "dshook@alumni.nmt.edu"

# location layout
ENV APP_BASE /opt/app
RUN mkdir -p $APP_BASE
WORKDIR ${APP_BASE}

# server options
ENV PORT 8000
EXPOSE ${PORT}/tcp

# app source and package-level deps
COPY ./package.json .
COPY ./yarn.lock .
COPY ./cards .
COPY ./lang .
COPY ./lib .
COPY ./maps .
COPY ./migrations .
COPY ./server .
COPY ./tests .

RUN yarn install

CMD yarn run dev-server