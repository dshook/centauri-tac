.PHONY: build

build:
	docker build -t stac:latest -f ./docker/Dockerfile .

build-dev:
	docker build -t stac-dev:latest -f ./docker/Dockerfile-dev .

down:
	docker-compose -p stac -f ./docker/docker-compose.yml kill
	docker-compose -p stac -f ./docker/docker-compose.yml rm -f -v

up: build-dev down
	docker-compose -p stac -f ./docker/docker-compose.yml up --force-recreate -d

logs:
	docker-compose -p stac -f ./docker/docker-compose.yml logs -f --tail 1000

down-prod:
	docker-compose -p stac -f ./docker/docker-compose-prod.yml kill
	docker-compose -p stac -f ./docker/docker-compose-prod.yml rm -f -v

up-prod: build down-prod
	docker-compose -p stac -f ./docker/docker-compose-prod.yml up --force-recreate -d

logs-prod:
	docker-compose -p stac -f ./docker/docker-compose-prod.yml logs -f --tail 1000


build-client-win:
	"C:\\Program Files\\Unity\\Editor\\Unity.exe" -batchmode -quit -executeMethod BuildScript.WinBuild -projectPath "C:\\Users\\dillo_000.DILLON\\Documents\\Programming\\centauri-tac\\centauri-tac"

build-client-win-dev:
	"C:\\Program Files\\Unity\\Editor\\Unity.exe" -batchmode -quit -executeMethod BuildScript.WinDevBuild -projectPath "C:\\Users\\dillo_000.DILLON\\Documents\\Programming\\centauri-tac\\centauri-tac"

build-client-all:
	"C:\\Program Files\\Unity\\Editor\\Unity.exe" -batchmode -quit -executeMethod BuildScript.BuildAll -projectPath "C:\\Users\\dillo_000.DILLON\\Documents\\Programming\\centauri-tac\\centauri-tac"
