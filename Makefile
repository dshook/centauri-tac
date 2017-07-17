.PHONY: build

build:
	docker build -t stac:latest -f ./docker/Dockerfile .

build-dev:
	docker build -t stac-dev:latest -f ./docker/Dockerfile-dev .

down:
	docker-compose -p stac -f ./docker/docker-compose.yml kill
	docker-compose -p stac -f ./docker/docker-compose.yml rm -f -v

up: down build-dev
	docker-compose -p stac -f ./docker/docker-compose.yml up --force-recreate -d

logs:
	docker-compose -p stac -f ./docker/docker-compose.yml logs -f --tail 1000

down-prod:
	docker-compose -p stac -f ./docker/docker-compose-prod.yml kill
	docker-compose -p stac -f ./docker/docker-compose-prod.yml rm -f -v

up-prod: down-prod build
	docker-compose -p stac -f ./docker/docker-compose-prod.yml up --force-recreate -d

logs-prod:
	docker-compose -p stac -f ./docker/docker-compose-prod.yml logs -f --tail 1000
