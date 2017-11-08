.PHONY: build build-dev down up logs down-prod up-prod logs-prod build-client-win build-client-win-dev build-client-all

build:
	docker build -t stac:latest -f ./docker/Dockerfile .

build-dev:
	docker build -t stac-dev:latest -f ./docker/Dockerfile-dev .

down-dev:
	docker-compose -p stac -f ./docker/docker-compose-dev.yml kill
	docker-compose -p stac -f ./docker/docker-compose-dev.yml rm -f -v

up-dev: build-dev down-dev
	docker-compose -p stac -f ./docker/docker-compose-dev.yml up --force-recreate -d

logs-dev:
	docker-compose -p stac -f ./docker/docker-compose-dev.yml logs -f --tail 1000

down:
	docker-compose -p stac -f ./docker/docker-compose.yml kill
	docker-compose -p stac -f ./docker/docker-compose.yml rm -f -v

up: build down
	docker-compose -p stac -f ./docker/docker-compose.yml up --force-recreate -d

logs:
	docker-compose -p stac -f ./docker/docker-compose.yml logs -f --tail 1000

down-site:
	docker-compose -p stac -f ./docker/docker-compose-site.yml kill
	docker-compose -p stac -f ./docker/docker-compose-site.yml rm -f -v

up-site: build down-site
	docker-compose -p stac -f ./docker/docker-compose-site.yml up --force-recreate -d

build-client-win:
	"C:\Program Files\Unity\Editor\Unity.exe" -batchmode -quit -executeMethod BuildScript.WinBuild -projectPath "C:\Users\dillo_000.DILLON\Documents\Programming\centauri-tac\centauri-tac"

build-client-win-dev:
	"C:\Program Files\Unity\Editor\Unity.exe" -batchmode -quit -executeMethod BuildScript.WinDevBuild -projectPath "C:\Users\dillo_000.DILLON\Documents\Programming\centauri-tac\centauri-tac"

build-client-all:
	"C:\Program Files\Unity\Editor\Unity.exe" -batchmode -quit -executeMethod BuildScript.BuildAll -projectPath "C:\Users\dillo_000.DILLON\Documents\Programming\centauri-tac\centauri-tac"
